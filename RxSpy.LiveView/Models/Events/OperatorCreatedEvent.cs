using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RxSpy.Events;

namespace RxSpy.Models.Events
{
    public class OperatorCreatedEvent : Event, IOperatorCreatedEvent
    {
        public long Id { get; set; }
        public string Name { get; set; }
        public ICallSite CallSite { get; set; }
        public IMethodInfo OperatorMethod { get; set; }
    }
}
