// Itinero - Routing for .NET
// Copyright (C) 2015 Abelshausen Ben
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

using Itinero.Algorithms;
using Itinero.Algorithms.Weights;
using Itinero.Data.Edges;
using Itinero.Profiles;
using System;
using System.Collections.Generic;

namespace Itinero.Graphs
{
    /// <summary>
    /// Contains extension methods for the graph.
    /// </summary>
    public static class GraphExtensions
    {
        /// <summary>
        /// Gets the vertex on this edge that is not the given vertex.
        /// </summary>
        /// <returns></returns>
        public static uint GetOther(this Edge edge, uint vertex)
        {
            if (edge.From == vertex)
            {
                return edge.To;
            }
            else if (edge.To == vertex)
            {
                return edge.From;
            }
            throw new ArgumentOutOfRangeException(string.Format("Vertex {0} is not part of edge {1}.",
                vertex, edge.Id));
        }

        /// <summary>
        /// Finds the best edge between the two given vertices.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="graph"></param>
        /// <param name="weightHandler"></param>
        /// <param name="vertex1"></param>
        /// <param name="vertex2"></param>
        /// <returns></returns>
        public static long FindBestEdge<T>(this Graph.EdgeEnumerator edgeEnumerator, WeightHandler<T> weightHandler, uint vertex1, uint vertex2, out T bestWeight)
            where T : struct
        {
            edgeEnumerator.MoveTo(vertex1);
            bestWeight = weightHandler.Infinite;
            long bestEdge = Constants.NO_EDGE;
            Factor factor;
            while (edgeEnumerator.MoveNext())
            {
                if (edgeEnumerator.To == vertex2)
                {
                    float distance;
                    ushort edgeProfile;
                    EdgeDataSerializer.Deserialize(edgeEnumerator.Data0, out distance, out edgeProfile);
                    var weight = weightHandler.Calculate(edgeProfile, distance, out factor);

                    if (factor.Value > 0 && (factor.Direction == 0 ||
                        ((factor.Direction == 1) && !edgeEnumerator.DataInverted) ||
                        ((factor.Direction == 2) && edgeEnumerator.DataInverted)))
                    { // it's ok; the edge can be traversed by the given vehicle.
                        if (weightHandler.IsSmallerThan(weight, bestWeight))
                        {
                            bestWeight = weight;
                            bestEdge = edgeEnumerator.IdDirected();
                        }
                    }
                }
            }
            return bestEdge;
        }
    }
}