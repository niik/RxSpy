using System;
using System.Reactive.Disposables;
using RxSpy.Events;

namespace RxSpy.Observables
{
    internal class OperatorConnection<T> : IObservable<T>, IOperatorObservable
    {
        readonly OperatorInfo _childInfo;
        private IObservable<T> _parent;
        private OperatorInfo _parentInfo;
        private RxSpySession _session;

        public OperatorInfo OperatorInfo { get { return _childInfo; } }
        protected RxSpySession Session { get { return _session; } }

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
            var subscriptionId = _session.EnqueueEvent(Event.Subscribe(_childInfo, _parentInfo));

            var disp = _parent.Subscribe(observer);

            return Disposable.Create(() =>
            {
                disp.Dispose();
                _session.EnqueueEvent(Event.Unsubscribe(subscriptionId));
            });
        }
    }
}
