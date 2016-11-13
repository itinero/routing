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

using Itinero.Osm.Vehicles;
using OsmSharp;
using OsmSharp.Tags;

namespace Itinero.IO.Osm.Relations
{
    /// <summary>
    /// Processes relation tags for a vehicle.
    /// </summary>
    public class VehicleRelationTagProcessor : RelationTagProcessor
    {
        private readonly Vehicle _vehicle;

        /// <summary>
        /// Creates a new vehicle relation tag processor.
        /// </summary>
        public VehicleRelationTagProcessor(Vehicle vehicle)
        {
            _vehicle = vehicle;
        }

        /// <summary>
        /// Returns true if relation is relevant.
        /// </summary>
        public override bool IsRelevant(Relation relation)
        {
            return _vehicle.IsRelevantRelation(new TagAttributeCollection(relation.Tags));
        }

        /// <summary>
        /// Adds tags to the given way.
        /// </summary>
        public override void AddTags(Way way, TagsCollectionBase attributes)
        {
            _vehicle.AddRelationTags(new TagAttributeCollection(attributes), new TagAttributeCollection(way.Tags));
        }
    }
}