/*
 *  Licensed to SharpSoftware under one or more contributor
 *  license agreements. See the NOTICE file distributed with this work for 
 *  additional information regarding copyright ownership.
 * 
 *  SharpSoftware licenses this file to you under the Apache License, 
 *  Version 2.0 (the "License"); you may not use this file except in 
 *  compliance with the License. You may obtain a copy of the License at
 * 
 *       http://www.apache.org/licenses/LICENSE-2.0
 * 
 *  Unless required by applicable law or agreed to in writing, software
 *  distributed under the License is distributed on an "AS IS" BASIS,
 *  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *  See the License for the specific language governing permissions and
 *  limitations under the License.
 */

using Itinero.Algorithms.Tiles;
using Itinero.Graphs.Geometric;
using System.Collections.Generic;
using System.Threading;

namespace Itinero.Algorithms.Networks.Analytics.Heatmaps
{
    /// <summary>
    /// Represents a tile-based heatmap builder.
    /// </summary>
    public class TileBasedHeatmapBuilder : AlgorithmBase
    {
        private readonly GeometricGraph _graph;
        private readonly IEdgeVisitor<float> _edgeVisitor;
        private readonly int _level;

        /// <summary>
        /// Creates a new tile-based heatmap builder.
        /// </summary>
        /// <param name="edgeVisitor">The algorithm that visits the edges.</param>
        /// <param name="level">The level of detail specified as an OpenStreetMap tile zoom level.</param>
        public TileBasedHeatmapBuilder(GeometricGraph graph, IEdgeVisitor<float> edgeVisitor, int level)
        {
            _graph = graph;
            _level = level;
            _edgeVisitor = edgeVisitor;
        }

        private HeatmapResult _result;

        /// <summary>
        /// Executes the actual algorithm.
        /// </summary>
        protected override void DoRun(CancellationToken cancellationToken)
        {
            var tiles = new Dictionary<TileIndex, RoutingTile>();

            _edgeVisitor.Visit += (path) =>
            {
                var e = path.Edge;
                var endWeight = path.Weight;
                if (e == Constants.NO_EDGE)
                {
                    return false;
                }

                // Calculate weight at start vertex.
                uint edgeId;
                if (e > 0)
                {
                    edgeId = (uint)e - 1;
                }
                else
                {
                    edgeId = (uint)((-e) - 1);
                }
                var edge = _graph.GetEdge(edgeId);
                var shape = _graph.GetShape(edge);

                var endCoordinate = shape[shape.Count - 1];
                var index = TileIndex.WorldToTileIndex(endCoordinate.Latitude, endCoordinate.Longitude, _level);
                RoutingTile tile;
                if (!tiles.TryGetValue(index, out tile))
                {
                    tile = new RoutingTile
                    {
                        Weight = endWeight,
                        Count = 1,
                        Index = index
                    };
                }
                else
                {
                    tile.Weight = (tile.Weight * tile.Count + endWeight) / tile.Count + 1;
                    tile.Count++;
                }
                tiles[index] = tile;

                return false;
            };
            _edgeVisitor.Run(cancellationToken);

            _result = new HeatmapResult();
            _result.Data = new HeatmapSample[tiles.Count];

            var max = 0f;
            var i = 0;
            foreach(var pair in tiles)
            {
                var location = TileIndex.TileIndexToWorld(pair.Key.X, pair.Key.Y, _level);
                _result.Data[i] = new HeatmapSample()
                {
                    Latitude = location.Latitude,
                    Longitude = location.Longitude,
                    Value = pair.Value.Weight
                };

                if (max < pair.Value.Weight)
                {
                    max = pair.Value.Weight;
                }

                i++;
            }
            _result.Max = max;

            this.HasSucceeded = true;
        }

        /// <summary>
        /// Gets the result.
        /// </summary>
        public HeatmapResult Result
        {
            get
            {
                return _result;
            }
        }
    }
}