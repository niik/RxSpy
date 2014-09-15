using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Subjects;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Threading;
using RxSpy.Events;
using RxSpy.Observables;
using RxSpy.Utils;
using System.Reactive.Linq;

namespace RxSpy.Proxy
{
    internal class QueryLanguageProxy : RealProxy, IRemotingTypeInfo
    {
        readonly static ConcurrentDictionary<System.Reflection.MethodInfo, Lazy<Func<IMethodCallMessage, IMethodReturnMessage>>> _methodHandlerCache =
            new ConcurrentDictionary<System.Reflection.MethodInfo, Lazy<Func<IMethodCallMessage, IMethodReturnMessage>>>();

        readonly static ConcurrentDictionary<System.Reflection.MethodInfo, int> _skipFrameCount =
            new ConcurrentDictionary<System.Reflection.MethodInfo, int>();

        readonly object _queryLanguage;
        readonly Type _queryLanguageType;
        readonly Type _queryLanguageInterface;
        readonly RxSpySession _session;
        readonly bool _explicitCapture;

        internal QueryLanguageProxy(RxSpySession session, object realQueryLanguage, bool explicitCapture)
            : base(typeof(ContextBoundObject))
        {
            _queryLanguage = realQueryLanguage;
            _queryLanguageType = realQueryLanguage.GetType();
            _queryLanguageInterface = _queryLanguageType.GetInterface("IQueryLanguage");
            _session = session;
            _explicitCapture = explicitCapture;
        }

        public override IMessage Invoke(IMessage msg)
        {
            var call = msg as IMethodCallMessage;

            if (call == null)
                throw new ArgumentException("QueryLanguageProxy only supports call messages");

            if (RxSpyGroup.IsActive)
            {
                return ForwardCall(call);
            }

            var method = (System.Reflection.MethodInfo)call.MethodBase;

            int skipFrames;

            // Walk the stack until we find the first invocation to IQueryLanguage, this is expensive
            // but fortunately unique per method so we can cache it.
            if (!_skipFrameCount.TryGetValue(method, out skipFrames))
            {
                skipFrames = -1;

                var trace = new StackTrace(0, false);
                var frames = trace.GetFrames();

                for (int i = 0; i < frames.Length; i++)
                {
                    if (frames[i].GetMethod().DeclaringType == _queryLanguageInterface)
                    {
                        for (; i < frames.Length; i++)
                        {
                            if (frames[i].GetMethod().DeclaringType != _queryLanguageInterface)
                            {
                                skipFrames = _skipFrameCount.GetOrAdd(method, i + 1);
                                break;
                            }
                        }
                    }
                }
            }

            Debug.Assert(skipFrames > 0);

            var callSite = skipFrames == -1 ? null : CallSiteCache.Get(skipFrames);
            var handler = GetHandler(call, method, callSite);

            return handler(call);
        }

        Func<IMethodCallMessage, IMethodReturnMessage> GetHandler(IMethodCallMessage call, System.Reflection.MethodInfo method, CallSite callSite)
        {
            if (call.MethodName == "GetAwaiter")
            {
                return ForwardCall;
            }

            var handler = _methodHandlerCache.GetOrAdd(
                method,
                _ => new Lazy<Func<IMethodCallMessage, IMethodReturnMessage>>(
                    () => CreateHandler(call, method, callSite), LazyThreadSafetyMode.ExecutionAndPublication));

            return handler.Value;
        }

        Func<IMethodCallMessage, IMethodReturnMessage> CreateHandler(IMethodCallMessage call, System.Reflection.MethodInfo method, CallSite callSite)
        {
            // IConnectableObservable parameters
            if (call.MethodName == "RefCount")
            {
                return CreateRefCountHandler(call, method, CreateOperatorInfo(method, callSite));
            }

            // IConnectableObservable return types
            if (Array.IndexOf(_connectableCandidates, call.MethodName) >= 0 &&
                IsGenericTypeDefinition(method.ReturnType, typeof(IConnectableObservable<>)))
            {
                return c => HandleConnectableReturnType(c, method, CreateOperatorInfo(method, callSite));
            }
            else if (IsGenericTypeDefinition(method.ReturnType, typeof(IObservable<>)))
            {
                return c => HandleObservableReturnType(c, method, CreateOperatorInfo(method, callSite));
            }

            return ForwardCall;
        }

