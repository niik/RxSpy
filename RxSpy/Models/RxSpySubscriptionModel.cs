using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;
using RxSpy.Events;

namespace RxSpy.Models
{
    public class RxSpySubscriptionModel: ReactiveObject
    {
        public long SubscriptionId { get; set; }

        RxSpyObservableModel _parent;
        public RxSpyObservableModel Parent
        {
            get { return _parent; }
            set { this.RaiseAndSetIfChanged(ref _parent, value); }
        }

        RxSpyObservableModel _child;
        public RxSpyObservableModel Child
        {
            get { return _child; }
            set { this.RaiseAndSetIfChanged(ref _child, value); }
        }

        bool _isActive;
        public bool IsActive
        {
            get { return _isActive; }
            set { this.RaiseAndSetIfChanged(ref _isActive, value); }
        }

        public RxSpySubscriptionModel(long subscriptionId, RxSpyObservableModel child, RxSpyObservableModel parent)
        {
            SubscriptionId = subscriptionId;
            Parent = parent;
            Child = child;
            IsActive = true;
        }
    }
}
