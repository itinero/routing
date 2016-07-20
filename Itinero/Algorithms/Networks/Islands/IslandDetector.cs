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

using System;
using Itinero.Profiles;
using Itinero.Graphs;
using Itinero.Algorithms.Collections;
using Itinero.Data.Edges;

namespace Itinero.Algorithms.Networks
{
	/// <summary>
	/// An island detector based on a set of profiles.
	/// </summary>
	public class IslandDetector : AlgorithmBase
	{
		private readonly Func<ushort, Factor>[] _profiles;
        private readonly ushort[] _islands; // holds the island # per vertex.
        private readonly Graph _graph;

        /// <summary>
        /// Creates a new island detector.
        /// </summary>
		public IslandDetector(Graph graph, Func<ushort, Factor>[] profiles)
		{
            _profiles = profiles;
            _graph = graph;

            _islands = new ushort[_graph.VertexCount];
		}

        private Graph.EdgeEnumerator _enumerator;
        private SparseLongIndex _vertexFlags;

        /// <summary>
        /// Runs the island detection.
        /// </summary>
		protected override void DoRun()
		{
            _enumerator = _graph.GetEdgeEnumerator();
            _vertexFlags = new SparseLongIndex();

            var island = (ushort)1;
            while (true)
            {
                // find a vertex without an island assignment.
                var vertex = uint.MaxValue;
                for(uint v = 0; v < _graph.VertexCount; v++)
                {
                    if (_islands[v] == 0)
                    {
                        vertex = v;
                        break;
                    }
                }

                if (vertex == uint.MaxValue)
                { // no more islands left.
                    break;
                }
                
                // expand island until no longer possible.                
                while(vertex != uint.MaxValue)
                {
                    _islands[vertex] = island;
                    _vertexFlags.Add(vertex);

                    var previous = vertex;
                    vertex = this.Expand(vertex, island);

                    if (vertex == uint.MaxValue)
                    {
                        while(previous < _graph.VertexCount)
                        {
                            if (_islands[previous] == island &&
                                !_vertexFlags.Contains(previous))
                            { // part of island but has not been used to expand yet.
                                vertex = previous;
                                break;
                            }
                            previous++;
                        }
                    }
                }

                // move to the next island.
                island++;
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

                if (!CanTraverse(edgeProfile))
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
	}
}

