using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RxSpy.Events
{
    internal class DisconnectedEvent : Event, IDisconnectedEvent
    {
        public long ConnectionId { get; set; }

        public DisconnectedEvent(long connectionId)
            : base(EventType.Disconnected)
        {
            ConnectionId = connectionId;
        }
    }
}
