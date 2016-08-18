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

using Itinero.Profiles;

namespace Itinero.Algorithms.Routes
{
    /// <summary>
    /// Abstract representation of a route builder.
    /// </summary>
    public interface IRouteBuilder
    {
        /// <summary>
        /// Tries to build a route for the given profile/source/target and path.
        /// </summary>
        Result<Route> TryBuild<T>(RouterDb db, Profile profile, RouterPoint source, RouterPoint target, EdgePath<T> path);
    }
}