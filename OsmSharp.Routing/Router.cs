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

using OsmSharp.Collections.Tags;
using OsmSharp.Collections.Tags.Index;
using OsmSharp.Math.Geo;
using OsmSharp.Osm.Streams;
using OsmSharp.Routing.CH;
using OsmSharp.Routing.CH.Preprocessing;
using OsmSharp.Routing.Graph;
using OsmSharp.Routing.Graph.Routing;
using OsmSharp.Routing.Interpreter;
using OsmSharp.Routing.Osm.Interpreter;
using OsmSharp.Routing.Osm.Streams;
using OsmSharp.Routing.Routers;
using OsmSharp.Routing.Vehicles;
using System;
using System.Collections.Generic;
using System.IO;

namespace OsmSharp.Routing
{
    /// <summary>
    /// Interface representing a router.
    /// </summary>
    public class Router : ITypedRouter
    {
        /// <summary>
        /// Holds the basic router implementation.
        /// </summary>
        private readonly ITypedRouter _router;

        /// <summary>
        /// Creates a new router with the given router implementation.
        /// </summary>
        /// <param name="router"></param>
        public Router(ITypedRouter router)
        {
            _router = router;
        }

        #region Static Creation Methods

        /// <summary>
        /// Creates a router using interpreted edges.
        /// </summary>
        /// <param name="reader">The OSM-stream reader.</param>
        /// <param name="interpreter">The routing interpreter.</param>
        /// <returns></returns>
        public static Router CreateFrom(OsmStreamSource reader, IOsmRoutingInterpreter interpreter)
        {
            var tagsIndex = new TagsIndex(); // creates a tagged index.

            // read from the OSM-stream.
            var memoryData = new RouterDataSource<Edge>(new Graph<Edge>(), tagsIndex);
            var targetData = new GraphOsmStreamTarget(memoryData, interpreter, tagsIndex);
            targetData.RegisterSource(reader);
            targetData.Pull();

            // creates the edge router.
            var typedRouter = new TypedRouterEdge(
                memoryData, interpreter, new Dykstra());

            return new Router(typedRouter); // create the actual router.
        }

        /// <summary>
        /// Creates a router using interpreted edges.
        /// </summary>
        /// <param name="data">The data to route on.</param>
        /// <param name="interpreter">The routing interpreter.</param>
        /// <returns></returns>
        public static Router CreateFrom(IRoutingAlgorithmData<Edge> data, IRoutingInterpreter interpreter)
        {
            // creates the edge router.
            var typedRouter = new TypedRouterEdge(
                data, interpreter, new Dykstra());

            return new Router(typedRouter); // create the actual router.
        }

        /// <summary>
        /// Creates a router using interpreted edges.
        /// </summary>
        /// <param name="data">The data to route on.</param>
        /// <param name="basicRouter">A custom routing implementation.</param>
        /// <param name="interpreter">The routing interpreter.</param>
        /// <returns></returns>
        public static Router CreateFrom(IRoutingAlgorithmData<Edge> data, IRoutingAlgorithm<Edge> basicRouter, 
            IRoutingInterpreter interpreter)
        {
            // creates the edge router.
            var typedRouter = new TypedRouterEdge(
                data, interpreter, basicRouter);

            return new Router(typedRouter); // create the actual router.
        }

        /// <summary>
        /// Creates a router using interpreted edges.
        /// </summary>
        /// <param name="data">The data to route on.</param>
        /// <param name="basicRouter">A custom routing implementation.</param>
        /// <param name="interpreter">The routing interpreter.</param>
        /// <returns></returns>
        public static Router CreateCHFrom(IRoutingAlgorithmData<CHEdgeData> data, IRoutingAlgorithm<CHEdgeData> basicRouter, 
            IRoutingInterpreter interpreter)
        {
            // creates the edge router.
            var typedRouter = new TypedRouterCHEdge(
                data, interpreter, basicRouter);

            return new Router(typedRouter); // create the actual router.
        }

