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

using OsmSharp.Collections.Coordinates.Collections;
using OsmSharp.Collections.Tags;
using OsmSharp.Logging;
using OsmSharp.Math.Geo;
using OsmSharp.Math.Geo.Simple;
using OsmSharp.Routing.Graph;
using OsmSharp.Routing.Graph.Routing;
using OsmSharp.Routing.Interpreter;
using OsmSharp.Routing.Metrics;
using OsmSharp.Routing.Vehicles;
using OsmSharp.Units.Distance;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OsmSharp.Routing.Routers
{
    /// <summary>
    /// A class that implements common functionality for any routing algorithm.
    /// </summary>
    public abstract class TypedRouter<TEdgeData> : ITypedRouter
        where TEdgeData : IGraphEdgeData
    {
        /// <summary>
        /// The default search delta.
        /// </summary>
        protected float DefaultSearchDelta = .0075f;

        /// <summary>
        /// Holds the graph object containing the routable network.
        /// </summary>
        private readonly IRoutingAlgorithmData<TEdgeData> _dataGraph;

        /// <summary>
        /// Holds the basic router that works on the dynamic graph.
        /// </summary>
        private readonly IRoutingAlgorithm<TEdgeData> _router;

        /// <summary>
        /// Interpreter for the routing network.
        /// </summary>
        private readonly IRoutingInterpreter _interpreter;

        /// <summary>
        /// Creates a new router.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="interpreter"></param>
        /// <param name="router"></param>
        public TypedRouter(IRoutingAlgorithmData<TEdgeData> graph, IRoutingInterpreter interpreter,
            IRoutingAlgorithm<TEdgeData> router)
        {
            _dataGraph = graph;
            _interpreter = interpreter;
            _router = router;

            _routerPoints = new Dictionary<string, Dictionary<GeoCoordinate, RouterPoint>>();
            _resolvedGraphs = new Dictionary<Vehicle, TypedRouterResolvedGraph>();
        }

        /// <summary>
        /// Returns the routing interpreter.
        /// </summary>
        protected IRoutingInterpreter Interpreter
        {
            get { return _interpreter; }
        }

        /// <summary>
        /// Returns the data.
        /// </summary>
        protected IRoutingAlgorithmData<TEdgeData> Data
        {
            get { return _dataGraph; }
        }

        /// <summary>
        /// Returns true if the given vehicle is supported.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <returns></returns>
        public virtual bool SupportsVehicle(Vehicle vehicle)
        {
            return _dataGraph.SupportsProfile(vehicle);
        }

        /// <summary>
        /// Calculates a route from source to target.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public virtual Route Calculate(Vehicle vehicle, RouterPoint source, RouterPoint target)
        {
            return this.Calculate(vehicle, source, target, float.MaxValue);
        }

        /// <summary>
        /// Calculates a route from source to target but does not search more than max around source or target location.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="max"></param>
        /// <param name="geometryOnly"></param>
        /// <returns></returns>
        public virtual Route Calculate(Vehicle vehicle, RouterPoint source, RouterPoint target, float max = float.MaxValue, bool geometryOnly = false)
        {
            // check routing profiles.
            if (!this.SupportsVehicle(vehicle))
            {
                throw new ArgumentOutOfRangeException("vehicle", string.Format("Routing profile {0} not supported by this router!",
                    vehicle.ToString()));
            }

            // calculate the route.
            var route = _router.Calculate(_dataGraph, _interpreter, vehicle,
                this.RouteResolvedGraph(vehicle, source, false), this.RouteResolvedGraph(vehicle, target, true), max, null);

            // convert to an OsmSharpRoute.
            return this.ConstructRoute(vehicle, route, source, target, geometryOnly);
        }

        /// <summary>
        /// Calculates a route from source to the closest target point.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="source"></param>
        /// <param name="targets"></param>
        /// <returns></returns>
        public virtual Route CalculateToClosest(Vehicle vehicle, RouterPoint source, RouterPoint[] targets)
        {
            return this.CalculateToClosest(vehicle, source, targets, float.MaxValue);
        }

        /// <summary>
        /// Calculates a route from source to the closest target point but does not search more than max around source location.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="source"></param>
        /// <param name="targets"></param>
        /// <param name="max"></param>
        /// <returns></returns>
        public virtual Route CalculateToClosest(Vehicle vehicle, RouterPoint source, RouterPoint[] targets, float max = float.MaxValue, bool geometryOnly = false)
        {
            // check routing profiles.
            if (!this.SupportsVehicle(vehicle))
            {
                throw new ArgumentOutOfRangeException("vehicle", string.Format("Routing profile {0} not supported by this router!",
                    vehicle.ToString()));
            }

            // calculate the route.
            var route = _router.CalculateToClosest(_dataGraph, _interpreter, vehicle,
                this.RouteResolvedGraph(vehicle, source, false), this.RouteResolvedGraph(vehicle, targets, true), max, null);

            // find the target.
            var target = targets.First(x => x.Id == route.VertexId);

            // convert to an OsmSharpRoute.
            return this.ConstructRoute(vehicle, route, source, target, geometryOnly);
        }

        /// <summary>
        /// Calculates all the routes between the source and all given targets.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="source"></param>
        /// <param name="targets"></param>
        /// <returns></returns>
        public virtual Route[] CalculateOneToMany(Vehicle vehicle, RouterPoint source, RouterPoint[] targets, float max = float.MaxValue, bool geometryOnly = false)
        {
            return this.CalculateManyToMany(vehicle, new[] { source }, targets, max, geometryOnly)[0];
        }

        /// <summary>
        /// Calculates all the routes between all the sources and all the targets.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="sources"></param>
        /// <param name="targets"></param>
        /// <returns></returns>
        public virtual Route[][] CalculateManyToMany(Vehicle vehicle, RouterPoint[] sources, RouterPoint[] targets, float max = float.MaxValue, bool geometryOnly = false)
        {
            // check routing profiles.
            if (!this.SupportsVehicle(vehicle))
            {
                throw new ArgumentOutOfRangeException("vehicle", string.Format("Routing profile {0} not supported by this router!",
                    vehicle.ToString()));
            }

            var routes = _router.CalculateManyToMany(_dataGraph, _interpreter, vehicle, this.RouteResolvedGraph(vehicle, sources, false),
                this.RouteResolvedGraph(vehicle, targets, true), max, null);

            var constructedRoutes = new Route[sources.Length][];
            for (int x = 0; x < sources.Length; x++)
            {
                constructedRoutes[x] = new Route[targets.Length];
                for (int y = 0; y < targets.Length; y++)
                {
                    constructedRoutes[x][y] =
                        this.ConstructRoute(vehicle, routes[x][y], sources[x], targets[y], geometryOnly);
                }
            }

            return constructedRoutes;
        }


        /// <summary>
        /// Calculates the weight from source to target.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public virtual double CalculateWeight(Vehicle vehicle, RouterPoint source, RouterPoint target)
        {
            // check routing profiles.
            if (!this.SupportsVehicle(vehicle))
            {
                throw new ArgumentOutOfRangeException("vehicle", string.Format("Routing profile {0} not supported by this router!",
                    vehicle.ToString()));
            }

            // calculate the route.
            return _router.CalculateWeight(_dataGraph, _interpreter, vehicle,
                this.RouteResolvedGraph(vehicle, source, false), this.RouteResolvedGraph(vehicle, target, true), float.MaxValue, null);
        }

        /// <summary>
        /// Calculates all the weights from source to all the targets.
        /// </summary>
        /// <returns></returns>
        public virtual double[] CalculateOneToManyWeight(Vehicle vehicle, RouterPoint source, RouterPoint[] targets, HashSet<int> invalidSet)
        {
            // check routing profiles.
            if (!this.SupportsVehicle(vehicle))
            {
                throw new ArgumentOutOfRangeException("vehicle", string.Format("Routing profile {0} not supported by this router!",
                    vehicle.ToString()));
            }

            return _router.CalculateOneToManyWeight(_dataGraph, _interpreter, vehicle, this.RouteResolvedGraph(vehicle, source, false),
                this.RouteResolvedGraph(vehicle, targets, true), double.MaxValue, null, invalidSet);
        }

        /// <summary>
        /// Calculates all the weights between all the sources and all the targets.
        /// </summary>
        /// <returns></returns>
        public virtual double[][] CalculateManyToManyWeight(Vehicle vehicle, RouterPoint[] sources, RouterPoint[] targets, HashSet<int> invalidSet)
        {
            // check routing profiles.
            if (!this.SupportsVehicle(vehicle))
            {
                throw new ArgumentOutOfRangeException("vehicle", string.Format("Routing profile {0} not supported by this router!",
                    vehicle.ToString()));
            }

            return _router.CalculateManyToManyWeight(_dataGraph, _interpreter, vehicle, this.RouteResolvedGraph(vehicle, sources, false),
                this.RouteResolvedGraph(vehicle, targets, true), double.MaxValue, null, invalidSet);
        }

        /// <summary>
        /// Returns true if range calculation is supported.
        /// </summary>
        public virtual bool IsCalculateRangeSupported
        {
            get
            {
                return _router.IsCalculateRangeSupported;
            }
        }

        /// <summary>
        /// Calculates the locations around the origin that have a given weight.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="orgin"></param>
        /// <param name="weight"></param>
        /// <returns></returns>
        public virtual HashSet<GeoCoordinate> CalculateRange(Vehicle vehicle, RouterPoint orgin, float weight)
        {
            // check routing profiles.
            if (!this.SupportsVehicle(vehicle))
            {
                throw new ArgumentOutOfRangeException("vehicle", string.Format("Routing profile {0} not supported by this router!",
                    vehicle.ToString()));
            }

            HashSet<long> objectsAtWeight = _router.CalculateRange(_dataGraph, _interpreter, vehicle, this.RouteResolvedGraph(vehicle, orgin, false),
                weight, null);

            var locations = new HashSet<GeoCoordinate>();
            foreach (long vertex in objectsAtWeight)
            {
                GeoCoordinate coordinate = this.GetCoordinate(vehicle, vertex);
                locations.Add(coordinate);
            }
            return locations;
        }

        /// <summary>
        /// Returns true if the given source is at least connected to vertices with at least a given weight.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="point"></param>
        /// <param name="weight"></param>
        /// <returns></returns>
        public virtual bool CheckConnectivity(Vehicle vehicle, RouterPoint point, float weight)
        {
            // check routing profiles.
            if (!this.SupportsVehicle(vehicle))
            {
                throw new ArgumentOutOfRangeException("vehicle", string.Format("Routing profile {0} not supported by this router!",
                    vehicle.ToString()));
            }

            return _router.CheckConnectivity(_dataGraph, _interpreter, vehicle, this.RouteResolvedGraph(vehicle, point, false), weight, null);
        }

        /// <summary>
        /// Returns an array of connectivity check results.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="point"></param>
        /// <param name="weight"></param>
        /// <returns></returns>
        public virtual bool[] CheckConnectivity(Vehicle vehicle, RouterPoint[] point, float weight)
        {
            // check routing profiles.
            if (!this.SupportsVehicle(vehicle))
            {
                throw new ArgumentOutOfRangeException("vehicle", string.Format("Routing profile {0} not supported by this router!",
                    vehicle.ToString()));
            }

            var connectivityArray = new bool[point.Length];
            for (int idx = 0; idx < point.Length; idx++)
            {
                connectivityArray[idx] = this.CheckConnectivity(vehicle, point[idx], weight);

                Logging.Log.TraceEvent("TypedRouter<TEdgeData>", TraceEventType.Information, "Checking connectivity... {0}%",
                    (int)(((float)idx / (float)point.Length) * 100));
            }
            return connectivityArray;
        }

        #region Route Building

        /// <summary>
        /// Holds the metrics calculator.
        /// </summary>
        private RouteMetricCalculator _metricsCalculator;

        /// <summary>
        /// Gets or sets the metrics calculator.
        /// </summary>
        public RouteMetricCalculator MetricCalculator
        {
            get
            {
                return _metricsCalculator;
            }
            set
            {
                _metricsCalculator = value;
            }
        }

        /// <summary>
        /// Converts a linked route to an OsmSharpRoute.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="route"></param>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        protected virtual Route ConstructRoute(Vehicle vehicle, PathSegment<long> route, RouterPoint source, RouterPoint target)
        {
            return this.ConstructRoute(vehicle, route, source, target, false);
        }

        /// <summary>
        /// Converts a linked route to an OsmSharpRoute.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="route"></param>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <param name="geometryOnly"></param>
        /// <returns></returns>
        protected virtual Route ConstructRoute(Vehicle vehicle, PathSegment<long> route, RouterPoint source, RouterPoint target, bool geometryOnly)
        {
            if (route != null)
            { // construct the actual route, get associated meta-data and calculate metrics.
                return this.ConstructRoute(vehicle, source, target, route.ToArrayWithWeight(), geometryOnly);
            }
            return null; // calculation failed!
        }

        /// <summary>
        /// Generates an route from a graph route.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="fromResolved"></param>
        /// <param name="toResolved"></param>
        /// <param name="vertices"></param>
        /// <param name="geometryOnly"></param>
        /// <returns></returns>
        protected virtual Route ConstructRoute(Vehicle vehicle, RouterPoint fromResolved, RouterPoint toResolved, Tuple<long, double>[] vertices, bool geometryOnly)
        {
            // create the route.
            Route route = null;

            if (vertices != null)
            {
                route = new Route();
                route.Vehicle = vehicle.UniqueName;
                RouteSegment[] segments;
                if (vertices.Length > 0)
                {
                    segments = this.ConstructRouteSegments(vehicle, vertices, geometryOnly);
                }
                else
                {
                    segments = new RouteSegment[0];
                }

                // create the from routing point.
                if (segments.Length > 0)
                { // there is at least one segment.
                    var from = new RoutePoint();
                    from.Latitude = (float)fromResolved.Location.Latitude;
                    from.Longitude = (float)fromResolved.Location.Longitude;
                    segments[0].Points = new RoutePoint[1];
                    segments[0].Points[0] = from;
                    segments[0].Points[0].Tags = RouteTagsExtensions.ConvertFrom(fromResolved.Tags);
                }

                // create the to routing point.
                if (segments.Length > 0)
                { // there is at least one segment.
                    var to = new RoutePoint();
                    to.Latitude = (float)toResolved.Location.Latitude;
                    to.Longitude = (float)toResolved.Location.Longitude;
                    //to.Tags = ConvertTo(to_point.Tags);
                    segments[segments.Length - 1].Points = new RoutePoint[1];
                    segments[segments.Length - 1].Points[0] = to;
                    segments[segments.Length - 1].Points[0].Tags = RouteTagsExtensions.ConvertFrom(toResolved.Tags);
                }

                // set the routing segments.
                route.Segments = segments;

                if (segments.Length > 0)
                { // set the distance/time.
                    route.TotalDistance = route.Segments[route.Segments.Length - 1].Distance;
                    route.TotalTime = route.Segments[route.Segments.Length - 1].Time;
                    route.HasTimes = (_router.WeightType == RouterWeightType.Time);
                }

                // calculate metrics.
                if (!geometryOnly && _metricsCalculator != null)
                { // calculate metrics.
                    route.Metrics = RouteMetric.ConvertFrom(_metricsCalculator.Calculate(route));
                }
            }

            return route;
        }

        /// <summary>
        /// Generates a list of entries.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="vertices"></param>
        /// <param name="geometryOnly"></param>
        /// <returns></returns>
        protected virtual RouteSegment[] ConstructRouteSegments(Vehicle vehicle, Tuple<long, double>[] vertices, bool geometryOnly)
        {
            // create an entries list.
            var entries = new List<RouteSegment>();

            // create the first entry.
            var coordinate = this.GetCoordinate(vehicle, vertices[0].Item1);
            var first = new RouteSegment();
            first.Latitude = (float)coordinate.Latitude;
            first.Longitude = (float)coordinate.Longitude;
            first.Type = RouteSegmentType.Start;
            first.Name = null;
            first.Names = null;
            first.Distance = 0;
            first.Time = 0;
            entries.Add(first);

            // create all the other entries except the last one.
            var distance = 0.0;
            var time = 0.0;
            var nodePrevious = vertices[0];
            var nodePreviousCoordinate = coordinate;
            TagsCollectionBase currentTags = new TagsCollection();
            var name = string.Empty;
            var names = new Dictionary<string, string>();
            for (int idx = 1; idx < vertices.Length; idx++)
            {
                // get all the data needed to calculate the next route entry.
                var nodeCurrent = vertices[idx];
                var nodeCurrentCoordinate = this.GetCoordinate(vehicle, nodeCurrent.Item1);
                Tuple<long, double> nodeNext = null;
                GeoCoordinate nodeNextCoordinate = null;
                if (idx < vertices.Length - 1)
                { // there is a next node.
                    nodeNext = vertices[idx + 1];
                    nodeNextCoordinate = this.GetCoordinate(vehicle, nodeNext.Item1);
                }

                // get coordinates and calculate distance.
                var coordinates = this.GetEdgeShape(vehicle, nodePrevious.Item1, nodeCurrent.Item1);
                var localDistance = 0.0;
                if (!geometryOnly)
                { // also get all metadata.
                    if (coordinates != null)
                    { // there are intermediates.
                        var prevCoordinate = nodePreviousCoordinate;
                        for (int coordinateIdx = 0; coordinateIdx < coordinates.Length; coordinateIdx++)
                        {
                            var curCoordinate = new GeoCoordinate(coordinates[coordinateIdx].Latitude, coordinates[coordinateIdx].Longitude);
                            localDistance = localDistance + prevCoordinate.DistanceReal(curCoordinate).Value;
                            prevCoordinate = curCoordinate;
                        }
                        localDistance = localDistance + prevCoordinate.DistanceReal(nodeCurrentCoordinate).Value;
                    }
                    else
                    { // there are no intermediates.
                        localDistance = nodeCurrentCoordinate.DistanceReal(nodePreviousCoordinate).Value;
                    }
                }

                // FIRST CALCULATE ALL THE ENTRY METRICS!
                var beforeTime = time;
                var beforeDistance = distance;
                var localTime = 0.0;
                distance = distance + localDistance;
                if(_router.WeightType == RouterWeightType.Time)
                { // if the router uses time as a metric, use this for the route too.
                    localTime = nodeCurrent.Item2 - time;
                    time = nodeCurrent.Item2;
                }

                // STEP1: Get the names.
                if(!geometryOnly)
                { // also get all metadata.
                    var edge = this.GetEdgeData(vehicle, nodePrevious.Item1, nodeCurrent.Item1);
                    currentTags = _dataGraph.TagsIndex.Get(edge.Tags);
                    name = _interpreter.EdgeInterpreter.GetName(currentTags);
                    names = _interpreter.EdgeInterpreter.GetNamesInAllLanguages(currentTags);
                }

                // add intermediate entries.
                if(coordinates != null)
                { // loop over coordinates.
                    var curDistance = 0.0;
                    var prevCoordinate = nodePreviousCoordinate;
                    for (int coordinateIdx = 0; coordinateIdx < coordinates.Length; coordinateIdx++)
                    {
                        var entry = new RouteSegment();
                        entry.Latitude = coordinates[coordinateIdx].Latitude;
                        entry.Longitude = coordinates[coordinateIdx].Longitude;
                        entry.Type = RouteSegmentType.Along;
                        entry.Tags = currentTags.ConvertFrom();
                        entry.Name = name;
                        entry.Names = names.ConvertFrom();
                        if (!geometryOnly)
                        { // also get all metadata.
                            var curCoordinate = new GeoCoordinate(coordinates[coordinateIdx].Latitude, coordinates[coordinateIdx].Longitude);
                            curDistance = curDistance + prevCoordinate.DistanceReal(curCoordinate).Value;
                            entry.Distance = curDistance;
                            if (_router.WeightType == RouterWeightType.Time)
                            { // if the router uses time as a metric, use this for the route too.
                                entry.Time = localTime * (curDistance / localDistance);
                            }
                            prevCoordinate = curCoordinate;
                        }
                        entries.Add(entry);
                    }
                }

                // STEP2: Get the side streets
                RouteSegmentBranch[] sideStreetsArray = null;
                if (!geometryOnly && idx < vertices.Length - 1)
                {
                    var sideStreets = new List<RouteSegmentBranch>();
                    var neighbours = this.GetNeighboursUndirectedWithEdges(vehicle, nodeCurrent.Item1, nodePrevious.Item1, nodeNext.Item1);
                    var consideredNeighbours = new HashSet<GeoCoordinate>();
                    if (neighbours.Count > 0)
                    { // construct neighbours list.
                        foreach (var neighbour in neighbours)
                        {
                            var neighbourKeyCoordinate = this.GetCoordinate(vehicle, neighbour.Key);
                            var neighbourValueCoordinates = this.GetEdgeShape(vehicle, nodeCurrent.Item1, neighbour.Key);
                            if (neighbourValueCoordinates != null &&
                                neighbourValueCoordinates.Length > 0)
                            { // get the first of the coordinates array.
                                neighbourKeyCoordinate = new GeoCoordinate(
                                    neighbourValueCoordinates[0].Latitude,
                                    neighbourValueCoordinates[0].Longitude);
                            }
                            if (!consideredNeighbours.Contains(neighbourKeyCoordinate))
                            { // neighbour has not been considered yet.
                                consideredNeighbours.Add(neighbourKeyCoordinate);

                                var neighbourCoordinate = this.GetCoordinate(vehicle, neighbour.Key);

                                // build the side street info.
                                var sideStreet = new RouteSegmentBranch();
                                sideStreet.Latitude = (float)neighbourCoordinate.Latitude;
                                sideStreet.Longitude = (float)neighbourCoordinate.Longitude;
                                if (!geometryOnly)
                                { // get metadata for this section too.
                                    var tags = _dataGraph.TagsIndex.Get(neighbour.Value.Tags);
                                    sideStreet.Tags = tags.ConvertFrom();
                                    sideStreet.Name = _interpreter.EdgeInterpreter.GetName(tags);
                                    sideStreet.Names = _interpreter.EdgeInterpreter.GetNamesInAllLanguages(tags).ConvertFrom();
                                }

                                sideStreets.Add(sideStreet);
                            }
                        }
                        sideStreetsArray = sideStreets.ToArray();
                    }
                }

                var routeEntry = new RouteSegment();
                routeEntry.Latitude = (float)nodeCurrentCoordinate.Latitude;
                routeEntry.Longitude = (float)nodeCurrentCoordinate.Longitude;
                routeEntry.SideStreets = sideStreetsArray;
                routeEntry.Tags = currentTags.ConvertFrom();
                if (idx < vertices.Length - 1)
                { // along.
                    routeEntry.Type = RouteSegmentType.Along;
                }
                else
                { // stop.
                    routeEntry.Type = RouteSegmentType.Stop;
                }
                routeEntry.Name = name;
                routeEntry.Names = names.ConvertFrom();
                routeEntry.Distance = distance;
                routeEntry.Time = time;
                entries.Add(routeEntry);

                // set the previous node.
                nodePrevious = nodeCurrent;
                nodePreviousCoordinate = nodeCurrentCoordinate;
            }

            // return the result.
            return entries.ToArray();
        }

        /// <summary>
        /// Returns all the edges to the neighbours of the given vertex except the shortest 
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="vertex1"></param>
        /// <param name="previousVertex"></param>
        /// <param name="nextVertex"></param>
        /// <returns></returns>
        protected virtual List<KeyValuePair<long, IGraphEdgeData>> GetNeighboursUndirectedWithEdges(Vehicle vehicle,
            long vertex1, long previousVertex, long nextVertex)
        {
            var neighbours = new List<KeyValuePair<long, IGraphEdgeData>>();
            var distanceToPrevious = double.MaxValue;
            var distanceToNext = double.MaxValue;
            var indexOfPrevious = -1;
            var indexOfNext = -1;

            var vertex1Coordinate = this.GetCoordinate(vehicle, vertex1);
            var nextCoordinate = this.GetCoordinate(vehicle, nextVertex);
            var previousCoordinate = this.GetCoordinate(vehicle, previousVertex);

            // get the resolved graph for the given profile.
            var graph = this.GetForProfile(vehicle);

            if (vertex1 > 0)
            { // only check 'real' vertices for any neighbours, intermediates will not have extra neighbours.
                var edges =  this.GetNeighboursUndirected(vertex1);

                // remove duplicates.
                // arcs = arcs.Distinct(new ArcEqualityComparer()).ToArray();

                // check if arcs left.
                if(edges.Count == 0)
                { // no neighbours for sure.
                    return neighbours;
                }

                // get 'real' neighbours.
                bool checkIntermediates = false;
                if (previousVertex < 0)
                { // get the real neighbour.
                    checkIntermediates = true;
                    RouterPoint resolvedPoint;
                    if (!this.GetRouterPoint(vehicle, previousVertex, out resolvedPoint))
                    { // oeps, point not found!
                        throw new Exception("Resolved point detected but not found as a router point!");
                    }
                    var visitList = this.RouteResolvedGraph(vehicle, resolvedPoint, null);
                    if(visitList.Count == 2)
                    { // two points.
                        var vertices = new HashSet<long>(visitList.GetVertices());
                        vertices.Remove(vertex1);
                        var other = vertices.First();
                        var path = visitList.GetPathTo(vertex1).ConcatenateAfter(
                            visitList.GetPathTo(other).Reverse()).Reverse();

                        // match path with edge.
                        for (int idx = 0; idx < edges.Count; idx++)
                        {
                            var coordinates = this.GetEdgeShape(vehicle, vertex1, edges[idx].Neighbour);
                            if (this.MatchArc(vehicle, vertex1, coordinates, edges[idx].Neighbour, path))
                            { // arc matches, remove from array.
                                var newArcs = new List<Edge<TEdgeData>>(edges);
                                newArcs.RemoveAt(idx);
                                edges = newArcs;
                                break;
                            }
                        }
                    }
                }
                if (nextVertex < 0)
                { // get the real neighbour.
                    checkIntermediates = true;
                    RouterPoint resolvedPoint;
                    if (!this.GetRouterPoint(vehicle, nextVertex, out resolvedPoint))
                    { // oeps, point not found!
                        throw new Exception("Resolved point detected but not found as a router point!");
                    }
                    var visitList = this.RouteResolvedGraph(vehicle, resolvedPoint, null);
                    if(visitList.Count == 2)
                    { // two points.
                        var vertices = new HashSet<long>(visitList.GetVertices());
                        vertices.Remove(vertex1);
                        var other = vertices.First();
                        var path = visitList.GetPathTo(other).ConcatenateAfter(
                            visitList.GetPathTo(vertex1).Reverse());

                        // match path with edge.
                        for (int idx = 0; idx < edges.Count; idx++)
                        {
                            var coordinates = this.GetEdgeShape(vehicle, vertex1, edges[idx].Neighbour);
                            if (this.MatchArc(vehicle, vertex1, coordinates, edges[idx].Neighbour, path))
                            { // arc matches, remove from array.
                                var newArcs = new List<Edge<TEdgeData>>(edges);
                                newArcs.RemoveAt(idx);
                                edges = newArcs;
                                break;
                            }
                        }
                    }
                }

                foreach (var arc in edges)
                {
                    if (arc.Neighbour == previousVertex)
                    { // this is an arc to the previous point.
                        var distance = 0.0;
                        var previous = vertex1Coordinate;
                        var coordinates = this.GetEdgeShape(vehicle, vertex1, arc.Neighbour);
                        if (coordinates != null)
                        { // there are intermediates.
                            for (int idx = 0; idx < coordinates.Length; idx++)
                            {
                                var current = new GeoCoordinate(coordinates[idx].Latitude,
                                    coordinates[idx].Longitude);
                                distance = distance + previous.DistanceReal(current).Value;
                                previous = current;
                            }
                        }
                        distance = distance + previous.DistanceReal(previousCoordinate).Value;
                        if (distance < distanceToPrevious)
                        { // a new shortest.
                            distanceToPrevious = distance;
                            indexOfPrevious = neighbours.Count;
                        }
                    }
                    else if (arc.Neighbour == nextVertex)
                    { // this is an arc to the next point.
                        var distance = 0.0;
                        var previous = vertex1Coordinate;
                        var coordinates = this.GetEdgeShape(vehicle, vertex1, arc.Neighbour);
                        if (coordinates != null)
                        { // there are intermediates.
                            for (int idx = 0; idx < coordinates.Length; idx++)
                            {
                                var current = new GeoCoordinate(coordinates[idx].Latitude,
                                    coordinates[idx].Longitude);
                                distance = distance + previous.DistanceReal(current).Value;
                                previous = current;
                            }
                        }
                        distance = distance + previous.DistanceReal(nextCoordinate).Value;
                        if (distance < distanceToNext)
                        { // a new shortest.
                            distanceToNext = distance;
                            indexOfNext = neighbours.Count;
                        }
                    }
                    if (checkIntermediates)
                    { // check all intermeditate coordinates for next/previous.
                        var coordinates = this.GetEdgeShape(vehicle, vertex1, arc.Neighbour);
                        if (coordinates != null)
                        { // loop over coordinates.
                            for (int idx = 0; idx < coordinates.Length; idx++)
                            {
                                if (coordinates[idx].Latitude == nextCoordinate.Latitude &&
                                    coordinates[idx].Longitude == nextCoordinate.Longitude)
                                {
                                    indexOfNext = neighbours.Count;
                                }
                                if (coordinates[idx].Latitude == previousCoordinate.Latitude &&
                                    coordinates[idx].Longitude == previousCoordinate.Longitude)
                                {
                                    indexOfPrevious = neighbours.Count;
                                }
                            }
                        }
                    }
                    neighbours.Add(new KeyValuePair<long, IGraphEdgeData>(arc.Neighbour, arc.EdgeData));
                }
            }

            // remove next/previous from the list.
            if (neighbours.Count > 0)
            { // there have been actual neighoubrs found.
                if (indexOfPrevious < indexOfNext)
                { // remove next first.
                    if (indexOfNext > -1) { neighbours.RemoveAt(indexOfNext); }
                    if (indexOfPrevious > -1 && neighbours.Count > indexOfPrevious) { neighbours.RemoveAt(indexOfPrevious); }
                }
                else
                { // remove previous first.
                    if (indexOfPrevious > -1) { neighbours.RemoveAt(indexOfPrevious); }
                    if (indexOfNext > -1 && neighbours.Count > indexOfNext) { neighbours.RemoveAt(indexOfNext); }
                }
            }

            return neighbours;
        }

        /// <summary>
        /// Tries to match an arc with a given path.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="from"></param>
        /// <param name="along"></param>
        /// <param name="to"></param>
        /// <param name="path"></param>
        /// <returns></returns>
        protected virtual bool MatchArc(Vehicle vehicle, long from, ICoordinate[] along, long to, PathSegment<long> path)
        {
            var vertices = path.ToArray();
            if(vertices[0] != from)
            { // this one is wrong at the start.
                return false;
            }
            if (vertices[vertices.Length - 1] != to)
            { // this one is wrong at the ned.
                return false;
            }
            int intermediateIdx = 0;
            for(int idx = 1; idx < vertices.Length; idx++)
            { // check for intermediates!
                if(this.IsIntermediate(vertices[idx]))
                { // this is an intermediate.
                    if(along == null || intermediateIdx >= along.Length)
                    { // oeps, more intermediates!
                        return false;
                    }
                    var intermediateCoordinate = this.GetCoordinate(vehicle, vertices[idx]);
                    if(intermediateCoordinate.Latitude != along[intermediateIdx].Latitude ||
                       intermediateCoordinate.Longitude != along[intermediateIdx].Longitude)
                    { // a deviation of the intermediates.
                        return false;
                    }
                    intermediateIdx++;
                }
            }
            return true;
        }

        /// <summary>
        /// Returns all the arcs representing neighbours for the given vertex.
        /// </summary>
        /// <param name="vertex1"></param>
        /// <returns></returns>
        protected virtual List<Edge<TEdgeData>> GetNeighboursUndirected(long vertex1)
        {
            return _dataGraph.GetEdges(Convert.ToUInt32(vertex1)).ToList();
        }

        /// <summary>
        /// Returns the edge data between two neighbouring vertices.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="vertex1"></param>
        /// <param name="vertex2"></param>
        /// <returns></returns>
        protected virtual IGraphEdgeData GetEdgeData(Vehicle vehicle, long vertex1, long vertex2)
        {
            // get the resolved graph for the given profile.
            var graph = this.GetForProfile(vehicle);

            if (vertex1 > 0 && vertex2 > 0)
            { // none of the vertixes was a resolved vertex.
                TEdgeData data;
                if(!this.GetEdge(_dataGraph, (uint)vertex1, (uint)vertex2, out data))
                { // try reverse edge.
                    TEdgeData reverse = default(TEdgeData);
                    if(!this.GetEdge(_dataGraph, (uint)vertex2, (uint)vertex1, out reverse))
                    {
                        throw new Exception(string.Format("Edge {0}->{1} not found!",
                            vertex1, vertex2));
                    }
                    return (TEdgeData)reverse.Reverse();
                }
                return data;
            }
            else
            { // one of the vertices was a resolved vertex.
                // edge should be in the resolved graph.
                var edges =  graph.GetEdges(vertex1);
                foreach (var edge in edges)
                {
                    if (edge.Key == vertex2)
                    {
                        return edge.Value;
                    }
                }
            }
            throw new Exception(string.Format("Edge {0}->{1} not found!",
                vertex1, vertex2));
        }

        /// <summary>
        /// Returns the edge shape between the two neighbouring vertices.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="vertex1"></param>
        /// <param name="vertex2"></param>
        /// <returns></returns>
        protected virtual ICoordinate[] GetEdgeShape(Vehicle vehicle, long vertex1, long vertex2)
        {
            // get the resolved graph for the given profile.
            var graph = this.GetForProfile(vehicle);

            if (vertex1 > 0 && vertex2 > 0)
            { // none of the vertixes was a resolved vertex.
                ICoordinateCollection shape;
                if (!this.GetEdgeShape(_dataGraph, (uint)vertex1, (uint)vertex2, out shape))
                { // try the reverse.
                    if (!this.GetEdgeShape(_dataGraph, (uint)vertex2, (uint)vertex1, out shape))
                    { // hmm information is missing! 
                        throw new Exception(string.Format("Edge {0}->{1} not found!",
                            vertex1, vertex2));
                    }
                    if (shape != null)
                    {
                        shape.Reset();
                        var list = shape.ToList();
                        list.Reverse();
                        return list.ToArray();
                    }
                }
                if (shape != null)
                {
                    shape.Reset();
                    return shape.ToArray();
                }
                return null;
            }
            else
            { // one of the vertices was a resolved vertex.
                // edge should be in the resolved graph.
                var edges =  graph.GetEdges(vertex1);
                foreach (var edge in edges)
                {
                    if (edge.Key == vertex2)
                    {
                        if (edge.Value.Coordinates == null)
                        {
                            return null;
                        }
                        return new CoordinateArrayCollection<GeoCoordinateSimple>(edge.Value.Coordinates).ToArray();
                    }
                }
            }
            throw new Exception(string.Format("Edge {0}->{1} not found!",
                vertex1, vertex2));
        }

        /// <summary>
        /// Returns the coordinate of the given vertex.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="vertex"></param>
        /// <returns></returns>
        protected virtual GeoCoordinate GetCoordinate(Vehicle vehicle, long vertex)
        {
            // get the resolved graph for the given profile.
            TypedRouterResolvedGraph graph = this.GetForProfile(vehicle);

            float latitude, longitude;
            if (vertex < 0)
            { // the vertex is resolved.
                if (!graph.GetVertex(vertex, out latitude, out longitude))
                {
                    throw new Exception(string.Format("Vertex with id {0} not found in resolved graph!",
                        vertex));
                }
            }
            else
            { // the vertex should be in the data graph.
                if (!_dataGraph.GetVertex(Convert.ToUInt32(vertex), out latitude, out longitude))
                {
                    throw new Exception(string.Format("Vertex with id {0} not found in graph!",
                        vertex));
                }
            }
            return new GeoCoordinate(latitude, longitude);
        }


        /// <summary>
        /// Returns an edge with a shape.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        protected virtual bool GetEdge(IGraphReadOnly<TEdgeData> graph, uint from, uint to, out TEdgeData data)
        {
            if (!graph.CanHaveDuplicates)
            {
                return graph.GetEdge(from, to, out data);
            }
            else
            {
                throw new NotSupportedException();
            }
        }

        /// <summary>
        /// Returns an edge with a shape.
        /// </summary>
        /// <param name="graph"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="data"></param>
        /// <returns></returns>
        protected virtual bool GetEdgeShape(IGraphReadOnly<TEdgeData> graph, uint from, uint to, out ICoordinateCollection data)
        {
            if (!graph.CanHaveDuplicates)
            {
                return graph.GetEdgeShape(from, to, out data);
            }
            else
            {
                throw new NotSupportedException();
            }
        }


        #endregion

        #region Resolving Points

        /// <summary>
        /// Holds all resolved points.
        /// </summary>
        private readonly Dictionary<string, Dictionary<GeoCoordinate, RouterPoint>> _routerPoints;

        /// <summary>
        /// Normalizes the router point.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        protected RouterPoint Normalize(Vehicle vehicle, RouterPoint point)
        {
            Dictionary<GeoCoordinate, RouterPoint> perLocation;
            if(!_routerPoints.TryGetValue(vehicle.UniqueName, out perLocation))
            {
                perLocation = new Dictionary<GeoCoordinate, RouterPoint>();
                perLocation.Add(point.Location, point);
                _routerPoints.Add(vehicle.UniqueName, perLocation);
                return point;
            }
            RouterPoint normalize;
            if (!perLocation.TryGetValue(point.Location, out normalize))
            {
                perLocation.Add(point.Location, point);
                normalize = point;
            }
            return normalize;
        }

        /// <summary>
        /// Returns a routerpoint for the given location and vehicle profile.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="location"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        protected bool GetRouterPoint(Vehicle vehicle, GeoCoordinate location, out RouterPoint point)
        {
            Dictionary<GeoCoordinate, RouterPoint> perLocation;
            point = null;
            return _routerPoints.TryGetValue(vehicle.UniqueName, out perLocation) &&
                perLocation != null &&
                perLocation.TryGetValue(location, out point);
        }

        /// <summary>
        /// Returns a routerpoint for the given resolvedId.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="resolvedId"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        protected bool GetRouterPoint(Vehicle vehicle, long resolvedId, out RouterPoint point)
        {
            Dictionary<GeoCoordinate, RouterPoint> perLocation;
            point = null;
            if (_routerPoints.TryGetValue(vehicle.UniqueName, out perLocation))
            {
                point = perLocation.Values.First(x => x.Id == resolvedId);
            }
            return point != null;
        }

        /// <summary>
        /// Resolves the given coordinate to the closest routable point.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="coordinate"></param>
        /// <returns></returns>
        public virtual RouterPoint Resolve(Vehicle vehicle, GeoCoordinate coordinate)
        {
            return this.Resolve(vehicle, coordinate, false);
        }

        /// <summary>
        /// Resolves the given coordinate to the closest routable point.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="coordinate"></param>
        /// <param name="verticesOnly"></param>
        /// <returns></returns>
        public virtual RouterPoint Resolve(Vehicle vehicle, GeoCoordinate coordinate, bool verticesOnly)
        {
            return this.Resolve(vehicle, this.DefaultSearchDelta, coordinate, null, null, verticesOnly);
        }

        /// <summary>
        /// Resolves the given coordinate to the closest routable point.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="delta"></param>
        /// <param name="coordinate"></param>
        /// <returns></returns>
        public virtual RouterPoint Resolve(Vehicle vehicle, float delta, GeoCoordinate coordinate)
        {
            return this.Resolve(vehicle, delta, coordinate, false);
        }

        /// <summary>
        /// Resolves the given coordinate to the closest routable point.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="delta"></param>
        /// <param name="coordinate"></param>
        /// <param name="verticesOnly"></param>
        /// <returns></returns>
        public virtual RouterPoint Resolve(Vehicle vehicle, float delta, GeoCoordinate coordinate, bool verticesOnly)
        {
            return this.Resolve(vehicle, delta, coordinate, null, null, verticesOnly);
        }

        /// <summary>
        /// Resolves the given coordinate to the closest routable point.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="coordinate"></param>
        /// <param name="pointTags"></param>
        /// <returns></returns>
        public virtual RouterPoint Resolve(Vehicle vehicle, GeoCoordinate coordinate, TagsCollectionBase pointTags)
        {
            return this.Resolve(vehicle, coordinate, pointTags, false);
        }

        /// <summary>
        /// Resolves the given coordinate to the closest routable point.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="coordinate"></param>
        /// <param name="pointTags"></param>
        /// <param name="verticesOnly"></param>
        /// <returns></returns>
        public virtual RouterPoint Resolve(Vehicle vehicle, GeoCoordinate coordinate, TagsCollectionBase pointTags, bool verticesOnly)
        {
            return this.Resolve(vehicle, this.DefaultSearchDelta, coordinate, pointTags, verticesOnly);
        }

        /// <summary>
        /// Resolves the given coordinate to the closest routable point.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="delta"></param>
        /// <param name="coordinate"></param>
        /// <param name="pointTags"></param>
        /// <returns></returns>
        public virtual RouterPoint Resolve(Vehicle vehicle, float delta, GeoCoordinate coordinate, TagsCollectionBase pointTags)
        {
            return this.Resolve(vehicle, delta, coordinate, pointTags, false);
        }

        /// <summary>
        /// Resolves the given coordinate to the closest routable point.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="delta"></param>
        /// <param name="coordinate"></param>
        /// <param name="pointTags"></param>
        /// <param name="verticesOnly"></param>
        /// <returns></returns>
        public virtual RouterPoint Resolve(Vehicle vehicle, float delta, GeoCoordinate coordinate, TagsCollectionBase pointTags, bool verticesOnly)
        {
            return this.Resolve(vehicle, delta, coordinate, null, pointTags, verticesOnly);
        }

        /// <summary>
        /// Resolves the given coordinate to the closest routable point.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="coordinate"></param>
        /// <param name="matcher"></param>
        /// <param name="matchingTags"></param>
        /// <returns></returns>
        public virtual RouterPoint Resolve(Vehicle vehicle, GeoCoordinate coordinate,
            IEdgeMatcher matcher, TagsCollectionBase matchingTags)
        {
            return this.Resolve(vehicle, coordinate, matcher, matchingTags, false);
        }

        /// <summary>
        /// Resolves the given coordinate to the closest routable point.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="coordinate"></param>
        /// <param name="matcher"></param>
        /// <param name="matchingTags"></param>
        /// <param name="verticesOnly"></param>
        /// <returns></returns>
        public virtual RouterPoint Resolve(Vehicle vehicle, GeoCoordinate coordinate,
            IEdgeMatcher matcher, TagsCollectionBase matchingTags, bool verticesOnly)
        {
            return this.Resolve(vehicle, this.DefaultSearchDelta, coordinate,
                                matcher, matchingTags, verticesOnly);
        }

        /// <summary>
        /// Resolves the given coordinate to the closest routable point.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="delta"></param>
        /// <param name="coordinate"></param>
        /// <param name="matcher"></param>
        /// <param name="matchingTags"></param>
        /// <returns></returns>
        public virtual RouterPoint Resolve(Vehicle vehicle, float delta, GeoCoordinate coordinate,
                                   IEdgeMatcher matcher, TagsCollectionBase matchingTags)
        {
            return this.Resolve(vehicle, delta, coordinate, matcher, matchingTags, false);
        }

        /// <summary>
        /// Resolves the given coordinate to the closest routable point.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="delta"></param>
        /// <param name="coordinate"></param>
        /// <param name="matcher"></param>
        /// <param name="matchingTags"></param>
        /// <param name="verticesOnly"></param>
        /// <returns></returns>
        public virtual RouterPoint Resolve(Vehicle vehicle, float delta, GeoCoordinate coordinate,
                                   IEdgeMatcher matcher, TagsCollectionBase matchingTags, bool verticesOnly)
        {
            // check routing profiles.
            if (!this.SupportsVehicle(vehicle))
            {
                throw new ArgumentOutOfRangeException("vehicle", string.Format("Routing profile {0} not supported by this router!",
                    vehicle.ToString()));
            }

            var result = _router.SearchClosest(_dataGraph, _interpreter,
                vehicle, coordinate, delta, matcher, matchingTags, verticesOnly, null); // search the closest routable object.
            if (result.Distance < double.MaxValue)
            { // a routable object was found.
                if (!result.Vertex2.HasValue)
                { // the result was a single vertex.
                    float latitude, longitude;
                    if (!_dataGraph.GetVertex(result.Vertex1.Value, out latitude, out longitude))
                    { // the vertex exists.
                        throw new Exception(string.Format("Vertex with id {0} not found!",
                            result.Vertex1.Value));
                    }
                    return this.Normalize(vehicle, new RouterPoint(result.Vertex1.Value, vehicle, new GeoCoordinate(latitude, longitude)));
                }
                else if(result.IntermediateIndex.HasValue)
                { // an exact match with an intermediate coordinate.
                    return this.AddResolvedPointExact(vehicle, result.Vertex1.Value, result.Vertex2.Value, new GeoCoordinate(
                        result.Coordinates[result.IntermediateIndex.Value].Latitude, result.Coordinates[result.IntermediateIndex.Value].Longitude), result.Edge);
                }
                else
                { // the result is on an edge.
                    return this.AddResolvedPoint(vehicle, result.Vertex1.Value, result.Vertex2.Value, result.Position, result.Edge);
                }
            }
            return null; // no routable object was found closeby.
        }

        /// <summary>
        /// Resolves the given coordinates to the closest routable points.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="coordinate"></param>
        /// <returns></returns>
        public virtual RouterPoint[] Resolve(Vehicle vehicle, GeoCoordinate[] coordinate)
        {
            return this.Resolve(vehicle, this.DefaultSearchDelta, coordinate);
        }

        /// <summary>
        /// Resolves the given coordinates to the closest routable points.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="delta"></param>
        /// <param name="coordinate"></param>
        /// <returns></returns>
        public virtual RouterPoint[] Resolve(Vehicle vehicle, float delta, GeoCoordinate[] coordinate)
        {
            var points = new RouterPoint[coordinate.Length];
            for (int idx = 0; idx < coordinate.Length; idx++)
            {
                points[idx] = this.Resolve(vehicle, delta, coordinate[idx]);
            }
            return points;
        }

        /// <summary>
        /// Resolves the given coordinates to the closest routable points.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="coordinate"></param>
        /// <param name="matcher"></param>
        /// <param name="matchingTags"></param>
        /// <returns></returns>
        public virtual RouterPoint[] Resolve(Vehicle vehicle, GeoCoordinate[] coordinate,
            IEdgeMatcher matcher, TagsCollectionBase[] matchingTags)
        {
            return this.Resolve(vehicle, this.DefaultSearchDelta, coordinate,
                                matcher, matchingTags);
        }

        /// <summary>
        /// Resolves the given coordinates to the closest routable points.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="delta"></param>
        /// <param name="coordinate"></param>
        /// <param name="matcher"></param>
        /// <param name="matchingTags"></param>
        /// <returns></returns>
        public virtual RouterPoint[] Resolve(Vehicle vehicle, float delta, GeoCoordinate[] coordinate,
                                     IEdgeMatcher matcher, TagsCollectionBase[] matchingTags)
        {
            var points = new RouterPoint[coordinate.Length];
            for (int idx = 0; idx < coordinate.Length; idx++)
            {
                points[idx] = this.Resolve(vehicle, delta, coordinate[idx], matcher, matchingTags[idx]);
            }
            return points;
        }

        /// <summary>
        /// Find the coordinates of the closest routable point.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="coordinate"></param>
        /// <returns></returns>
        public virtual GeoCoordinate Search(Vehicle vehicle, GeoCoordinate coordinate)
        {
            return this.Search(vehicle, coordinate, false);
        }

        /// <summary>
        /// Find the coordinates of the closest routable point.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="delta"></param>
        /// <param name="coordinate"></param>
        /// <returns></returns>
        public virtual GeoCoordinate Search(Vehicle vehicle, float delta, GeoCoordinate coordinate)
        {
            return this.Search(vehicle, delta, coordinate, false);
        }

        /// <summary>
        /// Find the coordinates of the closest routable point.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="coordinate"></param>
        /// <param name="verticesOnly"></param>
        /// <returns></returns>
        public virtual GeoCoordinate Search(Vehicle vehicle, GeoCoordinate coordinate, bool verticesOnly)
        {
            // check routing profiles.
            if (!this.SupportsVehicle(vehicle))
            {
                throw new ArgumentOutOfRangeException("vehicle", string.Format("Routing profile {0} not supported by this router!",
                    vehicle.ToString()));
            }

            return this.Search(vehicle,this.DefaultSearchDelta, coordinate, verticesOnly);
        }

        /// <summary>
        /// Find the coordinates of the closest routable point.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="delta"></param>
        /// <param name="coordinate"></param>
        /// <param name="verticesOnly"></param>
        /// <returns></returns>
        public virtual GeoCoordinate Search(Vehicle vehicle, float delta, GeoCoordinate coordinate, bool verticesOnly)
        {
            // check routing profiles.
            if (!this.SupportsVehicle(vehicle))
            {
                throw new ArgumentOutOfRangeException("vehicle", string.Format("Routing profile {0} not supported by this router!",
                    vehicle.ToString()));
            }

            // search for a close edge/vertex.
            var result = _router.SearchClosest(_dataGraph, _interpreter, vehicle, coordinate,
                delta, null, null, verticesOnly, null); // search the closest routable object.
            if (result.Distance < double.MaxValue)
            { // a routable object was found.
                if (!result.Vertex2.HasValue)
                { // the result was a single vertex.
                    float latitude, longitude;
                    if (!_dataGraph.GetVertex(result.Vertex1.Value, out latitude, out longitude))
                    { // the vertex exists.
                        throw new Exception(string.Format("Vertex with id {0} not found!",
                            result.Vertex1.Value));
                    }
                    return new GeoCoordinate(latitude, longitude);
                }
                else
                { // the result is on an edge.
                    throw new NotImplementedException();
                }
            }
            return null; // no routable object was found closeby.
        }

        #region Resolved Points Graph

        /// <summary>
        /// Holds the id of the next resolved point.
        /// </summary>
        private long _nextResolvedId = -1;

        /// <summary>
        /// Returns the next resolved id.
        /// </summary>
        /// <returns></returns>
        protected long GetNextResolvedId()
        {
            long next = _nextResolvedId;
            _nextResolvedId--;
            return next;
        }

        /// <summary>
        /// Holds the intermediate points ids.
        /// </summary>
        private const long IntermediatePoints = long.MinValue + (long.MaxValue / 2);

        /// <summary>
        /// Holds the id of the next intermediate point.
        /// </summary>
        private long _nextIntermediateId = long.MinValue;

        /// <summary>
        /// Returns the next intermediate id.
        /// </summary>
        /// <returns></returns>
        protected long GetNextIntermediateId()
        {
            long next = _nextIntermediateId;
            _nextIntermediateId++;
            return next;
        }

        /// <summary>
        /// Returns true if the given vertex is an intermediate.
        /// </summary>
        /// <param name="vertex"></param>
        /// <returns></returns>
        protected bool IsIntermediate(long vertex)
        {
            return vertex >= long.MinValue && vertex <= IntermediatePoints;
        }

        /// <summary>
        /// Holds the resolved graphs per used vehicle type.
        /// </summary>
        private readonly Dictionary<Vehicle, TypedRouterResolvedGraph> _resolvedGraphs;

        /// <summary>
        /// Gets/creates a TypedRouterResolvedGraph for the given profile.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <returns></returns>
        protected TypedRouterResolvedGraph GetForProfile(Vehicle vehicle)
        {
            TypedRouterResolvedGraph graph;
            if (!_resolvedGraphs.TryGetValue(vehicle, out graph))
            {
                graph = new TypedRouterResolvedGraph();
                _resolvedGraphs.Add(vehicle, graph);
            }
            return graph;
        }

        /// <summary>
        /// Adds a resolved point to the graph.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="vertex1"></param>
        /// <param name="vertex2"></param>
        /// <param name="resolvedCoordinate"></param>
        /// <param name="edgeData"></param>
        /// <returns></returns>
        protected RouterPoint AddResolvedPointExact(Vehicle vehicle, uint vertex1, uint vertex2, GeoCoordinate resolvedCoordinate, TEdgeData edgeData)
        {
            Meter epsilon = 1;

            // get the resolved graph for the given profile.
            var graph = this.GetForProfile(vehicle);

            // calculate a shortest path but make sure that is aligned with the coordinates in the edge.
            PathSegment<long> path = null;
            var intermediates = new List<long>();
            var edgeCoordinates = this.GetEdgeShape(vehicle, vertex1, vertex2);
            if (edgeCoordinates != null)
            { // the resolved edge has intermediate coordinates.
                RouterPoint intermediaRouterpoint;
                for (int idx = 0; idx < edgeCoordinates.Length; idx++)
                {
                    if (this.GetRouterPoint(vehicle, new GeoCoordinate(edgeCoordinates[idx].Latitude, edgeCoordinates[idx].Longitude),
                        out intermediaRouterpoint))
                    {
                        intermediates.Add(intermediaRouterpoint.Id);
                    }
                }

                // check if there are intermediate points, if yes calculate route along points.
                if (intermediates.Count > 0)
                {
                    long previousId = vertex1;
                    PathSegment<long> currentRoute = null;
                    for (int idx = 0; idx < intermediates.Count; idx++)
                    {
                        long currentId = intermediates[idx];
                        currentRoute = this.ResolvedShortest(vehicle, previousId, currentId);
                        if(currentRoute == null)
                        { // stop, the current route is not yet in the resolved graph.
                            path = null;
                            break;
                        }
                        if (path == null)
                        {
                            path = currentRoute;
                        }
                        else
                        {
                            path = currentRoute.ConcatenateAfter(path);
                        }
                        previousId = currentId;
                    }
                    currentRoute = this.ResolvedShortest(vehicle, previousId, vertex2);
                    if (currentRoute != null)
                    { // there is a current route.
                        path = currentRoute.ConcatenateAfter(path);
                    }
                    else
                    { // stop, the current route is not yet in the resolved graph.
                        path = null;
                    }
                }
            }
            else
            { // calculate a route between the two points and make sure there are no other intermediate points in between.
                // there can be only one edge without intermediate points.
                path = this.ResolvedShortest(vehicle, vertex1, vertex2);
                if (path != null)
                {
                    foreach (long vertex in path.ToArray())
                    {
                        if (vertex < TypedRouter<TEdgeData>.IntermediatePoints)
                        { // oeps, another intermediate point, discard current path.
                            path = null;
                            break;
                        }
                    }
                }
            }

            // augement path if any to include resolved point.
            var vertices = new long[0];
            if (path != null)
            { // the vertices in this path.
                vertices = path.ToArray();
            }

            // should contain vertex1 and vertex2.
            if (vertices.Length > 0 &&
                (vertices[0] != vertex1 || vertices[vertices.Length - 1] != vertex2))
            {
                throw new Exception("A shortest path between two vertices has to contain at least the source and target!");
            }

            // the vertices match.
            float longitude1, latitude1, longitude2, latitude2;
            if (!_dataGraph.GetVertex(vertex1, out latitude1, out longitude1) ||
                !_dataGraph.GetVertex(vertex2, out latitude2, out longitude2))
            { // oeps, one of the vertices is not routable!
                throw new Exception("A resolved position can only exist on an arc between two routable vertices.");
            }
            var vertex1Coordinate = new GeoCoordinate(
                latitude1, longitude1);
            var vertex2Coordinate = new GeoCoordinate(
                latitude2, longitude2);

            if (vertices.Length == 0)
            { // the path has a length of 0; the vertices are not in the resolved graph yet!
                // add the vertices in the resolved graph.
                float latitudeDummy, longitudeDummy;
                if (!graph.GetVertex(vertex1, out latitudeDummy, out longitudeDummy))
                {
                    graph.AddVertex(vertex1, latitude1, longitude1);
                }
                if (!graph.GetVertex(vertex2, out latitudeDummy, out longitudeDummy))
                {
                    graph.AddVertex(vertex2, latitude2, longitude2);
                }

                if (edgeCoordinates != null)
                { // add intermediate points.
                    // create the route manually.
                    vertices = new long[2 + edgeCoordinates.Length];
                    vertices[0] = vertex1;

                    long previousVertex = vertex1;
                    for (int idx = 0; idx < edgeCoordinates.Length; idx++)
                    {
                        long intermediateId = this.GetNextIntermediateId();
                        graph.AddVertex(intermediateId, edgeCoordinates[idx].Latitude, edgeCoordinates[idx].Longitude);
                        graph.AddEdge(previousVertex, intermediateId,
                            new TypedRouterResolvedGraph.RouterResolvedGraphEdge(edgeData.Tags,
                                                                                 edgeData.Forward));
                        graph.AddEdge(intermediateId, previousVertex,
                            new TypedRouterResolvedGraph.RouterResolvedGraphEdge(edgeData.Tags,
                                                                                 !edgeData.Forward));
                        vertices[idx + 1] = intermediateId;

                        // add as a resolved point.
                        this.Normalize(vehicle, new RouterPoint(intermediateId, vehicle, new GeoCoordinate(
                            edgeCoordinates[idx].Latitude, edgeCoordinates[idx].Longitude)));

                        previousVertex = intermediateId;
                    }
                    graph.AddEdge(previousVertex, vertex2,
                        new TypedRouterResolvedGraph.RouterResolvedGraphEdge(edgeData.Tags,
                                                                             edgeData.Forward));
                    graph.AddEdge(vertex2, previousVertex,
                        new TypedRouterResolvedGraph.RouterResolvedGraphEdge(edgeData.Tags,
                                                                             !edgeData.Forward));
                    vertices[vertices.Length - 1] = vertex2;
                }
                else
                { // no intermediate points, just create the route manually.
                    vertices = new long[2];
                    vertices[0] = vertex1;
                    vertices[1] = vertex2;
                }
            }
            else if (vertices.Length == 2)
            { // paths of length two are impossible!
                throw new Exception("A resolved position can only exist on an arc between two routable vertices.");
            }

            // calculate positions/resolved coordinates.
            float latitude, longitude;

            // check if the resolved coordinates is close to one of the vertices.
            for (int idx = 0; idx < vertices.Length; idx++)
            {
                graph.GetVertex(vertices[idx], out latitude, out longitude);
                if (new GeoCoordinate(latitude, longitude).DistanceReal(resolvedCoordinate).Value < epsilon.Value)
                { // distance to this vertex is small enough to consider the equal.
                    return new RouterPoint(vertices[idx], vehicle, new GeoCoordinate(latitude, longitude));
                }
            }
            throw new Exception("Intermediate point not in graph!");
        }

        /// <summary>
        /// Adds a resolved point to the graph.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="vertex1"></param>
        /// <param name="vertex2"></param>
        /// <param name="position"></param>
        /// <param name="edgeData"></param>
        /// <returns></returns>
        protected RouterPoint AddResolvedPoint(Vehicle vehicle, uint vertex1, uint vertex2, double position, TEdgeData edgeData)
        {
            Meter epsilon = 1;

            // get the resolved graph for the given profile.
            var graph = this.GetForProfile(vehicle);

            // calculate a shortest path but make sure that is aligned with the coordinates in the edge.
            PathSegment<long> path = null;
            var intermediates = new List<long>();
            var edgeCoordinates = this.GetEdgeShape(vehicle, vertex1, vertex2);
            if (edgeCoordinates != null && edgeCoordinates.Length > 0)
            { // the resolved edge has intermediate coordinates.
                RouterPoint intermediaRouterpoint;
                for (int idx = 0; idx < edgeCoordinates.Length; idx++)
                {
                    if (this.GetRouterPoint(vehicle, new GeoCoordinate(edgeCoordinates[idx].Latitude, edgeCoordinates[idx].Longitude), 
                        out intermediaRouterpoint))
                    {
                        intermediates.Add(intermediaRouterpoint.Id);
                    }
                }

                // check if there are intermediate points, if yes calculate route along points.
                if(intermediates.Count > 0)
                {
                    long previousId = vertex1;
                    for(int idx = 0; idx < intermediates.Count; idx++)
                    {
                        long currentId = intermediates[idx];
                        var currentRoute = this.ResolvedShortest(vehicle, previousId, currentId);
                        if(path == null)
                        {
                            path = currentRoute;
                        }
                        else
                        {
                            path = currentRoute.ConcatenateAfter(path);
                        }
                        previousId = currentId;
                    }
                    var resolvedShortest = this.ResolvedShortest(vehicle, previousId, vertex2);
                    path = resolvedShortest.ConcatenateAfter(path);
                }
            }
            else
            { // calculate a route between the two points and make sure there are no other intermediate points in between.
                // there can be only one edge without intermediate points.
                path = this.ResolvedShortest(vehicle, vertex1, vertex2);
                if(path != null)
                {
                    foreach(long vertex in path.ToArray())
                    {
                        if(vertex < TypedRouter<TEdgeData>.IntermediatePoints)
                        { // oeps, another intermediate point, discard current path.
                            path = null;
                            break;
                        }
                    }
                }
            }

            // augement path if any to include resolved point.
            var vertices = new long[0];
            if (path != null)
            { // the vertices in this path.
                vertices = path.ToArray();
            }

            // should contain vertex1 and vertex2.
            if (vertices.Length > 0 &&
                (vertices[0] != vertex1 || vertices[vertices.Length - 1] != vertex2))
            {
                throw new Exception("A shortest path between two vertices has to contain at least the source and target!");
            }

            // the vertices match.
            float longitude1, latitude1, longitude2, latitude2;
            if (!_dataGraph.GetVertex(vertex1, out latitude1, out longitude1) ||
                !_dataGraph.GetVertex(vertex2, out latitude2, out longitude2))
            { // oeps, one of the vertices is not routable!
                throw new Exception("A resolved position can only exist on an arc between two routable vertices.");
            }
            var vertex1Coordinate = new GeoCoordinate(
                latitude1, longitude1);
            var vertex2Coordinate = new GeoCoordinate(
                latitude2, longitude2);

            if (vertices.Length == 0)
            { // the path has a length of 0; the vertices are not in the resolved graph yet!
                // add the vertices in the resolved graph.
                float latitudeDummy, longitudeDummy;
                if (!graph.GetVertex(vertex1, out latitudeDummy, out longitudeDummy))
                {
                    graph.AddVertex(vertex1, latitude1, longitude1);
                }
                if (!graph.GetVertex(vertex2, out latitudeDummy, out longitudeDummy))
                {
                    graph.AddVertex(vertex2, latitude2, longitude2);
                }

                if (edgeCoordinates != null && edgeCoordinates.Length > 0)
                { // add intermediate points.
                    // create the route manually.
                    vertices = new long[2 + edgeCoordinates.Length];
                    vertices[0] = vertex1;

                    long previousVertex = vertex1;
                    for (int idx = 0; idx < edgeCoordinates.Length; idx++)
                    {
                        long intermediateId = this.GetNextIntermediateId();
                        graph.AddVertex(intermediateId, edgeCoordinates[idx].Latitude, edgeCoordinates[idx].Longitude);
                        graph.AddEdge(previousVertex, intermediateId,
                            new TypedRouterResolvedGraph.RouterResolvedGraphEdge(edgeData.Tags,
                                                                                 edgeData.Forward));
                        graph.AddEdge(intermediateId, previousVertex,
                            new TypedRouterResolvedGraph.RouterResolvedGraphEdge(edgeData.Tags,
                                                                                 !edgeData.Forward));
                        vertices[idx + 1] = intermediateId;

                        // add as a resolved point.
                        this.Normalize(vehicle, new RouterPoint(intermediateId, vehicle, new GeoCoordinate(
                            edgeCoordinates[idx].Latitude, edgeCoordinates[idx].Longitude)));

                        previousVertex = intermediateId;
                    }
                    graph.AddEdge(previousVertex, vertex2,
                        new TypedRouterResolvedGraph.RouterResolvedGraphEdge(edgeData.Tags,
                                                                             edgeData.Forward));
                    graph.AddEdge(vertex2, previousVertex,
                        new TypedRouterResolvedGraph.RouterResolvedGraphEdge(edgeData.Tags,
                                                                             !edgeData.Forward));
                    vertices[vertices.Length - 1] = vertex2;
                }
                else
                { // no intermediate points, just create the route manually.
                    vertices = new long[2];
                    vertices[0] = vertex1;
                    vertices[1] = vertex2;
                }
            }
            else if (vertices.Length == 2)
            { // paths of length two are impossible!
                throw new Exception("A resolved position can only exist on an arc between two routable vertices.");
            }

            // calculate positions/resolved coordinates.
            int positionIdx = 0;
            double totalDistance = 0;
            float latitude, longitude;
            var previous = new GeoCoordinate(latitude1, longitude1);
            GeoCoordinate current;
            if (edgeCoordinates != null)
            {
                for (int idx = 0; idx < edgeCoordinates.Length; idx++)
                {
                    current = new GeoCoordinate(edgeCoordinates[idx].Latitude, edgeCoordinates[idx].Longitude);
                    totalDistance = totalDistance + current.DistanceReal(previous).Value;
                    previous = current;
                }
            }
            current = new GeoCoordinate(latitude2, longitude2);
            totalDistance = totalDistance + current.DistanceReal(previous).Value;

            double currentDistance = 0;
            graph.GetVertex(vertices[0], out latitude, out longitude);
            previous = new GeoCoordinate(latitude, longitude);
            GeoCoordinate resolvedCoordinate = null; 
            for (int idx = 1; idx < vertices.Length; idx++)
            {
                graph.GetVertex(vertices[idx], out latitude, out longitude);
                current = new GeoCoordinate(latitude, longitude);
                var previousDistance = currentDistance;
                currentDistance = currentDistance + current.DistanceReal(previous).Value;
                var ratio = currentDistance / totalDistance;
                if(ratio >= position)
                { // the resolved position has been reached.
                    positionIdx = idx - 1;
                    var ratioBefore = previousDistance / totalDistance;
                    var ratioLocal = (position - ratioBefore) / (ratio - ratioBefore);
                    resolvedCoordinate = new GeoCoordinate(
                        previous.Latitude * (1.0 - ratioLocal) + current.Latitude * ratioLocal,
                        previous.Longitude * (1.0 - ratioLocal) + current.Longitude * ratioLocal);
                    break;
                }
                previous = current;
            }

            // check if the resolved coordinates is close to one of the vertices.
            for(int idx = 0; idx < vertices.Length; idx++)
            {
                graph.GetVertex(vertices[idx], out latitude, out longitude);
                if(new GeoCoordinate(latitude, longitude).DistanceReal(resolvedCoordinate).Value < epsilon.Value)
                { // distance to this vertex is small enough to consider the equal.
                    return new RouterPoint(vertices[idx], vehicle, new GeoCoordinate(latitude, longitude));
                }
            }

            // get the vertices and the arc between them.
            long vertexFrom = vertices[positionIdx];
            long vertexTo = vertices[positionIdx + 1];

            // remove the arc.
            graph.DeleteEdge(vertexFrom, vertexTo);
            graph.DeleteEdge(vertexTo, vertexFrom);

            // add new vertex.
            long resolvedVertex = this.GetNextResolvedId();
            graph.AddVertex(resolvedVertex, (float)resolvedCoordinate.Latitude,
                (float)resolvedCoordinate.Longitude);

            // add the arcs.
            graph.AddEdge(vertexFrom, resolvedVertex,
                                  new TypedRouterResolvedGraph.RouterResolvedGraphEdge(
                                      edgeData.Tags,
                                      edgeData.Forward));
            graph.AddEdge(resolvedVertex, vertexFrom,
                                  new TypedRouterResolvedGraph.RouterResolvedGraphEdge(
                                      edgeData.Tags,
                                      !edgeData.Forward));
            graph.AddEdge(resolvedVertex, vertexTo,
                                  new TypedRouterResolvedGraph.RouterResolvedGraphEdge(
                                      edgeData.Tags,
                                      edgeData.Forward));
            graph.AddEdge(vertexTo, resolvedVertex,
                                  new TypedRouterResolvedGraph.RouterResolvedGraphEdge(
                                      edgeData.Tags,
                                      !edgeData.Forward));

            return this.Normalize(vehicle,
                        new RouterPoint(resolvedVertex, vehicle, resolvedCoordinate));
        }

        #region Resolved Graph Routing

        /// <summary>
        /// Calculates all routes from a given resolved point to the routable graph.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="resolvedPoint"></param>
        /// <param name="backwards"></param>
        /// <returns></returns>
        protected PathSegmentVisitList RouteResolvedGraph(Vehicle vehicle, RouterPoint resolvedPoint, bool? backwards)
        {
            // get the resolved graph for the given profile.
            TypedRouterResolvedGraph graph = this.GetForProfile(vehicle);

            // initialize the resulting visit list.
            var result = new PathSegmentVisitList();

            // do a simple dykstra search and add all found routable vertices to the visit list.
            var settled = new HashSet<long>();
            var visitList = new PathSegmentVisitList();

            var current = new PathSegment<long>(resolvedPoint.Id);
            visitList.UpdateVertex(current);

            while (true)
            {
                // return the vertex on top of the list.
                current = visitList.GetFirst();
                // update the settled list.
                if (current != null) { settled.Add(current.VertexId); }
                while (current != null && current.VertexId > 0)
                {
                    // add to the visit list.
                    result.UpdateVertex(current);

                    // choose a new current one.
                    current = visitList.GetFirst();
                    // update the settled list.
                    if (current != null) { settled.Add(current.VertexId); }
                }

                // check if it is the target.
                if (current == null)
                { // current is empty; target not found!
                    return result;
                }

                // get the neighbours.
                KeyValuePair<long, TypedRouterResolvedGraph.RouterResolvedGraphEdge>[] arcs =
                    graph.GetEdges(current.VertexId);
                float latitude, longitude;
                graph.GetVertex(current.VertexId, out latitude, out longitude);
                var currentCoordinates = new GeoCoordinate(latitude, longitude);
                for (int idx = 0; idx < arcs.Length; idx++)
                {
                    KeyValuePair<long, TypedRouterResolvedGraph.RouterResolvedGraphEdge> arc = arcs[idx];
                    if (!settled.Contains(arc.Key))
                    {
                        // check oneway.
                        TagsCollectionBase tags = _dataGraph.TagsIndex.Get(arc.Value.Tags);
                        bool? oneway = vehicle.IsOneWay(tags);
                        if (!oneway.HasValue || (!backwards.HasValue || 
                            ((!backwards.Value && oneway.Value == arc.Value.Forward) ||
                            (backwards.Value && oneway.Value != arc.Value.Forward))))
                        { // ok edge is not oneway or oneway in the right direction.
                            graph.GetVertex(arc.Key, out latitude, out longitude);
                            var neighbourCoordinates = new GeoCoordinate(latitude, longitude);

                            // calculate the weight.
                            double weight = vehicle.Weight(tags, currentCoordinates, neighbourCoordinates);

                            visitList.UpdateVertex(new PathSegment<long>(arc.Key,
                                                                         weight + current.Weight, current));
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Calculates all routes from all the given resolved points to the routable graph.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="resolvedPoints"></param>
        /// <param name="backwards"></param>
        /// <returns></returns>
        protected PathSegmentVisitList[] RouteResolvedGraph(Vehicle vehicle, RouterPoint[] resolvedPoints, bool backwards)
        {
            var visitLists = new PathSegmentVisitList[resolvedPoints.Length];
            for (int idx = 0; idx < resolvedPoints.Length; idx++)
            {
                visitLists[idx] = this.RouteResolvedGraph(vehicle, resolvedPoints[idx], backwards);
            }
            return visitLists;
        }

        ///// <summary>
        ///// Returns the 'real' other neighour of vertex1 in the direction of resolvedVertex.
        ///// </summary>
        ///// <param name="vehicle"></param>
        ///// <param name="vertex1"></param>
        ///// <param name="resolvedVertex"></param>
        ///// <returns></returns>
        //private long GetRealNeighbour(Vehicle vehicle, long vertex1, long resolvedVertex)
        //{
        //    RouterPoint resolvedPoint;
        //    if (!this.GetRouterPoint(resolvedVertex, out resolvedPoint))
        //    { // oeps, point not found!
        //        throw new Exception("Resolved point detected but not found as a router point!");
        //    }
        //    var visitList = this.RouteResolvedGraph(vehicle, resolvedPoint, null);
        //    var visitSet = new HashSet<long>(visitList.GetVertices());
        //    if(visitSet.Count == 2)
        //    { // two points found.
        //        visitSet.Remove(vertex1);
        //    } 
        //    else if(visitSet.Count == 0)
        //    { // oeps, router point found without neighours!
        //        throw new Exception("Resolved point detected but did not find neighbours!");
        //    }
        //    else if (visitSet.Count > 2)
        //    { // oeps, router point found but with more than 2 neighours!
        //        throw new Exception("Resolved point detected but with more than 2 neighours!");
        //    }
        //    return visitSet.First();
        //}

        /// <summary>
        /// Calculates the shortest path between two points in the resolved vertex.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="vertex1"></param>
        /// <param name="vertex2"></param>
        /// <returns></returns>
        protected PathSegment<long> ResolvedShortest(Vehicle vehicle, long vertex1, long vertex2)
        {
            // get the resolved graph for the given profile.
            var graph = this.GetForProfile(vehicle);

            var settled = new HashSet<long>();
            var current = new PathSegment<long>(vertex1);
            var visit_list = new PathSegmentVisitList(vertex1, vertex1);
            visit_list.UpdateVertex(current);

            while (true)
            {
                // return the vertex on top of the list.
                current = visit_list.GetFirst();

                // check if it is the target.
                if (current == null)
                {
                    // current is empty; target not found!
                    return null;
                }
                if (current.VertexId == vertex2)
                {
                    // current is the target.
                    return current;
                }

                // update the settled list.
                settled.Add(current.VertexId);

                // get the neighbours.
                var edges =  graph.GetEdges(current.VertexId);
                float latitudeCurrent, longitudeCurrent;
                graph.GetVertex(current.VertexId, out latitudeCurrent, out longitudeCurrent);
                for (int idx = 0; idx < edges.Length; idx++)
                {
                    var edge = edges[idx];
                    if (!settled.Contains(edge.Key) && (edge.Key < 0 || edge.Key == vertex2))
                    {
                        float latitudeNeighbour, longitudeNeighbour;
                        graph.GetVertex(edge.Key, out latitudeNeighbour, out longitudeNeighbour);

                        double arcWeight = vehicle.Weight(_dataGraph.TagsIndex.Get(edge.Value.Tags),
                            new GeoCoordinate(latitudeCurrent, longitudeCurrent), new GeoCoordinate(latitudeNeighbour, longitudeNeighbour));

                        visit_list.UpdateVertex(new PathSegment<long>(edge.Key, arcWeight + current.Weight, current));
                    }
                }
            }
        }

        #endregion

        #endregion

        #endregion
    }
}