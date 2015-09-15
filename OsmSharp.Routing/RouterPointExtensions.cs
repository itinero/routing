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

using OsmSharp.Routing.Algorithms.Routing;
using OsmSharp.Routing.Graphs.Geometric;
using OsmSharp.Routing.Profiles;

namespace OsmSharp.Routing
{
    /// <summary>
    /// Contains extension methods for the routerpoint.
    /// </summary>
    public static class RouterPointExtensions
    {
        /// <summary>
        /// Converts the router point to paths leading to the closest 2 vertices.
        /// </summary>
        /// <returns></returns>
        public static Path[] ToPaths(this RouterPoint point, RouterDb routerDb, Profile profile)
        {
            var graph = routerDb.Network.GeometricGraph;
            var edge = graph.GetEdge(point.EdgeId);
            if(point.Offset == 0)
            {
                return new Path[] { new Path(edge.From) };
            }
            else if(point.Offset == ushort.MaxValue)
            {
                return new Path[] { new Path(edge.To) };
            }
            var offset = point.Offset / (float)ushort.MaxValue;
            float distance;
            ushort profileId;
            OsmSharp.Routing.Data.EdgeDataSerializer.Deserialize(edge.Data[0], out distance, out profileId);
            var speed = profile.Speed(routerDb.Profiles.Get(profileId));
            var length = graph.Length(edge);
            return new Path[] {
                new Path(edge.From, (length * offset), new Path(uint.MaxValue)),
                new Path(edge.To, (length * (1 - offset)), new Path(uint.MaxValue))
            };
        }
    }
}
