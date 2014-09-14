using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RxSpy.Observables;

namespace System.Reactive.Linq
{
    public static class SpyObservableExtensions
    {
        public static IObservable<T> SpyTag<T>(this IObservable<T> source, string tag)
        {
            var oobs = source as OperatorObservable<T>;

            if (oobs != null)
                oobs.Tag(tag);

            return source;
        }
    }
}
