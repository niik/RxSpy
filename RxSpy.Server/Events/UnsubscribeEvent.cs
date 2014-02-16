using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RxSpy.Events
{
    internal class UnsubscribeEvent : Event
    {
        public long ChildId { get; private set; }
        public long ParentId { get; private set; }

        public UnsubscribeEvent(OperatorInfo child, OperatorInfo parent)
            : base(EventType.Unsubscribe)
        {
            ChildId = child.Id;
            ParentId = parent.Id;
        }
    }
}
