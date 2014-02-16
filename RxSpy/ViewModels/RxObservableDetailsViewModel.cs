using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;
using RxSpy.Models;
using System.Reactive.Linq;

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

        public RxSpyObservableDetailsViewModel(RxSpyObservableModel model)
        {
            _model = model;

            this.WhenAnyValue(x => x._model.ObservedValues)
                .Select(x => x.CreateDerivedCollection(m => new RxSpyObservedValueViewModel(m)))
                .Scan((cur, prev) => { using (prev) return cur; })
                .ToProperty(this, x => x.ObservedValues, out _observedValues);
        }
    }
}