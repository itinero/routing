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

namespace Itinero.Algorithms.Networks.Analytics.Isochrones
{
    /// <summary>
    /// Represents a tile at a predefined zoom level.
    /// </summary>
    public struct TileIndex
    {
        /// <summary>
        /// Creates a new tile index.
        /// </summary>
        public TileIndex(int x, int y)
        {
            X = x;
            Y = y;
        }

        /// <summary>
        /// Keeps X.
        /// </summary>
        public int X;

        /// <summary>
        /// Keeps Y.
        /// </summary>
        public int Y;

        /// <summary>
        /// Returns true if the given tile index represents the same coordinates as this one.
        /// </summary>
        public bool Equals(TileIndex other)
        {
            return X == other.X && Y == other.Y;
        }

        /// <summary>
        /// Returns true if the given tile index represents the same coordinates as this one.
        /// </summary>
        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj)) return false;
            return obj is TileIndex && Equals((TileIndex)obj);
        }

        /// <summary>
        /// Serves as a hash function .
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            unchecked
            {
                return (X * 397) ^ Y;
            }
        }

        /// <summary>
        /// Returns true if the given tile index represents the same coordinates as this one.
        /// </summary>
        public static bool operator ==(TileIndex t1, TileIndex t2)
        {
            return t1.Equals(t2);
        }

        /// <summary>
        /// Returns false if the given tile index represents the same coordinates as this one.
        /// </summary>
        public static bool operator !=(TileIndex t1, TileIndex t2)
        {
            return !t1.Equals(t2);
        }

        /// <summary>
        /// Converts lat/lon to tile coordinates.
        /// </summary>
        public static TileIndex WorldToTileIndex(double latitude, double longitude, int zoom)
        {
            var n = (int)Math.Floor(Math.Pow(2, zoom));

            var rad = (latitude / 180d) * System.Math.PI;

            var x = (int)((longitude + 180.0f) / 360.0f * n);
            var y = (int)(
                (1.0f - Math.Log(Math.Tan(rad) + 1.0f / Math.Cos(rad))
                / Math.PI) / 2f * n);

            return new TileIndex { X = x, Y = y };
        }

        /// <summary>
        /// Converts tile coordinates to lat/lon.
        /// </summary>
        public static Coordinate TileIndexToWorld(double tileX, double tileY, int zoom)
        {
            var n = Math.PI - 2.0 * Math.PI * tileY / Math.Pow(2.0, zoom);
            return new Coordinate(
                (float)(180.0 / Math.PI * Math.Atan(Math.Sinh(n))),
                (float)(tileX / Math.Pow(2.0, zoom) * 360.0 - 180.0));
        }
    }
}