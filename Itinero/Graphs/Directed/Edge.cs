using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Itinero.Graphs.Directed
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
            this.Id = enumerator.Id;
        }

        /// <summary>
        /// Creates a new edge keeping the current state of the given enumerator.
        /// </summary>
        internal Edge(uint id, uint neighbour, uint[] data)
        {
            this.Neighbour = neighbour;
            this.Data = data;
            this.Id = id;
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
        /// Returns the id.
        /// </summary>
        public uint Id
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
            if(this.Data != null)
            {
                var dataString = "[" + this.Data[0].ToInvariantString();
                for (var i = 1; i < this.Data.Length; i++)
                {
                    dataString += ", " + this.Data[i].ToInvariantString();
                }
                return string.Format("{0} - {1} [{2}]",
                    this.Neighbour,
                    this.Id,
                    dataString);
            }
            return string.Format("{0} - {1} []",
                this.Neighbour,
                this.Id);
        }
    }
}