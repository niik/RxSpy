using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;
using RxSpy.Events;

namespace RxSpy.Models
{
    public class RxSpyObservableModel: ReactiveObject
    {
        public long Id { get; set; }
        public string Name { get; set; }

        public IMethodInfo OperatorMethod { get; private set; }
        public ICallSite CallSite { get; private set; }

        public TimeSpan Created { get; private set; }

        ReactiveList<RxSpySubscriptionModel> _subscriptions;
        public ReactiveList<RxSpySubscriptionModel> Subscriptions
        {
            get { return _subscriptions; }
            private set { this.RaiseAndSetIfChanged(ref _subscriptions, value); }
        }

        IReactiveDerivedList<RxSpySubscriptionModel> _activeSubscriptions;
        public IReactiveDerivedList<RxSpySubscriptionModel> ActiveSubscriptions
        {
            get { return _activeSubscriptions; }
            private set { this.RaiseAndSetIfChanged(ref _activeSubscriptions, value); }
        }

        ReactiveList<RxSpyObservedValueModel> _observedValues;
        public ReactiveList<RxSpyObservedValueModel> ObservedValues
        {
            get { return _observedValues; }
            private set { this.RaiseAndSetIfChanged(ref _observedValues, value); }
        }

        readonly ObservableAsPropertyHelper<bool> _hasSubscribers;
        public bool HasSubscribers
        {
            get { return _hasSubscribers.Value; }
        }

        RxSpyErrorModel _error;
        public RxSpyErrorModel Error
        {
            get { return _error; }
            set { this.RaiseAndSetIfChanged(ref _error, value); }
        }

        bool _isActive;
        public bool IsActive
        {
            get { return _isActive; }
            set { this.RaiseAndSetIfChanged(ref _isActive, value); }
        }

        long _valuesProduced;
        public long ValuesProduced
        {
            get { return _valuesProduced; }
            set { this.RaiseAndSetIfChanged(ref _valuesProduced, value); }
        }

        public RxSpyObservableModel(IOperatorCreatedEvent createdEvent)
        {
            Id = createdEvent.Id;
            Name = createdEvent.Name;
            OperatorMethod = createdEvent.OperatorMethod;
            CallSite = createdEvent.CallSite;
            IsActive = true;

            Created = TimeSpan.FromMilliseconds(createdEvent.EventTime);

            Subscriptions = new ReactiveList<RxSpySubscriptionModel>();
            ActiveSubscriptions = Subscriptions.CreateDerivedCollection(x => x, filter: x => x.IsActive);
            ObservedValues = new ReactiveList<RxSpyObservedValueModel>();

            this.WhenAnyValue(x => x.Subscriptions.Count, x => x > 0)
                .ToProperty(this, x => x.HasSubscribers, out _hasSubscribers);
        }

        public void OnNext(IOnNextEvent onNextEvent)
        {
            ObservedValues.Add(new RxSpyObservedValueModel(onNextEvent));
            ValuesProduced++;
        }

        public void OnCompleted(IOnCompletedEvent onCompletedEvent)
        {
            IsActive = false;
        }

        public void OnError(IOnErrorEvent onErrorEvent)
        {
            Error = new RxSpyErrorModel(onErrorEvent);
            IsActive = false;
        }
    }
}
