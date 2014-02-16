using System;
using System.Collections.Generic;
using System.Reactive.Disposables;
using System.Reactive.Subjects;

namespace RxSpy.Observables
{
    internal class MITMObservable<T>: ISubject<T>
    {
        readonly IObservable<T> _source;
        readonly List<IObserver<T>> _observers = new List<IObserver<T>>();
        IDisposable _subscriptionDisposable;

        protected IObservable<T> Source { get { return _source; } }

        public MITMObservable(IObservable<T> source)
        {
            _source = source;
        }

        public virtual void OnCompleted()
        {
            ForEachObserver(o => o.OnCompleted());
        }

        public virtual void OnError(Exception error)
        {
            ForEachObserver(o => o.OnError(error));
        }

        public virtual void OnNext(T value)
        {
            ForEachObserver(o => o.OnNext(value));
        }

        public virtual IDisposable Subscribe(IObserver<T> observer)
        {
            bool subscribe = false;
            lock (_observers)
            {
                subscribe = _observers.Count == 0;
                _observers.Add(observer);
            }

            if (subscribe)
            {
                _subscriptionDisposable = _source.Subscribe(this);
            }

            return Disposable.Create(() =>
            {
                lock (_observers)
                {
                    _observers.Remove(observer);

                    if (_observers.Count == 0)
                    {
                        _subscriptionDisposable.Dispose();
                    }
                }
            });
        }

        protected void ForEachObserver(Action<IObserver<T>> action)
        {
            foreach (var observer in GetObserverListSnapshot())
                action(observer);
        }

        protected IEnumerable<IObserver<T>> GetObserverListSnapshot()
        {
            lock (_observers)
            {
                return _observers.ToArray();
            }
        }
    }
}
