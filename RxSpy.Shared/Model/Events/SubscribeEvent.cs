using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RxSpy.Events;

namespace RxSpy.Models.Events
{
    public class SubscribeEvent : Event, ISubscribeEvent
    {
        public long ChildId { get; set; }
        public long ParentId { get; set; }
    }
}
