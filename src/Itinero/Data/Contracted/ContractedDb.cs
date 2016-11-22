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
        private readonly bool _edgeBased;
        private readonly DirectedDynamicGraph _graph;
        
        /// <summary>
        /// Creates a new edge-based contracted db.
        /// </summary>
        public ContractedDb(DirectedDynamicGraph graph, bool edgeBased = false)
        {
            if (graph == null) { throw new ArgumentNullException("graph"); }

            _edgeBased = edgeBased;
            _graph = graph;
        }

        /// <summary>
        /// Returns true if this contracted db is node-based.
        /// </summary>
        public bool HasNodeBasedGraph
        {
            get
            {
                return !_edgeBased;
            }
        }

        /// <summary>
        /// Gets the node-based graph if any.
        /// </summary>
        public DirectedDynamicGraph NodeBasedGraph
        {
            get
            {
                if (_edgeBased)
                {
                    throw new InvalidOperationException();
                }
                return _graph;
            }
        }

        /// <summary>
        /// Returns true if this contracted db is edge-based.
        /// </summary>
        public bool HasEdgeBasedGraph
        {
            get
            {
                return _edgeBased;
            }
        }

        /// <summary>
        /// Gets the edge-based graph if any.
        /// </summary>
        public DirectedDynamicGraph EdgeBasedGraph
        {
            get
            {
                if (!_edgeBased)
                {
                    throw new InvalidOperationException();
                }
                return _graph;
            }
        }

        /// <summary>
        /// Serializes the given contraction data 
        /// </summary>
        public long Serialize(Stream stream, bool toReadonly)
        {
            // write version # first:
            // 1: means regular non-edge contracted data.
            //     WARNING: cannot be read anymore by this Itinero version a breaking change.
            //     TODO:    convert the data on-the-fly to the new format.
            // 2: means regular edge contracted data.
            // 3: means node-based dynamic contracted data.

            if (!_edgeBased)
            {
                stream.WriteByte(3);
                return _graph.Serialize(stream, toReadonly) + 1;
            }
            else
            {
                stream.WriteByte(2);
                return _graph.Serialize(stream, toReadonly) + 1;
            }
        }

        /// <summary>
        /// Deserializes contraction data from the given stream.
        /// </summary>
        public static ContractedDb Deserialize(Stream stream, ContractedDbProfile profile)
        {
            // read version # first:
            // 1: means regular non-edge contracted data.
            //     WARNING: cannot be read anymore by this Itinero version a breaking change.
            //     TODO:    convert the data on-the-fly to the new format.
            // 2: means regular edge contracted data.
            // 3: means node-based dynamic contracted data.

            var version = stream.ReadByte();
            if (version == 1)
            {
                throw new Exception(string.Format("Cannot deserialize contracted graph, consider updating Intinero: Invalid version #: {0}.", version));
            }
            else if (version == 2)
            {
                return new ContractedDb(DirectedDynamicGraph.Deserialize(stream, profile == null ? null : profile.EdgeBasedProfile), true);
            }
            else if (version == 3)
            {
                return new ContractedDb(DirectedDynamicGraph.Deserialize(stream, profile == null ? null : profile.EdgeBasedProfile), false);
            }
            else
            {
                throw new Exception(string.Format("Cannot deserialize contracted graph, consider updating Intinero: Invalid version #: {0}.", version));
            }
        }
    }
}