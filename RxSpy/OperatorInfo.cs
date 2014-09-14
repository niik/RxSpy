using System.Threading;
using RxSpy.Events;

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
        readonly MethodInfo _operatorMethod;

        public string Name { get { return _name; } }
        public long Id { get { return _id; } }
        public CallSite CallSite { get { return _callSite; } }
        public MethodInfo OperatorMethod { get { return _operatorMethod; } }
        public bool IsAnonymous { get { return _anonymous; } }

        internal OperatorInfo(CallSite callSite, MethodInfo operatorMethod)
        {
            _id = Interlocked.Increment(ref idCounter);

            _callSite = callSite;
            _operatorMethod = operatorMethod;

            _name = _operatorMethod.Name;
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
