using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RxSpy.Observables;

namespace RxSpy
{
    public class RxSpyGroup
    {
        [ThreadStatic]
        internal static bool IsActive = false;

        public static IObservable<T> Create<T>(string name, Func<IObservable<T>> factory)
        {
            if (RxSpySession.Current == null)
                return factory();

            try
            {
                IsActive = true;
                return new OperatorObservable<T>(RxSpySession.Current, factory(), new OperatorInfo(name));
            }
            finally
            {
                IsActive = false;
            }
        }
    }
}
