using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using RxSpy.Events;

namespace RxSpy.StressTest
{
    public class StressTestEventHandler: IRxSpyEventHandler
    {
        IRxSpyEventHandler _inner;
        int eventCount;
        int observableCount;

        public int EventCount { get { return eventCount; } }
        public int ObservableCount { get { return observableCount; } }

        public StressTestEventHandler(IRxSpyEventHandler inner)
        {
            _inner = inner;
        }

        private void Increment()
        {
            Interlocked.Increment(ref eventCount);
        }

        public void OnCreated(IOperatorCreatedEvent onCreatedEvent)
        {
            Interlocked.Increment(ref observableCount);
            Increment();
            _inner.OnCreated(onCreatedEvent);
        }

        public void OnCompleted(IOnCompletedEvent onCompletedEvent)
        {
            Increment();
            _inner.OnCompleted(onCompletedEvent);
        }

        public void OnError(IOnErrorEvent onErrorEvent)
        {
            Increment();
            _inner.OnError(onErrorEvent);
        }

        public void OnNext(IOnNextEvent onNextEvent)
        {
            Increment();
            _inner.OnNext(onNextEvent);
        }

        public void OnSubscribe(ISubscribeEvent subscribeEvent)
        {
            Increment();
            _inner.OnSubscribe(subscribeEvent);
        }

        public void OnUnsubscribe(IUnsubscribeEvent unsubscribeEvent)
        {
            Increment();
            _inner.OnUnsubscribe(unsubscribeEvent);
        }

        public void OnConnected(IConnectedEvent connectedEvent)
        {
            Increment();
            _inner.OnConnected(connectedEvent);
        }

        public void OnDisconnected(IDisconnectedEvent disconnectedEvent)
        {
            Increment();
            _inner.OnDisconnected(disconnectedEvent);
        }

        public void OnTag(ITagOperatorEvent tagEvent)
        {
            Increment();
            _inner.OnTag(tagEvent);
        }

        public void Dispose()
        {
            _inner.Dispose();
        }
    }
}
