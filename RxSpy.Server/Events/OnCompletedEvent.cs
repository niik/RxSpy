using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RxSpy.Events
{
    internal class OnCompletedEvent : Event
    {
        public OperatorInfo OperatorId { get; private set; }

        public OnCompletedEvent(OperatorInfo operatorInfo)
            : base(EventType.OnCompleted)
        {
            OperatorId = operatorInfo;
        }
    }
}
