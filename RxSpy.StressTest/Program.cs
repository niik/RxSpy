using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Reactive.Threading.Tasks;
using System.Reactive.Concurrency;
using System.Reactive;
using System.Reactive.Subjects;

namespace RxSpy.StressTest
{
    class Program
    {
        static void Main(string[] args)
        {
            // This app is meant as a tool for benchmarking performance when developing features.
            // Run it a few times before starting work on a new feature and then continually as you
            // make changes to monitor your progress.

            var sw = Stopwatch.StartNew();

            Run();

            var warmupRun = sw.Elapsed;
            Console.WriteLine("Warm up round without RxSpy took {0:N4}", warmupRun.TotalSeconds);

            sw.Restart();
            Run();

            var pureRun = sw.Elapsed;
            Console.WriteLine("Stress test without RxSpy took {0:N4}", pureRun.TotalSeconds);

            // Launching with the streamwriter will benchmark serialization performance but not
            // create a huge file on disk.
            var fakeStream = new StressTestStream();
            var eventHandler = new StressTestEventHandler(new RxSpyStreamWriter(fakeStream));

            sw.Restart();

            using (RxSpySession.Launch(eventHandler))
            {
                Run();
            }

            var spyRun = sw.Elapsed;

            Log("Stress test run took {0:N4}, captured {1:N0} events, {2:N0} observables", spyRun.TotalSeconds, eventHandler.EventCount, eventHandler.ObservableCount);
            Log("Produced {0:N0} bytes of event output", fakeStream.Length);
            Log("RxSpy was {0:N2}x slower", spyRun.TotalSeconds / pureRun.TotalSeconds);

            if (!Debugger.IsAttached)
                Console.ReadLine();
        }

        static void Log(string format, params object[] args)
        {
            var text = String.Format(format, args);

            Console.WriteLine(text);
            Debug.WriteLine(text);

            File.AppendAllText("out.log", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " " + text + "\r\n");
        }

        private static void Run()
        {
            var waitForCompletion = new List<Task>();

            for (int i = 0; i < 10; i++)
            {
                waitForCompletion.AddRange(CreateSimpleObservables());
                waitForCompletion.AddRange(CreateConnectableObservables());
                waitForCompletion.AddRange(CreateAsyncObservables());
                waitForCompletion.AddRange(CreatLongObservableChain());
            }

            Task.WaitAll(waitForCompletion.ToArray());
        }

        private static IEnumerable<Task> CreateAsyncObservables()
        {
            for (int i = 0; i < 5; i++)
            {
                yield return Observable.Range(0, 1000)
                    .ObserveOn(TaskPoolScheduler.Default)
                    .Where(x => x % 2 == 0)
                    .ToTask();
            }
        }

        static IEnumerable<Task> CreateSimpleObservables()
        {
            yield return Observable.Range(0, 1000).Where(x => x % 2 == 0).ToTask();

            yield return Observable.Range(0, 1000).Where(x => x % 2 == 0)
                .Select(x => new CustomObjectWithDebuggerDisplay { Name = x.ToString() })
                .Select(x => x)
                .ToTask();
        }

        static IEnumerable<Task> CreateConnectableObservables()
        {
            var subject = new AsyncSubject<Unit>();

            yield return Observable.Defer(() => Observable.Start(() => Unit.Default))
                .Multicast(subject)
                .RefCount()
                .ToTask();
        }

        static IEnumerable<Task> CreatLongObservableChain()
        {
            var obs = Observable.Range(0, 1000);

            for (int i = 0; i < 50; i++)
            {
                obs = obs.Select(x => x).Where(x => true);
            }

            yield return obs.ToTask();
        }
    }
}
