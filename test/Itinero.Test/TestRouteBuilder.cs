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

using System;
using System.IO;
using Itinero.Attributes;
using Itinero.LocalGeo;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

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
        internal static Route BuildRoute(string json)
        {
//            var serializer = new Newtonsoft.Json.JsonSerializer();
//            serializer.Converters.Add(new CustomJsonConverter());
//            return serializer.Deserialize(new StringReader(json), typeof(Route)) as Route;

            dynamic dRoute = JsonConvert.DeserializeObject(json);
            var route = new Route();
            foreach (var token in dRoute)
            {
                switch (token.Name)
                {
                    case "Shape":
                        route.Shape = ReadShape(token);
                        break;
                    case "ShapeMeta":
                        route.ShapeMeta = ReadShapeMeta(token);
                        break;
                    case "Stops":
                        route.Stops = ReadStops(token);
                        break;
                    case "Branches":
                        route.Branches = ReadBranches(token);
                        break;
                    case "Attributes":
                        route.Attributes = ReadAttributes(token);
                        break;
                }
            }
            return route;
        }

        private static Coordinate[] ReadShape(dynamic token)
        {
            var dArray = token.First;
            var shape = new Coordinate[dArray.Count];
            for (var i = 0; i < dArray.Count; i++)
            {
                var dItem = dArray[i];
                shape[i] = new Coordinate((float)dItem[1].Value, (float)dItem[0].Value);
            }

            return shape;
        }

        private static Route.Meta[] ReadShapeMeta(dynamic token)
        {
            var dArray = token.First;
            var shapeMetas = new Route.Meta[dArray.Count];
            for (var i = 0; i < dArray.Count; i++)
            {
                var dItem = dArray[i];
                shapeMetas[i] = new Route.Meta();
                foreach (var token1 in dItem)
                {
                    switch (token1.Name)
                    {
                        case "Shape":
                            shapeMetas[i].Shape = token1.Value;
                            break;
                        case "Attributes":
                            shapeMetas[i].Attributes = ReadAttributes(token1);
                            break;
                    }
                }
            }

            return shapeMetas;
        }

        private static Route.Stop[] ReadStops(dynamic token)
        {
            var dArray = token.First;
            var stops = new Route.Stop[dArray.Count];
            for (var i = 0; i < dArray.Count; i++)
            {
                var dItem = dArray[i];
                stops[i] = new Route.Stop();
                foreach (var token1 in dItem)
                {
                    switch (token1.Name)
                    {
                        case "Shape":
                            stops[i].Shape = token1.Value;
                            break;
                        case "Attributes":
                            stops[i].Attributes = ReadAttributes(token1);
                            break;
                        case "Coordinates":
                            stops[i].Coordinate = ReadCoordinate(token1);
                            break;
                    }
                }
            }

            return stops;
        }

        private static Route.Branch[] ReadBranches(dynamic token)
        {
            var dArray = token.First;
            var branches = new Route.Branch[dArray.Count];
            for (var i = 0; i < dArray.Count; i++)
            {
                var dItem = dArray[i];
                branches[i] = new Route.Branch();
                foreach (var token1 in dItem)
                {
                    switch (token1.Name)
                    {
                        case "Shape":
                            branches[i].Shape = token1.Value;
                            break;
                        case "Attributes":
                            branches[i].Attributes = ReadAttributes(token1);
                            break;
                        case "Coordinates":
                            branches[i].Coordinate = ReadCoordinate(token1);
                            break;
                    }
                }
            }

            return branches;
        }

        private static Coordinate ReadCoordinate(dynamic token)
        {
            var dCoordinate = token.First;
            return new Coordinate((float) dCoordinate[1].Value, (float) dCoordinate[0].Value);
        }

        private static IAttributeCollection ReadAttributes(dynamic token)
        {
            var attributeCollection = new AttributeCollection();
            foreach (var aAttribute in token)
            {
                var aAttributeToken = aAttribute as Newtonsoft.Json.Linq.JToken;
                if (!(aAttributeToken?.First is JProperty aAttributeProperty))
                {
                    continue;
                }
                
                var name = aAttributeProperty.Name;
                var value = string.Empty;
                if (aAttributeProperty.Value != null)
                {
                    value = aAttributeProperty.Value.ToString();
                }
                attributeCollection.AddOrReplace(name, value);
            }
            return attributeCollection;
        }

//        private class CustomJsonConverter : JsonConverter
//        {
//            /// <summary>
//            /// Determines whether this instance can convert the specified object type.
//            /// </summary>
//            public override bool CanConvert(Type objectType)
//            {
//                return objectType.IsSubclassOf(typeof(IAttributeCollection));
//            }
//
//            /// <summary>
//            /// Reads the JSON representation of the object.
//            /// </summary>
//            public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
//            {
//                return new AttributeCollection();
//            }
//
//            /// <summary>
//            /// Writes the JSON representation of the object.
//            /// </summary>
//            public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
//            {
//                throw new NotSupportedException("Write JSON using the Itinero native writer.");
//            }
//        }
    }
}