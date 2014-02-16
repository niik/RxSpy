using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using RxSpy.Models;

namespace RxSpy.ViewModels
{
    public class RxSpyObservablesGridViewModel : ReactiveObject
    {
        private RxSpySessionModel _model;

        readonly ObservableAsPropertyHelper<IReactiveDerivedList<RxSpyObservableGridItemViewModel>> _observables;
        public IReactiveDerivedList<RxSpyObservableGridItemViewModel> Observables
        {
            get { return _observables.Value; }
        }


        public RxSpyObservablesGridViewModel(RxSpySessionModel model)
        {
            _model = model;

            this.WhenAnyValue(x => x._model.TrackedObservables)
                .Select(x => x.CreateDerivedCollection(m => new RxSpyObservableGridItemViewModel(m)))
                .Scan((cur, prev) => { using (prev) return cur; })
                .ToProperty(this, x => x.Observables, out _observables);
        }
    }
}
