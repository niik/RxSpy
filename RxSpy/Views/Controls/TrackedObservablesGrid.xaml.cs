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
using RxSpy.Models;
using RxSpy.ViewModels;

namespace RxSpy.Views.Controls
{
    /// <summary>
    /// Interaction logic for TrackedObservablesGrid.xaml
    /// </summary>
    public partial class TrackedObservablesGrid : UserControl, IViewFor<RxSpyObservablesGridViewModel>
    {
        public TrackedObservablesGrid()
        {
            InitializeComponent();

            DataContextChanged += (s, e) => ViewModel = e.NewValue as RxSpyObservablesGridViewModel;

            this.OneWayBind(ViewModel, vm => vm.Observables, v => v.observablesGrid.ItemsSource);
        }

        public RxSpyObservablesGridViewModel ViewModel
        {
            get { return GetValue(ViewModelProperty) as RxSpyObservablesGridViewModel; }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            "ViewModel",
            typeof(RxSpyObservablesGridViewModel),
            typeof(TrackedObservablesGrid),
            new PropertyMetadata(null)
        );

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = value as RxSpyObservablesGridViewModel; }
        }
    }
}
