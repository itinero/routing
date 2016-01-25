// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2016 Abelshausen Ben
// 
// This file is part of OsmSharp.
// 
// OsmSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// OsmSharp is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with OsmSharp. If not, see <http://www.gnu.org/licenses/>.

using OsmSharp.Collections.Tags;
using OsmSharp.Geo.Attributes;
using OsmSharp.Geo.Features;
using OsmSharp.Geo.Geometries;
using System.Collections.Generic;

namespace OsmSharp.Routing.Navigation
{
    /// <summary>
    /// Contains extension methods related to instructions.
    /// </summary>
    public static class InstructionExtensions
    {
        /// <summary>
        /// Converts the instructions to geojson.
        /// </summary>
        public static string ToGeoJson(this IList<Instruction> instructions, Route route)
        {
            return OsmSharp.Geo.Streams.GeoJson.GeoJsonConverter.ToGeoJson(
                instructions.ToFeatureCollection(route));
        }

        /// <summary>
        /// Converts to instructions to features.
        /// </summary>
        public static FeatureCollection ToFeatureCollection(this IList<Instruction> instructions, Route route)
        {
            var features = new FeatureCollection();
            for(var i = 0; i < instructions.Count; i++)
            {
                var instruction = instructions[i];
                var segment = route.Segments[instruction.Segment];
                
                features.Add(
                    new Feature(
                        new Point(
                            new Math.Geo.GeoCoordinate(segment.Latitude, segment.Longitude)), new SimpleGeometryAttributeCollection(
                            new Tag[] {
                                Tag.Create("text", instruction.Text),
                                Tag.Create("type", instruction.Type.ToInvariantString())
                            })));
            }
            return features;
        }
    }
}
