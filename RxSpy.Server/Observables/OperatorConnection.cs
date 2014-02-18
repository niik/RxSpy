using System;
using RxSpy.Events;

namespace RxSpy.Observables
{
    internal class OperatorConnection<T> : IObservable<T>, IOperatorObservable
    {
        readonly OperatorInfo _childInfo;
        private IObservable<T> _parent;
        private OperatorInfo _parentInfo;
        private RxSpySession _session;

        public OperatorInfo OperatorInfo
        {
            get { return _childInfo; }
        }

        class Observer : IObserver<T>, IOperatorObservable
        {
            readonly OperatorConnection<T> _connection;
            readonly IObserver<T> _inner;

            public OperatorInfo OperatorInfo { get { return _connection._childInfo; } }

            public Observer(OperatorConnection<T> connection, IObserver<T> inner)
            {
                _connection = connection;
                _inner = inner;
            }

            public void OnCompleted()
            {
                _connection._session.EnqueueEvent(Event.OnCompleted(_connection._parentInfo));
                _inner.OnCompleted();
            }

            public void OnError(Exception error)
            {
                _connection._session.EnqueueEvent(Event.OnError(_connection._parentInfo, error));
                _inner.OnError(error);
            }

            public void OnNext(T value)
            {
                _connection._session.EnqueueEvent(Event.OnNext(_connection._parentInfo, typeof(T), value));
                _inner.OnNext(value);
            }
        }

        public OperatorConnection(RxSpySession session, IObservable<T> parent, OperatorInfo childInfo)
        {
            _session = session;
            _parent = parent;
            _parentInfo = session.GetOperatorInfoFor(parent);
            _childInfo = childInfo;
        }

        public override string ToString()
        {
            return _childInfo.ToString() + "::Connection";
        }

        public virtual IDisposable Subscribe(IObserver<T> observer)
        {
            _session.EnqueueEvent(Event.Subscribe(_childInfo, _parentInfo));
            return _parent.Subscribe(new Observer(this, observer));
        }
    }
}
