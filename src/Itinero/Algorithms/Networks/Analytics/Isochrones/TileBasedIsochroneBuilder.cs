// Itinero - Routing for .NET
// Copyright (C) 2016 Paul Den Dulk, Abelshausen Ben
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

using Itinero.LocalGeo;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Itinero.Algorithms.Networks.Analytics.Isochrones
{
    /// <summary>
    /// Represents a tile-based isochrone builder. This calculates actual polygons of area that can be reached given some time limits. The level of detail depends on the zoom-level parameter.
    /// </summary>
    public class TileBasedIsochroneBuilder : AlgorithmBase
    {
        private readonly List<float> _limits;
        private readonly IEdgeVisitor _edgeVisitor;
        private readonly int _level;
        private readonly float _walkingSpeed = 1.4f;

        /// <summary>
        /// Creates a new tile-based isochrone builder.
        /// </summary>
        /// <param name="edgeVisitor">The algorithm that visits the edges.</param>
        /// <param name="limits">The limits to generate isochrones for.</param>
        /// <param name="level">The level of detail specified as an OpenStreetMap tile zoom level.</param>
        public TileBasedIsochroneBuilder(IEdgeVisitor edgeVisitor, List<float> limits, int level)
        {
            _limits = limits;
            _level = level;
            _edgeVisitor = edgeVisitor;
        }
        
        private List<Polygon> _polygons;

        /// <summary>
        /// Executes the actual algorithm.
        /// </summary>
        protected override void DoRun()
        {
            var tiles = new Dictionary<TileIndex, RoutingTile>();
            _polygons = new List<Polygon>();

            _edgeVisitor.Visit += (id, startWeight, endWeight, shape) =>
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
            
            var tileList = tiles.Values.ToList();
            tileList = UpdateForWalking(tileList, _level, _walkingSpeed, _limits.Max());

            foreach (var isochroneLimit in _limits)
            {
                var tilesWithin = tileList.Where(t => t.Weight < isochroneLimit).ToList();
                var polygonOfTileIndexes = TilesToPolygon.TileSetToPolygon(tilesWithin);
                _polygons.Add(new Polygon { ExteriorRing = TileHelper.ToWorldCoordinates(polygonOfTileIndexes, _level) });
            }
        }

        /// <summary>
        /// Gets the resulting polygons.
        /// </summary>
        public List<Polygon> Polygons
        {
            get
            {
                return _polygons;
            }
        }

        private struct Size
        {
            public double Width;
            public double Height;
        }

        private static List<RoutingTile> UpdateForWalking(List<RoutingTile> tiles, int level,
            float speedInMetersPerSecond, float isochroneLimit)
        {
            // todo: Determine the range the proper way. It should depend on level
            var result = new Dictionary<TileIndex, RoutingTile>();

            var size = GetTileSize(tiles.First(), level); // estimate the tile size

            foreach (var tile in tiles)
            {
                UpdateForWalking(result, tile, level, speedInMetersPerSecond, isochroneLimit, size);
            }

            return result.Values.ToList();
        }

        private static void UpdateForWalking(Dictionary<TileIndex, RoutingTile> result, RoutingTile sourceTile,
            int level, float speedInMetersPerSecond, float isochroneLimit, Size size)
        {
            var tileRange = TileHelper.CalculateTileRange(sourceTile, isochroneLimit, speedInMetersPerSecond, level);

            // The code below could be optimized if we start at the center and stop as soon
            // as an update does not improve a weight.

            for (var x = tileRange.Left; x <= tileRange.Right; x++)
            {
                for (var y = tileRange.Top; y <= tileRange.Bottom; y++)
                {
                    // note: The value of the node itself is included by adding zero in case distance is zero.

                    // todo: Ignore tiles outside tile schema.

                    var distance = Math.Sqrt(
                        Math.Pow((sourceTile.Index.X - x) * size.Width, 2) +
                        Math.Pow((sourceTile.Index.Y - y) * size.Height, 2));

                    var walkingWeight = sourceTile.Weight + distance * speedInMetersPerSecond;

                    var tileIndex = new TileIndex(x, y);

                    if (result.ContainsKey(tileIndex))
                    {
                        var routingTile = result[tileIndex];
                        routingTile.Weight = Math.Min(walkingWeight, routingTile.Weight);
                        result[tileIndex] = routingTile;
                    }
                    else
                    {
                        result[tileIndex] = new RoutingTile
                        {
                            Weight = walkingWeight,
                            Index = tileIndex,
                        };
                    }
                }
            }
        }

        private static Size GetTileSize(RoutingTile sourceTile, int level)
        {
            // There is probably a more direct way to calculate this.
            var sourceTopLeft = TileIndex.TileIndexToWorld(sourceTile.Index.X, sourceTile.Index.Y, level);
            var sourceBottomLeft = TileIndex.TileIndexToWorld(sourceTile.Index.X, sourceTile.Index.Y + 1, level);
            var sourceTopRight = TileIndex.TileIndexToWorld(sourceTile.Index.X + 1, sourceTile.Index.Y, level);

            var tileHeight = Coordinate.DistanceEstimateInMeter(sourceTopLeft, sourceBottomLeft);
            var tileWidth = Coordinate.DistanceEstimateInMeter(sourceTopLeft, sourceTopRight);

            return new Size { Width = tileWidth, Height = tileHeight };
        }

		//private static TileRange CalculateTileRange(RoutingTile sourceTile, double isochroneLimit,
		//	double speedInMetersPerSecond, int level)
		//{
		//	var tileRange = new TileRange();

		//	var sourceLocation = TileTransform.TileIndexToWorld(sourceTile.Index.X + 0.5, sourceTile.Index.Y + 0.5, level);

		//	var secondsLeft = isochroneLimit - sourceTile.Weight;
		//	var walkingReach = (float)(secondsLeft * speedInMetersPerSecond);

		//	if (walkingReach <= 0)
		//		return new TileRange
		//		{
		//			Right = sourceTile.Index.X,
		//			Left = sourceTile.Index.X,
		//			Top = sourceTile.Index.Y,
		//			Bottom = sourceTile.Index.Y
		//		};

		//	var topLeft = sourceLocation.OffsetWithDirection(walkingReach, DirectionEnum.NorthWest);
		//	var bottomRight = sourceLocation.OffsetWithDirection(walkingReach, DirectionEnum.SouthEast);

		//	var topLeftTile = TileTransform.WorldToTileIndex(topLeft.Latitude, topLeft.Longitude, level);
		//	var bottomRightTile = TileTransform.WorldToTileIndex(bottomRight.Latitude, bottomRight.Longitude, level);

		//	tileRange.Top = topLeftTile.Y;
		//	tileRange.Left = topLeftTile.X;
		//	tileRange.Bottom = bottomRightTile.Y;
		//	tileRange.Right = bottomRightTile.X;

		//	Debug.WriteLine($"range north south: {tileRange.Bottom - tileRange.Top}");
		//	Debug.WriteLine($"range west east:   {tileRange.Right - tileRange.Left}");
		//	return tileRange;
		//}
    }
}
