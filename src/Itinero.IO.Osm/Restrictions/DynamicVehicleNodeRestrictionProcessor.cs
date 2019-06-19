using Itinero.Attributes;
using Itinero.IO.Osm.Streams;
using Itinero.Profiles;
using Itinero.Profiles.Lua;
using OsmSharp;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Itinero.IO.Osm.Restrictions
{
    /// <summary>
    /// A processor that enables lua profiles to add vertex meta data.
    /// </summary>
    public class DynamicVehicleNodeRestrictionProcessor : ITwoPassProcessor
    {
        private readonly RouterDb _routerDb;
        private readonly object _nodeRestrictionFunc;
        private readonly Action<string, List<uint>> _foundRestriction; // restriction found action.
        private readonly Func<Node, uint> _markCore; // marks the node as core.
        private readonly Table _attributesTable;
        private readonly Table _resultsTable;
        private readonly DynamicVehicle _vehicle;

        /// <summary>
        /// Creates a new processor.
        /// </summary>
        public DynamicVehicleNodeRestrictionProcessor(RouterDb routerDb, DynamicVehicle vehicle, Func<Node, uint> markCore, 
            Action<string, List<uint>> foundRestriction)
        {
            _foundRestriction = foundRestriction;
            _routerDb = routerDb;
            _vehicle = vehicle;
            _markCore = markCore;
            _nodeRestrictionFunc = vehicle.Script.Globals["node_restriction"];
            _markCore = markCore;

            if (_nodeRestrictionFunc != null)
            {
                _attributesTable = new Table(vehicle.Script);
                _resultsTable = new Table(vehicle.Script);
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
            if (node.Tags == null ||
                node.Tags.Count == 0)
            {
                return;
            }

            if (_nodeRestrictionFunc == null) return;

            lock (_vehicle.Script)
            {
                // build lua table.
                _attributesTable.Clear();
                foreach (var attribute in node.Tags)
                {
                    _attributesTable.Set(attribute.Key, DynValue.NewString(attribute.Value));
                }

                // call factor_and_speed function.
                _resultsTable.Clear();
                _vehicle.Script.Call(_nodeRestrictionFunc, _attributesTable, _resultsTable);
                
                // get the vehicle type if any.
                var vehicleTypeVal = _resultsTable.Get("vehicle");
                if (vehicleTypeVal == null ||
                    vehicleTypeVal.Type != DataType.String)
                { // no restriction found.
                    return;
                }
                
                // there is a restriction, mark it as such.
                var vertex = _markCore(node);
                var vehicleType = vehicleTypeVal.String;
                var r = new List<uint> {vertex};
                _foundRestriction(vehicleType, r);

                // get attributes to keep and add the vertex meta.
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

                // add or set the attributes on the vertex.
                var existing = _routerDb.VertexMeta[vertex];
                if (existing != null)
                {
                    existing = new AttributeCollection(existing);
                    existing.AddOrReplace(resultAttributes);
                }
                else
                {
                    existing = resultAttributes;
                }
                _routerDb.VertexMeta[vertex] = existing;
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