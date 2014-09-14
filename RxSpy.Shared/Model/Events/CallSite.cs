using System;
using System.Collections.Generic;
using System.IO;
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

        public override string ToString()
        {
            if (Method.Name == null)
                return "";

            string typeAndMethod = Method.DeclaringType + "." + Method.Signature;

            if (File != null && Line != -1)
                return typeAndMethod + " in " + Path.GetFileName(File) + ":" + Line;

            return typeAndMethod;
        }
    }
}
