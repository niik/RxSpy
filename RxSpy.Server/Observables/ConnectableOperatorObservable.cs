using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using RxSpy.Events;

namespace RxSpy.Observables
{
    internal class ConnectableOperatorObservable<T> : OperatorObservable<T>, IConnectableObservable<T>
    {
        readonly IConnectableObservable<T> _connectableObservable;

        public ConnectableOperatorObservable(RxSpySession session, IConnectableObservable<T> parent, OperatorInfo operatorInfo)
            : base(session, parent, operatorInfo)
        {
            _connectableObservable = parent;
        }

        public IDisposable Connect()
        {
            var connectionId = Session.EnqueueEvent(Event.Connect(OperatorInfo));
            var disp = _connectableObservable.Connect();

            return Disposable.Create(() =>
            {
                disp.Dispose();
                Session.EnqueueEvent(Event.Disconnect(connectionId));
            });
        }
    }
}
