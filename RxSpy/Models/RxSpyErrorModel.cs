using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RxSpy.Events;

namespace RxSpy.Models
{
    public class RxSpyErrorModel
    {
        public ITypeInfo ErrorType { get; set; }
        public string Message { get; set; }
        public TimeSpan Received { get; set; }
        public string StackTrace { get; set; }
        public long OperatorId { get; set; }

        public RxSpyErrorModel(IOnErrorEvent onErrorEvent)
        {
            OperatorId = onErrorEvent.OperatorId;
            Received = TimeSpan.FromMilliseconds(onErrorEvent.EventTime);
            ErrorType = onErrorEvent.ErrorType;
            Message = onErrorEvent.Message;
            StackTrace = onErrorEvent.StackTrace;
        }

    }
}
