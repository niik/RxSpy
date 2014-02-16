using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace RxSpy.Events
{
    public class CallSite : ICallSite
    {
        public int Line { get; private set; }
        public string File { get; private set; }
        public int ILOffset { get; private set; }
        public IMethodInfo Method { get; private set; }

        public CallSite(StackFrame frame)
        {
            Line = frame.GetFileLineNumber();
            File = frame.GetFileName();
            ILOffset = frame.GetILOffset();

            var method = frame.GetMethod();

            if (method != null)
                Method = new MethodInfo(method);
        }

        public CallSite(MethodBase method)
        {
            Method = new MethodInfo(method);
        }

    }
}
