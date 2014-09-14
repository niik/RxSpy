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
using System.Windows.Shapes;
using ReactiveUI;
using RxSpy.ViewModels;

namespace RxSpy
{
    /// <summary>
    /// Interaction logic for GraphWindow.xaml
    /// </summary>
    public partial class GraphWindow : Window, IViewFor<ObservableGraphViewModel>
    {
        public GraphWindow()
        {
            InitializeComponent();

            DataContextChanged += (s, e) => ViewModel = e.NewValue as ObservableGraphViewModel;

            this.OneWayBind(ViewModel, vm => vm.Graph, v => v.graphLayout.Graph);
        }

        public ObservableGraphViewModel ViewModel
        {
            get { return GetValue(ViewModelProperty) as ObservableGraphViewModel; }
            set { SetValue(ViewModelProperty, value); }
        }

        public static readonly DependencyProperty ViewModelProperty = DependencyProperty.Register(
            "ViewModel",
            typeof(ObservableGraphViewModel),
            typeof(GraphWindow),
            new PropertyMetadata(null)
        );

        object IViewFor.ViewModel
        {
            get { return ViewModel; }
            set { ViewModel = value as ObservableGraphViewModel; }
        }
    }
}
