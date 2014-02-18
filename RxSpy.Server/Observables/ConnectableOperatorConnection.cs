using System;
using System.Reactive.Subjects;

namespace RxSpy.Observables
{
    internal class ConnectableOperatorConnection<T> : OperatorConnection<T>, IOperatorObservable, IConnectableObservable<T>
    {
        readonly IConnectableObservable<T> _connectableObservable;

        public ConnectableOperatorConnection(IConnectableObservable<T> source, OperatorInfo operatorInfo)
            : base(source, operatorInfo)
        {
            _connectableObservable = source;
        }

        public IDisposable Connect()
        {
            return _connectableObservable.Connect();
        }
    }
}
