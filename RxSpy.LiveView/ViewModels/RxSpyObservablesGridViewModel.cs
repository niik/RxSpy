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
        IReactiveDerivedList<RxSpyObservableGridItemViewModel> _observables;
        public IReactiveDerivedList<RxSpyObservableGridItemViewModel> Observables
        {
            get { return _observables; }
            set { this.RaiseAndSetIfChanged(ref _observables, value); }
        }

        RxSpyObservableGridItemViewModel _selectedItem;
        public RxSpyObservableGridItemViewModel SelectedItem
        {
            get { return _selectedItem; }
            set { this.RaiseAndSetIfChanged(ref _selectedItem, value); }
        }

        public RxSpyObservablesGridViewModel(IReadOnlyReactiveList<RxSpyObservableModel> model)
        {
            Observables = model.CreateDerivedCollection(x => new RxSpyObservableGridItemViewModel(x));
        }
    }
}
