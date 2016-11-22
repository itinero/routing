using Reminiscence.Arrays;

namespace Itinero.Graphs.Directed
{
    /// <summary>
    /// A profile with settings for a memory-mapped meta-graph.
    /// </summary>
    public class DirectedMetaGraphProfile
    {
        /// <summary>
        /// Gets or sets the directed graph profile.
        /// </summary>
        public DirectedDynamicGraphProfile DirectedGraphProfile { get; set; }

        /// <summary>
        /// Gets or sets the vertex meta array profile.
        /// </summary>
        public ArrayProfile VertexMetaProfile { get; set; }

        /// <summary>
        /// Gets or sets the edge meta array profile.
        /// </summary>
        public ArrayProfile EdgeMetaProfile { get; set; }

        /// <summary>
        /// A profile that tells the graph to do no caching.
        /// </summary>
        public static DirectedMetaGraphProfile NoCache = new DirectedMetaGraphProfile()
        {
            DirectedGraphProfile = DirectedDynamicGraphProfile.NoCache,
            EdgeMetaProfile = ArrayProfile.NoCache,
            VertexMetaProfile = ArrayProfile.NoCache
        };

        /// <summary>
        /// A profile that tells the graph to prepare for sequential access.
        /// </summary>
        public static DirectedMetaGraphProfile OneBuffer = new DirectedMetaGraphProfile()
        {
            DirectedGraphProfile = DirectedDynamicGraphProfile.OneBuffer,
            EdgeMetaProfile = ArrayProfile.Aggressive8,
            VertexMetaProfile = ArrayProfile.OneBuffer
        };

        /// <summary>
        /// An array profile that aggressively caches data with potenally 40Kb of cached data.
        /// </summary>
        public static DirectedMetaGraphProfile Aggressive40 = new DirectedMetaGraphProfile()
        {
            DirectedGraphProfile = DirectedDynamicGraphProfile.Aggressive24,
            EdgeMetaProfile = ArrayProfile.Aggressive8,
            VertexMetaProfile = ArrayProfile.Aggressive8
        };
    }
}
