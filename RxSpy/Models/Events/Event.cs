using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RxSpy.Events;

namespace RxSpy.Models.Events
{
    public class Event: IEvent
    {
        public EventType EventType { get; set; }
        public long EventId { get; set; }
        public long EventTime { get; set; }
    }
}
