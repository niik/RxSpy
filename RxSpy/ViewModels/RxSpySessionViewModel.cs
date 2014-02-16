using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;
using RxSpy.Models;

namespace RxSpy.ViewModels
{
    public class RxSpySessionViewModel: ReactiveObject
    {
        public RxSpySessionModel Model { get; set; }

        readonly ObservableAsPropertyHelper<IReadOnlyReactiveList<RxSpyObservableModel>> _trackedObservables;
        public IReadOnlyReactiveList<RxSpyObservableModel> TrackedObservables
        {
            get { return _trackedObservables.Value; }
        }

        readonly ObservableAsPropertyHelper<IReadOnlyReactiveList<RxSpyObservableModel>> _activeTrackedObservables;
        public IReadOnlyReactiveList<RxSpyObservableModel> ActiveTrackedObservables
        {
            get { return _activeTrackedObservables.Value; }
        }

        RxSpyObservablesGridViewModel _gridViewModel;
        public RxSpyObservablesGridViewModel GridViewModel
        {
            get { return _gridViewModel; }
            set { this.RaiseAndSetIfChanged(ref _gridViewModel, value); }
        }

        public RxSpySessionViewModel(RxSpySessionModel model)
        {
            Model = model;

            this.WhenAnyValue(x => x.Model.TrackedObservables)
                .ToProperty(this, x => x.TrackedObservables, out _trackedObservables);

            this.WhenAnyValue(x => x.Model.ActiveTrackedObservables)
                .ToProperty(this, x => x.ActiveTrackedObservables, out _activeTrackedObservables);

            GridViewModel = new RxSpyObservablesGridViewModel(model);
        }
    }
}
