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

using System.Collections.Generic;
using OsmSharp.Collections.Tags;
using OsmSharp.Math.Geo;
using OsmSharp.Routing.Interpreter;
using OsmSharp.Math.Geo.Simple;
using OsmSharp.Collections.Coordinates;
using OsmSharp.Collections.Coordinates.Collections;
using OsmSharp.Routing.Vehicles;

namespace OsmSharp.Routing.Graph.Routing
{
    /// <summary>
    /// Abstract a router that works on a dynamic graph.
    /// </summary>
    public interface IRoutingAlgorithm<TEdgeData>
        where TEdgeData : IGraphEdgeData
    {
        /// <summary>
        /// Calculates a shortest path between two given vertices.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="interpreter"></param>
        /// <param name="vehicle"></param>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="max"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        PathSegment<long> Calculate(IRoutingAlgorithmData<TEdgeData> graph, IRoutingInterpreter interpreter, Vehicle vehicle,
            PathSegmentVisitList source, PathSegmentVisitList target, double max, Dictionary<string, object> parameters);

        /// <summary>
        /// Calculates all routes between all source and all target vertices.
        /// </summary>
        /// <returns></returns>
        PathSegment<long>[][] CalculateManyToMany(IRoutingAlgorithmData<TEdgeData> dataGraph, IRoutingInterpreter interpreter,
            Vehicle vehicle, PathSegmentVisitList[] sources, PathSegmentVisitList[] targets, double maxSearch, 
            Dictionary<string, object> parameters);

