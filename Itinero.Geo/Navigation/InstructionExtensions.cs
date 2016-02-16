using GeoAPI.Geometries;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using Itinero.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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
                attributes.AddAttribute("text", instruction.Text);
                attributes.AddAttribute("type", instruction.Type.ToInvariantString().ToLowerInvariant());

                var point = new Point(coordinate);

                features.Add(new Feature(point, attributes));
            }
            return features;
        }
    }
}