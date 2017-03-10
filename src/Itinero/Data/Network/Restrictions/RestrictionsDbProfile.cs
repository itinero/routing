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