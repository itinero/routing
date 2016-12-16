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

using Reminiscence.Arrays;

namespace Itinero.Data.Network.Restrictions
{
    /// <summary>
    /// A profile with settings for a memory-mapped restrictions db.
    /// </summary>
    public class RestrictionsDbProfile
    {
        /// <summary>
        /// Gets or sets the hashes profile.
        /// </summary>
        public ArrayProfile HashesProfile { get; set; }

        /// <summary>
        /// Gets or sets the index profile.
        /// </summary>
        public ArrayProfile IndexProfile { get; set; }

        /// <summary>
        /// Gets or sets the restrictions profile.
        /// </summary>
        public ArrayProfile RestrictionsProfile { get; set; }

        /// <summary>
        /// A profile that tells the graph to use no caching.
        /// </summary>
        public static RestrictionsDbProfile NoCache = new RestrictionsDbProfile()
        {
            HashesProfile = ArrayProfile.NoCache,
            IndexProfile = ArrayProfile.NoCache,
            RestrictionsProfile = ArrayProfile.NoCache
        };
    }
}