        /// <summary>
        /// Creates a router using interpreted edges.
        /// </summary>
        /// <param name="reader">The data to route on.</param>
        /// <param name="interpreter">The routing interpreter.</param>
        /// <param name="vehicle">The vehicle profile.</param>
        /// <returns></returns>
        public static Router CreateCHFrom(OsmStreamSource reader,
            IOsmRoutingInterpreter interpreter, Vehicle vehicle)
        {
            var tagsIndex = new TagsIndex(); // creates a tagged index.

            // read from the OSM-stream.
            var data = CHEdgeGraphOsmStreamTarget.Preprocess(
                reader, interpreter, vehicle);

            // creates the edge router.
            var typedRouter = new TypedRouterCHEdge(
                data, interpreter, new CHRouter());

            return new Router(typedRouter); // create the actual router.
        }

        #endregion

        #region ITypedRouter

        /// <summary>
        /// Returns true if the given vehicle type is supported.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <returns></returns>
        public bool SupportsVehicle(Vehicle vehicle)
        {
            return _router.SupportsVehicle(vehicle);
        }

        /// <summary>
        /// Calculates a route between two given points.
        /// </summary>
        /// <param name="vehicle">The vehicle profile.</param>
        /// <param name="source">The source point.</param>
        /// <param name="target">The target point.</param>
        /// <returns></returns>
        public Route Calculate(Vehicle vehicle, RouterPoint source, RouterPoint target)
        {
            if (vehicle == null) { throw new ArgumentNullException("vehicle"); }
            if (source == null) { throw new ArgumentNullException("source"); }
            if (target == null) { throw new ArgumentNullException("target"); }

            if(source.Vehicle.UniqueName != target.Vehicle.UniqueName)
            { // vehicles are different.
                throw new ArgumentException(string.Format("Not all vehicle profiles match, {0} and {1} are given, expecting identical profiles.",
                    source.Vehicle.UniqueName, target.Vehicle.UniqueName));
            }
            if(vehicle.UniqueName != target.Vehicle.UniqueName)
            { // vehicles are different.
                throw new ArgumentException(string.Format("Given vehicle profile does not match resolved points, {0} and {1} are given, expecting identical profiles.",
                    vehicle.UniqueName, target.Vehicle.UniqueName));
            }

            return _router.Calculate(vehicle, source, target);
        }

        /// <summary>
        /// Calculates a route between two given points.
        /// </summary>
        /// <param name="vehicle">The vehicle profile.</param>
        /// <param name="source">The source point.</param>
        /// <param name="target">The target point.</param>
        /// <param name="max">The maximum weight to stop the calculation.</param>
        /// <param name="geometryOnly">Returns the geometry only when true.</param>
        /// <returns></returns>
        public Route Calculate(Vehicle vehicle, RouterPoint source, RouterPoint target, float max = float.MaxValue, bool geometryOnly = false)
        {
            if (vehicle == null) { throw new ArgumentNullException("vehicle"); }
            if (source == null) { throw new ArgumentNullException("source"); }
            if (target == null) { throw new ArgumentNullException("target"); }

            if (source.Vehicle.UniqueName != target.Vehicle.UniqueName)
            { // vehicles are different.
                throw new ArgumentException(string.Format("Not all vehicle profiles match, {0} and {1} are given, expecting identical profiles.",
                    source.Vehicle.UniqueName, target.Vehicle.UniqueName));
            }
            if (vehicle.UniqueName != target.Vehicle.UniqueName)
            { // vehicles are different.
                throw new ArgumentException(string.Format("Given vehicle profile does not match resolved points, {0} and {1} are given, expecting identical profiles.",
                    vehicle.UniqueName, target.Vehicle.UniqueName));
            }

            return _router.Calculate(vehicle, source, target, max, geometryOnly);
        }

        /// <summary>
        /// Calculates a shortest route from a given point to any of the targets points.
        /// </summary>
        /// <param name="vehicle">The vehicle profile.</param>
        /// <param name="source">The source point.</param>
        /// <param name="targets">The target point(s).</param>
        /// <returns></returns>
        public Route CalculateToClosest(Vehicle vehicle, RouterPoint source, RouterPoint[] targets)
        {
            if (vehicle == null) { throw new ArgumentNullException("vehicle"); }
            if (source == null) { throw new ArgumentNullException("source"); }
            if (targets == null) { throw new ArgumentNullException("targets"); }

            foreach (var target in targets)
            {
                if (source.Vehicle.UniqueName != target.Vehicle.UniqueName)
                { // vehicles are different.
                    throw new ArgumentException(string.Format("Not all vehicle profiles match, {0} and {1} are given, expecting identical profiles.",
                        source.Vehicle.UniqueName, target.Vehicle.UniqueName));
                }
                if (vehicle.UniqueName != target.Vehicle.UniqueName)
                { // vehicles are different.
                    throw new ArgumentException(string.Format("Given vehicle profile does not match resolved points, {0} and {1} are given, expecting identical profiles.",
                        vehicle.UniqueName, target.Vehicle.UniqueName));
                }
            }

            return _router.CalculateToClosest(vehicle, source, targets);
        }

