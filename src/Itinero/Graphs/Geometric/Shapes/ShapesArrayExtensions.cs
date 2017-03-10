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
using System.Collections.Generic;

namespace Itinero.Graphs.Geometric.Shapes
{
    /// <summary>
    /// Contains extension methods for the shape index.
    /// </summary>
    public static class ShapesArrayExtensions
    {
        /// <summary>
        /// Adds a new shape.
        /// </summary>
        public static void Set(this ShapesArray index, long id, IEnumerable<Coordinate> shape)
        {
            index[id] = new ShapeEnumerable(shape);
        }

        /// <summary>
        /// Adds a new shape.
        /// </summary>
        public static void Set(this ShapesArray index, long id, params Coordinate[] shape)
        {
            index[id] = new ShapeEnumerable(shape);
        }
    }
}