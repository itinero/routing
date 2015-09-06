// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2015 Abelshausen Ben
// 
// This file is part of OsmSharp.
// 
// OsmSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// OsmSharp is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with OsmSharp. If not, see <http://www.gnu.org/licenses/>.

using OsmSharp.Collections.Sorting;
using OsmSharp.Math.Algorithms;
using OsmSharp.Routing.Graph;
using OsmSharp.Routing.Graph.Geometric;
using System;

namespace OsmSharp.Routing.Algorithms.Resolver
{
    /// <summary>
    /// Hilbert sorting.
    /// </summary>
    public static class Hilbert
    {
        /// <summary>
        /// Holds the default hilbert steps.
        /// </summary>
        public static int DefaultHilbertSteps = (int)System.Math.Pow(2, 15);

        /// <summary>
        /// Copies all data from the given graph.
        /// </summary>
        /// <typeparam name="TEdgeData"></typeparam>
        public static void Sort<TEdgeData>(this GeometricGraph<TEdgeData> graph)
            where TEdgeData : struct, IEdgeData
        {
            graph.Sort(Hilbert.DefaultHilbertSteps);
        }

        /// <summary>
        /// Copies all data from the given graph.
        /// </summary>
        /// <typeparam name="TEdgeData"></typeparam>
        public static void Sort<TEdgeData>(this GeometricGraph<TEdgeData> graph, int n)
            where TEdgeData : struct, IEdgeData
        {
            QuickSort.Sort((vertex) =>
            {
                return graph.Distance(n, (uint)vertex);
            },
            (vertex1, vertex2) =>
            {
                graph.Switch((uint)vertex1, (uint)vertex2);
            }, 0, graph.VertexCount - 1);
        }

        /// <summary>
        /// Returns the hibert distance for n and the given vertex.
        /// </summary>
        /// <typeparam name="TEdgeData"></typeparam>
        /// <returns></returns>
        public static long Distance<TEdgeData>(this GeometricGraph<TEdgeData> graph, int n, uint vertex)
            where TEdgeData : struct, IEdgeData
        {
            float latitude, longitude;
            if(!graph.GetVertex(vertex, out latitude, out longitude))
            {
                throw new Exception(string.Format("Cannot calculate hilbert distance, vertex {0} does not exist.",
                    vertex));
            }
            return HilbertCurve.HilbertDistance(latitude, longitude, n);
        }
    }
}
