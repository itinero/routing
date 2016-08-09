// Itinero - OpenStreetMap (OSM) SDK
// Copyright (C) 2016 Paul Den Dulk, Abelshausen Ben
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

using Itinero.LocalGeo;
using System.Collections.Generic;

namespace Itinero.Algorithms.Networks.Analytics
{
    /// <summary>
    /// Abstract representation of an algorithm that generates edge visits.
    /// </summary>
    public interface IEdgeVisitor : IAlgorithm
    {
        /// <summary>
        /// Gets or sets the visit delegate.
        /// </summary>
        VisitDelegate Visit { get; set; }
    }

    /// <summary>
    /// A delegate that defines a visit.
    /// </summary>
    public delegate void VisitDelegate(uint edgeId, float startWeight, float endWeight, List<Coordinate> coordinates);
}
