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

using System.Collections.Generic;
using Itinero.Algorithms.Networks.Analytics.Trees.Models;
using Itinero.Graphs.Geometric;

namespace Itinero.Algorithms.Networks.Analytics.Trees
{
    /// <summary>
    /// A tree builder.
    /// </summary>
    public class TreeBuilder : AlgorithmBase
    {
        private readonly GeometricGraph _graph;
        private readonly IEdgeVisitor<float> _edgeVisitor;

        /// <summary>
        /// Creates a new tree builder.
        /// </summary>
        /// <param name="edgeVisitor">The algorithm that visits the edges.</param>
        public TreeBuilder(GeometricGraph graph, IEdgeVisitor<float> edgeVisitor)
        {
            _graph = graph;
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

            _edgeVisitor.Visit += (path) =>
            {
                var e = path.Edge;
                var weight2 = path.Weight;
                if (e == Constants.NO_EDGE)
                {
                    return false;
                }

                var previousEdgeId = Constants.NO_EDGE;
                var weight1 = 0f;
                if (path.From != null)
                {
                    weight1 = path.From.Weight;
                    if (path.From.Edge > 0)
                    {
                        previousEdgeId = (uint)path.From.Edge - 1;
                    }
                    else
                    {
                        previousEdgeId = (uint)((-path.From.Edge) - 1);
                    }
                }
                
                uint edgeId;
                if (e > 0)
                {
                    edgeId = (uint)e - 1;
                }
                else
                {
                    edgeId = (uint)((-e) - 1);
                }
                var edge = _graph.GetEdge(edgeId);
                var shape = _graph.GetShape(edge);
                if (e < 0)
                {
                    shape.Reverse();
                }

                var shapeArray = new float[shape.Count][];
                for(var i = 0; i < shapeArray.Length; i++)
                {
                    shapeArray[i] = new float[2];
                    shapeArray[i][1] = shape[i].Latitude;
                    shapeArray[i][0] = shape[i].Longitude;
                }

                var treeEdge = new TreeEdge()
                {
                    EdgeId = edgeId,
                    PreviousEdgeId = previousEdgeId,
                    Shape = shapeArray,
                    Weight1 = weight1,
                    Weight2 = weight2
                };
                _treeEdges.Add(treeEdge);

                if (_max < weight2)
                {
                    _max = weight2;
                }

                return false;
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