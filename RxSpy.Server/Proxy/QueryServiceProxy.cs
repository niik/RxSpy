using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Reactive;
using System.Reactive.Subjects;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using System.Threading;
using RxSpy.Events;
using RxSpy.Observables;

namespace RxSpy.Proxy
{
    internal class QueryLanguageProxy : RealProxy, IRemotingTypeInfo
    {
        readonly static ConcurrentDictionary<System.Reflection.MethodInfo, Lazy<Func<IMethodCallMessage, IMethodReturnMessage>>> _methodHandlerCache =
            new ConcurrentDictionary<System.Reflection.MethodInfo, Lazy<Func<IMethodCallMessage, IMethodReturnMessage>>>();

        readonly object _queryService;
        readonly Type _queryServiceType;
        readonly RxSpySession _session;

        internal QueryLanguageProxy(RxSpySession session, object realQueryService)
            : base(typeof(ContextBoundObject))
        {
            _queryService = realQueryService;
            _queryServiceType = realQueryService.GetType();
            _session = session;
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
            var handler = GetHandler(call, method);

            return handler(call);
        }

        private Func<IMethodCallMessage, IMethodReturnMessage> GetHandler(IMethodCallMessage call, System.Reflection.MethodInfo method)
        {
            var handler = _methodHandlerCache.GetOrAdd(
                method, 
                _ => new Lazy<Func<IMethodCallMessage, IMethodReturnMessage>> (
                    () => CreateHandler(call, method), LazyThreadSafetyMode.ExecutionAndPublication));

            return handler.Value;
        }

        Func<IMethodCallMessage, IMethodReturnMessage> CreateHandler(IMethodCallMessage call, System.Reflection.MethodInfo method)
        {
            if (call.MethodName == "GetAwaiter")
            {
                return ForwardCall;
            }

            // IConnectableObservable parameters
            if (call.MethodName == "RefCount")
            {
                return CreateRefCountHandler(call);
            }

            // IConnectableObservable return types
            if (Array.IndexOf(_connectableCandidates, call.MethodName) >= 0 &&
                IsGenericTypeDefinition(method.ReturnType, typeof(IConnectableObservable<>)))
            {
                return c => HandleConnectableReturnType(c, method, CreateOperatorInfo(call));
            }
            else if (IsGenericTypeDefinition(method.ReturnType, typeof(IObservable<>)))
            {
                return c => HandleObservableReturnType(c, method, CreateOperatorInfo(call));
            }
             
            return ForwardCall;
        }

        private static OperatorInfo CreateOperatorInfo(IMethodCallMessage call)
        {
            var operatorCallSite = new MethodInfo(call.MethodBase);
            var callSite = new CallSite(new StackFrame(5, true));
            var operatorInfo = new OperatorInfo(callSite, operatorCallSite);
            return operatorInfo;
        }

        private IMethodReturnMessage HandleObservableReturnType(IMethodCallMessage call, System.Reflection.MethodInfo method, OperatorInfo operatorInfo)
        {
            var actualObservable = ProduceActualObservable(call, method, operatorInfo);

            var ret = CreateOperatorObservable(actualObservable, method.ReturnType.GetGenericArguments()[0], operatorInfo);
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
            var args = ReplaceAllObservablesWithConnections(method, call.InArgs, operatorInfo);
            var actualObservable = call.MethodBase.Invoke(_queryService, args);
            return actualObservable;
        }

        private IMethodReturnMessage ForwardCall(IMethodCallMessage call)
        {
            return new ReturnMessage(call.MethodBase.Invoke(_queryService, call.InArgs), null, 0, null, call);
        }

        static readonly string[] _connectableCandidates = new[] { "Multicast", "Publish", "PublishLast", "Replay" };
        static readonly string[] _blockingCandidates = new[] { "Wait", "First", "FirstOrDefault", "Last", "LastOrDefault", "ForEach", };

        bool IsGenericTypeDefinition(Type source, Type genericTypeComparand)
        {
            return source.IsGenericType && source.GetGenericTypeDefinition() == genericTypeComparand;
        }

        Func<IMethodCallMessage, IMethodReturnMessage> CreateRefCountHandler(IMethodCallMessage call)
        {
            var signalType = call.MethodBase.GetGenericArguments()[0];
            var connectableOperatorConnectionType = typeof(ConnectableOperatorConnection<>).MakeGenericType(signalType);

            return c => HandleRefCount(c, connectableOperatorConnectionType, signalType);
        }

