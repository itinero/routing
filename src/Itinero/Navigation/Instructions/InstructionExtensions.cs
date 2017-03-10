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

using Itinero.Attributes;
using System.Collections.Generic;

namespace Itinero.Navigation.Instructions
{
    /// <summary>
    /// Contains extension methods related to instructions.
    /// </summary>
    public static class InstructionExtensions
    {
        /// <summary>
        /// Adds instructions as attributes to route.
        /// </summary>
        public static void AddInstructions(this Route route, IEnumerable<Instruction> instructions)
        {
            var metas = new Dictionary<int, Route.Meta>();
            foreach (var meta in route.ShapeMeta)
            {
                metas.Add(meta.Shape, meta);
            }

            foreach (var instruction in instructions)
            {
                var shapeIndex = instruction.Shape;
                Route.Meta shapeMeta;
                if (!metas.TryGetValue(shapeIndex, out shapeMeta))
                {
                    shapeMeta = new Route.Meta()
                    {
                        Attributes = new AttributeCollection(),
                        Shape = shapeIndex
                    };
                    float distance, time;
                    route.DistanceAndTimeAt(shapeIndex, out distance, out time);
                    shapeMeta.Distance = distance;
                    shapeMeta.Time = time;

                    metas.Add(shapeIndex, shapeMeta);
                }

                shapeMeta.Attributes.AddOrReplace("instruction", instruction.Text);
                shapeMeta.Attributes.AddOrReplace("instruction_type", instruction.Type);
            }
        }
    }
}