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
        private const ushort NO_ISLAND = ushort.MaxValue;

        /// <summary>
        /// A value representing a singleton island.
        /// </summary>
        public const ushort SINGLETON_ISLAND = ushort.MaxValue;

        /// <summary>
        /// Creates a new island detector.
        /// </summary>
		public IslandDetector(RouterDb routerDb, Func<ushort, Factor>[] profiles)
		{
            _profiles = profiles;
            _routerDb = routerDb;

            _islands = new ushort[_routerDb.Network.VertexCount];
            _islandSizes = new Dictionary<ushort, uint>();
		}

        private Graph.EdgeEnumerator _enumerator;
        private Dictionary<ushort, uint> _islandSizes;

        private ArrayBase<uint> _index;
        private Collections.Stack<uint> _stack;
        private SparseLongIndex _onStack;

        private uint _nextIndex = 0;
        private ushort _nextIsland = 0;

        /// <summary>
        /// Runs the island detection.
        /// </summary>
		protected override void DoRun()
        {
            _enumerator = _routerDb.Network.GeometricGraph.Graph.GetEdgeEnumerator();
            _onStack = new SparseLongIndex();
            var vertexCount = _routerDb.Network.GeometricGraph.Graph.VertexCount;

            // initialize all islands to NO_ISLAND.
            for (var i = 0; i < _islands.Length; i++)
            {
                _islands[i] = NO_ISLAND;
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
        }

        private void StrongConnect(uint v)
        {
            _enumerator.MoveTo(v);

            _index[v * 2 + 0] = _nextIndex;
            _index[v * 2 + 1] = _nextIndex;
            _nextIndex++;

            _stack.Push(v);

            if (_enumerator.MoveTo(v))
            {
                while (_enumerator.MoveNext())
                {
                    float distance;
                    ushort edgeProfile;
                    EdgeDataSerializer.Deserialize(_enumerator.Data0, out distance, out edgeProfile);

                    var access = this.GetAccess(edgeProfile);

                    if (_enumerator.DataInverted)
                    {
                        if (access == Access.OnewayBackward)
                        {
                            access = Access.OnewayForward;
                        }
                        else if(access == Access.OnewayForward)
                        {
                            access = Access.OnewayForward;
                        }
                    }

                    if (access != Access.OnewayForward ||
                        access != Access.Bidirectional)
                    {
                        continue;
                    }

                    var n = _enumerator.To;
                    var nIndex = _index[n * 2 + 0];
                    if (nIndex == NO_DATA)
                    {
                        StrongConnect(v);
                        var nLowLink = _index[n * 2 + 1];
                        if (nLowLink < _index[v * 2 + 1])
                        {
                            _index[v * 2 + 1] = nLowLink; 
                        }
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

            if (_index[v * 2 + 0] == _index[v * 2 + 1])
            { // this was a root node so this is an island!
                // pop from stack until root reached.
                var island = _nextIsland;
                _nextIsland++;

                uint size = 0;
                var islandVertex = _stack.Pop();
                do
                {
                    size++;

                    _islands[islandVertex] = island;
                } while (islandVertex != v);
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

