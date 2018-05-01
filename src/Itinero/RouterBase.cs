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

using Itinero.Algorithms;
using Itinero.Algorithms.Search;
using Itinero.Algorithms.Weights;
using Itinero.Data.Network;
using Itinero.Profiles;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Itinero
{
    /// <summary>
    /// The base-class for generic routing functionality.
    /// </summary>
    public abstract class RouterBase
    {
        /// <summary>
        /// Gets the db.
        /// </summary>
        public abstract RouterDb Db
        {
            get;
        }

        /// <summary>
        /// Gets or sets the profile factor and speed cache.
        /// </summary>
        public ProfileFactorAndSpeedCache ProfileFactorAndSpeedCache { get; set; }

        /// <summary>
        /// Flag to check all resolved points if stopping at the resolved location is possible.
        /// </summary>
        public bool VerifyAllStoppable { get; set; }

        /// <summary>
        /// Searches for the closest point on the routing network that's routable for the given profiles.
        /// </summary>
        /// <returns></returns>
        public Result<RouterPoint> TryResolve(IProfileInstance[] profiles, float latitude, float longitude,
            Func<RoutingEdge, bool> isBetter, float searchDistanceInMeter = Constants.SearchDistanceInMeter,
            ResolveSettings settings = null)
        {
            return this.TryResolve(profiles, latitude, longitude, isBetter, searchDistanceInMeter, settings, CancellationToken.None);
        }

        /// <summary>
        /// Searches for the closest point on the routing network that's routable for the given profiles.
        /// </summary>
        /// <returns></returns>
        public abstract Result<RouterPoint> TryResolve(IProfileInstance[] profiles, float latitude, float longitude,
            Func<RoutingEdge, bool> isBetter, float searchDistanceInMeter,
            ResolveSettings settings, CancellationToken cancellationToken);

        /// <summary>
        /// Checks if the given point is connected to the rest of the network. Use this to detect points on routing islands.
        /// </summary>
        /// <param name="radiusInMeter">The radius metric, that's always a distance.</param>
        /// <returns></returns>
        public Result<bool> TryCheckConnectivity(IProfileInstance profile, RouterPoint point, float radiusInMeter, bool? forward = null)
        {
            return this.TryCheckConnectivity(profile, point, radiusInMeter, forward, CancellationToken.None);
        }

        /// <summary>
        /// Checks if the given point is connected to the rest of the network. Use this to detect points on routing islands.
        /// </summary>
        /// <param name="radiusInMeter">The radius metric, that's always a distance.</param>
        /// <returns></returns>
        public abstract Result<bool> TryCheckConnectivity(IProfileInstance profile, RouterPoint point, float radiusInMeter, bool? forward, CancellationToken cancellationToken);

        /// <summary>
        /// Calculates a route between the two locations.
        /// </summary>
        /// <returns></returns>
        public Result<EdgePath<T>> TryCalculateRaw<T>(IProfileInstance profile, WeightHandler<T> weightHandler, RouterPoint source, RouterPoint target,
            RoutingSettings<T> settings = null) where T : struct
        {
            return this.TryCalculateRaw(profile, weightHandler, source, target, settings, CancellationToken.None);
        }

        /// <summary>
        /// Calculates a route between the two locations.
        /// </summary>
        /// <returns></returns>
        public abstract Result<EdgePath<T>> TryCalculateRaw<T>(IProfileInstance profile, WeightHandler<T> weightHandler, RouterPoint source, RouterPoint target,
            RoutingSettings<T> settings, CancellationToken cancellationToken) where T : struct;

        /// <summary>
        /// Calculates a route between the two directed edges. The route starts in the direction of the edge and ends with an arrive in the direction of the target edge.
        /// </summary>
        /// <returns></returns>
        public Result<EdgePath<T>> TryCalculateRaw<T>(IProfileInstance profile, WeightHandler<T> weightHandler, long sourceDirectedEdge, long targetDirectedEdge,
            RoutingSettings<T> settings = null) where T : struct
        {
            return this.TryCalculateRaw(profile, weightHandler, sourceDirectedEdge, targetDirectedEdge, settings, CancellationToken.None);
        }

        /// <summary>
        /// Calculates a route between the two locations, attempts to start/end in the requested directions.
        /// </summary>
        public Result<EdgePath<T>> TryCalculateRaw<T>(IProfileInstance profileInstance, WeightHandler<T> weightHandler, RouterPoint source, bool? sourceForward,
            RouterPoint target, bool? targetForward, RoutingSettings<T> settings) where T : struct
        {
            return this.TryCalculateRaw(profileInstance, weightHandler, source, sourceForward, target, targetForward, settings, CancellationToken.None);
        }

        /// <summary>
        /// Calculates a route between the two locations, attempts to start/end in the requested directions.
        /// </summary>
        public abstract Result<EdgePath<T>> TryCalculateRaw<T>(IProfileInstance profileInstance, WeightHandler<T> weightHandler, RouterPoint source, bool? sourceForward,
            RouterPoint target, bool? targetForward, RoutingSettings<T> settings, CancellationToken cancellationToken) where T : struct;

        /// <summary>
        /// Calculates a route between the two directed edges. The route starts in the direction of the edge and ends with an arrive in the direction of the target edge.
        /// </summary>
        /// <returns></returns>
        public abstract Result<EdgePath<T>> TryCalculateRaw<T>(IProfileInstance profile, WeightHandler<T> weightHandler, long sourceDirectedEdge, long targetDirectedEdge,
            RoutingSettings<T> settings, CancellationToken cancellationToken) where T : struct;

        /// <summary>
        /// Calculates all routes between all sources and all targets.
        /// </summary>
        /// <returns></returns>
        public Result<EdgePath<T>[][]> TryCalculateRaw<T>(IProfileInstance profile, WeightHandler<T> weightHandler, RouterPoint[] sources, RouterPoint[] targets,
            RoutingSettings<T> settings = null) where T : struct
        {
            return this.TryCalculateRaw(profile, weightHandler, sources, targets, settings, CancellationToken.None);
        }

        /// <summary>
        /// Calculates all routes between all sources and all targets.
        /// </summary>
        /// <returns></returns>
        public abstract Result<EdgePath<T>[][]> TryCalculateRaw<T>(IProfileInstance profile, WeightHandler<T> weightHandler, RouterPoint[] sources, RouterPoint[] targets,
            RoutingSettings<T> settings, CancellationToken cancellationToken) where T : struct;

        /// <summary>
        /// Calculates all weights between all sources and all targets.
        /// </summary>
        /// <returns></returns>
        public Result<T[][]> TryCalculateWeight<T>(IProfileInstance profile, WeightHandler<T> weightHandler, RouterPoint[] sources, RouterPoint[] targets,
            ISet<int> invalidSources, ISet<int> invalidTargets, RoutingSettings<T> settings = null) where T : struct
        {
            return this.TryCalculateWeight(profile, weightHandler, sources, targets, invalidSources, invalidTargets, settings, CancellationToken.None);
        }

        /// <summary>
        /// Calculates all weights between all sources and all targets.
        /// </summary>
        /// <returns></returns>
        public abstract Result<T[][]> TryCalculateWeight<T>(IProfileInstance profile, WeightHandler<T> weightHandler, RouterPoint[] sources, RouterPoint[] targets,
            ISet<int> invalidSources, ISet<int> invalidTargets, RoutingSettings<T> settings, CancellationToken cancellationToken) where T : struct;

        /// <summary>
        /// Builds a route based on a raw path.
        /// </summary>
        public Result<Route> BuildRoute<T>(IProfileInstance profile, WeightHandler<T> weightHandler, RouterPoint source, RouterPoint target, EdgePath<T> path) where T : struct
        {
            return this.BuildRoute(profile, weightHandler, source, target, path, CancellationToken.None);
        }

        /// <summary>
        /// Builds a route based on a raw path.
        /// </summary>
        public abstract Result<Route> BuildRoute<T>(IProfileInstance profile, WeightHandler<T> weightHandler, RouterPoint source, RouterPoint target, EdgePath<T> path, CancellationToken cancellationToken) where T : struct;
    }
}