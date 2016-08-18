// Itinero - Routing for .NET
// Copyright (C) 2016 Abelshausen Ben
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
using Itinero.Data.Network;
using Itinero.Profiles;
using System;
using System.Collections.Generic;

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
        public abstract Result<RouterPoint> TryResolve(Profile[] profiles, float latitude, float longitude,
            Func<RoutingEdge, bool> isBetter, float searchDistanceInMeter = Constants.SearchDistanceInMeter);

        /// <summary>
        /// Checks if the given point is connected to the rest of the network. Use this to detect points on routing islands.
        /// </summary>
        /// <returns></returns>
        public abstract Result<bool> TryCheckConnectivity(Profile profile, RouterPoint point, float radiusInMeters);

        /// <summary>
        /// Calculates a route between the two locations.
        /// </summary>
        /// <returns></returns>
        public abstract Result<EdgePath<T>> TryCalculateRaw<T>(Profile profile, WeightHandler<T> weightHandler, RouterPoint source, RouterPoint target) where T : struct;

        /// <summary>
        /// Calculates the weight between the two locations.
        /// </summary>
        /// <returns></returns>
        /// <remarks>The weight is the distance * factor from the given profile.</remarks>
        public abstract Result<T> TryCalculateWeight<T>(Profile profile, WeightHandler<T> weightHandler, RouterPoint source, RouterPoint target) where T : struct;

        /// <summary>
        /// Calculates all routes between all sources and all targets.
        /// </summary>
        /// <returns></returns>
        public abstract Result<EdgePath<T>[][]> TryCalculateRaw<T>(Profile profile, WeightHandler<T> weightHandler, RouterPoint[] sources, RouterPoint[] targets,
            ISet<int> invalidSources, ISet<int> invalidTargets) where T : struct;

        /// <summary>
        /// Calculates all weights between all sources and all targets.
        /// </summary>
        /// <returns></returns>
        public abstract Result<T[][]> TryCalculateWeight<T>(Profile profile, WeightHandler<T> weightHandler, RouterPoint[] sources, RouterPoint[] targets,
            ISet<int> invalidSources, ISet<int> invalidTargets) where T : struct;
        
        /// <summary>
        /// Builds a route based on a raw path.
        /// </summary>
        public abstract Result<Route> BuildRoute<T>(Profile profile, WeightHandler<T> weightHandler, RouterPoint source, RouterPoint target, EdgePath<T> path) where T : struct;
    }
}