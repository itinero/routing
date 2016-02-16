// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2015 Abelshausen Ben
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

using System.IO;

namespace OsmSharp.Routing.Test
{
    /// <summary>
    /// Builds tests routes from embedded data.
    /// </summary>
    public static class TestRouteBuilder
    {
        /// <summary>
        /// Builds a route.
        /// </summary>
        public static Route BuildRoute(Stream stream)
        {
            using (var streamReader = new StreamReader(stream))
            {
                return TestRouteBuilder.BuildRoute(streamReader.ReadToEnd());
            }
        }

        /// <summary>
        /// Builds a route.
        /// </summary>
        public static Route BuildRoute(string json)
        {
            throw new System.NotImplementedException();
            //var serializer = new OsmSharp.IO.Json.JsonSerializer();
            //return serializer.Deserialize(new StringReader(json), typeof(Route)) as Route;
        }
    }
}