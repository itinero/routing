// Itinero - Routing for .NET
// Copyright (C) 2016 Abelshausen Ben
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