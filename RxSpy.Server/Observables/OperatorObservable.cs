using System;
using System.Reactive.Disposables;
using RxSpy.Events;
using RxSpy.Utils;

namespace RxSpy.Observables
{
    internal class OperatorObservable<T> : MITMObservable<T>, IOperatorObservable
    {
        readonly OperatorInfo _operatorInfo;
        readonly RxSpySession _session;

        public OperatorInfo OperatorInfo { get { return _operatorInfo; } }

        public OperatorObservable(RxSpySession session, IObservable<T> source, OperatorInfo operatorInfo)
            : base(source)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            // This should never happen
            if (source is OperatorObservable<T>)
                throw new ArgumentException("Cannot wrap operator observable in another operator observable");

            _session = session;
            _operatorInfo = operatorInfo;

            _session.EnqueueEvent(Event.OperatorCreated(operatorInfo));
        }

        public override void OnNext(T value)
        {
            _session.EnqueueEvent(Event.OnNext(_operatorInfo, typeof(T), value));
            base.OnNext(value);
        }

        public override void OnError(Exception error)
        {
            _session.EnqueueEvent(Event.OnError(_operatorInfo, error));
            base.OnError(error);
        }

        public override void OnCompleted()
        {
            _session.EnqueueEvent(Event.OnCompleted(_operatorInfo));
            base.OnCompleted();
        }

        public override IDisposable Subscribe(IObserver<T> observer)
        {
            var subscriberOperatorInfo = _session.GetOperatorInfoFor(observer);

            var ev = Event.Subscribe(subscriberOperatorInfo, OperatorInfo);
            _session.EnqueueEvent(ev);

            var disp = base.Subscribe(observer);

            return Disposable.Create(() =>
            {
                disp.Dispose();
                _session.EnqueueEvent(Event.Unsubscribe(ev.EventId));
            });
        }

        public override string ToString()
        {
            return _operatorInfo.ToString();
        }

        internal void Tag(string tag)
        {
            _session.EnqueueEvent(Event.Tag(_operatorInfo, tag));
        }
    }
}
