using System.Reactive.Linq;
using ReactiveUI;
using RxSpy.Models;

namespace RxSpy.ViewModels
{
    public class RxSpyObservableDetailsViewModel : ReactiveObject
    {
        private RxSpyObservableModel _model;

        readonly ObservableAsPropertyHelper<IReadOnlyReactiveList<RxSpyObservedValueViewModel>> _observedValues;
        public IReadOnlyReactiveList<RxSpyObservedValueViewModel> ObservedValues
        {
            get { return _observedValues.Value; }
        }

        readonly ObservableAsPropertyHelper<bool> _valuesGridIsEnabled;
        public bool ValuesGridIsEnabled
        {
            get { return _valuesGridIsEnabled.Value; }
        }

        readonly ObservableAsPropertyHelper<RxSpyObservablesGridViewModel> _parents;
        public RxSpyObservablesGridViewModel Parents
        {
            get { return _parents.Value; }
        }

        readonly ObservableAsPropertyHelper<RxSpyObservablesGridViewModel> _children;
        public RxSpyObservablesGridViewModel Children
        {
            get { return _children.Value; }
        }

        public RxSpyObservableDetailsViewModel(RxSpyObservableModel model)
        {
            _model = model;

            this.WhenAnyValue(x => x._model.ObservedValues)
                .Select(x => x.CreateDerivedCollection(m => new RxSpyObservedValueViewModel(m)))
                .Scan((prev, cur) => { using (prev) return cur; })
                .ToProperty(this, x => x.ObservedValues, out _observedValues);

            this.WhenAnyValue(x => x._model.Parents, x => new RxSpyObservablesGridViewModel(x))
                .ToProperty(this, x => x.Parents, out _parents);

            this.WhenAnyValue(x => x._model.Children, x => new RxSpyObservablesGridViewModel(x))
                .ToProperty(this, x => x.Children, out _children);
        }
    }
}