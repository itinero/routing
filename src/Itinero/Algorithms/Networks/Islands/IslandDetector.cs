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

using System;
using Itinero.Profiles;
using Itinero.Graphs;
using Itinero.Algorithms.Collections;
using Itinero.Data.Edges;
using System.Collections.Generic;
using Itinero.Algorithms.PriorityQueues;
using Reminiscence.Arrays;
using System.Threading;

namespace Itinero.Algorithms.Networks
{
	/// <summary>
	/// An island detector based on a set of profiles.
	/// </summary>
	public class IslandDetector : AlgorithmBase
	{
		private readonly Func<ushort, Factor>[] _profiles;
        private readonly ushort[] _islands; // holds the island # per vertex.
        private readonly RouterDb _routerDb;
        private const uint NO_DATA = uint.MaxValue;
        private const ushort NO_ISLAND = ushort.MaxValue - 1;

        /// <summary>
        /// A value representing a restricted vertex, could be part of multiple islands.
        /// </summary>
        public const ushort RESTRICTED = ushort.MaxValue - 2;

        private readonly Restrictions.RestrictionCollection _restrictionCollection;

        /// <summary>
        /// A value representing a singleton island.
        /// </summary>
        public const ushort SINGLETON_ISLAND = ushort.MaxValue;

        /// <summary>
        /// Creates a new island detector.
        /// </summary>
		public IslandDetector(RouterDb routerDb, Func<ushort, Factor>[] profiles, Restrictions.RestrictionCollection restrictionCollection = null)
		{
            _profiles = profiles;
            _routerDb = routerDb;
            _restrictionCollection = restrictionCollection;

            _islands = new ushort[_routerDb.Network.VertexCount];
            _islandSizes = new Dictionary<ushort, uint>();
		}
        
        private Dictionary<ushort, uint> _islandSizes;

        private ArrayBase<uint> _index;
        private Collections.Stack<uint> _stack;
        private SparseLongIndex _onStack;

        private uint _nextIndex = 0;
        private ushort _nextIsland = 0;

        /// <summary>
        /// Runs the island detection.
        /// </summary>
		protected override void DoRun(CancellationToken cancellationToken)
        {
            _onStack = new SparseLongIndex();
            var vertexCount = _routerDb.Network.GeometricGraph.Graph.VertexCount;

            // initialize all islands to NO_ISLAND.
            for (uint i = 0; i < _islands.Length; i++)
            {
                _islands[i] = NO_ISLAND;

                if (_restrictionCollection != null)
                {
                    _restrictionCollection.Update(i);

                    for (var r = 0; r < _restrictionCollection.Count; r++)
                    {
                        var restriction = _restrictionCollection[r];

                        if (restriction.Vertex2 == Constants.NO_VERTEX &&
                            restriction.Vertex3 == Constants.NO_VERTEX)
                        {
                            _islands[i] = RESTRICTED;
                            break;
                        }
                        else
                        {
                            // TODO: support other restrictions.
                        }
                    }
                }
            }

            // build index data structure and stack.
            _index = new MemoryArray<uint>(vertexCount * 2);
            for (var i = 0; i < _index.Length; i++)
            {
                _index[i] = NO_DATA;
            }
            _stack = new Collections.Stack<uint>();

            // https://en.wikipedia.org/wiki/Tarjan's_strongly_connected_components_algorithm
            for (uint v = 0; v < vertexCount; v++)
            {
                var vIndex = _index[v * 2];
                if (vIndex != NO_DATA)
                {
                    continue;
                }

                StrongConnect(v);
            }
            
            // sort islands.
            var sortedIslands = new List<KeyValuePair<ushort, uint>>(_islandSizes);
            sortedIslands.Sort((x, y) => -x.Value.CompareTo(y.Value));
            var newIds = new Dictionary<ushort, ushort>();
            for (ushort i = 0; i < sortedIslands.Count; i++)
            {
                newIds[sortedIslands[i].Key] = i;
            }
            for (var v = 0; v < _islands.Length; v++)
            {
                ushort newId;
                if (newIds.TryGetValue(_islands[v], out newId))
                {
                    _islands[v] = newId;
                }
            }
            _islandSizes.Clear();
            foreach (var sortedIsland in sortedIslands)
            {
                ushort newId;
                if (newIds.TryGetValue(sortedIsland.Key, out newId))
                {
                    _islandSizes[newId] = sortedIsland.Value;
                }
            }
        }

