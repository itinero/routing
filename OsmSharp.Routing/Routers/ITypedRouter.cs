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

using OsmSharp.Collections.Tags;
using OsmSharp.Math.Geo;
using OsmSharp.Routing.Vehicles;
using System.Collections.Generic;

namespace OsmSharp.Routing.Routers
{
    /// <summary>
    /// Abstracts the functionality of implemented by any TypeRouter class.
    /// </summary>
    public interface ITypedRouter
    {
        #region Capabilities

        /// <summary>
        /// Returns true if the given vehicle type is supported.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <returns></returns>
        bool SupportsVehicle(Vehicle vehicle);

        #endregion

        #region Routing

        /// <summary>
        /// Calculates a route between two given points.
        /// </summary>
        /// <param name="vehicle">The vehicle profile.</param>
        /// <param name="source">The source point.</param>
        /// <param name="target">The target point.</param>
        /// <param name="max">The maximum weight to stop the calculation.</param>
        /// <param name="geometryOnly">Returns only the route geometry when true.</param>
        /// <returns></returns>
        Route Calculate(Vehicle vehicle, RouterPoint source, RouterPoint target, float max = float.MaxValue, bool geometryOnly = false);

        /// <summary>
        /// Calculates a shortest route from a given point to any of the targets points.
        /// </summary>
        /// <param name="vehicle">The vehicle profile.</param>
        /// <param name="source">The source point.</param>
        /// <param name="targets">The target point(s).</param>
        /// <returns></returns>
        Route CalculateToClosest(Vehicle vehicle, RouterPoint source, RouterPoint[] targets, float max = float.MaxValue, bool geometryOnly = false);

        /// <summary>
        /// Calculates all routes between one source and many target points.
        /// </summary>
        /// <param name="vehicle">The vehicle profile.</param>
        /// <param name="source"></param>
        /// <param name="targets"></param>
        /// <returns></returns>
        Route[] CalculateOneToMany(Vehicle vehicle, RouterPoint source, RouterPoint[] targets, float max = float.MaxValue, bool geometryOnly = false);

        /// <summary>
        /// Calculates all routes between many sources/targets.
        /// </summary>
        /// <param name="vehicle">The vehicle profile.</param>
        /// <param name="sources"></param>
        /// <param name="targets"></param>
        /// <returns></returns>
        Route[][] CalculateManyToMany(Vehicle vehicle, RouterPoint[] sources, RouterPoint[] targets, float max = float.MaxValue, bool geometryOnly = false);

        /// <summary>
        /// Calculates the weight between two given points.
        /// </summary>
        /// <param name="vehicle">The vehicle profile.</param>
        /// <param name="source"></param>
        /// <param name="target"></param>
        /// <returns></returns>
        double CalculateWeight(Vehicle vehicle, RouterPoint source, RouterPoint target);

        /// <summary>
        /// Calculates a route between one source and many target points.
        /// </summary>
        /// <param name="vehicle">The vehicle profile.</param>
        /// <param name="source"></param>
        /// <param name="targets"></param>
        /// <returns></returns>
        double[] CalculateOneToManyWeight(Vehicle vehicle, RouterPoint source, RouterPoint[] targets, HashSet<int> invalidSet);

        /// <summary>
        /// Calculates all routes between many sources/targets.
        /// </summary>
        /// <returns></returns>
        double[][] CalculateManyToManyWeight(Vehicle vehicle, RouterPoint[] sources, RouterPoint[] targets, HashSet<int> invalidSet);

        #endregion

        #region Range Calculation

        /// <summary>
        /// Returns true if range calculation is supported.
        /// </summary>
        bool IsCalculateRangeSupported
        {
            get;
        }

        /// <summary>
        /// Returns all points located at a given weight (distance/time) from the orgin.
        /// </summary>
        /// <param name="vehicle">The vehicle profile.</param>
        /// <param name="orgine"></param>
        /// <param name="weight"></param>
        /// <returns></returns>
        HashSet<GeoCoordinate> CalculateRange(Vehicle vehicle, RouterPoint orgine, float weight);

        #endregion

        #region Error Detection/Error Handling

        /// <summary>
        /// Returns true if the given point is connected for a radius of at least the given weight.
        /// </summary>
        /// <param name="vehicle">The vehicle profile.</param>
        /// <param name="point"></param>
        /// <param name="weight"></param>
        /// <returns></returns>
        bool CheckConnectivity(Vehicle vehicle, RouterPoint point, float weight);

        /// <summary>
        /// Returns true if the given point is connected for a radius of at least the given weight.
        /// </summary>
        /// <param name="vehicle">The vehicle profile.</param>
        /// <param name="point"></param>
        /// <param name="weight"></param>
        /// <returns></returns>
        bool[] CheckConnectivity(Vehicle vehicle, RouterPoint[] point, float weight);

