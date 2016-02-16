
namespace Itinero.Graphs.Directed
{
    /// <summary>
    /// Abstract representation of an edge.
    /// </summary>
    public class MetaEdge
    {
        /// <summary>
        /// Creates a new edge keeping the current state of the given enumerator.
        /// </summary>
        internal MetaEdge(DirectedMetaGraph.EdgeEnumerator enumerator)
        {
            this.Neighbour = enumerator.Neighbour;
            this.Data = enumerator.Data;
            this.MetaData = enumerator.MetaData;
            this.Id = enumerator.Id;
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
        /// Returns the edge meta-data.
        /// </summary>
        public uint[] MetaData
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
            return string.Format("{0} - {1}",
                this.Neighbour,
                this.Data.ToInvariantString());
        }
    }
}