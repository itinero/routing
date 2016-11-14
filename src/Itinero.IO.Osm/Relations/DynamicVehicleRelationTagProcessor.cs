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

using Itinero.Attributes;
using Itinero.Profiles;
using Itinero.Profiles.Lua;
using OsmSharp;
using OsmSharp.Tags;
using System.Linq;

namespace Itinero.IO.Osm.Relations
{
    /// <summary>
    /// Processes relation tags for a vehicle.
    /// </summary>
    public class DynamicVehicleRelationTagProcessor : RelationTagProcessor
    {
        private readonly DynamicVehicle _vehicle;
        private readonly object _relationTagProcessor;
        private readonly Table _attributesTable;
        private readonly Table _resultsTable;

        /// <summary>
        /// Creates a new vehicle relation tag processor.
        /// </summary>
        public DynamicVehicleRelationTagProcessor(DynamicVehicle vehicle)
        {
            _vehicle = vehicle;
            _relationTagProcessor = vehicle.Script.Globals["relation_tag_processor"];

            if (_relationTagProcessor != null)
            {
                _attributesTable = new Table(vehicle.Script);
                _resultsTable = new Table(vehicle.Script);
            }
        }

        /// <summary>
        /// Returns true if relation is relevant.
        /// </summary>
        public override bool IsRelevant(Relation relation)
        {
            if (_relationTagProcessor == null)
            {
                return false;
            }

            var attributes = relation.Tags;
            lock (_vehicle.Script)
            {
                // build lua table.
                _attributesTable.Clear();
                if (attributes == null || attributes.Count == 0)
                {
                    return false;
                }
                foreach (var attribute in attributes)
                {
                    _attributesTable.Set(attribute.Key, DynValue.NewString(attribute.Value));
                }

                // call factor_and_speed function.
                _resultsTable.Clear();
                _vehicle.Script.Call(_relationTagProcessor, _attributesTable, _resultsTable);

                // get the result.
                var dynAttributesToKeep = _resultsTable.Get("attributes_to_keep");
                if (dynAttributesToKeep != null &&
                    dynAttributesToKeep.Table != null &&
                    dynAttributesToKeep.Table.Keys.Count() > 0)
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Adds tags to the given way.
        /// </summary>
        public override void AddTags(Way way, TagsCollectionBase attributes)
        {
            if (_relationTagProcessor == null)
            {
                return;
            }

            var wayTags = way.Tags;
            var relationTags = attributes;
            lock (_vehicle.Script)
            {
                // build lua table.
                _attributesTable.Clear();
                if (relationTags == null || relationTags.Count == 0)
                {
                    return;
                }
                foreach (var attribute in relationTags)
                {
                    _attributesTable.Set(attribute.Key, DynValue.NewString(attribute.Value));
                }

                // call factor_and_speed function.
                _resultsTable.Clear();
                _vehicle.Script.Call(_relationTagProcessor, _attributesTable, _resultsTable);

                // get the result.
                var dynAttributesToKeep = _resultsTable.Get("attributes_to_keep");
                if (dynAttributesToKeep != null &&
                    dynAttributesToKeep.Table.Keys.Count() > 0)
                {
                    foreach (var attribute in dynAttributesToKeep.Table.Pairs)
                    {
                        wayTags.AddOrReplace(attribute.Key.String, attribute.Value.String);
                    }
                }
            }
        }
    }
}