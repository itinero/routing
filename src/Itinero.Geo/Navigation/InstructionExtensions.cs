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

 using GeoAPI.Geometries;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using Itinero.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Itinero.Navigation.Instructions;

namespace Itinero.Geo.Navigation
{
    /// <summary>
    /// Contains extension methods related to instructions.
    /// </summary>
    public static class InstructionExtensions
    {
        /// <summary>
        /// Converts to instructions to features.
        /// </summary>
        public static FeatureCollection ToFeatureCollection(this IList<Instruction> instructions, Route route)
        {
            var features = new FeatureCollection();
            for (var i = 0; i < instructions.Count; i++)
            {
                var instruction = instructions[i];
                var coordinate = route.Shape[instruction.Shape].ToCoordinate();

                var attributes = new AttributesTable();
                attributes.Add("text", instruction.Text);
                attributes.Add("type", instruction.Type.ToInvariantString().ToLowerInvariant());

                var point = new Point(coordinate);

                features.Add(new Feature(point, attributes));
            }
            return features;
        }
    }
}