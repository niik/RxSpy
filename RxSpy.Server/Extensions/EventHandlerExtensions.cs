using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RxSpy.Communication;
using RxSpy.Events;

namespace RxSpy.Events
{
    public static class EventHandlerExtensions
    {
        static long Publish<T>(Action<T> publishAction, T ev) where T: IEvent
        {
            publishAction(ev);
            return ev.EventId;
        }

        public static long OnConnected(this IRxSpyEventHandler This, OperatorInfo operatorInfo)
        {
            return Publish(This.OnConnected, Event.Connect(operatorInfo));
        }

        public static long OnDisconnected(this IRxSpyEventHandler This, long subscriptionId)
        {
            return Publish(This.OnDisconnected, Event.Disconnect(subscriptionId));
        }

        public static long OnSubscribe(this IRxSpyEventHandler This, OperatorInfo child, OperatorInfo parent)
        {
            return Publish(This.OnSubscribe, Event.Subscribe(child, parent));
        }

        public static long OnUnsubscribe(this IRxSpyEventHandler This, long subscriptionId)
        {
            return Publish(This.OnUnsubscribe, Event.Unsubscribe(subscriptionId));
        }
    }
}
