using System;

namespace RxSpy.Observables
{
    internal class OperatorConnection<T>: MITMObservable<T>, IOperatorObservable
    {
        readonly OperatorInfo _operatorInfo;

        public OperatorInfo OperatorInfo
        {
            get { return _operatorInfo; }
        }

        public OperatorConnection(IObservable<T> source, OperatorInfo operatorInfo): base(source)
        {
            _operatorInfo = operatorInfo;
        }

        public override string ToString()
        {
            return _operatorInfo.ToString() + "::Connection";
        }
    }
}
