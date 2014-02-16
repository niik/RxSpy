using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuickGraph;
using RxSpy.Models;

namespace RxSpy.ViewModels.Graphs
{
    public class ObserveableEdge : Edge<RxSpyObservableModel>
    {
        public ObserveableEdge(RxSpyObservableModel child, RxSpyObservableModel parent)
            : base(child, parent)
        {

        }
    }
}
