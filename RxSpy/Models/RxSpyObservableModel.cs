using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;

namespace RxSpy.Models
{
    public class RxSpyObservableModel: ReactiveObject
    {
        ReactiveList<RxSpySubscription> _subscriptions;
        public ReactiveList<RxSpySubscription> Subscriptions
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

        IReactiveDerivedList<RxSpySubscription> _activeSubscriptions;
        public IReactiveDerivedList<RxSpySubscription> ActiveSubscriptions
        {
            get { return _activeSubscriptions; }
            private set { this.RaiseAndSetIfChanged(ref _activeSubscriptions, value); }
        }

        public RxSpyObservableModel()
        {
            Subscriptions = new ReactiveList<RxSpySubscription>();
            ActiveSubscriptions = Subscriptions.CreateDerivedCollection(x => x, filter: x => x.IsActive);
        }
    }
}
