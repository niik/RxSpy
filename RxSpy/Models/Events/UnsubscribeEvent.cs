using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RxSpy.Events;

namespace RxSpy.Models.Events
{
    public class UnsubscribeEvent: Event, IUnsubscribeEvent
    {
        public long SubscriptionId { get; set; }

        public UnsubscribeEvent()
        {
        }
    }
}
