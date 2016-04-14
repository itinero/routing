// Itinero - OpenStreetMap (OSM) SDK
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
                return _nodeBasedGraph.Serialize(stream) + 1;
            }
            else
            {
                stream.WriteByte(2);
                return _edgeBasedGraph.Serialize(stream) + 1;
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