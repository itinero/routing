// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2016 Abelshausen Ben
// 
// This file is part of Itinero.
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
// along with Itinero. If not, see <http://www.gnu.org/licenses/>.

using OsmSharp;

namespace Itinero.IO.Osm.Restrictions
{
    /// <summary>
    /// A collection of helper functions to process restrictions.
    /// </summary>
    public static class Helper
    {
        /// <summary>
        /// Returns true if the given relation represents a restriction and 
        /// </summary>
        public static bool IsRestriction(this Relation relation, out string vehicleType, out bool positive)
        {
            var type = string.Empty;
            var restriction = string.Empty;
            positive = false;
            vehicleType = string.Empty;
            if (relation.Tags == null ||
                !relation.Tags.TryGetValue("type", out type) ||
                !relation.Tags.TryGetValue("restriction", out restriction))
            {
                return false;
            }
            if (restriction.StartsWith("no_"))
            { // 'only'-restrictions not supported yet.
                positive = false;
            }
            else if (restriction.StartsWith("only_"))
            {
                positive = true;
            }
            else
            {
                return false;
            }
            if (type != "restriction")
            {
                if (!type.StartsWith("restriction:"))
                {
                    return false;
                }
                vehicleType = type.Substring("restriction:".Length, type.Length - "restriction:".Length);
            }
            return true;
        }
    }
}