        #endregion

        #region Resolving

        /// <summary>
        /// Resolves a point.
        /// </summary>
        /// <param name="vehicle">The vehicle profile.</param>
        /// <param name="coordinate">The location of the point to resolve.</param>
        /// <param name="verticesOnly">When true only vertices are returned.</param>
        /// <returns></returns>
        RouterPoint Resolve(Vehicle vehicle, GeoCoordinate coordinate, bool verticesOnly);

        /// <summary>
        /// Resolves a point.
        /// </summary>
        /// <param name="vehicle">The vehicle profile.</param>
        /// <param name="delta">The size of the box to search in.</param>
        /// <param name="coordinate">The location of the point to resolve.</param>
        /// <returns></returns>
        RouterPoint Resolve(Vehicle vehicle, float delta, GeoCoordinate coordinate);

        /// <summary>
        /// Resolves a point.
        /// </summary>
        /// <param name="vehicle">The vehicle profile.</param>
        /// <param name="coordinate">The location of the point to resolve.</param>
        /// <param name="matcher">The matcher containing some matching algorithm.</param>
        /// <param name="matchingTags">Extra matching data.</param>
        /// <returns></returns>
        RouterPoint Resolve(Vehicle vehicle, GeoCoordinate coordinate,
            IEdgeMatcher matcher, TagsCollectionBase matchingTags);

        /// <summary>
        /// Resolves a point.
        /// </summary>
        /// <param name="vehicle">The vehicle profile.</param>
        /// <param name="delta">The size of the box to search in.</param>
        /// <param name="coordinate">The location of the point to resolve.</param>
        /// <param name="matcher">The matcher containing some matching algorithm.</param>
        /// <param name="matchingTags">Extra matching data.</param>
        /// <returns></returns>
        RouterPoint Resolve(Vehicle vehicle, float delta, GeoCoordinate coordinate,
            IEdgeMatcher matcher, TagsCollectionBase matchingTags);

        /// <summary>
        /// Resolves all the given points.
        /// </summary>
        /// <param name="vehicle">The vehicle profile.</param>
        /// <param name="coordinate">The location of the point to resolve.</param>
        /// <returns></returns>
        RouterPoint[] Resolve(Vehicle vehicle, GeoCoordinate[] coordinate);

        /// <summary>
        /// Resolves all the given points.
        /// </summary>
        /// <param name="vehicle">The vehicle profile.</param>
        /// <param name="delta">The size of the box to search in.</param>
        /// <param name="coordinate">The location of the point to resolve.</param>
        /// <returns></returns>
        RouterPoint[] Resolve(Vehicle vehicle, float delta, GeoCoordinate[] coordinate);

        /// <summary>
        /// Resolves all the given points.
        /// </summary>
        /// <param name="vehicle">The vehicle profile.</param>
        /// <param name="coordinates">The location of the points to resolve.</param>
        /// <param name="matcher">The matcher containing some matching algorithm.</param>
        /// <param name="matchingTags">Extra matching data.</param>
        /// <returns></returns>
        RouterPoint[] Resolve(Vehicle vehicle, GeoCoordinate[] coordinates,
            IEdgeMatcher matcher, TagsCollectionBase[] matchingTags);

        /// <summary>
        /// Resolves all the given points.
        /// </summary>
        /// <param name="vehicle">The vehicle profile.</param>
        /// <param name="delta">The size of the box to search in.</param>
        /// <param name="coordinates">The location of the points to resolve.</param>
        /// <param name="matcher">The matcher containing some matching algorithm.</param>
        /// <param name="matchingTags">Extra matching data.</param>
        /// <returns></returns>
        RouterPoint[] Resolve(Vehicle vehicle, float delta, GeoCoordinate[] coordinates,
            IEdgeMatcher matcher, TagsCollectionBase[] matchingTags);

        #region Search

        /// <summary>
        /// Searches for a closeby link to the road network.
        /// </summary>
        /// <param name="vehicle">The vehicle profile.</param>
        /// <param name="coordinate">The location of the point to search.</param>
        /// <returns></returns>
        /// <remarks>Similar to resolve except no resolved point is created.</remarks>
        GeoCoordinate Search(Vehicle vehicle, GeoCoordinate coordinate);

        /// <summary>
        /// Searches for a closeby link to the road network.
        /// </summary>
        /// <param name="vehicle">The vehicle profile.</param>
        /// <param name="delta">The size of the box to search in.</param>
        /// <param name="coordinate">The location of the point to search.</param>
        /// <returns></returns>
        /// <remarks>Similar to resolve except no resolved point is created.</remarks>
        GeoCoordinate Search(Vehicle vehicle, float delta, GeoCoordinate coordinate);

        #endregion

        #endregion
    }
}