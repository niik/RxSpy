using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Runtime.Remoting;
using System.Runtime.Remoting.Messaging;
using System.Runtime.Remoting.Proxies;
using RxSpy.Events;
using RxSpy.Observables;

namespace RxSpy.Proxy
{
    internal class QueryLanguageProxy : RealProxy, IRemotingTypeInfo
    {
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
                throw new NotImplementedException();

            if (RxSpyGroup.IsActive)
            {
                return new ReturnMessage(call.MethodBase.Invoke(_queryService, call.InArgs), null, 0, null, call);
            }

            var operatorCallSite = new MethodInfo(call.MethodBase);
            var callSite = new CallSite(new StackTrace(4, true).GetFrames()[0]);
            var operatorInfo = new OperatorInfo(callSite, operatorCallSite);

            var args = ReplaceAllObservablesWithConnections(call.InArgs, operatorInfo);
            var actualObservable = call.MethodBase.Invoke(_queryService, args);

            var ret = TryCreateOperatorObservable(actualObservable, operatorInfo);

            return new ReturnMessage(ret, null, 0, null, call);
        }

        object[] ReplaceAllObservablesWithConnections(object[] args, OperatorInfo operatorInfo)
        {
            object[] parameters = new object[args.Length];

            for (int i = 0; i < parameters.Length; i++)
            {
                parameters[i] = TryCreateOperatorConnection(args[i], operatorInfo);
            }

            return parameters;
        }

        object TryCreateOperatorConnection(object arg, OperatorInfo operatorInfo)
        {
            if (arg == null)
                return null;

            var argType = arg.GetType();

            if (argType.IsArray)
            {
                var elementType = argType.GetElementType();

                if (!IsObservable(elementType))
                    return arg;

                var argArray = (Array)arg;
                var newArray = (Array)Activator.CreateInstance(argType, new object[] { argArray.Length });

                for (int i = 0; i < argArray.Length; i++)
                {
                    newArray.SetValue(CreateObservableConnection(argArray.GetValue(i), elementType, operatorInfo), i);
                }

                return newArray;
            }

            if (arg is System.Collections.IEnumerable && argType.IsGenericType)
            {
                var ifaces = argType.GetInterfaces();

                foreach (var iface in ifaces)
                {
                    if (iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(IEnumerable<>))
                    {
                        var elementType = argType.GetGenericArguments()[0];

                        if (!IsObservable(elementType))
                            return arg;

                        var enumerableConnectionType = typeof(DeferredOperatorConnectionEnumerable<>)
                            .MakeGenericType(elementType);

                        var enumerable = Activator.CreateInstance(enumerableConnectionType, new object[] { 
                            arg, 
                            new Func<object, object>(o => CreateObservableConnection(o, elementType, operatorInfo))
                        });

                        return enumerable;
                    }
                }
            }

            if (IsObservable(argType))
            {
                return CreateObservableConnection(arg, argType, operatorInfo);
            }

            return arg;
        }

        object CreateObservableConnection(object source, Type sourceType, OperatorInfo operatorInfo)
        {
            var iface = GetIObserverInterface(sourceType);

            if (iface == null)
                return source;

            var operatorObservable = typeof(OperatorConnection<>).MakeGenericType(iface.GetGenericArguments());

            var instance = operatorObservable.GetConstructor(new[] { sourceType, typeof(OperatorInfo) })
                .Invoke(new object[] { source, operatorInfo });

            return instance;
        }

        object TryCreateOperatorObservable(object source, OperatorInfo operatorInfo)
        {
            if (source == null)
                return source;

            var sourceType = source.GetType();

            var iface = GetIObserverInterface(sourceType);

            if (iface == null)
                return source;

            var operatorObservable = typeof(OperatorObservable<>).MakeGenericType(iface.GetGenericArguments());

            var instance = operatorObservable.GetConstructor(new[] { typeof(RxSpySession), sourceType, typeof(OperatorInfo) })
                .Invoke(new object[] { _session, source, operatorInfo });

            return instance;
        }

        Type GetIObserverInterface(Type t)
        {
            // todo: cache

            if (t.IsInterface && t.GetGenericTypeDefinition() == typeof(IObservable<>))
                return t;

            foreach (var iface in t.GetInterfaces())
            {
                if (iface.IsGenericType && iface.GetGenericTypeDefinition() == typeof(IObservable<>))
                    return iface;
            }

            return null;
        }

        bool IsObservable(Type t)
        {
            return GetIObserverInterface(t) != null;
        }

        public bool CanCastTo(Type fromType, object o)
        {
            return true;
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