        /// <summary>
        /// Calculates a shortest route from a given point to any of the targets points.
        /// </summary>
        /// <param name="vehicle">The vehicle profile.</param>
        /// <param name="source">The source point.</param>
        /// <param name="targets">The target point(s).</param>
        /// <param name="max">The maximum weight to stop the calculation.</param>
        /// <param name="geometryOnly">Flag to return .</param>
        /// <returns></returns>
        public Route CalculateToClosest(Vehicle vehicle, RouterPoint source, RouterPoint[] targets, float max = float.MaxValue, bool geometryOnly = false)
        {
            if (vehicle == null) { throw new ArgumentNullException("vehicle"); }
            if (source == null) { throw new ArgumentNullException("source"); }
            if (targets == null) { throw new ArgumentNullException("targets"); }

            foreach (var target in targets)
            {
                if (source.Vehicle.UniqueName != target.Vehicle.UniqueName)
                { // vehicles are different.
                    throw new ArgumentException(string.Format("Not all vehicle profiles match, {0} and {1} are given, expecting identical profiles.",
                        source.Vehicle.UniqueName, target.Vehicle.UniqueName));
                }
                if (vehicle.UniqueName != target.Vehicle.UniqueName)
                { // vehicles are different.
                    throw new ArgumentException(string.Format("Given vehicle profile does not match resolved points, {0} and {1} are given, expecting identical profiles.",
                        vehicle.UniqueName, target.Vehicle.UniqueName));
                }
            }

            return _router.CalculateToClosest(vehicle, source, targets, max);
        }

        /// <summary>
        /// Calculates all routes between one source and many target points.
        /// </summary>
        /// <param name="vehicle">The vehicle profile.</param>
        /// <param name="source"></param>
        /// <param name="targets"></param>
        /// <param name="max">The maximum weight to stop the calculation.</param>
        /// <param name="geometryOnly">Flag to return .</param>
        /// <returns></returns>
        public Route[] CalculateOneToMany(Vehicle vehicle, RouterPoint source, RouterPoint[] targets, float max = float.MaxValue, bool geometryOnly = false)
        {
            if (vehicle == null) { throw new ArgumentNullException("vehicle"); }
            if (source == null) { throw new ArgumentNullException("source"); }
            if (targets == null) { throw new ArgumentNullException("targets"); }

            foreach (var target in targets)
            {
                if (source.Vehicle.UniqueName != target.Vehicle.UniqueName)
                { // vehicles are different.
                    throw new ArgumentException(string.Format("Not all vehicle profiles match, {0} and {1} are given, expecting identical profiles.",
                        source.Vehicle.UniqueName, target.Vehicle.UniqueName));
                }
                if (vehicle.UniqueName != target.Vehicle.UniqueName)
                { // vehicles are different.
                    throw new ArgumentException(string.Format("Given vehicle profile does not match resolved points, {0} and {1} are given, expecting identical profiles.",
                        vehicle.UniqueName, target.Vehicle.UniqueName));
                }
            }

            return _router.CalculateOneToMany(vehicle, source, targets, max, geometryOnly);
        }

