using System;
using System.Diagnostics;
using System.IO;
using System.Reactive;
using System.Reactive.Linq;
using System.Reflection;
using System.Threading;
using RxSpy.Communication;
using RxSpy.Events;
using RxSpy.Observables;
using RxSpy.Proxy;
using RxSpy.Utils;

namespace RxSpy
{
    public class RxSpySession
    {
        static int _launched = 0;
        readonly IRxSpyServer _server;
        readonly WeakObserverCache _cache = new WeakObserverCache();

        internal static RxSpySession Current { get; private set; }

        RxSpySession(IRxSpyServer server)
        {
            _server = server;
        }

        public static void Launch()
        {
            Launch(TimeSpan.FromHours(5));
        }

        public static void Launch(TimeSpan timeout)
        {
            if (_launched == 1)
                throw new InvalidOperationException("Session already created");

            string pathToGui = FindGuiPath();

            if (pathToGui == null)
                throw new FileNotFoundException("Could not locate RxSpy.exe");

            var server = CreateServer();

            Console.WriteLine("Server running at " + server.Address);

            //Process.Start(Path.Combine(Environment.CurrentDirectory, pathToGui), server.Address.AbsoluteUri);

            var psi = new ProcessStartInfo(pathToGui);
            psi.Arguments = server.Address.AbsoluteUri;

            server.WaitForConnection(timeout);
            var session = new RxSpySession(server);
            Current = session;

            if (Interlocked.CompareExchange(ref _launched, 1, 0) != 0)
                throw new InvalidOperationException("Session already created");

            InstallInterceptingQueryLanguage(session);
        }

        static IRxSpyServer CreateServer()
        {
            return new RxSpyHttpServer();
        }

        static string FindGuiPath()
        {
            return @"..\..\..\RxSpy\bin\Debug\RxSpy.exe";
        }

        static void InstallInterceptingQueryLanguage(RxSpySession session)
        {
            // TODO: Verify that the version is supported
            var rxLinqAssembly = Assembly.Load(new AssemblyName("System.Reactive.Linq"));

            // Make sure it's initialized
            Observable.Empty<Unit>();

            var observableType = typeof(Observable);
            var defaultImplementationField = observableType.GetField("s_impl", BindingFlags.Static | BindingFlags.NonPublic);

            var actualImplementation = defaultImplementationField.GetValue(null);

            object proxy = new QueryLanguageProxy(session, actualImplementation)
                .GetTransparentProxy();

            defaultImplementationField.SetValue(null, proxy);
        }

        internal void EnqueueEvent(Event ev)
        {
            _server.EnqueueEvent(ev);
        }

        internal OperatorInfo GetOperatorInfoFor<T>(IObserver<T> observer)
        {
            var oobs = observer as IOperatorObservable;

            if (oobs != null)
                return oobs.OperatorInfo;

            OperatorInfo operatorInfo;

            if (!_cache.TryGetOrAdd(observer, out operatorInfo))
                EnqueueEvent(Event.OperatorCreated(operatorInfo));

            return operatorInfo;
        }
    }
}
