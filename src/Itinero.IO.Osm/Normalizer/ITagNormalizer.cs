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

using Itinero.Attributes;
using Itinero.IO.Osm.Streams;
using Itinero.Profiles;
using System.Collections.Generic;

namespace Itinero.IO.Osm.Normalizer
{
    /// <summary>
    /// Abstract representation of a tags normalizer.
    /// </summary>
    public interface ITagNormalizer
    {
        /// <summary>
        /// Splits the given tags into a normalized version, profile tags, and the rest in metatags.
        /// </summary>
        bool Normalize(AttributeCollection tags, AttributeCollection profileTags,
            AttributeCollection metaTags, VehicleCache vehicleCach, Whitelist whitelist);
    }
}