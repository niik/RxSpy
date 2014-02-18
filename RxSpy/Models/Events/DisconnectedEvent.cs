using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RxSpy.Events;

namespace RxSpy.Models.Events
{
    public class DisconnectedEvent : Event, IDisconnectedEvent
    {
        public long ConnectionId { get; set; }
    }
}
