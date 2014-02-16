using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RxSpy.Events;

namespace RxSpy.Models
{
    public class RxSpyObservedValue
    {
        public string ValueType { get; set; }
        public string Value { get; set; }
        public TimeSpan Received { get; set; }

        public RxSpyObservedValue(IOnNextEvent onNextEvent)
        {
            ValueType = onNextEvent.ValueType;
            Value = onNextEvent.Value;

            Received = TimeSpan.FromMilliseconds(onNextEvent.EventTime);
        }
    }
}
