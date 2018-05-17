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
using Itinero.Profiles.Lua;
using OsmSharp;
using System;
using System.Linq;

namespace Itinero.IO.Osm.Nodes
{
    /// <summary>
    /// A processor that enables lua profiles to add vertex meta data.
    /// </summary>
    public class DynamicVehicleNodeTagProcessor : ITwoPassProcessor
    {
        private readonly RouterDb _routerDb;
        private readonly DynamicVehicle _vehicle;
        private readonly object _nodeTagProcessor;
        private readonly Table _attributesTable;
        private readonly Table _resultsTable;
        private readonly Func<Node, uint> _markCore; // marks the node as core.

        /// <summary>
        /// Creates a new processor.
        /// </summary>
        public DynamicVehicleNodeTagProcessor(RouterDb routerDb, DynamicVehicle vehicle, Func<Node, uint> markCore)
        {
            _routerDb = routerDb;
            _vehicle = vehicle;
            _nodeTagProcessor = vehicle.Script.Globals["node_tag_processor"];
            _markCore = markCore;

            if (_nodeTagProcessor != null)
            {
                _attributesTable = new Table(vehicle.Script);
                _resultsTable = new Table(vehicle.Script);
            }
        }


        /// <summary>
        /// Returns true if node is relevant.
        /// </summary>
        private IAttributeCollection GetAttributesFor(Node node)
        {
            if (_nodeTagProcessor == null)
            {
                return null;
            }
            
            var nodeTags = node.Tags;
            if (nodeTags == null || nodeTags.Count == 0)
            {
                return null;
            }
            lock (_vehicle.Script)
            {
                // build lua table.
                _attributesTable.Clear();
                foreach (var attribute in nodeTags)
                {
                    _attributesTable.Set(attribute.Key, DynValue.NewString(attribute.Value));
                }

                // call factor_and_speed function.
                _resultsTable.Clear();
                _vehicle.Script.Call(_nodeTagProcessor, _attributesTable, _resultsTable);

                // get the result.
                var resultAttributes = new AttributeCollection();
                var dynAttributesToKeep = _resultsTable.Get("attributes_to_keep");
                if (dynAttributesToKeep != null &&
                    dynAttributesToKeep.Type != DataType.Nil &&
                    dynAttributesToKeep.Table.Keys.Count() > 0)
                {
                    foreach (var attribute in dynAttributesToKeep.Table.Pairs)
                    {
                        resultAttributes.AddOrReplace(attribute.Key.String, attribute.Value.String);
                    }
                }
                return resultAttributes;
            }
        }

        /// <summary>
        /// Processes the first pass of this way.
        /// </summary>
        public void FirstPass(Way way)
        {

        }

        /// <summary>
        /// Processes the first pass of this relation.
        /// </summary>
        public bool FirstPass(Relation relation)
        {
            return false;
        }

        /// <summary>
        /// Processes a node in the second pass.
        /// </summary>
        public void SecondPass(Node node)
        {
            var attributes = this.GetAttributesFor(node);
            if (attributes != null &&
                attributes.Count > 0)
            {
                var vertex = _markCore(node);
                if (vertex != Itinero.Constants.NO_VERTEX)
                {
                    _routerDb.VertexMeta[vertex] = attributes;
                }
            }
        }

        /// <summary>
        /// Processes a way in the second pass.
        /// </summary>
        public void SecondPass(Way way)
        {

        }

        /// <summary>
        /// Processes a relation in a second pass.
        /// </summary>
        public void SecondPass(Relation relation)
        {

        }
    }
}