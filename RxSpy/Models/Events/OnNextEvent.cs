using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RxSpy.Events;

namespace RxSpy.Models.Events
{
    public class OnNextEvent: Event, IOnNextEvent
    {
        public long OperatorId { get; set; }
        public string ValueType { get; set; }
        public string Value { get; set; }
        public int Thread { get; set; }
    }
}
