using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RxSpy.Events;

namespace RxSpy.Models.Events
{
    public class ConnectedEvent: Event, IConnectedEvent
    {
        public long OperatorId { get; set; }
    }
}
