using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RxSpy.Events
{
    internal class OnErrorEvent: Event, IOnErrorEvent
    {
        public Type ErrorType { get; private set; }
        public string Message { get; private set; }
        public long OperatorId { get; private set; }

        public OnErrorEvent(OperatorInfo operatorInfo, Exception error)
            : base(EventType.OnError)
        {
            if (error == null)
                return;

            OperatorId = operatorInfo.Id;
            ErrorType = error.GetType();
            Message = error.Message;
        }

    }
}
