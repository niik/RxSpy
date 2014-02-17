using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using RxSpy.Utils;

namespace RxSpy.TestConsole
{
    class Program
    {
        [DebuggerDisplay("{foo,nq}")]
        class Dummy
        {
            string foo = "bar";
        }

        static void Main(string[] args)
        {
            RxSpySession.Launch();

            var dummy = new [] { "Foo", "Bar", "Baz" };

            while (true)
            {
                var obs1 = Observable.Timer(TimeSpan.Zero, TimeSpan.FromSeconds(1));
                var obs2 = obs1.Select(x => dummy[x % dummy.Length]);
                var obs3 = obs1.Select(x => "---").SpyTag("Fofofofof");

                var obs4 = obs2.Where(x => x.StartsWith("B"));
                var obsErr = Observable.Throw<string>(new InvalidOperationException()).Catch(Observable.Return(""));

                var toJoin = new List<IObservable<string>> { obs3, obs4, obsErr };

                var obs5 = Observable.CombineLatest(toJoin);
                var obs6 = obs5.Select(x => string.Join(", ", x));

                //using (obs.Subscribe())
                using (obs6.Subscribe())
                {
                    Console.ReadLine();
                    Console.WriteLine("Disposing of all observables");
                }

                Console.WriteLine("Press enter to begin again");
                Console.ReadLine();
            }
        }
    }
}
