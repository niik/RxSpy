using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
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

        public static void Launch(string pathToRxSpy = null)
        {
            Launch(TimeSpan.FromHours(5), pathToRxSpy);
        }

        public static void Launch(TimeSpan timeout, string pathToRxSpy = null)
        {
            if (_launched == 1)
                throw new InvalidOperationException("Session already created");

            string pathToGui = FindGuiPath(pathToRxSpy);

            if (pathToGui == null)
                throw new FileNotFoundException("Could not locate RxSpy.exe");

            var server = CreateServer();

            Console.WriteLine("Server running at " + server.Address);

            var psi = new ProcessStartInfo(pathToGui);
            psi.Arguments = server.Address.AbsoluteUri;

            Process.Start(psi);
            server.WaitForConnection(timeout);
            
            var session = new RxSpySession(server);
            Current = session;

            if (Interlocked.CompareExchange(ref _launched, 1, 0) != 0)
                throw new InvalidOperationException("Session already created");

            InstallInterceptingQueryLanguage(session);
        }

        public static void LogToFile(string path)
        {
            var server = new RxSpyFileWriter(path);

            server.WaitForConnection(TimeSpan.Zero);
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

        static string FindGuiPath(string explicitPathToRxSpy)
        {
            // Try a few different things attempting to find RxSpy.exe, depending
            // on how things are configured
            if (explicitPathToRxSpy != null) return explicitPathToRxSpy;

            // Same directory as us?
            var ourAssembly = typeof(RxSpySession).Assembly;
            var rxSpyDir = Path.GetDirectoryName(ourAssembly.Location);
            var target = Path.Combine(rxSpyDir, "RxSpy.exe");
            if (File.Exists(target))
            {
                return target;
            }

            // Attempt to find the solution directory
            var st = new StackTrace(true);
            var firstExternalFrame = Enumerable.Range(0, 1000)
                .Select(x => st.GetFrame(x))
                .First(x => x.GetMethod().DeclaringType.Assembly != ourAssembly);

            var di = new DirectoryInfo(Path.GetDirectoryName(firstExternalFrame.GetFileName()));

            while (di != null) {
                if (di.GetFiles("*.sln").Any()) break;
                di = di.Parent;
            }

            // Debug mode?
            var fi = new FileInfo(Path.Combine(di.FullName, "RxSpy", "bin", "Debug", "RxSpy.exe"));
            if (fi.Exists) return fi.FullName;

            // Attempt to track down our own version
            fi = new FileInfo(Path.Combine(di.FullName,
                "packages",
                String.Format("RxSpy.{0}", ourAssembly.GetCustomAttribute<AssemblyFileVersionAttribute>().Version),
                "tools",
                "RxSpy.exe"));
            if (fi.Exists) return fi.FullName;

            throw new ArgumentException("Can't find RxSpy.exe - either copy it and its DLLs to your output directory or pass in a path to Create");
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
