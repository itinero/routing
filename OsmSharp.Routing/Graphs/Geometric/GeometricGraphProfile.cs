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

using Reminiscence.Arrays;

namespace OsmSharp.Routing.Graphs.Geometric
{
    /// <summary>
    /// A profile with settings for a memory-mapped geometric graph.
    /// </summary>
    public class GeometricGraphProfile
    {
        /// <summary>
        /// Gets or sets the graph profile.
        /// </summary>
        public GraphProfile GraphProfile { get; set; }

        /// <summary>
        /// Gets or sets the coordinates profile.
        /// </summary>
        public ArrayProfile CoordinatesProfile { get; set; }

        /// <summary>
        /// A profile that tells the graph to use no caching.
        /// </summary>
        public static GeometricGraphProfile NoCache = new GeometricGraphProfile()
        {
            CoordinatesProfile = ArrayProfile.NoCache,
            GraphProfile = GraphProfile.NoCache
        };

        /// <summary>
        /// A profile that tells the graph to prepare for sequential access.
        /// </summary>
        public static GeometricGraphProfile OneBuffer = new GeometricGraphProfile()
        {
            CoordinatesProfile = ArrayProfile.OneBuffer,
            GraphProfile = GraphProfile.OneBuffer
        };

        /// <summary>
        /// An profile that aggressively caches data with potenally 32Kb of cached data.
        /// </summary>
        public static GeometricGraphProfile Aggressive32 = new GeometricGraphProfile()
        {
            CoordinatesProfile = ArrayProfile.Aggressive8,
            GraphProfile = GraphProfile.Aggressive24
        };

        /// <summary>
        /// A default profile that use no caching for coordinates but aggressive for graph.
        /// </summary>
        public static GeometricGraphProfile Default = new GeometricGraphProfile()
        {
            CoordinatesProfile = ArrayProfile.NoCache,
            GraphProfile = GraphProfile.Aggressive24
        };
    }
}
