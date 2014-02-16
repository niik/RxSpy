using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RxSpy.Events
{
    internal class SubscribeEvent : Event, ISubscribeEvent
    {
        public long ChildId { get; private set; }
        public long ParentId { get; private set; }

        public SubscribeEvent(OperatorInfo child, OperatorInfo parent)
            : base(EventType.Subscribe)
        {
            ChildId = child.Id;
            ParentId = parent.Id;
        }
    }
}
