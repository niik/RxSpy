using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Linq;
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

        string _tag;
        public string Tag
        {
            get { return _tag; }
            set { this.RaiseAndSetIfChanged(ref _tag, value); }
        }

        public TimeSpan Created { get; private set; }

        ReactiveList<RxSpyObservableModel> _parents;
        public ReactiveList<RxSpyObservableModel> Parents
        {
            get { return _parents; }
            private set { this.RaiseAndSetIfChanged(ref _parents, value); }
        }

        ReactiveList<RxSpyObservableModel> _children;
        public ReactiveList<RxSpyObservableModel> Children
        {
            get { return _children; }
            private set { this.RaiseAndSetIfChanged(ref _children, value); }
        }

        ReactiveList<RxSpySubscriptionModel> _subscriptions;
        public ReactiveList<RxSpySubscriptionModel> Subscriptions
        {
            get { return _subscriptions; }
            private set { this.RaiseAndSetIfChanged(ref _subscriptions, value); }
        }

        ReactiveList<RxSpyObservedValueModel> _observedValues;
        public ReactiveList<RxSpyObservedValueModel> ObservedValues
        {
            get { return _observedValues; }
            private set { this.RaiseAndSetIfChanged(ref _observedValues, value); }
        }
        RxSpyErrorModel _error;
        public RxSpyErrorModel Error
        {
            get { return _error; }
            set { this.RaiseAndSetIfChanged(ref _error, value); }
        }

        readonly ObservableAsPropertyHelper<bool> _hasError;
        public bool HasError
        {
            get { return _hasError.Value; }
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

        readonly ObservableAsPropertyHelper<int> _descendants;
        public int Descendants
        {
            get { return _descendants.Value; }
        }

        readonly ObservableAsPropertyHelper<int> _ancestors;
        public int Ancestors
        {
            get { return _ancestors.Value; }
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
            Parents = new ReactiveList<RxSpyObservableModel>();
            Children = new ReactiveList<RxSpyObservableModel>();

            ObservedValues = new ReactiveList<RxSpyObservedValueModel>();

            this.WhenAnyValue(x => x.Error, x => x == null ? false : true)
                .ToProperty(this, x => x.HasError, out _hasError);

            this.WhenAnyValue(x => x.Children.Count)
                .Select(_ => Observable.CombineLatest(Children.Select(c => c.WhenAnyValue(x => x.Descendants))))
                .Switch()
                .Select(x => x.Sum() + Children.Count)
                .ToProperty(this, x => x.Descendants, out _descendants);

            this.WhenAnyValue(x => x.Parents.Count)
                .Select(_ => Observable.CombineLatest(Parents.Select(c => c.WhenAnyValue(x => x.Ancestors))))
                .Switch()
                .Select(x => x.Sum() + Parents.Count)
                .ToProperty(this, x => x.Ancestors, out _ancestors);
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

        public void OnTag(ITagOperatorEvent onTagEvent)
        {
            Tag = onTagEvent.Tag;
        }
    }
}
