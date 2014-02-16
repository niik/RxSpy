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
        public long Id { get; private set; }
        public string Name { get; private set; }

        public IMethodInfo OperatorMethod { get; private set; }
        public ICallSite CallSite { get; private set; }

        public TimeSpan Created { get; private set; }

        ReactiveList<RxSpySubscriptionModel> _subscriptions;
        public ReactiveList<RxSpySubscriptionModel> Subscriptions
        {
            get { return _subscriptions; }
            private set { this.RaiseAndSetIfChanged(ref _subscriptions, value); }
        }

        long _totalSubscriptionCount;
        public long TotalSubscriptionCount
        {
            get { return _totalSubscriptionCount; }
            set { this.RaiseAndSetIfChanged(ref _totalSubscriptionCount, value); }
        }

        IReactiveDerivedList<RxSpySubscriptionModel> _activeSubscriptions;
        public IReactiveDerivedList<RxSpySubscriptionModel> ActiveSubscriptions
        {
            get { return _activeSubscriptions; }
            private set { this.RaiseAndSetIfChanged(ref _activeSubscriptions, value); }
        }

        readonly ObservableAsPropertyHelper<bool> _isActive;
        public bool IsActive
        {
            get { return _isActive.Value; }
        }

        public RxSpyObservableModel(IOperatorCreatedEvent createdEvent)
        {
            Id = createdEvent.Id;
            Name = createdEvent.Name;
            OperatorMethod = createdEvent.OperatorMethod;
            CallSite = createdEvent.CallSite;

            Created = TimeSpan.FromMilliseconds(createdEvent.EventTime);

            Subscriptions = new ReactiveList<RxSpySubscriptionModel>();
            ActiveSubscriptions = Subscriptions.CreateDerivedCollection(x => x, filter: x => x.IsActive);

            this.WhenAnyValue(x => x.Subscriptions.Count, x => x > 0)
                .ToProperty(this, x => x.IsActive, out _isActive);
        }
    }
}
