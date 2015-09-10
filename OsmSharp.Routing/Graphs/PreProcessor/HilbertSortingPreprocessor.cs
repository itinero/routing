// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2015 Abelshausen Ben
// 
// This file is part of OsmSharp.
// 
// OsmSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// OsmSharp is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with OsmSharp. If not, see <http://www.gnu.org/licenses/>.

namespace OsmSharp.Routing.Graph.PreProcessor
{
    /// <summary>
    /// A pre-processor that sorts the given graph's vertices but maintains topology.
    /// </summary>
    /// <remarks>Use only on graphs with edges independent of vertex id's as they will change.</remarks>
    public class HilbertSortingPreprocessor<TEdgeData> : IPreprocessor
            where TEdgeData : struct, IGraphEdgeData
    {
        private readonly GraphBase<TEdgeData> _graph;

        /// <summary>
        /// Creates a new hilbert sorting preprocessor.
        /// </summary>
        /// <param name="graph"></param>
        public HilbertSortingPreprocessor(GraphBase<TEdgeData> graph)
        {
            _graph = graph;
        }
        
        /// <summary>
        /// Starts this preprocessor.
        /// </summary>
        public void Start()
        {
            // sort vertices.
            OsmSharp.Logging.Log.TraceEvent("GraphOsmStreamTargetBase", Logging.TraceEventType.Information,
                "Spatially sorting vertices...");
            _graph.SortHilbert();
        }
    }
}