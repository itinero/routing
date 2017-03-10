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

using Itinero.Graphs.Directed;
using System;
using System.IO;

namespace Itinero.Data.Contracted
{
    /// <summary>
    /// Represents a contracted graph. 
    /// </summary>
    public class ContractedDb
    {
        private readonly DirectedMetaGraph _nodeBasedGraph;
        private readonly DirectedDynamicGraph _edgeBasedGraph;

        /// <summary>
        /// Creates a new node-based contracted db.
        /// </summary>
        public ContractedDb(DirectedMetaGraph nodeBasedGraph)
        {
            if (nodeBasedGraph == null) { throw new ArgumentNullException("nodeBasedGraph"); }

            _nodeBasedGraph = nodeBasedGraph;
            _edgeBasedGraph = null;
        }

        /// <summary>
        /// Creates a new edge-based contracted db.
        /// </summary>
        public ContractedDb(DirectedDynamicGraph edgeBasedGraph)
        {
            if (edgeBasedGraph == null) { throw new ArgumentNullException("edgeBasedGraph"); }

            _nodeBasedGraph = null;
            _edgeBasedGraph = edgeBasedGraph;
        }

        /// <summary>
        /// Returns true if this contracted db is node-based.
        /// </summary>
        public bool HasNodeBasedGraph
        {
            get
            {
                return _nodeBasedGraph != null;
            }
        }

        /// <summary>
        /// Gets the node-based graph if any.
        /// </summary>
        public DirectedMetaGraph NodeBasedGraph
        {
            get
            {
                if (_nodeBasedGraph == null)
                {
                    throw new InvalidOperationException();
                }
                return _nodeBasedGraph;
            }
        }

        /// <summary>
        /// Returns true if this contracted db is edge-based.
        /// </summary>
        public bool HasEdgeBasedGraph
        {
            get
            {
                return _edgeBasedGraph != null;
            }
        }

        /// <summary>
        /// Gets the edge-based graph if any.
        /// </summary>
        public DirectedDynamicGraph EdgeBasedGraph
        {
            get
            {
                if (_edgeBasedGraph == null)
                {
                    throw new InvalidOperationException();
                }
                return _edgeBasedGraph;
            }
        }
        /// <summary>
        /// Serializes the given contraction data 
        /// </summary>
        public long Serialize(Stream stream, bool toReadonly)
        {
            // write version # first:
            // 1: means regular non-edge contracted data.
            // 2: means regular edge contracted data.

            if (_nodeBasedGraph != null)
            {
                stream.WriteByte(1);
                return _nodeBasedGraph.Serialize(stream, toReadonly) + 1;
            }
            else
            {
                stream.WriteByte(2);
                return _edgeBasedGraph.Serialize(stream, toReadonly) + 1;
            }
        }

        /// <summary>
        /// Deserializes contraction data from the given stream.
        /// </summary>
        public static ContractedDb Deserialize(Stream stream, ContractedDbProfile profile)
        {
            // read version # first:
            // 1: means regular non-edge contracted data.
            // 2: means regular edge contracted data.

            var version = stream.ReadByte();
            if (version == 1)
            {
                return new ContractedDb(DirectedMetaGraph.Deserialize(stream, profile == null ? null : profile.NodeBasedProfile));
            }
            else if (version == 2)
            {
                return new ContractedDb(DirectedDynamicGraph.Deserialize(stream, profile == null ? null : profile.EdgeBasedProfile));
            }
            else
            {
                throw new Exception(string.Format("Cannot deserialize contracted graph: Invalid version #: {0}.", version));
            }
        }
    }
}