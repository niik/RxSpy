using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using RxSpy.Events;

namespace RxSpy.Observables
{
    internal class ConnectableOperatorConnection<T> : OperatorConnection<T>, IConnectableObservable<T>
    {
        readonly IConnectableObservable<T> _connectableObservable;

        public ConnectableOperatorConnection(RxSpySession session, IConnectableObservable<T> parent, OperatorInfo childInfo)
            : base(session, parent, childInfo)
        {
            _connectableObservable = parent;
        }

        public IDisposable Connect()
        {
            var connectionId = Session.OnConnected(OperatorInfo);
            var disp = _connectableObservable.Connect();

            return Disposable.Create(() =>
            {
                disp.Dispose();
                Session.OnDisconnected(Event.Disconnect(connectionId));
            });
        }
    }
}
