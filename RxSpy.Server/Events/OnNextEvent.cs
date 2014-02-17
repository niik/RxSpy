using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RxSpy.Utils;

namespace RxSpy.Events
{
    internal class OnNextEvent : Event, IOnNextEvent
    {
        public long OperatorId { get; private set; }
        public string ValueType { get; private set; }
        public string Value { get; private set; }
        public int Thread { get; private set; }

        public OnNextEvent(OperatorInfo operatorInfo, Type valueType, object value, int thread)
            : base(EventType.OnNext)
        {
            OperatorId = operatorInfo.Id;
            ValueType = TypeUtils.ToFriendlyName(valueType);
            Value = GetValueRepresentation(value);
        }

        string GetValueRepresentation(object value)
        {
            return ValueFormatter.ToString(value);
        }

    }
}
