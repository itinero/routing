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

using System;
using System.Collections.Generic;

namespace Itinero.IO.Osm.Streams
{
    /// <summary>
    /// A datastructure to map nodes to their vertex-equivalents.
    /// </summary>
    /// <remarks>
    /// Why not use a regular dictionary?
    /// - Size limitations of one object.
    /// - We can have in some exceptional circumstances have two vertices for one original node.
    /// </remarks>
    public class CoreNodeIdMap
    {
        private readonly HugeDictionary<long, uint> _firstMap; // holds the first vertex for a node, will be enough in 99.9% of cases.
        private readonly HugeDictionary<long, LinkedListNode> _secondMap; // holds the second and beyond vertices for a node.

        /// <summary>
        /// Creates a new core node id map.
        /// </summary>
        public CoreNodeIdMap()
        {
            _firstMap = new HugeDictionary<long, uint>();
            _secondMap = new HugeDictionary<long, LinkedListNode>();
        }

        /// <summary>
        /// Adds a pair.
        /// </summary>
        public void Add(long nodeId, uint vertex)
        {
            if (!_firstMap.ContainsKey(nodeId))
            {
                _firstMap.Add(nodeId, vertex);
                return;
            }
            LinkedListNode existing;
            if (!_secondMap.TryGetValue(nodeId, out existing))
            {
                _secondMap.Add(nodeId, new LinkedListNode()
                {
                    Value = vertex
                });
            }
            _secondMap[nodeId] = new LinkedListNode()
            {
                Value = vertex,
                Next = existing
            };
        }

        /// <summary>
        /// Fills the given array with the vertices for the given node.
        /// </summary>
        public int Get(long nodeId, ref uint[] vertices)
        {
            if (vertices == null || vertices.Length == 0) { throw new ArgumentException("Target array needs to be non-null and have a size > 0."); }
            uint first;
            if (!_firstMap.TryGetValue(nodeId, out first))
            {
                return 0;
            }
            vertices[0] = first;

            LinkedListNode node;
            if (!_secondMap.TryGetValue(nodeId, out node))
            {
                return 1;
            }
            var i = 1;
            while (i < vertices.Length && node != null)
            {
                vertices[i] = node.Value;
                node = node.Next;
                i++;
            }
            return i;
        }

        /// <summary>
        /// Tries to get the first vertex that was added for this node.
        /// </summary>
        public bool TryGetFirst(long nodeId, out uint vertex)
        {
            return _firstMap.TryGetValue(nodeId, out vertex);
        }

        /// <summary>
        /// Calculates the maximum vertices per node in this map.
        /// </summary>
        /// <returns></returns>
        public int MaxVerticePerNode()
        {
            if (_firstMap.Count == 0)
            {
                return 0;
            }

            var max = 1;
            foreach (var keyValue in _secondMap)
            {
                var c = 1;
                var node = keyValue.Value.Next;
                while (node != null)
                {
                    c++;
                    node = node.Next;
                }
                if (c + 1 > max)
                {
                    max = c + 1;
                }
            }
            return max;
        }

        /// <summary>
        /// An enumerable with all nodes in this map.
        /// </summary>
        public IEnumerable<long> Nodes
        {
            get
            {
                return _firstMap.Keys;
            }
        }
		
        private class LinkedListNode
        {
            public uint Value { get; set; }
            public LinkedListNode Next { get; set; }
        }
    }
}