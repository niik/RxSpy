using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using QuickGraph;
using RxSpy.Models;

namespace RxSpy.ViewModels.Graphs
{
    [DebuggerDisplay("{Source} -> {Target}")]
    public class ObserveableEdge : Edge<ObservableVertex>, IEquatable<ObserveableEdge>
    {
        public ObserveableEdge(ObservableVertex child, ObservableVertex parent)
            : base(child, parent)
        {

        }

        public bool Equals(ObserveableEdge other)
        {
            if (other == null) return false;

            return other.Source.Equals(Source) && other.Target.Equals(Target);
        }

        public override int GetHashCode()
        {
            return Source.GetHashCode() ^ Target.GetHashCode();
        }
    }
}
