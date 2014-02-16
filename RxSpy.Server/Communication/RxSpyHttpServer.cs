using System;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using RxSpy.Communication.Serialization;
using RxSpy.Events;

namespace RxSpy.Communication
{
    // I feel the need to appologize for the messy TPL code in here. This would have been so
    // clean if I could only use Rx but obviously I can't since I replaced the entire 
    // Rx implementation and it would only add noise to the receiving clients.
    internal sealed class RxSpyHttpServer : IRxSpyServer
    {
        readonly HttpListener _server;
        readonly Task _serverTask;
        readonly CancellationTokenSource _cancellationTokenSource;
        readonly IJsonSerializerStrategy _serializerStrategy;

        readonly AutoResetEvent _connnectionSignal = new AutoResetEvent(false);
        bool _hasConnection = false;

        public Uri Address { get; private set; }

        readonly BufferBlock<Event> _queue = new BufferBlock<Event>();

        public RxSpyHttpServer()
        {
            _server = new HttpListener();
            Address = new Uri("http://localhost:" + GetRandomTcpPort() + "/rxspy/");

            _serializerStrategy = new RxSpyJsonSerializerStrategy();

            _server.Prefixes.Add(Address.AbsoluteUri);
            _server.Start();

            var cts = new CancellationTokenSource();

            _serverTask = Task.Factory.StartNew(() => Run(cts.Token));
            _cancellationTokenSource = cts;
        }

        public void WaitForConnection(TimeSpan timeout)
        {
            if (_hasConnection)
                return;

            if (!_connnectionSignal.WaitOne(timeout) || !_hasConnection)
                throw new TimeoutException("No connection received");
        }

        public void EnqueueEvent(Event ev)
        {
            if (_hasConnection)
                _queue.Post(ev);
        }

        async Task Run(CancellationToken ct)
        {
            do
            {
                var ctx = await _server.GetContextAsync();

                Task.Factory.StartNew(() => RunRequest(ctx, ct));

            } while (!ct.IsCancellationRequested);
        }

        async Task RunRequest(HttpListenerContext ctx, CancellationToken ct)
        {
            var requestPath = ctx.Request.Url.PathAndQuery;

            if (requestPath == "/rxspy/stream")
            {
                await RunStream(ctx, ct);
            }
            else
            {
                ctx.Response.StatusCode = 404;
                ctx.Response.StatusDescription = "Yeah I dunno what you're trying to do";
                ctx.Response.ContentType = "text/plain";

                using (var sw = new StreamWriter(ctx.Response.OutputStream))
                {
                    await sw.WriteLineAsync("No clue.");
                }
            }

            ctx.Response.Close();
        }

        async Task RunStream(HttpListenerContext ctx, CancellationToken ct)
        {
            ctx.Response.ContentType = "text/plain";
            ctx.Response.SendChunked = true;

            try
            {
                _hasConnection = true;
                _connnectionSignal.Set();

                using (var sw = new StreamWriter(ctx.Response.OutputStream))
                {
                    sw.AutoFlush = true;

                    while (!ct.IsCancellationRequested)
                    {
                        try
                        {
                            var ev = await _queue.ReceiveAsync();
                            await sw.WriteLineAsync(SimpleJson.SerializeObject(ev, _serializerStrategy));
                        }
                        catch (Exception e)
                        {
                            Debug.WriteLine("Request failed: " + e.Message);
                            return;
                        }
                    }
                }
            }
            finally
            {
                _hasConnection = false;
            }
        }

        // ugh...
        int GetRandomTcpPort()
        {
            var s = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            try
            {
                s.Bind(new IPEndPoint(IPAddress.Loopback, 0));
                return ((IPEndPoint)s.LocalEndPoint).Port;
            }
            finally
            {
                s.Close();
            }
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _cancellationTokenSource.Cancel();
        }
    }
}
