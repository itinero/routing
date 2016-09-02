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