using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using ReactiveUI;
using RxSpy.ViewModels;

namespace RxSpy.Views.Controls
{
    /// <summary>
    /// Interaction logic for ObservableDetails.xaml
    /// </summary>
    public partial class ObservableDetails : UserControl, IViewFor<RxSpyObservableDetailsViewModel>
    {
        public ObservableDetails()
        {
            InitializeComponent();

            this.OneWayBind(ViewModel, vm => vm.ObservedValues, v => v.observableValuesGrid.ItemsSource);
            this.OneWayBind(ViewModel, vm => vm.Parents, v => v.parentsView.ViewModel);
            this.OneWayBind(ViewModel, vm => vm.Children, v => v.childrenView.ViewModel);

            this.OneWayBind(ViewModel, vm => vm.ShowErrorTab, v => v.errorTab.Visibility);
            this.OneWayBind(ViewModel, vm => vm.ErrorText, v => v.errorText.Text);
        }

        public RxSpyObservableDetailsViewModel ViewModel
        {
            get { return GetValue(ViewModelProperty) as RxSpyObservableDetailsViewModel; }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            "ViewModel",
            typeof(RxSpyObservableDetailsViewModel),
            typeof(ObservableDetails),
            new PropertyMetadata(null)
        );

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = value as RxSpyObservableDetailsViewModel; }
        }
    }
}