        /// <summary>
        /// Calculates the weight of the shortest path between two given vertices.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="interpreter"></param>
        /// <param name="vehicle"></param>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="max"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        double CalculateWeight(IRoutingAlgorithmData<TEdgeData> graph, IRoutingInterpreter interpreter, Vehicle vehicle,
            PathSegmentVisitList source, PathSegmentVisitList target, double max, Dictionary<string, object> parameters);

        /// <summary>
        /// Calculates a shortest path between the source vertex and any of the targets and returns the shortest.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="interpreter"></param>
        /// <param name="vehicle"></param>
        /// <param name="source"></param>
        /// <param name="targets"></param>
        /// <param name="max"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        PathSegment<long> CalculateToClosest(IRoutingAlgorithmData<TEdgeData> graph, IRoutingInterpreter interpreter, Vehicle vehicle,
            PathSegmentVisitList source, PathSegmentVisitList[] targets, double max, Dictionary<string, object> parameters);

        /// <summary>
        /// Calculates all routes from a given source to all given targets.
        /// </summary>
        /// <returns></returns>
        double[] CalculateOneToManyWeight(IRoutingAlgorithmData<TEdgeData> graph, IRoutingInterpreter interpreter, Vehicle vehicle,
            PathSegmentVisitList source, PathSegmentVisitList[] targets, double max, Dictionary<string, object> parameters, HashSet<int> invalidSet);

        /// <summary>
        /// Calculates all routes from a given sources to all given targets.
        /// </summary>
        /// <returns></returns>
        double[][] CalculateManyToManyWeight(IRoutingAlgorithmData<TEdgeData> graph, IRoutingInterpreter interpreter, Vehicle vehicle,
            PathSegmentVisitList[] sources, PathSegmentVisitList[] targets, double max, Dictionary<string, object> parameters, HashSet<int> invalidSet);

        /// <summary>
        /// Returns true if range calculation is supported.
        /// </summary>
        bool IsCalculateRangeSupported { get; }

        /// <summary>
        /// Calculates all points that are at or close to the given weight.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="interpreter"></param>
        /// <param name="vehicle"></param>
        /// <param name="source"></param>
        /// <param name="weight"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        HashSet<long> CalculateRange(IRoutingAlgorithmData<TEdgeData> graph, IRoutingInterpreter interpreter, Vehicle vehicle,
            PathSegmentVisitList source, double weight, Dictionary<string, object> parameters);

        /// <summary>
        /// Returns true if the search can move beyond the given weight.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="interpreter"></param>
        /// <param name="vehicle"></param>
        /// <param name="source"></param>
        /// <param name="weight"></param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        bool CheckConnectivity(IRoutingAlgorithmData<TEdgeData> graph, IRoutingInterpreter interpreter, Vehicle vehicle,
            PathSegmentVisitList source, double weight, Dictionary<string, object> parameters);

        /// <summary>
        /// Searches for the closest routable point.
        /// </summary>
        /// <param name="graph">The graph to search.</param>
        /// <param name="interpreter">The routing interpreter.</param>
        /// <param name="vehicle">The vehicle to search for.</param>
        /// <param name="coordinate">The coordinate to search around.</param>
        /// <param name="delta">The size of the box to search in.</param>
        /// <param name="matcher">The matcher to match to edges.</param>
        /// <param name="pointTags">The properties of the point to match.</param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        SearchClosestResult<TEdgeData> SearchClosest(IRoutingAlgorithmData<TEdgeData> graph, IRoutingInterpreter interpreter, Vehicle vehicle,
            GeoCoordinate coordinate, float delta, IEdgeMatcher matcher, TagsCollectionBase pointTags, Dictionary<string, object> parameters);

        /// <summary>
        /// Searches for the closest routable point.
        /// </summary>
        /// <param name="graph">The graph to search.</param>
        /// <param name="interpreter">The routing interpreter.</param>
        /// <param name="vehicle">The vehicle to search for.</param>
        /// <param name="coordinate">The coordinate to search around.</param>
        /// <param name="delta">The size of the box to search in.</param>
        /// <param name="matcher">The matcher to match to edges.</param>
        /// <param name="pointTags">The properties of the point to match.</param>
        /// <param name="verticesOnly">Only match vertices.</param>
        /// <param name="parameters"></param>
        /// <returns></returns>
        SearchClosestResult<TEdgeData> SearchClosest(IRoutingAlgorithmData<TEdgeData> graph, IRoutingInterpreter interpreter, Vehicle vehicle,
            GeoCoordinate coordinate, float delta, IEdgeMatcher matcher, TagsCollectionBase pointTags, bool verticesOnly, Dictionary<string, object> parameters);

        /// <summary>
        /// Gets the weight type.
        /// </summary>
        RouterWeightType WeightType
        {
            get;
        }
    }

    /// <summary>
    /// Enumerates the type of weights used by a basis router.
    /// </summary>
    public enum RouterWeightType
    {
        /// <summary>
        /// The router weights are time-estimates.
        /// </summary>
        Time,
        /// <summary>
        /// The router weights are distances.
        /// </summary>
        Distance,
        /// <summary>
        /// The router-weights are completely custom.
        /// </summary>
        Custom
    }

    /// <summary>
    /// The result the search closest returns.
    /// </summary>
    public struct SearchClosestResult<TEdgeData>
    {
        /// <summary>
        /// The result is located exactly at one vertex.
        /// </summary>
        /// <param name="distance"></param>
        /// <param name="vertex"></param>
        public SearchClosestResult(double distance, uint vertex)
            : this()
        {
            this.Distance = distance;
            this.Vertex1 = vertex;
            this.Position = 0;
            this.Vertex2 = null;
        }

        /// <summary>
        /// The result is located between two other vertices but on an intermediate point.
        /// </summary>
        /// <param name="distance"></param>
        /// <param name="vertex1"></param>
        /// <param name="vertex2"></param>
        /// <param name="intermediateIndex"></param>
        /// <param name="edge"></param>
        /// <param name="coordinates"></param>
        public SearchClosestResult(double distance, uint vertex1, uint vertex2, int intermediateIndex, TEdgeData edge, ICoordinate[] coordinates)
            : this()
        {
            this.Distance = distance;
            this.Vertex1 = vertex1;
            this.Vertex2 = vertex2;
            this.IntermediateIndex = intermediateIndex;
            this.Edge = edge;
            this.Coordinates = coordinates;
        }

        /// <summary>
        /// The result is located between two other vertices.
        /// </summary>
        /// <param name="distance"></param>
        /// <param name="vertex1"></param>
        /// <param name="vertex2"></param>
        /// <param name="position"></param>
        /// <param name="edge"></param>
        /// <param name="coordinates"></param>
        public SearchClosestResult(double distance, uint vertex1, uint vertex2, double position, TEdgeData edge, ICoordinate[] coordinates)
            : this()
        {
            this.Distance = distance;
            this.Vertex1 = vertex1;
            this.Vertex2 = vertex2;
            this.Position = position;
            this.Edge = edge;
            this.Coordinates = coordinates;
        }

        /// <summary>
        /// The first vertex.
        /// </summary>
        public uint? Vertex1 { get; private set; }

        /// <summary>
        /// The second vertex.
        /// </summary>
        public uint? Vertex2 { get; private set; }

        /// <summary>
        /// The intermediate point position.
        /// </summary>
        public int? IntermediateIndex { get; private set; }

        /// <summary>
        /// The position between vertex1 and vertex2 (0=vertex1, 1=vertex2).
        /// </summary>
        public double Position { get; private set; }

        /// <summary>
        /// The distance from the point being resolved.
        /// </summary>
        public double Distance { get; private set; }

        /// <summary>
        /// The edge data.
        /// </summary>
        public TEdgeData Edge { get; private set; }

        /// <summary>
        /// The coordinates.
        /// </summary>
        public ICoordinate[] Coordinates { get; set; }
    }
}