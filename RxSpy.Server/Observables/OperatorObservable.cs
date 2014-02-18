using System;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using RxSpy.Events;
using RxSpy.Utils;

namespace RxSpy.Observables
{
    internal class OperatorObservable<T> : IObservable<T>, IOperatorObservable
    {
        readonly OperatorInfo _operatorInfo;
        readonly RxSpySession _session;
        private IObservable<T> _source;

        protected RxSpySession Session { get { return _session; } }
        public OperatorInfo OperatorInfo { get { return _operatorInfo; } }

        public OperatorObservable(RxSpySession session, IObservable<T> source, OperatorInfo operatorInfo)
        {
            if (source == null)
                throw new ArgumentNullException("source");

            _source = source;
            _session = session;
            _operatorInfo = operatorInfo;

            _session.EnqueueEvent(Event.OperatorCreated(operatorInfo));
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            var oobs = observer as IOperatorObservable;

            if (oobs != null)
                return _source.Subscribe(observer);

            _session.EnqueueEvent(Event.Subscribe(_session.GetOperatorInfoFor(observer), _operatorInfo));
            return _source.Subscribe(new OperatorObserver<T>(_session, observer, _operatorInfo));
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
