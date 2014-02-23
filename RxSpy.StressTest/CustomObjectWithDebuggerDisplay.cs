using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RxSpy.StressTest
{
    [DebuggerDisplay("{Name} {Value,nq}")]
    public class CustomObjectWithDebuggerDisplay
    {
        public string Name { get; set; }
        public string Value { get; set; }
    }
}
