using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RxSpy.Events
{
    public enum EventType
    {
        OperatorCreated,
        OperatorCollected,
        Subscribe,
        Unsubscribe,
     
        OnNext,
        OnError,
        OnCompleted,
    }
}
