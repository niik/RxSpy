using System.Windows;
using System.Windows.Controls;
using ReactiveUI;
using RxSpy.ViewModels;

namespace RxSpy.Views
{
    /// <summary>
    /// Interaction logic for MainView.xaml
    /// </summary>
    public partial class MainView : UserControl, IViewFor<MainViewModel>
    {
        public MainView()
        {
            InitializeComponent();

            DataContextChanged += (s, e) => ViewModel = e.NewValue as MainViewModel;

            this.OneWayBind(ViewModel, vm => vm.GridViewModel, v => v.observablesGrid.ViewModel);
            this.OneWayBind(ViewModel, vm => vm.DetailsViewModel, v => v.detailsView.ViewModel);
        }

        public MainViewModel ViewModel
        {
            get { return GetValue(ViewModelProperty) as MainViewModel; }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            "ViewModel",
            typeof(MainViewModel),
            typeof(MainView),
            new PropertyMetadata(null)
        );

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = value as MainViewModel; }
        }
    }
}
