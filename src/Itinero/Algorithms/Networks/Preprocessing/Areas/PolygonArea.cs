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

using System.Linq;
using Itinero.LocalGeo;

namespace Itinero.Algorithms.Networks.Preprocessing.Areas
{
    /// <summary>
    /// An area implemenation based on a simple polygon.
    /// </summary>
    public class PolygonArea : IArea
    {
        private readonly Polygon _polygon;
        private readonly Box _box;

        /// <summary>
        /// Creates a new polygon area.
        /// </summary>
        /// <param name="polygon">The polygon</param>
        public PolygonArea(Polygon polygon)
        {
            _polygon = polygon;

            _polygon.BoundingBox(out var north, out var east, out var south, out var west);
            _box = new Box(north, west, south, east);
        }

        /// <summary>
        /// Returns the location(s) the given line intersects with the area's boundary. Returns null if there is no intersection.
        /// </summary>
        public Coordinate[] Intersect(float latitude1, float longitude1, float latitude2, float longitude2)
        {
            var box = new Box(latitude1, longitude1, latitude2, longitude2);
            if (!box.Overlaps(_box))
            {
                return null;
            }

            // intersect with polyon.
            return _polygon.Intersect(latitude1, longitude1, latitude2, longitude2).ToArray();
        }

        /// <summary>
        /// Returns true if the given coordinate is inside the area.
        /// </summary>
        public bool Overlaps(float latitude, float longitude)
        {
            return _polygon.PointIn(new Coordinate(latitude, longitude));
        }
    }
}