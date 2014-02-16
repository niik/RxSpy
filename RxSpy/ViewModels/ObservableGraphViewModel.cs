using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ReactiveUI;
using RxSpy.Models;
using RxSpy.ViewModels.Graphs;

namespace RxSpy.ViewModels
{
    public class ObservableGraphViewModel: ReactiveObject
    {
        ObservableGraph _graph;
        public ObservableGraph Graph
        {
            get { return _graph; }
            set { this.RaiseAndSetIfChanged(ref _graph, value); }
        }

        public ObservableGraphViewModel(RxSpyObservableModel model)
        {
            Graph = new ObservableGraph();

            Graph.AddVertex(model);

            AddAncestors(model);
            AddDescendants(model);
        }

        private void AddDescendants(RxSpyObservableModel model)
        {
            foreach (var child in model.Children)
            {
                Graph.AddVerticesAndEdge(new ObserveableEdge(model, child));
                AddDescendants(child);
            }
        }

        void AddAncestors(RxSpyObservableModel model)
        {
            foreach (var parent in model.Parents)
            {
                Graph.AddVerticesAndEdge(new ObserveableEdge(parent, model));
                AddAncestors(parent);
            }
        }
    }
}
