using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RxSpy.Events;

namespace RxSpy.Models.Events
{
    public class CallSite : ICallSite
    {
        public string File { get; set; }
        public int ILOffset { get; set; }
        public int Line { get; set; }
        public IMethodInfo Method { get; set; }
    }
}
