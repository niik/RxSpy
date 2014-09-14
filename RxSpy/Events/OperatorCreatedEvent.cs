using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RxSpy.Events
{
    internal class OperatorCreatedEvent : Event, IOperatorCreatedEvent
    {
        readonly OperatorInfo _operatorInfo;

        public long Id { get { return _operatorInfo.Id; } }
        public string Name { get { return _operatorInfo.Name; } }
        public ICallSite CallSite { get { return _operatorInfo.CallSite; } }
        public IMethodInfo OperatorMethod { get { return _operatorInfo.OperatorMethod; } }

        public OperatorCreatedEvent(OperatorInfo operatorInfo)
            : base(EventType.OperatorCreated)
        {
            _operatorInfo = operatorInfo;
        }
    }
}
