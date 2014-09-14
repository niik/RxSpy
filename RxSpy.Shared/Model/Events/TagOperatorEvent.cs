using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RxSpy.Events;

namespace RxSpy.Models.Events
{
    public class TagOperatorEvent: Event, ITagOperatorEvent
    {
        public long OperatorId { get; set; }
        public string Tag { get; set; }
    }
}
