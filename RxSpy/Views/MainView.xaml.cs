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

            this.OneWayBind(ViewModel, vm => vm.SpySessionViewModel.GridViewModel, v => v.observablesGrid.DataContext);
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
