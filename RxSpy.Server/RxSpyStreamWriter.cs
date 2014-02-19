using System;
using System.Collections.Concurrent;
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

namespace RxSpy
{
    public sealed class RxSpyStreamWriter : IRxSpyEventHandler
    {
        string _path;
        Stream _stream;
        RxSpyJsonSerializerStrategy _serializerStrategy;
        ConcurrentQueue<IEvent> _queue = new ConcurrentQueue<IEvent>();
        CancellationTokenSource _cancellationTokenSource;

        public RxSpyStreamWriter(string path)
        {
            _path = path;
            _serializerStrategy = new RxSpyJsonSerializerStrategy();
        }

        public RxSpyStreamWriter(Stream stream)
        {
            _stream = stream;
            _serializerStrategy = new RxSpyJsonSerializerStrategy();
        }

        public void WaitForConnection(TimeSpan timeout)
        {
            _cancellationTokenSource = new CancellationTokenSource();

            Task.Factory.StartNew(() => RunQueue(_cancellationTokenSource.Token), TaskCreationOptions.LongRunning);
        }

        async Task RunQueue(CancellationToken ct)
        {
            using (var sw = GetStreamWriter())
            {
                IEvent ev;

                while (!ct.IsCancellationRequested)
                {
                    while (!ct.IsCancellationRequested && _queue.TryDequeue(out ev))
                    {
                        _queue.TryDequeue(out ev);
                        sw.Write('.');
                    }

                    await Task.Delay(200, ct);
                }
            }
        }

        TextWriter GetStreamWriter()
        {
            if (_path != null)
                return new StreamWriter(_path, append: false, encoding: Encoding.UTF8);
            
            return new StreamWriter(_stream, Encoding.UTF8, 1024, leaveOpen: true);
        }

        void EnqueueEvent(IEvent ev)
        {
            _queue.Enqueue(ev);
        }

        public void Dispose()
        {
            GC.SuppressFinalize(this);
            _cancellationTokenSource.Cancel();
        }

        public void OnCreated(IOperatorCreatedEvent onCreatedEvent)
        {
            EnqueueEvent(onCreatedEvent);
        }

        public void OnCompleted(IOnCompletedEvent onCompletedEvent)
        {
            EnqueueEvent(onCompletedEvent);
        }

        public void OnError(IOnErrorEvent onErrorEvent)
        {
            EnqueueEvent(onErrorEvent);
        }

        public void OnNext(IOnNextEvent onNextEvent)
        {
            EnqueueEvent(onNextEvent);
        }

        public void OnSubscribe(ISubscribeEvent subscribeEvent)
        {
            EnqueueEvent(subscribeEvent);
        }

        public void OnUnsubscribe(IUnsubscribeEvent unsubscribeEvent)
        {
            EnqueueEvent(unsubscribeEvent);
        }

        public void OnConnected(IConnectedEvent connectedEvent)
        {
            EnqueueEvent(connectedEvent);
        }

        public void OnDisconnected(IDisconnectedEvent disconnectedEvent)
        {
            EnqueueEvent(disconnectedEvent);
        }

        public void OnTag(ITagOperatorEvent tagEvent)
        {
            EnqueueEvent(tagEvent);
        }
    }
}
