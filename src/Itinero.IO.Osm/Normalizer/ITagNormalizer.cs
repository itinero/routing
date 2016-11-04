// Itinero - Routing for .NET
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

using Itinero.Attributes;
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
            AttributeCollection metaTags, IEnumerable<Vehicle> vehicles);
    }
}