        private static OperatorInfo CreateOperatorInfo(System.Reflection.MethodInfo method, CallSite callsite)
        {
            return new OperatorInfo(callsite, new MethodInfo(method));
        }

        private IMethodReturnMessage HandleObservableReturnType(IMethodCallMessage call, System.Reflection.MethodInfo method, OperatorInfo operatorInfo)
        {
            var actualObservable = ProduceActualObservable(call, method, operatorInfo);

            var ret = OperatorFactory.CreateOperatorObservable(actualObservable, method.ReturnType.GetGenericArguments()[0], operatorInfo);
            return new ReturnMessage(ret, null, 0, null, call);
        }

        private IMethodReturnMessage HandleConnectableReturnType(IMethodCallMessage call, System.Reflection.MethodInfo method, OperatorInfo operatorInfo)
        {
            var genericType = method.ReturnType.GetGenericArguments()[0];
            var actualObservable = ProduceActualObservable(call, method, operatorInfo);

            var connectable = Activator.CreateInstance(
                typeof(ConnectableOperatorObservable<>).MakeGenericType(genericType),
                new object[] { _session, actualObservable, operatorInfo }
            );

            return new ReturnMessage(connectable, null, 0, null, call);
        }

        private object ProduceActualObservable(IMethodCallMessage call, System.Reflection.MethodInfo method, OperatorInfo operatorInfo)
        {
            var args = ReplaceAllObservablesWithConnections(
                method.GetParameters().Select(p => p.ParameterType).ToArray(),
                call.InArgs,
                operatorInfo
            );

            return MethodInvoker.Invoke(_queryLanguage, _queryLanguageType, method, args);
        }

        private IMethodReturnMessage ForwardCall(IMethodCallMessage call)
        {
            var ret = MethodInvoker.Invoke(_queryLanguage, _queryLanguageType, (System.Reflection.MethodInfo)call.MethodBase, call.InArgs);

            return new ReturnMessage(ret, null, 0, null, call);
        }

        static readonly string[] _connectableCandidates = new[] { "Multicast", "Publish", "PublishLast", "Replay" };
        static readonly string[] _blockingCandidates = new[] { "Wait", "First", "FirstOrDefault", "Last", "LastOrDefault", "ForEach", };

        static bool IsGenericTypeDefinition(Type source, Type genericTypeComparand)
        {
            return source.IsGenericType && source.GetGenericTypeDefinition() == genericTypeComparand;
        }

        Func<IMethodCallMessage, IMethodReturnMessage> CreateRefCountHandler(IMethodCallMessage call, System.Reflection.MethodInfo method, OperatorInfo operatorInfo)
        {
            var signalType = method.GetGenericArguments()[0];
            var connectableOperatorConnectionType = typeof(ConnectableOperatorConnection<>).MakeGenericType(signalType);

            return c => HandleRefCount(c, method, connectableOperatorConnectionType, signalType, operatorInfo);
        }

        IMethodReturnMessage HandleRefCount(IMethodCallMessage call, System.Reflection.MethodInfo method, Type connectableOperatorConnectionType, Type signalType, OperatorInfo operatorInfo)
        {
            Debug.Assert(call.InArgs.Length == 1);

            var args = new object[] {
                Activator.CreateInstance(
                    connectableOperatorConnectionType,
                    new object[] { _session, call.InArgs[0], operatorInfo }
                )
            };

            var actualObservable = MethodInvoker.Invoke(_queryLanguage, _queryLanguageType, method, call.InArgs); 
            var ret = OperatorFactory.CreateOperatorObservable(actualObservable, signalType, operatorInfo);

            return new ReturnMessage(ret, null, 0, null, call);
        }

        object[] ReplaceAllObservablesWithConnections(Type[] parameterTypes, object[] args, OperatorInfo operatorInfo)
        {
            object[] parameterValues = new object[args.Length];

            for (int i = 0; i < parameterValues.Length; i++)
            {
                object connectionObject;

                if (ConnectionFactory.TryCreateConnection(parameterTypes[i], args[i], operatorInfo, out connectionObject))
                {
                    parameterValues[i] = connectionObject;
                }
                else
                {
                    parameterValues[i] = args[i];
                }
            }

            return parameterValues;
        }

        public bool CanCastTo(Type fromType, object o)
        {
            return fromType.Name == "IQueryLanguage";
        }

        public string TypeName
        {
            get { return this.GetType().Name; }
            set { }
        }

    }
}
