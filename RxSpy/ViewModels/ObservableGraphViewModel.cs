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

            var vertex = new ObservableVertex(model);
            Graph.AddVertex(vertex);

            AddAncestors(vertex);
            AddDescendants(vertex);
        }

        private void AddDescendants(ObservableVertex vertex)
        {
            foreach (var child in vertex.Model.Children)
            {
                var childVertex = new ObservableVertex(child);

                Graph.AddVerticesAndEdge(new ObserveableEdge(vertex, childVertex));
                AddDescendants(childVertex);
            }
        }

        void AddAncestors(ObservableVertex vertex)
        {
            foreach (var parent in vertex.Model.Parents)
            {
                var parentVertex = new ObservableVertex(parent);

                Graph.AddVerticesAndEdge(new ObserveableEdge(parentVertex, vertex));
                AddAncestors(parentVertex);
            }
        }
    }
}
