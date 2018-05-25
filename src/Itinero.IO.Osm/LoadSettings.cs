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

using Itinero.IO.Osm.Streams;
using System.Collections.Generic;

namespace Itinero.IO.Osm
{
    /// <summary>
    /// Contains a set of settings that the OSM data loader can use.
    /// </summary>
    public class LoadSettings
    {
        /// <summary>
        /// Creates a new settings object.
        /// </summary>
        public LoadSettings()
        {
            this.AllCore = false;
            this.ProcessRestrictions = true;
            this.Processors = null;
            this.NetworkSimplificationEpsilon = 1;
            this.OptimizeNetwork = false;
            this.KeepNodeIds = false;
            this.KeepWayIds = false;
        }

        /// <summary>
        /// Gets or sets the all core flag.
        /// </summary>
        /// <remarks>When true this will convert all nodes into a vertex.</remarks>
        public bool AllCore { get; set; }

        /// <summary>
        /// Gets or sets the process restrictions flag.
        /// </summary>
        public bool ProcessRestrictions { get; set; }

        /// <summary>
        /// Gets or sets a collection of extra processors.
        /// </summary>
        public IEnumerable<ITwoPassProcessor> Processors { get; set; }

        /// <summary>
        /// Gets or sets the network simplification epsilon (in meter). When zero no network simplifcation is done.
        /// </summary>
        public float NetworkSimplificationEpsilon { get; set; }

        /// <summary>
        /// Gets or sets the network optimization flag.
        /// </summary>
        public bool OptimizeNetwork { get; set; }

        /// <summary>
        /// Gets or sets the flag to keep node id's.
        /// </summary>
        public bool KeepNodeIds { get; set; }

        /// <summary>
        /// Gets or sets the flag to keep way id's.
        /// </summary>
        public bool KeepWayIds { get; set; }
    }
}