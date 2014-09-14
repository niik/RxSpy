using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RxSpy.Events
{
    internal class UnsubscribeEvent : Event, IUnsubscribeEvent
    {
        public long SubscriptionId { get; private set; }

        public UnsubscribeEvent(long subscriptionId)
            : base(EventType.Unsubscribe)
        {
            SubscriptionId = subscriptionId;
        }
    }
}
