using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RxSpy.Events;

namespace RxSpy.Models
{
    public class RxSpyObservedValueModel
    {
        public string ValueType { get; set; }
        public string Value { get; set; }
        public TimeSpan Received { get; set; }
        public int Thread { get; set; }

        public RxSpyObservedValueModel(IOnNextEvent onNextEvent)
        {
            ValueType = onNextEvent.ValueType;
            Value = onNextEvent.Value;
            Thread = onNextEvent.Thread;

            Received = TimeSpan.FromMilliseconds(onNextEvent.EventTime);
        }

    }
}
