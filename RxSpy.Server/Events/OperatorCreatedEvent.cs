using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RxSpy.Events
{
    internal class OperatorCreatedEvent : Event
    {
        readonly OperatorInfo _operatorInfo;

        public long Id { get { return _operatorInfo.Id; } }
        public string Name { get { return _operatorInfo.Name; } }
        public CallSite EntryPoint { get { return _operatorInfo.CallSite; } }

        public OperatorCreatedEvent(OperatorInfo operatorInfo)
            : base(EventType.OperatorCreated)
        {
            _operatorInfo = operatorInfo;
        }
    }
}
