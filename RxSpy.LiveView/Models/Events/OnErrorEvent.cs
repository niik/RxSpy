using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RxSpy.Events;

namespace RxSpy.Models.Events
{
    public class OnErrorEvent: Event, IOnErrorEvent
    {
        public ITypeInfo ErrorType { get; set; }
        public string Message { get; set; }
        public string StackTrace { get; set; }
        public long OperatorId { get; set; }
    }
}
