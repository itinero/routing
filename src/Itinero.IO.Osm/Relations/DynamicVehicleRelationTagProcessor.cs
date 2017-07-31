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
            if (attributes == null || attributes.Count == 0)
            {
                return false;
            }
            lock (_vehicle.Script)
            {
                // build lua table.
                _attributesTable.Clear();
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
                    dynAttributesToKeep.Type != DataType.Nil &&
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
            if (relationTags == null || relationTags.Count == 0)
            {
                return;
            }
            lock (_vehicle.Script)
            {
                // build lua table.
                _attributesTable.Clear();
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
                    dynAttributesToKeep.Type != DataType.Nil &&
                    dynAttributesToKeep.Table.Keys.Count() > 0)
                {
                    foreach (var attribute in dynAttributesToKeep.Table.Pairs)
                    {
                        wayTags.AddOrAppend(new Tag(attribute.Key.String, attribute.Value.String));
                    }
                }
            }
        }
    }
}