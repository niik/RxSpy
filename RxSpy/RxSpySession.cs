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
    public class RxSpySession: IRxSpyEventHandler, IDisposable
    {
        static int _launched = 0;
        readonly IRxSpyEventHandler _eventHandler;

        internal static RxSpySession Current { get; private set; }

        RxSpySession(IRxSpyEventHandler eventHandler)
        {
            _eventHandler = eventHandler;
        }

        public static RxSpySession Launch(string pathToRxSpy = null)
        {
            return Launch(TimeSpan.FromSeconds(10), pathToRxSpy);
        }

        public static RxSpySession Launch(TimeSpan timeout, string pathToRxSpy = null)
        {
            if (_launched == 1)
                throw new InvalidOperationException("Session already created");

            string pathToGui = FindGuiPath(pathToRxSpy);

            if (pathToGui == null)
                throw new FileNotFoundException("Could not locate RxSpy.LiveView.exe");

            var server = new RxSpyHttpServer();

            Debug.WriteLine("RxSpy server running at " + server.Address);

            var psi = new ProcessStartInfo(pathToGui);
            psi.Arguments = server.Address.AbsoluteUri;

            Process.Start(psi);
            server.WaitForConnection(timeout);

            return Launch(server);
        }

        public static RxSpySession Launch(IRxSpyEventHandler eventHandler)
        {
            var session = new RxSpySession(eventHandler);
            Current = session;

            if (Interlocked.CompareExchange(ref _launched, 1, 0) != 0)
                throw new InvalidOperationException("Session already created");

            InstallInterceptingQueryLanguage(session);

            return session;
        }

        static string FindGuiPath(string explicitPathToRxSpy)
        {
            // Try a few different things attempting to find RxSpy.LiveView.exe
            // depending on how things are configured
            if (explicitPathToRxSpy != null) return explicitPathToRxSpy;

            // Same directory as us?
            var ourAssembly = typeof(RxSpySession).Assembly;
            var rxSpyDir = Path.GetDirectoryName(ourAssembly.Location);
            var target = Path.Combine(rxSpyDir, "RxSpy.LiveView.exe");
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
            var fi = new FileInfo(Path.Combine(di.FullName, "RxSpy.LiveView", "bin", "Debug", "RxSpy.LiveView.exe"));
            if (fi.Exists) return fi.FullName;

            // Attempt to track down our own version
            fi = new FileInfo(Path.Combine(di.FullName,
                "packages",
                String.Format("RxSpy.LiveView.{0}", ourAssembly.GetCustomAttribute<AssemblyFileVersionAttribute>().Version),
                "tools",
                "RxSpy.LiveView.exe"));
            if (fi.Exists) return fi.FullName;

            throw new ArgumentException("Can't find RxSpy.LiveView.exe - either copy it and its DLLs to your output directory or pass in a path to Create");
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

        public void OnCreated(IOperatorCreatedEvent onCreatedEvent)
        {
            _eventHandler.OnCreated(onCreatedEvent);
        }

        public void OnCompleted(IOnCompletedEvent onCompletedEvent)
        {
            _eventHandler.OnCompleted(onCompletedEvent);
        }

        public void OnError(IOnErrorEvent onErrorEvent)
        {
            _eventHandler.OnError(onErrorEvent);
        }

        public void OnNext(IOnNextEvent onNextEvent)
        {
            _eventHandler.OnNext(onNextEvent);
        }

        public void OnSubscribe(ISubscribeEvent subscribeEvent)
        {
            _eventHandler.OnSubscribe(subscribeEvent);
        }

        public void OnUnsubscribe(IUnsubscribeEvent unsubscribeEvent)
        {
            _eventHandler.OnUnsubscribe(unsubscribeEvent);
        }

        public void OnConnected(IConnectedEvent connectedEvent)
        {
            _eventHandler.OnConnected(connectedEvent);
        }

        public void OnDisconnected(IDisconnectedEvent disconnectedEvent)
        {
            _eventHandler.OnDisconnected(disconnectedEvent);
        }

        public void OnTag(ITagOperatorEvent tagEvent)
        {
            _eventHandler.OnTag(tagEvent);
        }

        public void Dispose()
        {
            _eventHandler.Dispose();
            GC.SuppressFinalize(this);
        }
    }
}
