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
        private SparseLongIndex _vertexFlags;
        private HashSet<ushort> _canTraverse;
        private Dictionary<ushort, uint> _islandSizes;

        /// <summary>
        /// Runs the island detection.
        /// </summary>
		protected override void DoRun()
        {
            _enumerator = _routerDb.Network.GeometricGraph.Graph.GetEdgeEnumerator();
            _vertexFlags = new SparseLongIndex();
            var vertexCount = _routerDb.Network.GeometricGraph.Graph.VertexCount;

            // precalculate all edge types for the given profiles.
            _canTraverse = new HashSet<ushort>();
            for (ushort p = 0; p < _routerDb.EdgeProfiles.Count; p++)
            {
                if (this.CanTraverse(p))
                {
                    _canTraverse.Add(p);
                }
            }

            // find vertices that are deadends but oneway and mark them as singleton islands.
            var island = (ushort)1;
            for (uint v = 0; v < vertexCount; v++)
            {
                if (_islands[v] == 0)
                {
                    var current = v;
                    var neighbour = IsOnewayDeadend(current);

                    if (neighbour != Constants.NO_VERTEX)
                    {
                        var nextIsland = island;
                        island++;

                        while (neighbour != Constants.NO_VERTEX)
                        {
                            _islands[current] = nextIsland;
                            _vertexFlags.Add(current);

                            current = neighbour;
                            neighbour = IsOnewayDeadend(current);
                        }
                    }
                }
            }

            uint lower = 0;
            while (true)
            {
                // find the first vertex without an island assignment.
                var vertex = uint.MaxValue;
                for (uint v = lower; v < vertexCount; v++)
                {
                    if (_islands[v] == 0)
                    {
                        lower = v;
                        vertex = v;
                        break;
                    }
                }

                if (vertex == uint.MaxValue)
                { // no more islands left.
                    break;
                }

                // expand island until no longer possible.
                var current = vertex;
                _islands[vertex] = island;
                _vertexFlags.Add(vertex);
                vertex = this.Expand(vertex, island);

                if (vertex == uint.MaxValue)
                { // expanding failed, still just the source vertex, this is an island of one.
                    _islands[current] = SINGLETON_ISLAND;
                }
                else
                {
                    while (vertex != uint.MaxValue)
                    {
                        _islands[vertex] = island;
                        _vertexFlags.Add(vertex);

                        vertex = this.Expand(vertex, island);

                        if (vertex < current)
                        {
                            current = vertex;
                        }

                        if (vertex == uint.MaxValue)
                        {
                            while (current < vertexCount)
                            {
                                if (_islands[current] == island &&
                                   !_vertexFlags.Contains(current))
                                { // part of island but has not been used to expand yet.
                                    vertex = current;
                                    break;
                                }
                                current++;
                            }
                        }
                    }

                    // island was no singleton, move to next island.
                    island++;
                }
            }

            // calculate island sizes.
            uint count = 1, size = 0;
            ushort lastIsland = _islands[0];
            for (var v = 1; v < _islands.Length; v++)
            {
                var currentIsland = _islands[v];
                if (currentIsland == SINGLETON_ISLAND)
                {
                    continue;
                }

                if (currentIsland != lastIsland)
                {
                    if (!_islandSizes.TryGetValue(lastIsland, out size))
                    {
                        size = 0;
                    }
                    size += count;
                    if (lastIsland != SINGLETON_ISLAND)
                    {
                        _islandSizes[lastIsland] = size;
                    }

                    lastIsland = currentIsland;
                    count = 1;
                }
                else
                {
                    count++;
                }
            }
            if (!_islandSizes.TryGetValue(lastIsland, out size))
            {
                size = 0;
            }
            size += count;
            if (lastIsland != SINGLETON_ISLAND)
            {
                _islandSizes[lastIsland] = size;
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
        /// Expands an island starting the given vertex.
        /// </summary>
        private uint Expand(uint vertex, ushort island)
        {
            var min = uint.MaxValue;
            _enumerator.MoveTo(vertex);

            while(_enumerator.MoveNext())
            {
                var neighbour = _enumerator.To;
                if (_vertexFlags.Contains(neighbour))
                {
                    continue;
                }

                float distance;
                ushort edgeProfile;
                EdgeDataSerializer.Deserialize(_enumerator.Data0, out distance, out edgeProfile);

                if (!_canTraverse.Contains(edgeProfile))
                {
                    continue;
                }

                _islands[neighbour] = island; // set the island.

                if (neighbour < min)
                {
                    min = neighbour;
                }
            }

            return min;
        }

        /// <summary>
        /// Returns true if the edge profile can be traversed by any of the profiles.
        /// </summary>
        private bool CanTraverse(ushort edgeProfile)
        {
            for(var p = 0; p < _profiles.Length; p++)
            {
                var f = _profiles[p](edgeProfile);
                if (f.Value != 0)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns true if the edge profile can be traversed by any of the profiles and at least on of the profiles notifies as oneway.
        /// </summary>
        private bool IsOneway(ushort edgeProfile)
        {
            for (var p = 0; p < _profiles.Length; p++)
            {
                var f = _profiles[p](edgeProfile);
                if (f.Value != 0 &&
                    f.Direction != 0)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns the only neighbour if this vertex is a oneway deadend, ignoring neighbours marked as singletons. 
        /// </summary>
        private uint IsOnewayDeadend(uint v)
        {
            _enumerator.MoveTo(v);

            uint onewayDeadend = Constants.NO_VERTEX;
            while (_enumerator.MoveNext())
            {
                if (_islands[_enumerator.Current.To] != 0)
                { // the other vertex is a singleton, ignore.
                    continue;
                }
                else
                {
                    float distance;
                    ushort edgeProfile;
                    EdgeDataSerializer.Deserialize(_enumerator.Data0, out distance, out edgeProfile);

                    if (this.CanTraverse(edgeProfile))
                    {
                        if (!this.IsOneway(edgeProfile))
                        { // bidirectional neighbour, never a oneway deadend, skip this one.
                            return Constants.NO_VERTEX;
                        }

                        // neighbour found, and it's oneway.
                        if (onewayDeadend != Constants.NO_VERTEX)
                        { // two oneway neighbours found, skip this one.
                            return Constants.NO_VERTEX;
                        }

                        // mark as oneway deadend, up until now there is one oneway neighbour.
                        onewayDeadend = _enumerator.Current.To;
                    }
                }
            }

            return onewayDeadend;
        }
    }
}

