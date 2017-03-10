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
using System;

namespace Itinero.Algorithms.Tiles
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