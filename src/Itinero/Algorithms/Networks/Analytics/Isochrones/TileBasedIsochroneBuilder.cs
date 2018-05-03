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
using Itinero.LocalGeo;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;

namespace Itinero.Algorithms.Networks.Analytics.Isochrones
{
    /// <summary>
    /// Represents a tile-based isochrone builder. This calculates actual polygons of area that can be reached given some time limits. The level of detail depends on the zoom-level parameter.
    /// </summary>
    public class TileBasedIsochroneBuilder : AlgorithmBase
    {
        private readonly List<float> _limits;
        private readonly IEdgeVisitor<float> _edgeVisitor;
        private readonly int _level;
        private readonly float _walkingSpeed = 1.4f;
        private readonly GeometricGraph _graph;

        /// <summary>
        /// Creates a new tile-based isochrone builder.
        /// </summary>
        /// <param name="edgeVisitor">The algorithm that visits the edges.</param>
        /// <param name="limits">The limits to generate isochrones for.</param>
        /// <param name="level">The level of detail specified as an OpenStreetMap tile zoom level.</param>
        public TileBasedIsochroneBuilder(GeometricGraph graph, IEdgeVisitor<float> edgeVisitor, List<float> limits, int level)
        {
            _graph = graph;
            _limits = limits;
            _level = level;
            _edgeVisitor = edgeVisitor;
        }
        
        private List<Polygon> _polygons;

        /// <summary>
        /// Executes the actual algorithm.
        /// </summary>
        protected override void DoRun(CancellationToken cancellationToken)
        {
            var tiles = new Dictionary<TileIndex, RoutingTile>();
            _polygons = new List<Polygon>();

            _edgeVisitor.Visit += (path) =>
            {
                var e = path.Edge;
                var endWeight = path.Weight;
                if (e == Constants.NO_EDGE ||
                    path.From == null)
                {
                    return false;
                }
                var startWeight = path.From.Weight;

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

                this.AddEdgeVisit(tiles, startWeight, endWeight, shape);

                return false;
            };
            _edgeVisitor.Run(cancellationToken);
            
            var tileList = tiles.Values.ToList();
            tileList = UpdateForWalking(tileList, _level, _walkingSpeed, _limits.Max());

            foreach (var isochroneLimit in _limits)
            {
                var tilesWithin = tileList.Where(t => t.Weight < isochroneLimit).ToList();
                var polygonOfTileIndexes = TilesToPolygon.TileSetToPolygon(tilesWithin);
                _polygons.Add(new Polygon { ExteriorRing = TileHelper.ToWorldCoordinates(polygonOfTileIndexes, _level) });
            }
        }

        private void AddEdgeVisit(Dictionary<TileIndex, RoutingTile> tiles, float startWeight, float endWeight, List<Coordinate> shape)
        {
            var startCoordinate = shape[0];
            var startTile = TileIndex.WorldToTileIndex(startCoordinate.Latitude, startCoordinate.Longitude, _level);
            var startTileTopLeft = TileIndex.TileIndexToWorld(startTile.X, startTile.Y, _level);
            var startTileBottomRight = TileIndex.TileIndexToWorld(startTile.X + 1, startTile.Y + 1, _level);

            var latitudeDiff = System.Math.Abs(startTileTopLeft.Latitude - startTileBottomRight.Latitude);
            var longitudeDiff = System.Math.Abs(startTileTopLeft.Longitude - startTileBottomRight.Longitude);

            var offsetLat = Coordinate.DistanceEstimateInMeter(startCoordinate.Latitude, startCoordinate.Longitude,
                startCoordinate.Latitude + latitudeDiff, startCoordinate.Longitude);
            var offsetLon = Coordinate.DistanceEstimateInMeter(startCoordinate.Latitude, startCoordinate.Longitude,
                startCoordinate.Latitude, startCoordinate.Longitude + longitudeDiff);
            var offsetMin = System.Math.Min(offsetLat, offsetLon);

            var length = Coordinate.DistanceEstimateInMeter(shape);
            var weight = endWeight - startWeight;
            var currentLength = 0f;

            for(var i = 0; i < shape.Count; i++)
            {
                var coordinate = shape[i];
                var index = TileIndex.WorldToTileIndex(coordinate.Latitude, coordinate.Longitude, _level);

                if (i > 0)
                {
                    currentLength += Coordinate.DistanceEstimateInMeter(shape[i - 1], shape[i]);
                }

                var tileWeight = startWeight + (currentLength / length) * weight;
                RoutingTile tile;
                if (!tiles.TryGetValue(index, out tile))
                {
                    tile = new RoutingTile
                    {
                        Weight = tileWeight,
                        Count = 1,
                        Index = index
                    };
                }
                else
                {
                    tile.Weight = (tile.Weight * tile.Count + tileWeight) / tile.Count + 1;
                    tile.Count++;
                }
                tiles[index] = tile;

                if (i < shape.Count - 1)
                {
                    var distance = Coordinate.DistanceEstimateInMeter(shape[i], shape[i + 1]);
                    if (distance > offsetMin)
                    { // it's possible these two skipped over a tile.
                        var count = (float)System.Math.Ceiling(distance / offsetMin);
                        var latOffset = (shape[i + 1].Latitude - shape[i].Latitude) / count;
                        var lonOffset = (shape[i + 1].Longitude - shape[i].Longitude) / count;

                        for(var j = 0; j < count - 1; j++)
                        {
                            coordinate = new Coordinate(shape[i].Latitude + (latOffset * (j + 1)),
                                shape[i].Longitude + (lonOffset * (j + 1)));
                            index = TileIndex.WorldToTileIndex(coordinate.Latitude, coordinate.Longitude, _level);

                            var offsetLength = Coordinate.DistanceEstimateInMeter(shape[i], coordinate);
                            tileWeight = startWeight + ((currentLength + offsetLength) / length) * weight;

                            if (!tiles.TryGetValue(index, out tile))
                            {
                                tile = new RoutingTile
                                {
                                    Weight = tileWeight,
                                    Count = 1,
                                    Index = index
                                };
                            }
                            else
                            {
                                tile.Weight = (tile.Weight * tile.Count + tileWeight) / tile.Count + 1;
                                tile.Count++;
                            }
                            tiles[index] = tile;
                        }
                    }
                }
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
                        routingTile.Weight = (float)Math.Min(walkingWeight, routingTile.Weight);
                        result[tileIndex] = routingTile;
                    }
                    else
                    {
                        result[tileIndex] = new RoutingTile
                        {
                            Weight = (float)walkingWeight,
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
