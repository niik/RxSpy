using System;
using System.Reactive.Disposables;
using System.Reactive.Subjects;
using System.Threading;
using RxSpy.Events;
using RxSpy.Utils;

namespace RxSpy.Observables
{
    internal class OperatorObservable<T> : IObservable<T>, IOperatorObservable
    {
        readonly OperatorInfo _operatorInfo;
        readonly RxSpySession _session;
        readonly IObservable<T> _inner;

        protected RxSpySession Session { get { return _session; } }
        public OperatorInfo OperatorInfo { get { return _operatorInfo; } }

        // We can't subscribe an extra observer to out inner observable just for the event stuff
        // since that could potentially modify the behavior of the inner (like if it was designed 
        // for one subscription only. So instead of doing that we wrap all incoming observers in
        // a private class which forwards all signals while keeping track of whether or not it's
        // the wrapper currently responsible for reporting events.
        Observer _currentlyReportingObserver;

        sealed class Observer: IObserver<T>, IDisposable
        {
            readonly OperatorObservable<T> _parent;
            readonly IObserver<T> _inner;

            bool _isReporting = false;

            public Observer(OperatorObservable<T> parent, IObserver<T> inner)
            {
                _parent = parent;
                _inner = inner;
            }

            bool isReporting()
            {
                if (!_isReporting && Interlocked.CompareExchange(ref _parent._currentlyReportingObserver, this, null) == null)
                    _isReporting = true;

                return _isReporting;
            }

            public void OnCompleted()
            {
                if (isReporting())
                    _parent._session.OnCompleted(Event.OnCompleted(_parent._operatorInfo));

                _inner.OnCompleted();
            }

            public void OnError(Exception error)
            {
                if (isReporting())
                    _parent._session.OnError(Event.OnError(_parent._operatorInfo, error));
                
                _inner.OnError(error);
            }

            public void OnNext(T value)
            {
                if (isReporting())
                    _parent._session.OnNext(Event.OnNext(_parent._operatorInfo, typeof(T), value));

                _inner.OnNext(value);
            }

            public void Dispose() 
            {
                if (_isReporting)
                {
                    Interlocked.CompareExchange(ref _parent._currentlyReportingObserver, null, this);
                    _isReporting = false;
                }
            }
        }

        public OperatorObservable(RxSpySession session, IObservable<T> inner, OperatorInfo operatorInfo)
        {
            if (inner == null)
                throw new ArgumentNullException("source");

            _inner = inner;
            _session = session;
            _operatorInfo = operatorInfo;

            _session.OnCreated(Event.OperatorCreated(operatorInfo));
        }

        public IDisposable Subscribe(IObserver<T> observer)
        {
            var obs = new Observer(this, observer);
            var disp = _inner.Subscribe(obs);

            return new CompositeDisposable(disp, obs);
        }

        public override string ToString()
        {
            return _operatorInfo.ToString();
        }

        internal void Tag(string tag)
        {
            _session.OnTag(Event.Tag(_operatorInfo, tag));
        }
    }
}
