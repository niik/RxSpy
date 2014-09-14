using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RxSpy.Events
{
    internal class ConnectedEvent : Event, IConnectedEvent
    {
        public long OperatorId { get; set; }

        public ConnectedEvent(OperatorInfo operatorInfo)
            : base(EventType.Connected)
        {
            OperatorId = operatorInfo.Id;
        }
    }
}
