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

namespace Itinero.IO.Osm.Streams
{
    /// <summary>
    /// A cache for node coordinates.
    /// </summary>
    public sealed class NodeIndex
    {
        private readonly UnsignedNodeIndex _negativeNodeIndex;
        private readonly UnsignedNodeIndex _postiveNodeIndex;

        public NodeIndex()
        {
            _negativeNodeIndex = new UnsignedNodeIndex();
            _postiveNodeIndex = new UnsignedNodeIndex();
        }

        /// <summary>
        /// Adds a node id to the index.
        /// </summary>
        public void AddId(long id)
        {
            if (id >= 0)
            {
                _postiveNodeIndex.AddId(id);
            }
            else
            {
                _negativeNodeIndex.AddId(-id);
            }
        }

        /// <summary>
        /// Sorts and converts the index.
        /// </summary>
        public void SortAndConvertIndex()
        {
            _postiveNodeIndex.SortAndConvertIndex();
            _negativeNodeIndex.SortAndConvertIndex();
        }

        /// <summary>
        /// Gets the node id at the given index.
        /// </summary>
        public long this[long idx]
        {
            get
            {
                if (idx >= _negativeNodeIndex.Count)
                {
                    return _postiveNodeIndex[idx - _negativeNodeIndex.Count];
                }
                return _negativeNodeIndex[idx];
            }
        }

        /// <summary>
        /// Sets a vertex id for the given vertex.
        /// </summary>
        public void Set(long id, uint vertex)
        {
            if (id >= 0)
            {
                _postiveNodeIndex.Set(id, vertex);
            }
            else
            {
                _negativeNodeIndex.Set(-id, vertex);
            }
        }

        /// <summary>
        /// Gets the coordinate for the given node.
        /// </summary>
        public long TryGetIndex(long id)
        {
            if (id >= 0)
            {
                return _postiveNodeIndex.TryGetIndex(id);
            }
            else
            {
                var result = _negativeNodeIndex.TryGetIndex(-id);
                if (result == long.MaxValue)
                {
                    return long.MaxValue;
                }
                return -(result + 1);
            }
        }

        /// <summary>
        /// Sets the coordinate for the given index.
        /// </summary>
        public void SetIndex(long idx, float latitude, float longitude)
        {
            if (idx >= 0)
            {
                _postiveNodeIndex.SetIndex(idx, latitude, longitude);
            }
            else
            {
                idx = -idx - 1;
                _negativeNodeIndex.SetIndex(idx, latitude, longitude);
            }
        }
        /// <summary>
        /// Tries to get a core node and it's matching vertex.
        /// </summary>
        public bool TryGetCoreNode(long id, out uint vertex)
        {
            if (id >= 0)
            {
                return _postiveNodeIndex.TryGetCoreNode(id, out vertex);
            }
            else
            {
                return _negativeNodeIndex.TryGetCoreNode(-id, out vertex);
            }
        }
        
        /// <summary>
        /// Gets all relevant info on the given node.
        /// </summary>
        public bool TryGetValue(long id, out float latitude, out float longitude, out bool isCore, out uint vertex)
        {
            if (id >= 0)
            {
                return _postiveNodeIndex.TryGetValue(id, out latitude, out longitude, out isCore, out vertex);
            }
            else
            {
                return _negativeNodeIndex.TryGetValue(-id, out latitude, out longitude, out isCore, out vertex);
            }
        }
    }
}
