using System;
using System.Collections.Generic;
using System.Reactive.Subjects;

namespace RxSpy.Observables
{
    internal class ConnectableOperatorObservable<T>: OperatorObservable<T>, IConnectableObservable<T>
    {
        readonly List<IObservable<T>> subscribers = new List<IObservable<T>>();
        readonly IConnectableObservable<T> _connectableObservable;

        public ConnectableOperatorObservable(RxSpySession session, IConnectableObservable<T> source, OperatorInfo operatorInfo)
            : base(session, source, operatorInfo)
        {
            _connectableObservable = source;
        }

        public IDisposable Connect()
        {
            return _connectableObservable.Connect();
        }
    }
}
