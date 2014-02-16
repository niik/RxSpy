using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RxSpy.Events;

namespace RxSpy.Models.Events
{
    public class OnCompletedEvent: Event, IOnCompletedEvent
    {
        public long OperatorId { get; set; }
    }
}
