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
using System;
using System.Collections.Generic;
using System.Linq;

namespace Itinero.Algorithms.Networks.Analytics.Isochrones
{
    /// <summary>
    /// Types of touch strategies.
    /// </summary>
    public enum TouchStrategy
    {
        /// <summary>
        /// If touching another tile on a single point include it in the polygon.
        /// </summary>
        Inclusive,
        /// <summary>
        /// If touching another tile on a single point exclude it from the polygon.
        /// </summary>
        Exclusive
    }

    /// <summary>
    /// Holds tiles to polygon code.
    /// </summary>
    public static class TilesToPolygon
    {
        /// <summary>
        /// Converts the given tile set to a polygon.
        /// </summary>
        public static List<TileIndex> TileSetToPolygon(List<RoutingTile> tiles, TouchStrategy touchStrategy = 
            TouchStrategy.Inclusive)
        {
            // note: There is still one case where the algoritm is not correct. This is when a tile is touched
            // on all four corners but not on any of the sides. In that case this tile will be excluded.

            var corners = ToCornersNodes(tiles);
            var cornersLists = corners.ToList();
            var polygons = new List<List<TileIndex>>();
            int currentIndex = 0;

            do
            {
                var polygon = ExtractPolygon(ref currentIndex, corners, cornersLists, touchStrategy);
                cornersLists = corners.ToList();
                if (currentIndex >= corners.Count) break; // Finished
                if (!IsClockwisePolygon(polygon)) polygons.Add(polygon); // Ignore holes for now
            } while (true);

            return polygons.OrderByDescending(p => p.Count).First(); // Return only biggest for now
        }
        
        internal enum Corner
        {
            None = 0,
            NorthEast = 1,
            NorthWest = 2,
            SouthEast = 4,
            SouthWest = 8,
            All = 15
        }

        private static IDictionary<TileIndex, Corner> ToCornersNodes(List<RoutingTile> tiles)
        {
            var corners = new Dictionary<TileIndex, Corner>();

            foreach (var tile in tiles)
            {
                // makes sure they all exists. These keys will be set multiple times. 
                // Four times in most cases. There must be a better way
                corners[tile.Index] = Corner.None;
                corners[new TileIndex(tile.Index.X, tile.Index.Y + 1)] = Corner.None;
                corners[new TileIndex(tile.Index.X + 1, tile.Index.Y + 1)] = Corner.None;
                corners[new TileIndex(tile.Index.X + 1, tile.Index.Y)] = Corner.None;
            }

            foreach (var tile in tiles)
            {
                // The flags are corner centric not tile centric. 
                // So the flags indicate on which side the corner is touched.
                // Not on which side of the tiles the corner is. It is the inverse of that.
                UpdateCornersForTileAdd(corners, tile.Index);
            }

            return corners;
        }

        private static void UpdateCornersForTileAdd(IDictionary<TileIndex, Corner> corners, TileIndex index)
        {
            corners[index] |= Corner.SouthEast;
            corners[new TileIndex(index.X + 1, index.Y)] |= Corner.SouthWest;
            corners[new TileIndex(index.X + 1, index.Y + 1)] |= Corner.NorthWest;
            corners[new TileIndex(index.X, index.Y + 1)] |= Corner.NorthEast;
        }

