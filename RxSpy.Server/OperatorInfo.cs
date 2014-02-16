using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using RxSpy.Events;
using RxSpy.Observables;
using RxSpy.Utils;

namespace RxSpy
{
    public class OperatorInfo
    {
        static long idCounter = 0;

        readonly string _name;
        readonly bool _anonymous;
        readonly long _id;
        readonly string _friendlyName;
        readonly CallSite _callSite;
        readonly CallSite _operatorCallSite;

        public string Name { get { return _name; } }
        public long Id { get { return _id; } }
        public CallSite CallSite { get { return _callSite; } }
        public CallSite OperatorCallSite { get { return _operatorCallSite; } }
        public bool IsAnonymous { get { return _anonymous; } }

        internal OperatorInfo(CallSite callSite, CallSite operatorCallSite)
        {
            _id = Interlocked.Increment(ref idCounter);

            _callSite = callSite;
            _operatorCallSite = operatorCallSite;

            _name = _operatorCallSite.Method.Name;
            _friendlyName = _name + "#" + _id;
            _anonymous = false;
        }

        internal OperatorInfo(string name)
        {
            _id = Interlocked.Increment(ref idCounter);
            _name = name;
            _friendlyName = _name + "#" + _id;
            _anonymous = true;
        }

        public override string ToString()
        {
            return _friendlyName;
        }
    }
}