        IMethodReturnMessage HandleRefCount(IMethodCallMessage call, Type connectableOperatorConnectionType, Type signalType)
        {
            var operatorInfo = CreateOperatorInfo(call);

            Debug.Assert(call.InArgs.Length == 1);

            var args = new object[] {
                Activator.CreateInstance(
                    connectableOperatorConnectionType,
                    new object[] { _session, call.InArgs[0], operatorInfo }
                )
            };

            var actualObservable = call.MethodBase.Invoke(_queryService, call.InArgs);
            var ret = CreateOperatorObservable(actualObservable, signalType, operatorInfo);

            return new ReturnMessage(ret, null, 0, null, call);
        }

        object[] ReplaceAllObservablesWithConnections(System.Reflection.MethodInfo method, object[] args, OperatorInfo operatorInfo)
        {
            object[] parameterValues = new object[args.Length];
            var parameterInfos = method.GetParameters();

            for (int i = 0; i < parameterValues.Length; i++)
            {
                var pt = parameterInfos[i].ParameterType;

                if (IsGenericTypeDefinition(pt, typeof(IObservable<>)))
                {
                    var signalType = pt.GetGenericArguments()[0];

                    parameterValues[i] = CreateObservableConnection(args[i], signalType, operatorInfo);
                }
                else if (pt.IsArray)
                {
                    var observableType = pt.GetElementType();

                    if (IsGenericTypeDefinition(observableType, typeof(IObservable<>)))
                    {
                        var signalType = observableType.GetGenericArguments()[0];

                        var argArray = (Array)args[i];
                        var newArray = Array.CreateInstance(observableType, argArray.Length);

                        for (int j = 0; j < argArray.Length; j++)
                        {
                            newArray.SetValue(CreateObservableConnection(argArray.GetValue(j), signalType, operatorInfo), j);
                        }

                        parameterValues[i] = args[i];
                    }
                    else
                    {
                        parameterValues[i] = args[i];
                    }
                }
                else if (IsGenericTypeDefinition(pt, typeof(IEnumerable<>)) &&
                    IsGenericTypeDefinition(pt.GetGenericArguments()[0], typeof(IObservable<>)))
                {
                    var observableType = pt.GetGenericArguments()[0];
                    var signalType = observableType.GetGenericArguments()[0];

                    var enumerableConnectionType = typeof(DeferredOperatorConnectionEnumerable<>)
                            .MakeGenericType(observableType);

                    parameterValues[i] = Activator.CreateInstance(
                        enumerableConnectionType,
                        new object[] { 
                            args[i], 
                            new Func<object, object>(o => CreateObservableConnection(o, signalType, operatorInfo)) 
                        });
                }
                else
                {
                    parameterValues[i] = args[i];
                }
            }

            return parameterValues;
        }

        object CreateObservableConnection(object source, Type signalType, OperatorInfo operatorInfo)
        {
            var operatorObservable = typeof(OperatorConnection<>).MakeGenericType(signalType);

            var instance = operatorObservable.GetConstructor(new[] { typeof(RxSpySession), typeof(IObservable<>).MakeGenericType(signalType), typeof(OperatorInfo) })
                .Invoke(new object[] { _session, source, operatorInfo });

            return instance;
        }

        object CreateOperatorObservable(object source, Type signalType, OperatorInfo operatorInfo)
        {
            var operatorObservable = typeof(OperatorObservable<>).MakeGenericType(signalType);

            var instance = operatorObservable.GetConstructor(new[] { typeof(RxSpySession), typeof(IObservable<>).MakeGenericType(signalType), typeof(OperatorInfo) })
                .Invoke(new object[] { _session, source, operatorInfo });

            return instance;
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

        class DeferredOperatorConnectionEnumerable<T> : IEnumerable<T>
        {
            readonly IEnumerable<T> _source;
            readonly Func<object, object> _selector;

            public DeferredOperatorConnectionEnumerable(IEnumerable<T> source, Func<object, object> selector)
            {
                _source = source;
                _selector = selector;
            }

            public IEnumerator<T> GetEnumerator()
            {
                foreach (var item in _source)
                    yield return (T)_selector(item);
            }

            System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
            {
                return GetEnumerator();
            }
        }
    }
}
