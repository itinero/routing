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

using OsmSharp.Collections.Coordinates.Collections;
using OsmSharp.Collections.Tags.Index;
using OsmSharp.IO;
using OsmSharp.Math.Geo.Simple;
using OsmSharp.Routing.Graph;
using OsmSharp.Routing.Graph.Serialization;
using OsmSharp.Routing.Vehicles;
using ProtoBuf;
using ProtoBuf.Meta;
using System.Collections.Generic;

namespace OsmSharp.Routing.Graph.Serialization
{
    /// <summary>
    /// Serializes/deserializes a graph.
    /// </summary>
    public class RoutingDataSourceSerializer : RoutingDataSourceSerializerBase<Edge>
    {
        /// <summary>
        /// Returns the version string.
        /// </summary>
        public override string VersionString
        {
            get { return "Flatfile.v2.0"; }
        }

        /// <summary>
        /// Serializes the given graph and tags index to the given stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="graph"></param>
        protected override void DoSerialize(LimitedStream stream, RouterDataSource<Edge> graph)
        {
            graph.Serialize(stream, Edge.SizeUints, Edge.MapFromDelegate, Edge.MapToDelegate);
        }

        /// <summary>
        /// Deserializes the given stream into a routable graph.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="lazy"></param>
        /// <param name="vehicles"></param>
        /// <returns></returns>
        protected override RouterDataSource<Edge> DoDeserialize(LimitedStream stream, bool lazy, IEnumerable<string> vehicles)
        {
            var routerDataSource = RouterDataSource<Edge>.Deserialize(stream, Edge.SizeUints, Edge.MapFromDelegate, Edge.MapToDelegate, !lazy); 
            if (vehicles != null)
            {
                foreach (var vehicleName in vehicles)
                {
                    Vehicle vehicle;
                    if (!Vehicle.TryGetByUniqueName(vehicleName, out vehicle))
                    {
                        OsmSharp.Logging.Log.TraceEvent("CHEdgeSerializer", Logging.TraceEventType.Information,
                            string.Format("Vehicle profile {0} not found during serialization. It won't be supported by the router.", vehicleName));
                    }
                    routerDataSource.AddSupportedProfile(vehicle);
                }
            }
            return routerDataSource;
        }
    }
}
