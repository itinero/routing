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
using System;

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

        private Dictionary<uint, Tuple<float, float, List<Coordinate>>> _trees;

        /// <summary>
        /// Executes the actual algorithm.
        /// </summary>
        protected override void DoRun()
        {
            _trees = new Dictionary<uint, Tuple<float, float, List<Coordinate>>>();
            _edgeVisitor.Visit += (id, startWeight, endWeight, shape) =>
            {
                if (!_trees.ContainsKey(id))
                {
                    _trees[id] = new Tuple<float, float, List<Coordinate>>(
                        startWeight, endWeight, shape);
                }
            };
            _edgeVisitor.Run();

            this.HasSucceeded = true;
        }

        /// <summary>
        /// Gets all visited edges.
        /// </summary>
        public Dictionary<uint, Tuple<float, float, List<Coordinate>>> Tree
        {
            get
            {
                return _trees;
            }
        }
    }
}