        /// <summary>
        /// Calculates all routes between many sources/targets.
        /// </summary>
        /// <param name="vehicle">The vehicle profile.</param>
        /// <param name="sources"></param>
        /// <param name="targets"></param>
        /// <param name="max">The maximum weight to stop the calculation.</param>
        /// <param name="geometryOnly">Flag to return .</param>
        /// <returns></returns>
        public Route[][] CalculateManyToMany(Vehicle vehicle, RouterPoint[] sources, RouterPoint[] targets, float max = float.MaxValue, bool geometryOnly = false)
        {
            if (vehicle == null) { throw new ArgumentNullException("vehicle"); }
            if (sources == null) { throw new ArgumentNullException("sources"); }
            if (targets == null) { throw new ArgumentNullException("targets"); }

            foreach (var source in sources)
            {
                foreach (var target in targets)
                {
                    if (source.Vehicle.UniqueName != target.Vehicle.UniqueName)
                    { // vehicles are different.
                        throw new ArgumentException(string.Format("Not all vehicle profiles match, {0} and {1} are given, expecting identical profiles.",
                            source.Vehicle.UniqueName, target.Vehicle.UniqueName));
                    }
                    if (vehicle.UniqueName != target.Vehicle.UniqueName)
                    { // vehicles are different.
                        throw new ArgumentException(string.Format("Given vehicle profile does not match resolved points, {0} and {1} are given, expecting identical profiles.",
                            vehicle.UniqueName, target.Vehicle.UniqueName));
                    }
                }
            }

            return _router.CalculateManyToMany(vehicle, sources, targets, max, geometryOnly);
        }

        /// <summary>
        /// Calculates the weight between two given points.
        /// </summary>
        /// <param name="vehicle">The vehicle profile.</param>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        public double CalculateWeight(Vehicle vehicle, RouterPoint source, RouterPoint target)
        {
            if (vehicle == null) { throw new ArgumentNullException("vehicle"); }
            if (source == null) { throw new ArgumentNullException("source"); }
            if (target == null) { throw new ArgumentNullException("target"); }

            if (source.Vehicle.UniqueName != target.Vehicle.UniqueName)
            { // vehicles are different.
                throw new ArgumentException(string.Format("Not all vehicle profiles match, {0} and {1} are given, expecting identical profiles.",
                    source.Vehicle.UniqueName, target.Vehicle.UniqueName));
            }
            if (vehicle.UniqueName != target.Vehicle.UniqueName)
            { // vehicles are different.
                throw new ArgumentException(string.Format("Given vehicle profile does not match resolved points, {0} and {1} are given, expecting identical profiles.",
                    vehicle.UniqueName, target.Vehicle.UniqueName));
            }

            return _router.CalculateWeight(vehicle, source, target);
        }

        /// <summary>
        /// Calculates a route between one source and many target points.
        /// </summary>
        /// <returns></returns>
        public double[] CalculateOneToManyWeight(Vehicle vehicle, RouterPoint source, RouterPoint[] targets)
        {
            return this.CalculateOneToManyWeight(vehicle, source, targets, new HashSet<int>());
        }

        /// <summary>
        /// Calculates a route between one source and many target points.
        /// </summary>
        /// <returns></returns>
        public double[] CalculateOneToManyWeight(Vehicle vehicle, RouterPoint source, RouterPoint[] targets, HashSet<int> invalidSet)
        {
            if (vehicle == null) { throw new ArgumentNullException("vehicle"); }
            if (source == null) { throw new ArgumentNullException("source"); }
            if (targets == null) { throw new ArgumentNullException("targets"); }
            if (invalidSet == null) { throw new ArgumentNullException("invalidSet"); }

            foreach (var target in targets)
            {
                if (source.Vehicle.UniqueName != target.Vehicle.UniqueName)
                { // vehicles are different.
                    throw new ArgumentException(string.Format("Not all vehicle profiles match, {0} and {1} are given, expecting identical profiles.",
                        source.Vehicle.UniqueName, target.Vehicle.UniqueName));
                }
                if (vehicle.UniqueName != target.Vehicle.UniqueName)
                { // vehicles are different.
                    throw new ArgumentException(string.Format("Given vehicle profile does not match resolved points, {0} and {1} are given, expecting identical profiles.",
                        vehicle.UniqueName, target.Vehicle.UniqueName));
                }
            }

            return _router.CalculateOneToManyWeight(vehicle, source, targets, invalidSet);
        }

        /// <summary>
        /// Calculates all routes between many sources/targets.
        /// </summary>
        /// <returns></returns>
        public double[][] CalculateManyToManyWeight(Vehicle vehicle, RouterPoint[] sources, RouterPoint[] targets)
        {
            return this.CalculateManyToManyWeight(vehicle, sources, targets, new HashSet<int>());
        }

