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
