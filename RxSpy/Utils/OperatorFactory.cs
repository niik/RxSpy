using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using RxSpy.Observables;

namespace RxSpy.Utils
{
    public static class OperatorFactory
    {
        readonly static ConcurrentDictionary<Type, Lazy<ConstructorInfo>> _connectionConstructorCache =
            new ConcurrentDictionary<Type, Lazy<ConstructorInfo>>();


        public static object CreateOperatorObservable(object source, Type signalType, OperatorInfo operatorInfo)
        {
            var ctor = _connectionConstructorCache.GetOrAdd(
                signalType,
                _ => new Lazy<ConstructorInfo>(() => GetOperatorConstructor(signalType)));

            return ctor.Value.Invoke(new object[] { RxSpySession.Current, source, operatorInfo });
        }

        static ConstructorInfo GetOperatorConstructor(Type signalType)
        {
            var operatorObservable = typeof(OperatorObservable<>).MakeGenericType(signalType);

            return operatorObservable.GetConstructor(new[] { 
                typeof(RxSpySession), 
                typeof(IObservable<>).MakeGenericType(signalType), 
                typeof(OperatorInfo) 
            });
        }
    }
}