        /// <summary>
        /// Calculates all routes between many sources/targets.
        /// </summary>
        /// <returns></returns>
        public double[][] CalculateManyToManyWeight(Vehicle vehicle, RouterPoint[] sources, RouterPoint[] targets, HashSet<int> invalidSet)
        {
            if (vehicle == null) { throw new ArgumentNullException("vehicle"); }
            if (sources == null) { throw new ArgumentNullException("source"); }
            if (targets == null) { throw new ArgumentNullException("targets"); }
            if (invalidSet == null) { throw new ArgumentNullException("invalidSet"); }

            foreach (var source in sources)
            {
                foreach (var target in targets)
                {
                    if (source.Vehicle.UniqueName != target.Vehicle.UniqueName)
                    { // vehicles are different.
                        throw new ArgumentException(string.Format("Not all vehicle profiles match, {0} and {1} are given, expecting identical profiles.",
                            source.Vehicle.UniqueName, target.Vehicle.UniqueName));
                    }
                    if (vehicle.UniqueName != target.Vehicle.UniqueName)
                    { // vehicles are different.
                        throw new ArgumentException(string.Format("Given vehicle profile does not match resolved points, {0} and {1} are given, expecting identical profiles.",
                            vehicle.UniqueName, target.Vehicle.UniqueName));
                    }
                }
            }

            return _router.CalculateManyToManyWeight(vehicle, sources, targets, invalidSet);
        }

        /// <summary>
        /// Returns true if range calculation is supported.
        /// </summary>
        public bool IsCalculateRangeSupported
        {
            get { return _router.IsCalculateRangeSupported; }
        }

        /// <summary>
        /// Returns all points located at a given weight (distance/time) from the orgin.
        /// </summary>
        /// <param name="vehicle">The vehicle profile.</param>
        /// <param name="orgine"></param>
        /// <param name="weight"></param>
        /// <returns></returns>
        public HashSet<GeoCoordinate> CalculateRange(Vehicle vehicle, RouterPoint orgine, float weight)
        {
            if (vehicle == null) { throw new ArgumentNullException("vehicle"); }
            if (orgine == null) { throw new ArgumentNullException("orgine"); }

            if (vehicle.UniqueName != orgine.Vehicle.UniqueName)
            { // vehicles are different.
                throw new ArgumentException(string.Format("Given vehicle profile does not match resolved points, {0} and {1} are given, expecting identical profiles.",
                    vehicle.UniqueName, orgine.Vehicle.UniqueName));
            }

            return _router.CalculateRange(vehicle, orgine, weight);
        }

        /// <summary>
        /// Returns true if the given point is connected for a radius of at least the given weight.
        /// </summary>
        /// <param name="vehicle">The vehicle profile.</param>
        /// <param name="point"></param>
        /// <param name="weight"></param>
        /// <returns></returns>
        public bool CheckConnectivity(Vehicle vehicle, RouterPoint point, float weight)
        {
            if (vehicle == null) { throw new ArgumentNullException("vehicle"); }
            if (point == null) { throw new ArgumentNullException("point"); }

            if (vehicle.UniqueName != point.Vehicle.UniqueName)
            { // vehicles are different.
                throw new ArgumentException(string.Format("Given vehicle profile does not match resolved points, {0} and {1} are given, expecting identical profiles.",
                    vehicle.UniqueName, point.Vehicle.UniqueName));
            }

            return _router.CheckConnectivity(vehicle, point, weight);
        }

        /// <summary>
        /// Returns true if the given point is connected for a radius of at least the given weight.
        /// </summary>
        /// <param name="vehicle">The vehicle profile.</param>
        /// <param name="points"></param>
        /// <param name="weight"></param>
        /// <returns></returns>
        public bool[] CheckConnectivity(Vehicle vehicle, RouterPoint[] points, float weight)
        {
            if (vehicle == null) { throw new ArgumentNullException("vehicle"); }
            if (points == null) { throw new ArgumentNullException("points"); }

            foreach (var point in points)
            {
                if (vehicle.UniqueName != point.Vehicle.UniqueName)
                { // vehicles are different.
                    throw new ArgumentException(string.Format("Given vehicle profile does not match resolved points, {0} and {1} are given, expecting identical profiles.",
                        vehicle.UniqueName, point.Vehicle.UniqueName));
                }
            }

            return _router.CheckConnectivity(vehicle, points, weight);
        }

