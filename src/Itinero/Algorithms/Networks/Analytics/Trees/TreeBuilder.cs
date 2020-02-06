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

using System.Collections.Generic;
using System.Threading;
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
        /// <param name="graph">The graph.</param>
        /// <param name="edgeVisitor">The algorithm that visits the edges.</param>
        public TreeBuilder(GeometricGraph graph, IEdgeVisitor<float> edgeVisitor)
        {
            _graph = graph;
            _edgeVisitor = edgeVisitor;
        }

        private HashSet<long> _edges;
        private List<TreeEdge> _treeEdges;
        private float _max = 0;
        private Tree _tree;

        /// <summary>
        /// Executes the actual algorithm.
        /// </summary>
        protected override void DoRun(CancellationToken cancellationToken)
        {
            _edges = new HashSet<long>();
            _treeEdges = new List<TreeEdge>();

            bool HandleVisit(EdgePath<float> path)
            {
                var e = path.Edge;
                var weight2 = path.Weight;
                if (e == Constants.NO_EDGE)
                {
                    return false;
                }

                if (_edges.Contains(e))
                {
                    return false;
                }
                _edges.Add(e);
                
                var edgeId = new DirectedEdgeId(e);
                var previousEdgeId = DirectedEdgeId.NO_EDGE;
                var weight1 = 0f;
                if (path.From != null)
                {
                    weight1 = path.From.Weight;
                    if (path.From.Edge != Constants.NO_EDGE)
                    {
                        previousEdgeId = new DirectedEdgeId(path.From.Edge);
                    }
                }
                
                if (_max < weight1)
                {
                    _max = weight1;
                }
                
                var edge = _graph.GetEdge(edgeId.EdgeId);
                var shape = _graph.GetShape(edge);
                if (!edgeId.Forward)
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
                    EdgeId = edgeId.EdgeId,
                    EdgeDirection =  edgeId.Forward,
                    PreviousEdgeId = previousEdgeId.EdgeId,
                    PreviousEdgeDirection = previousEdgeId.Forward,
                    Shape = shapeArray,
                    Weight1 = weight1,
                    Weight2 = weight2
                };
                _treeEdges.Add(treeEdge);

                return false;
            }

            _edgeVisitor.Visit+= (path) =>
            {
                if (path?.From?.From != null) return false;
                return HandleVisit(path);
            };
            _edgeVisitor.VisitNeighbour += HandleVisit;
            _edgeVisitor.Run(cancellationToken);

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