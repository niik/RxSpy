using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GraphSharp;
using QuickGraph;
using RxSpy.Models;

namespace RxSpy.ViewModels.Graphs
{
    public class ObservableGraph : BidirectionalGraph<RxSpyObservableModel, ObserveableEdge>
    {
    }
}
