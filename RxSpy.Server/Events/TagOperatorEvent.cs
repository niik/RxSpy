using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RxSpy.Events
{
    internal class TagOperatorEvent : Event, ITagOperatorEvent
    {
        public long OperatorId { get; set; }
        public string Tag { get; set; }

        public TagOperatorEvent(OperatorInfo info, string tag)
            : base(EventType.TagOperator)
        {
            OperatorId = info.Id;
            Tag = tag;
        }
    }
}