        /// <summary>
        /// Resolves a point.
        /// </summary>
        /// <param name="vehicle">The vehicle profile.</param>
        /// <param name="coordinate">The location of the point to resolve.</param>
        /// <returns></returns>
        public RouterPoint Resolve(Vehicle vehicle, GeoCoordinate coordinate)
        {
            if (vehicle == null) { throw new ArgumentNullException("vehicle"); }
            if (coordinate == null) { throw new ArgumentNullException("coordinate"); }
            
            return _router.Resolve(vehicle, coordinate, false);
        }

        /// <summary>
        /// Resolves a point.
        /// </summary>
        /// <param name="vehicle">The vehicle profile.</param>
        /// <param name="coordinate">The location of the point to resolve.</param>
        /// <param name="verticesOnly">Returns vertices only..</param>
        /// <returns></returns>
        public RouterPoint Resolve(Vehicle vehicle, GeoCoordinate coordinate, bool verticesOnly)
        {
            if (vehicle == null) { throw new ArgumentNullException("vehicle"); }
            if (coordinate == null) { throw new ArgumentNullException("coordinate"); }
            
            return _router.Resolve(vehicle, coordinate, verticesOnly);
        }

        /// <summary>
        /// Resolves a point.
        /// </summary>
        /// <param name="vehicle">The vehicle profile.</param>
        /// <param name="delta">The size of the box to search in.</param>
        /// <param name="coordinate">The location of the point to resolve.</param>
        /// <returns></returns>
        public RouterPoint Resolve(Vehicle vehicle, float delta, GeoCoordinate coordinate)
        {
            if (vehicle == null) { throw new ArgumentNullException("vehicle"); }
            if (coordinate == null) { throw new ArgumentNullException("coordinate"); }
            
            return _router.Resolve(vehicle, delta, coordinate);
        }

        /// <summary>
        /// Resolves a point.
        /// </summary>
        /// <param name="vehicle">The vehicle profile.</param>
        /// <param name="coordinate">The location of the point to resolve.</param>
        /// <param name="matcher">The matcher containing some matching algorithm.</param>
        /// <param name="matchingTags">Extra matching data.</param>
        /// <returns></returns>
        public RouterPoint Resolve(Vehicle vehicle, GeoCoordinate coordinate, IEdgeMatcher matcher, 
            TagsCollectionBase matchingTags)
        {
            if (vehicle == null) { throw new ArgumentNullException("vehicle"); }
            if (coordinate == null) { throw new ArgumentNullException("coordinate"); }
            if (matcher == null) { throw new ArgumentNullException("matcher"); }
            if (matchingTags == null) { throw new ArgumentNullException("matchingTags"); }
            
            return _router.Resolve(vehicle, coordinate, matcher, matchingTags);
        }

        /// <summary>
        /// Resolves a point.
        /// </summary>
        /// <param name="vehicle">The vehicle profile.</param>
        /// <param name="delta">The size of the box to search in.</param>
        /// <param name="coordinate">The location of the point to resolve.</param>
        /// <param name="matcher">The matcher containing some matching algorithm.</param>
        /// <param name="matchingTags">Extra matching data.</param>
        /// <returns></returns>
        public RouterPoint Resolve(Vehicle vehicle, float delta, GeoCoordinate coordinate, 
            IEdgeMatcher matcher, TagsCollectionBase matchingTags)
        {
            if (vehicle == null) { throw new ArgumentNullException("vehicle"); }
            if (coordinate == null) { throw new ArgumentNullException("coordinate"); }
            if (matcher == null) { throw new ArgumentNullException("matcher"); }
            if (matchingTags == null) { throw new ArgumentNullException("matchingTags"); }

            return _router.Resolve(vehicle, delta, coordinate, matcher, matchingTags);
        }

