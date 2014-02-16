using System;
using System.Collections.Concurrent;
using System.Diagnostics;
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

        public RxSpySessionModel()
        {
            TrackedObservables = new ReactiveList<RxSpyObservableModel>();
        }

        internal void OnEvent(IEvent ev)
        {
            switch (ev.EventType)
            {
                case EventType.OperatorCreated:
                    OnOperatorCreated((IOperatorCreatedEvent)ev);
                    break;

                case EventType.Subscribe:
                    OnSubscribe((ISubscribeEvent)ev);
                    break;

                case EventType.Unsubscribe:
                    OnUnsubscribe((IUnsubscribeEvent)ev);
                    break;

                case EventType.OnCompleted:
                    OnCompleted((IOnCompletedEvent)ev);
                    break;

                case EventType.OnNext:
                    OnNext((IOnNextEvent)ev);
                    break;

                case EventType.OnError:
                    OnError((IOnErrorEvent)ev);
                    break;

                case EventType.TagOperator:
                    OnTag((ITagOperatorEvent)ev);
                    break;
            }
        }

        void OnOperatorCreated(IOperatorCreatedEvent operatorCreatedEvent)
        {
            var operatorModel = new RxSpyObservableModel(operatorCreatedEvent);

            observableRepository.TryAdd(operatorCreatedEvent.Id, operatorModel);
            TrackedObservables.Add(operatorModel);
        }

        void OnSubscribe(ISubscribeEvent subscribeEvent)
        {
            RxSpyObservableModel child, parent;

            observableRepository.TryGetValue(subscribeEvent.ChildId, out child);
            observableRepository.TryGetValue(subscribeEvent.ParentId, out parent);

            var subscriptionModel = new RxSpySubscriptionModel(subscribeEvent, child, parent)
            {
                IsActive = true
            };

            subscriptionRepository.TryAdd(subscribeEvent.EventId, subscriptionModel);
            
            parent.Subscriptions.Add(subscriptionModel);
            
            parent.Children.Add(child);
            parent.Descendants++;

            child.Parents.Add(parent);
            child.Ancestors += 1 + parent.Ancestors;

            AddDescendants(parent, child.Descendants);
        }

        void AddDescendants(RxSpyObservableModel current, int count)
        {
            current.Descendants += count;

            foreach (var parent in current.Parents)
                AddDescendants(parent, count);
        }

        void OnUnsubscribe(IUnsubscribeEvent unsubscribeEvent)
        {
            RxSpySubscriptionModel subscriptionModel;

            subscriptionRepository.TryGetValue(unsubscribeEvent.SubscriptionId, out subscriptionModel);

            if (subscriptionModel != null)
            {
                subscriptionModel.IsActive = false;
            }
        }

        private void OnError(IOnErrorEvent onErrorEvent)
        {
            RxSpyObservableModel operatorModel;
            observableRepository.TryGetValue(onErrorEvent.OperatorId, out operatorModel);

            operatorModel.OnError(onErrorEvent);
        }

        private void OnNext(IOnNextEvent onNextEvent)
        {
            RxSpyObservableModel operatorModel;
            observableRepository.TryGetValue(onNextEvent.OperatorId, out operatorModel);

            operatorModel.OnNext(onNextEvent);
        }

        void OnCompleted(IOnCompletedEvent onCompletedEvent)
        {
            RxSpyObservableModel operatorModel;
            observableRepository.TryGetValue(onCompletedEvent.OperatorId, out operatorModel);

            if (operatorModel != null)
            {
                operatorModel.OnCompleted(onCompletedEvent);
            }
        }

        private void OnTag(ITagOperatorEvent tagOperatorEvent)
        {
            RxSpyObservableModel operatorModel;
            observableRepository.TryGetValue(tagOperatorEvent.OperatorId, out operatorModel);

            if (operatorModel != null)
            {
                operatorModel.OnTag(tagOperatorEvent);
            }
        }
    }
}
