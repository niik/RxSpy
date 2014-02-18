using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using RxSpy.Events;

namespace RxSpy.Observables
{
    public class OperatorObserver<T>: IObserver<T>, IOperatorObservable
    {
        readonly IObserver<T> _inner;
        readonly OperatorInfo _operatorInfo;
        private RxSpySession _session;

        public OperatorInfo OperatorInfo
        {
            get { return _operatorInfo; }
        }

        public OperatorObserver(RxSpySession session, IObserver<T> inner, OperatorInfo operatorInfo)
        {
            _session = session;
            _inner = inner;
            _operatorInfo = operatorInfo;
        }

        public void OnCompleted()
        {
            _session.EnqueueEvent(Event.OnCompleted(_operatorInfo));
            _inner.OnCompleted();
        }

        public void OnError(Exception error)
        {
            _session.EnqueueEvent(Event.OnError(_operatorInfo, error));
            _inner.OnError(error);
        }

        public void OnNext(T value)
        {
            _session.EnqueueEvent(Event.OnNext(_operatorInfo, typeof(T), value));
            _inner.OnNext(value);
        }

    }
}