        private void StrongConnect(uint v)
        {
            var nextStack = new Collections.Stack<uint>();
            nextStack.Push(Constants.NO_VERTEX);
            nextStack.Push(v);

            while (nextStack.Count > 0)
            {
                v = nextStack.Pop();
                var parent = nextStack.Pop();

                if (_islands[v] != NO_ISLAND)
                {
                    continue;
                }
                
                // 2 options: 
                // OPTION 1: vertex was already processed, check if it's a root vertex.
                if (_index[v * 2 + 0] != NO_DATA)
                { // vertex was already processed, do wrap-up.
                    if (parent != Constants.NO_VERTEX)
                    {
                        var vLowLink = _index[v * 2 + 1];
                        if (vLowLink < _index[parent * 2 + 1])
                        {
                            _index[parent * 2 + 1] = vLowLink;
                        }
                    }

                    if (_index[v * 2 + 0] == _index[v * 2 + 1])
                    { // this was a root node so this is an island!
                      // pop from stack until root reached.
                        var island = _nextIsland;
                        _nextIsland++;

                        uint size = 0;
                        uint islandVertex = Constants.NO_VERTEX;
                        do
                        {
                            islandVertex = _stack.Pop();
                            _onStack.Remove(islandVertex);

                            size++;
                            _islands[islandVertex] = island;
                        } while (islandVertex != v);

                        if (size == 1)
                        { // only the root vertex, meaning this is a singleton.
                            _islands[v] = SINGLETON_ISLAND;
                            _nextIsland--; // reset island counter.
                        }
                        else
                        { // keep island size.
                            _islandSizes[island] = size;
                        }
                    }

                    continue;
                }

                // OPTION 2: vertex wasn't already processed, process it and queue it's neigbours.
                // push again to trigger OPTION1.
                nextStack.Push(parent);
                nextStack.Push(v);

                var enumerator = _routerDb.Network.GeometricGraph.Graph.GetEdgeEnumerator();
                enumerator.MoveTo(v);

                _index[v * 2 + 0] = _nextIndex;
                _index[v * 2 + 1] = _nextIndex;
                _nextIndex++;

                _stack.Push(v);
                _onStack.Add(v);

                if (enumerator.MoveTo(v))
                {
                    while (enumerator.MoveNext())
                    {
                        float distance;
                        ushort edgeProfile;
                        EdgeDataSerializer.Deserialize(enumerator.Data0, out distance, out edgeProfile);

                        var access = this.GetAccess(edgeProfile);

                        if (enumerator.DataInverted)
                        {
                            if (access == Access.OnewayBackward)
                            {
                                access = Access.OnewayForward;
                            }
                            else if (access == Access.OnewayForward)
                            {
                                access = Access.OnewayBackward;
                            }
                        }

                        if (access != Access.OnewayForward &&
                            access != Access.Bidirectional)
                        {
                            continue;
                        }

                        var n = enumerator.To;
                        
                        if (_islands[n] == RESTRICTED)
                        { // check if this neighbour is restricted, if so ignore.
                            continue;
                        }

                        var nIndex = _index[n * 2 + 0];
                        if (nIndex == NO_DATA)
                        { // queue parent and neighbour.
                            nextStack.Push(v);
                            nextStack.Push(n);
                        }
                        else if (_onStack.Contains(n))
                        {
                            if (nIndex < _index[v * 2 + 1])
                            {
                                _index[v * 2 + 1] = nIndex;
                            }
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Gets the islands.
        /// </summary>
        public ushort[] Islands
        {
            get
            {
                return _islands;
            }
        }

        /// <summary>
        /// Gets the island sizes.
        /// </summary>
        public Dictionary<ushort, uint> IslandSizes
        {
            get
            {
                return _islandSizes;
            }
        }
        
        /// <summary>
        /// Returns true if the edge profile can be traversed by any of the profiles and at least on of the profiles notifies as oneway.
        /// </summary>
        private Access GetAccess(ushort edgeProfile)
        {
            var access = Access.None;
            for (var p = 0; p < _profiles.Length; p++)
            {
                var f = _profiles[p](edgeProfile);
                if (f.Value != 0)
                {
                    if (f.Direction == 0)
                    {
                        if (access == Access.None)
                        {
                            access = Access.Bidirectional;
                        }
                    }
                    else if (f.Direction == 1)
                    {
                        if (access == Access.OnewayBackward)
                        {
                            return Access.None;
                        }
                        access = Access.OnewayForward;
                    }
                    else if (f.Direction == 2)
                    {
                        if (access == Access.OnewayForward)
                        {
                            return Access.None;
                        }
                        access = Access.OnewayBackward;
                    }
                }
            }
            return access;
        }

        private enum Access
        {
            None,
            OnewayForward,
            OnewayBackward,
            Bidirectional
        }
    }
}