        private static List<TileIndex> ExtractPolygon(ref int currentIndex, IDictionary<TileIndex, Corner> corners,
            List<KeyValuePair<TileIndex, Corner>> cornersList, TouchStrategy touchStrategy = TouchStrategy.Inclusive)
        {
            var results = new List<TileIndex>(); // The tile indexes returned indicate coordinates, not a tiles.

            currentIndex = FindPolygonStart(currentIndex, corners, cornersList);
            if (currentIndex >= corners.Count) return null; // past end

            var first = cornersList[currentIndex].Key;
            var current = first;
            var next = new TileIndex();
            var previous = current;

            do
            {
                results.Add(current);

                // single tile touches
                if (corners[current] == Corner.NorthWest) next = new TileIndex(current.X - 1, current.Y);
                else if (corners[current] == Corner.NorthEast) next = new TileIndex(current.X, current.Y - 1);
                else if (corners[current] == Corner.SouthEast) next = new TileIndex(current.X + 1, current.Y);
                else if (corners[current] == Corner.SouthWest) next = new TileIndex(current.X, current.Y + 1);

                // double tile touches
                else if (corners[current] == (Corner.NorthWest | Corner.NorthEast)) next = new TileIndex(current.X - 1, current.Y);
                else if (corners[current] == (Corner.NorthEast | Corner.SouthEast)) next = new TileIndex(current.X, current.Y - 1);
                else if (corners[current] == (Corner.SouthEast | Corner.SouthWest)) next = new TileIndex(current.X + 1, current.Y);
                else if (corners[current] == (Corner.SouthWest | Corner.NorthWest)) next = new TileIndex(current.X, current.Y + 1);

                // triple tile touches
                else if (corners[current] == (Corner.NorthWest | Corner.NorthEast | Corner.SouthEast)) next = new TileIndex(current.X - 1, current.Y);
                else if (corners[current] == (Corner.NorthEast | Corner.SouthEast | Corner.SouthWest)) next = new TileIndex(current.X, current.Y - 1);
                else if (corners[current] == (Corner.SouthEast | Corner.SouthWest | Corner.NorthWest)) next = new TileIndex(current.X + 1, current.Y);
                else if (corners[current] == (Corner.SouthWest | Corner.NorthWest | Corner.NorthEast)) next = new TileIndex(current.X, current.Y + 1);

                else if (corners[current] == (Corner.NorthWest | Corner.SouthEast))
                {
                    var correction = touchStrategy == TouchStrategy.Inclusive ? current.Y - previous.Y :
                        previous.Y - current.Y;
                    next = new TileIndex(current.X + correction, current.Y);
                }
                else if (corners[current] == (Corner.NorthEast | Corner.SouthWest))
                {
                    var correction = touchStrategy == TouchStrategy.Inclusive ? previous.X - current.X :
                        current.X - previous.X;
                    next = new TileIndex(current.X, current.Y + correction);
                }

                previous = current;
                current = next;

            } while (first != current);

            results.Add(first); // close the polygon

            if (results.Count <= 2) throw new Exception("Only two nodes in the polygon. This should not be possible");

            WhipeCornersUsed(corners, results);

            return results;
        }

        private static int FindPolygonStart(int currentIndex, IDictionary<TileIndex, Corner> corners,
            List<KeyValuePair<TileIndex, Corner>> cornersList)
        {
            while (currentIndex < corners.Count)
            {
                var c = cornersList[currentIndex].Value;

                if (c != Corner.All // All corners set means there is no corner 
                    && c != Corner.None // Nothing here so ignore
                    && c != (Corner.NorthWest | Corner.SouthEast) // Don't start at touching corner
                    && c != (Corner.NorthEast | Corner.SouthWest)) // Same for other touching corner
                {
                    return currentIndex;
                }
                currentIndex++;
            }
            return currentIndex;
        }

        private static void WhipeCornersUsed(IDictionary<TileIndex, Corner> corners, List<TileIndex> results)
        {
            foreach (var cornerIndex in results)
            {
                // This is complicated. We whipe all corners so that they will not be found
                // again. But not the diagonal touching corners because they could be part 
                // of other polygons. Since the algorithm never starts on a touching corner
                // this is not a problem. 

                if (corners[cornerIndex] == (Corner.NorthWest | Corner.SouthEast) ||
                    (corners[cornerIndex] == (Corner.NorthEast | Corner.SouthWest)))
                    continue;

                corners[cornerIndex] = Corner.None;
            }
        }

        private static bool IsClockwisePolygon(List<TileIndex> polygon)
        {
            double sum = 0;
            for (var i = 0; i < polygon.Count - 1; i++)
            {
                sum += (polygon[i + 1].X - polygon[i].X) * (polygon[i + 1].Y + polygon[i].Y);
            }
            return sum > 0;
        }
    }
}