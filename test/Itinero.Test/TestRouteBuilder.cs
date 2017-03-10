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

using System.IO;

namespace Itinero.Test
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