        /// <summary>
        /// Resolves all the given points.
        /// </summary>
        /// <param name="vehicle">The vehicle profile.</param>
        /// <param name="coordinate">The location of the point to resolve.</param>
        /// <returns></returns>
        public RouterPoint[] Resolve(Vehicle vehicle, GeoCoordinate[] coordinate)
        {
            if (vehicle == null) { throw new ArgumentNullException("vehicle"); }
            if (coordinate == null) { throw new ArgumentNullException("coordinate"); }

            return _router.Resolve(vehicle, coordinate);
        }

        /// <summary>
        /// Resolves all the given points.
        /// </summary>
        /// <param name="vehicle">The vehicle profile.</param>
        /// <param name="delta">The size of the box to search in.</param>
        /// <param name="coordinate">The location of the point to resolve.</param>
        /// <returns></returns>
        public RouterPoint[] Resolve(Vehicle vehicle, float delta, GeoCoordinate[] coordinate)
        {
            if (vehicle == null) { throw new ArgumentNullException("vehicle"); }
            if (coordinate == null) { throw new ArgumentNullException("coordinate"); }

            return _router.Resolve(vehicle, delta, coordinate);
        }

        /// <summary>
        /// Resolves all the given points.
        /// </summary>
        /// <param name="vehicle">The vehicle profile.</param>
        /// <param name="coordinates">The location of the points to resolve.</param>
        /// <param name="matcher">The matcher containing some matching algorithm.</param>
        /// <param name="matchingTags">Extra matching data.</param>
        /// <returns></returns>
        public RouterPoint[] Resolve(Vehicle vehicle, GeoCoordinate[] coordinates, IEdgeMatcher matcher, 
            TagsCollectionBase[] matchingTags)
        {
            if (vehicle == null) { throw new ArgumentNullException("vehicle"); }
            if (coordinates == null) { throw new ArgumentNullException("coordinates"); }
            if (matcher == null) { throw new ArgumentNullException("matcher"); }
            if (matchingTags == null) { throw new ArgumentNullException("matchingTags"); }

            return _router.Resolve(vehicle, coordinates, matcher, matchingTags);
        }

        /// <summary>
        /// Resolves all the given points.
        /// </summary>
        /// <param name="vehicle">The vehicle profile.</param>
        /// <param name="delta">The size of the box to search in.</param>
        /// <param name="coordinates">The location of the points to resolve.</param>
        /// <param name="matcher">The matcher containing some matching algorithm.</param>
        /// <param name="matchingTags">Extra matching data.</param>
        /// <returns></returns>
        public RouterPoint[] Resolve(Vehicle vehicle, float delta, GeoCoordinate[] coordinates, 
            IEdgeMatcher matcher, TagsCollectionBase[] matchingTags)
        {
            if (vehicle == null) { throw new ArgumentNullException("vehicle"); }
            if (coordinates == null) { throw new ArgumentNullException("coordinates"); }
            if (matcher == null) { throw new ArgumentNullException("matcher"); }
            if (matchingTags == null) { throw new ArgumentNullException("matchingTags"); }

            return _router.Resolve(vehicle, delta, coordinates, matcher, matchingTags);
        }

        /// <summary>
        /// Searches for a closeby link to the road network.
        /// </summary>
        /// <param name="vehicle">The vehicle profile.</param>
        /// <param name="coordinate">The location of the point to search.</param>
        /// <returns></returns>
        /// <remarks>Similar to resolve except no resolved point is created.</remarks>
        public GeoCoordinate Search(Vehicle vehicle, GeoCoordinate coordinate)
        {
            if (vehicle == null) { throw new ArgumentNullException("vehicle"); }
            if (coordinate == null) { throw new ArgumentNullException("coordinate"); }

            return _router.Search(vehicle, coordinate);
        }

        /// <summary>
        /// Searches for a closeby link to the road network.
        /// </summary>
        /// <param name="vehicle">The vehicle profile.</param>
        /// <param name="delta">The size of the box to search in.</param>
        /// <param name="coordinate">The location of the point to search.</param>
        /// <returns></returns>
        /// <remarks>Similar to resolve except no resolved point is created.</remarks>
        public GeoCoordinate Search(Vehicle vehicle, float delta, GeoCoordinate coordinate)
        {
            if (vehicle == null) { throw new ArgumentNullException("vehicle"); }
            if (coordinate == null) { throw new ArgumentNullException("coordinate"); }

            return _router.Search(vehicle, delta, coordinate);
        }

        #endregion
    }
}
