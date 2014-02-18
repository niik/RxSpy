using System;
using System.Collections.Generic;
using System.Reactive.Subjects;
using RxSpy.Events;

namespace RxSpy.Observables
{
    internal class ConnectableOperatorConnection<T> : OperatorConnection<T>, IOperatorObservable, IConnectableObservable<T>
    {
        readonly IConnectableObservable<T> _connectableObservable;

        public ConnectableOperatorConnection(RxSpySession session, IConnectableObservable<T> parent, OperatorInfo childInfo)
            : base(session, parent, childInfo)
        {
        }

        public IDisposable Connect()
        {
            return _connectableObservable.Connect();
        }
    }
}
