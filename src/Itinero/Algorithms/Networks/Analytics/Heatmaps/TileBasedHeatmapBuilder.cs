// Itinero - Routing for .NET
// Copyright (C) 2016 Abelshausen Ben
// 
// This file is part of Itinero.
// 
// Itinero is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// Itinero is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Itinero. If not, see <http://www.gnu.org/licenses/>.

using Itinero.Algorithms.Networks.Analytics.Isochrones;
using Itinero.Algorithms.Tiles;
using System.Collections.Generic;

namespace Itinero.Algorithms.Networks.Analytics.Heatmaps
{
    /// <summary>
    /// Represents a tile-based heatmap builder.
    /// </summary>
    public class TileBasedHeatmapBuilder : AlgorithmBase
    {
        private readonly IEdgeVisitor _edgeVisitor;
        private readonly int _level;

        /// <summary>
        /// Creates a new tile-based heatmap builder.
        /// </summary>
        /// <param name="edgeVisitor">The algorithm that visits the edges.</param>
        /// <param name="level">The level of detail specified as an OpenStreetMap tile zoom level.</param>
        public TileBasedHeatmapBuilder(IEdgeVisitor edgeVisitor, int level)
        {
            _level = level;
            _edgeVisitor = edgeVisitor;
        }

        private HeatmapResult _result;

        /// <summary>
        /// Executes the actual algorithm.
        /// </summary>
        protected override void DoRun()
        {
            var tiles = new Dictionary<TileIndex, RoutingTile>();

            _edgeVisitor.Visit += (id, startVertex, startWeight, endVertex, endWeight, shape) =>
            {
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
            };
            _edgeVisitor.Run();

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