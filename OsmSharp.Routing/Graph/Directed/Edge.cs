using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OsmSharp.Routing.Graph.Directed
{
    /// <summary>
    /// Abstract representation of an edge.
    /// </summary>
    /// <typeparam name="TEdgeData"></typeparam>
    public class Edge<TEdgeData>
        where TEdgeData : struct, IEdgeData
    {
        /// <summary>
        /// Creates a new edge keeping the current state of the given enumerator.
        /// </summary>
        internal Edge(DirectedGraph<TEdgeData>.EdgeEnumerator enumerator)
        {
            this.Neighbour = enumerator.Neighbour;
            this.EdgeData = enumerator.EdgeData;
        }

        /// <summary>
        /// Returns the current neighbour.
        /// </summary>
        public uint Neighbour
        {
            get;
            set;
        }

        /// <summary>
        /// Returns the edge data.
        /// </summary>
        public TEdgeData EdgeData
        {
            get;
            set;
        }

        /// <summary>
        /// Returns a string representing this edge.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0} - {1}",
                this.Neighbour,
                this.EdgeData.ToInvariantString());
        }
    }
}