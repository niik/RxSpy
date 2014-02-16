using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RxSpy.Events
{
    internal class OnCompletedEvent : Event, IOnCompletedEvent
    {
        public long OperatorId { get; private set; }

        public OnCompletedEvent(OperatorInfo operatorInfo)
            : base(EventType.OnCompleted)
        {
            OperatorId = operatorInfo.Id;
        }
    }
}
