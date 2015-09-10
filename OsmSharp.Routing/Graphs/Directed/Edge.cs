using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OsmSharp.Routing.Graphs.Directed
{
    /// <summary>
    /// Abstract representation of an edge.
    /// </summary>
    public class Edge
    {
        /// <summary>
        /// Creates a new edge keeping the current state of the given enumerator.
        /// </summary>
        internal Edge(DirectedGraph.EdgeEnumerator enumerator)
        {
            this.Neighbour = enumerator.Neighbour;
            this.Data = enumerator.Data;
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
        public uint[] Data
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
                this.Data.ToInvariantString());
        }
    }
}