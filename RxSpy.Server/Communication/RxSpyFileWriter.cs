using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Threading.Tasks.Dataflow;
using RxSpy.Communication.Serialization;
using RxSpy.Events;

namespace RxSpy.Communication
{
    internal class RxSpyFileWriter : IRxSpyServer
    {
        string _path;
        RxSpyJsonSerializerStrategy _serializerStrategy;
        BufferBlock<Event> _queue = new BufferBlock<Event>();
        CancellationTokenSource _cancellationTokenSource;

        public Uri Address
        {
            get { return new Uri(_path); }
        }

        public RxSpyFileWriter(string path)
        {
            _path = path;
            _serializerStrategy = new RxSpyJsonSerializerStrategy();
        }

        public void WaitForConnection(TimeSpan timeout)
        {
            _cancellationTokenSource = new CancellationTokenSource();

            Task.Factory.StartNew(() => RunQueue(_cancellationTokenSource.Token));
        }

        async Task RunQueue(CancellationToken ct)
        {
            using (var sw = new StreamWriter(File.OpenWrite(_path)))
            {
                while (!ct.IsCancellationRequested)
                {
                    try
                    {
                        var ev = await _queue.ReceiveAsync(ct);

                        if (!ct.IsCancellationRequested)
                            sw.WriteLine(SimpleJson.SerializeObject(ev, _serializerStrategy));
                    }
                    catch (Exception e)
                    {
                        Debug.WriteLine("Write failed: " + e.Message);
                        return;
                    }
                }
            }
        }

        public void EnqueueEvent(Event ev)
        {
            _queue.Post(ev);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _cancellationTokenSource.Cancel();
        }
    }
}
