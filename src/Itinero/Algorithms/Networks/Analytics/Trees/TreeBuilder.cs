// Itinero - Routing for .NET
// Copyright (C) 2016 Paul Den Dulk, Abelshausen Ben
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

using Itinero.LocalGeo;
using System.Collections.Generic;
using Itinero.Algorithms.Networks.Analytics.Trees.Models;

namespace Itinero.Algorithms.Networks.Analytics.Trees
{
    /// <summary>
    /// A tree builder.
    /// </summary>
    public class TreeBuilder : AlgorithmBase
    {
        private readonly IEdgeVisitor _edgeVisitor;

        /// <summary>
        /// Creates a new tree builder.
        /// </summary>
        /// <param name="edgeVisitor">The algorithm that visits the edges.</param>
        public TreeBuilder(IEdgeVisitor edgeVisitor)
        {
            _edgeVisitor = edgeVisitor;
        }

        private HashSet<uint> _edges;
        private List<TreeEdge> _treeEdges;
        private float _max = 0;
        private Tree _tree;

        /// <summary>
        /// Executes the actual algorithm.
        /// </summary>
        protected override void DoRun()
        {
            _edges = new HashSet<uint>();
            _treeEdges = new List<TreeEdge>();
            _edgeVisitor.Visit += (directedEdgeId, startVertex, startWeight, endVertex, endWeight, shape) =>
            {
                uint edgeId;
                if (directedEdgeId > 0)
                {
                    edgeId = (uint)directedEdgeId - 1;
                }
                else
                {
                    edgeId = (uint)((-directedEdgeId) - 1);
                }

                if (!_edges.Contains(edgeId))
                {
                    _treeEdges.Add(new TreeEdge()
                    {
                        Weight1 = startWeight,
                        Vertex1 = startVertex,
                        Weight2 = endWeight,
                        Vertex2 = endVertex,
                        Shape = shape.ToLonLatArray()
                    });

                    if (_max < endWeight)
                    {
                        _max = endWeight;
                    }
                }
            };
            _edgeVisitor.Run();

            _tree = new Tree()
            {
                Edges = _treeEdges.ToArray(),
                Max = _max
            };

            this.HasSucceeded = true;
        }

        /// <summary>
        /// Gets all visited edges.
        /// </summary>
        public Tree Tree
        {
            get
            {
                return _tree;
            }
        }
    }
}