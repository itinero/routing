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

using Itinero.Data.Network;
using Itinero.Graphs.Geometric;
using System;

namespace Itinero.Algorithms.Search
{
    /// <summary>
    /// Contains extensions and helper functions related to resolvers.
    /// </summary>
    public static class IResolveExtensions
    {
        /// <summary>
        /// Delegate to create a resolver.
        /// </summary>
        public delegate IResolver CreateResolver(float latitude, float longitude, Func<GeometricEdge, bool> isAcceptable, Func<RoutingEdge, bool> isBetter);
    }
}