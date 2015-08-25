// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2013 Abelshausen Ben
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

using OsmSharp.Routing.CH.Preprocessing;
using OsmSharp.Routing.Graph.Routing;
using OsmSharp.Routing.Interpreter;
using System;
using System.Collections.Generic;
using OsmSharp.Routing.Graph;
using System.Linq;
using OsmSharp.Routing.Vehicles;

namespace OsmSharp.Routing.Routers
{
    /// <summary>
    /// A version of the typedrouter using edges of type CHEdgeData.
    /// </summary>
    internal class TypedRouterCHEdge : TypedRouter<CHEdgeData>
    {
        /// <summary>
        /// Creates a new type router using edges of type CHEdgeData.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="interpreter"></param>
        /// <param name="router"></param>
        public TypedRouterCHEdge(IRoutingAlgorithmData<CHEdgeData> graph, IRoutingInterpreter interpreter, IRoutingAlgorithm<CHEdgeData> router)
            : base(graph, interpreter, router)
        {
            DefaultSearchDelta = 0.0125f;
        }

        /// <summary>
        /// Returns true if the given vehicle is supported.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <returns></returns>
        public override bool SupportsVehicle(Vehicle vehicle)
        {
            // TODO: ask interpreter.
            return true;
        }

        /// <summary>
        /// Returns all the arcs representing neighbours for the given vertex.
        /// </summary>
        /// <param name="vertex1"></param>
        /// <returns></returns>
        protected override List<Edge<CHEdgeData>> GetNeighboursUndirected(long vertex1)
        {
            var edges =  this.Data.GetDirectNeighbours(Convert.ToUInt32(vertex1)).ToList();
            return edges.KeepUncontracted();
        }


        /// <summary>
        /// Returns an edge with a forward weight.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        protected override bool GetEdge(IGraphReadOnly<CHEdgeData> graph, uint from, uint to, out CHEdgeData data)
        {
            var lowestWeight = float.MaxValue;
            data = new CHEdgeData();
            var edges =  graph.GetEdges(from, to);
            while (edges.MoveNext())
            {
                var edgeData = edges.EdgeData;
                if (edgeData.CanMoveForward &&
                    edgeData.Weight < lowestWeight)
                {
                    data = edgeData;
                    lowestWeight = edgeData.Weight;
                }
            }
            edges = graph.GetEdges(to, from);
            while (edges.MoveNext())
            {
                var edgeData = edges.EdgeData;
                if (edgeData.CanMoveBackward &&
                    edgeData.Weight < lowestWeight)
                {
                    data = edgeData;
                    lowestWeight = edgeData.Weight;
                }
            }
            return lowestWeight < float.MaxValue;
        }

        /// <summary>
        /// Returns a shape between the given vertices.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        protected override bool GetEdgeShape(IGraphReadOnly<CHEdgeData> graph, uint from, uint to, out Collections.Coordinates.Collections.ICoordinateCollection data)
        {
            var lowestWeight = float.MaxValue;
            data = null;
            var edges =  graph.GetEdges(from, to);
            while (edges.MoveNext())
            {
                var edgeData = edges.EdgeData;
                if (edgeData.CanMoveForward &&
                    edgeData.RepresentsNeighbourRelations &&
                    edgeData.Weight < lowestWeight)
                {
                    data = edges.Intermediates;
                    lowestWeight = edgeData.Weight;
                }
            }
            edges = graph.GetEdges(to, from);
            while (edges.MoveNext())
            {
                var edgeData = edges.EdgeData;
                if (edgeData.CanMoveBackward &&
                    edgeData.RepresentsNeighbourRelations &&
                    edgeData.Weight < lowestWeight)
                {
                    if (edges.Intermediates != null)
                    {
                        data = edges.Intermediates.Reverse();
                    }
                    else
                    {
                        data = null;
                    }
                    lowestWeight = edgeData.Weight;
                }
            }
            return lowestWeight < float.MaxValue;
        }
    }
}