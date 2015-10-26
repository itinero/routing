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

using System.Linq;
using System.Collections.Generic;

namespace OsmSharp.Routing
{
    /// <summary>
    /// Contains extension methods related to the results class.
    /// </summary>
    public static class ResultExtensions
    {
        /// <summary>
        /// Returns all valid results.
        /// </summary>
        public static IEnumerable<T> AllValid<T>(this IEnumerable<Result<T>> results)
        {
            if(results == null)
            { // nothing valid there!
                return new List<T>();
            }
            return results.Where(x => !x.IsError).Select<Result<T>, T>(x => x.Value);
        }

        /// <summary>
        /// Returns all results in error.
        /// </summary>
        public static IEnumerable<Result<T>> AllErrors<T>(this IEnumerable<Result<T>> results)
        {
            if (results == null)
            { // nothing valid there!
                return new List<Result<T>>();
            }
            return results.Where(x => x.IsError);
        }
    }
}