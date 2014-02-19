using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RxSpy.Events;

namespace RxSpy
{
    public interface IRxSpyEventHandler: IDisposable
    {
        void OnCreated(IOperatorCreatedEvent onCreatedEvent);
        void OnCompleted(IOnCompletedEvent onCompletedEvent);
        void OnError(IOnErrorEvent onErrorEvent);
        void OnNext(IOnNextEvent onNextEvent);
        void OnSubscribe(ISubscribeEvent subscribeEvent);
        void OnUnsubscribe(IUnsubscribeEvent unsubscribeEvent);
        void OnConnected(IConnectedEvent connectedEvent);
        void OnDisconnected(IDisconnectedEvent disconnectedEvent);
        void OnTag(ITagOperatorEvent tagEvent);
    }
}
