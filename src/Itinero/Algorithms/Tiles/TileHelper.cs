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

using Itinero.LocalGeo;
using Itinero.Navigation.Directions;
using System.Collections.Generic;

namespace Itinero.Algorithms.Tiles
{
    /// <summary>
    /// Holds helper functions.
    /// </summary>
    public static class TileHelper
    {
        /// <summary>
        /// Converts the given polygon.
        /// </summary>
        public static List<Coordinate> ToWorldCoordinates(List<TileIndex> tileIndexes, int level)
        {
            var coordinates = new List<Coordinate>();

            foreach (var tileIndex in tileIndexes)
            {
                var position = TileIndex.TileIndexToWorld(tileIndex.X, tileIndex.Y, level);
                coordinates.Add(position);
            }

            return coordinates;
        }

        internal static TileRange CalculateTileRange(RoutingTile sourceTile, double isochroneLimit,
            double speedInMetersPerSecond, int level)
        {
            var tileRange = new TileRange();

            var sourceLocation = TileIndex.TileIndexToWorld(sourceTile.Index.X + 0.5, sourceTile.Index.Y + 0.5, level);

            var secondsLeft = isochroneLimit - sourceTile.Weight;
            var walkingReach = (float)(secondsLeft * speedInMetersPerSecond);

            if (walkingReach <= 0)
                return new TileRange
                {
                    Right = sourceTile.Index.X,
                    Left = sourceTile.Index.X,
                    Top = sourceTile.Index.Y,
                    Bottom = sourceTile.Index.Y
                };

            var topLeft = sourceLocation.OffsetWithDirection(walkingReach, DirectionEnum.NorthWest);
            var bottomRight = sourceLocation.OffsetWithDirection(walkingReach, DirectionEnum.SouthEast);

            var topLeftTile = TileIndex.WorldToTileIndex(topLeft.Latitude, topLeft.Longitude, level);
            var bottomRightTile = TileIndex.WorldToTileIndex(bottomRight.Latitude, bottomRight.Longitude, level);

            tileRange.Top = topLeftTile.Y;
            tileRange.Left = topLeftTile.X;
            tileRange.Bottom = bottomRightTile.Y;
            tileRange.Right = bottomRightTile.X;

            //Itinero.Logging.Logger.Log("TileHelper", Logging.TraceEventType.Information, 
            //    $"range north south: {tileRange.Bottom - tileRange.Top}");
            //Itinero.Logging.Logger.Log("TileHelper", Logging.TraceEventType.Information,
            //    $"range west east:   {tileRange.Right - tileRange.Left}");
            return tileRange;
        }
    }
}