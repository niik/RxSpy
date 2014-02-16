using System;
using System.Collections.Concurrent;
using ReactiveUI;
using RxSpy.Events;

namespace RxSpy.Models
{
    public class RxSpySessionModel : ReactiveObject
    {
        readonly ConcurrentDictionary<long, RxSpyObservableModel> observableRepository
            = new ConcurrentDictionary<long, RxSpyObservableModel>();

        readonly ConcurrentDictionary<long, RxSpySubscriptionModel> subscriptionRepository
            = new ConcurrentDictionary<long, RxSpySubscriptionModel>();

        public ReactiveList<RxSpyObservableModel> TrackedObservables { get; set; }
        public IReadOnlyReactiveList<RxSpyObservableModel> ActiveTrackedObservables { get; set; }

        public RxSpySessionModel()
        {
            TrackedObservables = new ReactiveList<RxSpyObservableModel>();
            ActiveTrackedObservables = TrackedObservables.CreateDerivedCollection(x => x, x => x.IsActive);
        }

        internal void OnEvent(IEvent ev)
        {
            switch (ev.EventType)
            {
                case EventType.OperatorCreated:
                    OnOperatorCreated((IOperatorCreatedEvent)ev);
                    break;

                case EventType.Subscribe:
                    OnSubscription((ISubscribeEvent)ev);
                    break;

                case EventType.Unsubscribe:
                    OnUnsubscription((IUnsubscribeEvent)ev);
                    break;
            }
        }

        void OnOperatorCreated(IOperatorCreatedEvent operatorCreatedEvent)
        {
            var operatorModel = new RxSpyObservableModel(operatorCreatedEvent);

            observableRepository.TryAdd(operatorCreatedEvent.Id, operatorModel);
            TrackedObservables.Add(operatorModel);
        }

        private void OnSubscription(ISubscribeEvent subscribeEvent)
        {
            RxSpyObservableModel child, parent;

            observableRepository.TryGetValue(subscribeEvent.ChildId, out child);
            observableRepository.TryGetValue(subscribeEvent.ParentId, out parent);

            var subscriptionModel = new RxSpySubscriptionModel(subscribeEvent.EventId, child, parent) {
                IsActive = true
            };

            subscriptionRepository.TryAdd(subscribeEvent.EventId, subscriptionModel);
            parent.Subscriptions.Add(subscriptionModel);
        }

        private void OnUnsubscription(IUnsubscribeEvent unsubscribeEvent)
        {
            RxSpySubscriptionModel subscriptionModel;

            subscriptionRepository.TryGetValue(unsubscribeEvent.SubscriptionId, out subscriptionModel);

            if (subscriptionModel != null)
            {
                subscriptionModel.IsActive = false;
            }
        }
    }
}
