using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RxSpy.Events;

namespace RxSpy.Models
{
    public class RxSpyError
    {
        public ITypeInfo ErrorType { get; set; }
        public string Message { get; set; }
        public TimeSpan Received { get; set; }

        public RxSpyError(IOnErrorEvent onErrorEvent)
        {
            Received = TimeSpan.FromMilliseconds(onErrorEvent.EventTime);
            ErrorType = onErrorEvent.ErrorType;
            Message = onErrorEvent.Message;
        }
    }
}
