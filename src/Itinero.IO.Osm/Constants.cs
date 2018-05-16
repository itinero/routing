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

namespace Itinero.IO.Osm
{
    /// <summary>
    /// Holds constants.
    /// </summary>
    public static class Constants
    {
        /// <summary>
        /// Holds the name of vertex meta data collection keeping node id's.
        /// </summary>
        public static string NODE_ID_META_NAME = "node_id";
        
        /// <summary>
        /// Holds the name of edge meta data collection keeping way id's.
        /// </summary>
        public static string WAY_ID_META_NAME = "way_id";
        
        /// <summary>
        /// Holds the name of edge meta data collection keeping starting node index in way's.
        /// </summary>
        public static string WAY_NODE_IDX_META_NAME = "way_node_idx";
    }
}