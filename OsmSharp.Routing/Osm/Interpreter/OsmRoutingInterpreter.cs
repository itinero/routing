// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2013 Abelshausen Ben
// 
// This file is part of OsmSharp.
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
// along with OsmSharp. If not, see <http://www.gnu.org/licenses/>.

using OsmSharp.Collections.Tags;
using OsmSharp.Osm;
using OsmSharp.Osm.Data;
using OsmSharp.Routing.Constraints;
using OsmSharp.Routing.Interpreter.Roads;
using OsmSharp.Units.Speed;
using System.Collections.Generic;

namespace OsmSharp.Routing.Osm.Interpreter
{
    /// <summary>
    /// A routing interpreter for OSM data.
    /// </summary>
    public class OsmRoutingInterpreter : IOsmRoutingInterpreter
    {
        /// <summary>
        /// Holds the edge interpreter.
        /// </summary>
        private readonly IEdgeInterpreter _edgeInterpreter;

        /// <summary>
        /// Holds the routing constraints.
        /// </summary>
        private readonly IRoutingConstraints _constraints;

        /// <summary>
        /// Holds the relevant routing keys.
        /// </summary>
        private HashSet<string> _relevantRoutingKeys;

        /// <summary>
        /// Holds the relevant keys.
        /// </summary>
        private HashSet<string> _relevantKeys; 

        /// <summary>
        /// Creates a new routing intepreter with default settings.
        /// </summary>
        public OsmRoutingInterpreter()
        {
            _edgeInterpreter = new Edge.EdgeInterpreter();
            _constraints = null;

            this.FillRelevantTags();
        }

        /// <summary>
        /// Creates a new routing interpreter with given constraints.
        /// </summary>
        /// <param name="constraints"></param>
        public OsmRoutingInterpreter(IRoutingConstraints constraints)
        {
            _edgeInterpreter = new Edge.EdgeInterpreter();
            _constraints = constraints;
            
            this.FillRelevantTags();
        } 
	        
        /// <summary>
        /// Creates a new routing interpreter a custom edge interpreter.
        /// </summary>
        /// <param name="interpreter"></param>
        public OsmRoutingInterpreter(IEdgeInterpreter interpreter)
        {
            _edgeInterpreter = interpreter;
            _constraints = null;
        }	  

        /// <summary>
        /// Builds the list of relevant tags.
        /// </summary>
        private void FillRelevantTags()
        {
            _relevantRoutingKeys = new HashSet<string> { "oneway", "highway", "motor_vehicle", "bicycle", "foot", "access", "maxspeed", "junction", "type", "barrier" }; 
            _relevantKeys = new HashSet<string> { "name" };
        }

        /// <summary>
        /// Returns true if the given tags is relevant.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool IsRelevant(string key)
        {
            return _relevantRoutingKeys.Contains(key) ||
                _relevantKeys.Contains(key);
        }

        /// <summary>
        /// Returns true if the given key value pair is relevant.
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        /// <returns></returns>
        public bool IsRelevant(string key, string value)
        {
            KilometerPerHour speed;
            if (this.IsRelevant(key))
            { // check the value.
                switch(key)
                {
                    case "oneway":
                        return value == "yes" || value == "reverse" || value == "-1";
                    case "maxspeed":
                        return TagExtensions.TryParseSpeed(value, out speed);
                }
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns true if the given tag is relevant for routing.
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        public bool IsRelevantRouting(string key)
        {
            return _relevantRoutingKeys.Contains(key);
        }

        /// <summary>
        /// Returns true if the given vertices can be traversed in the given order.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="along"></param>
        /// <param name="to"></param>
        /// <returns></returns>
        public bool CanBeTraversed(long from, long along, long to)
        {
            return true;
        }

        /// <summary>
        /// Returns and edge interpreter.
        /// </summary>
        public IEdgeInterpreter EdgeInterpreter
        {
            get 
            {
                return _edgeInterpreter; 
            }
        }

        /// <summary>
        /// Returns the constraints.
        /// </summary>
        public IRoutingConstraints Constraints
        {
            get
            {
                return _constraints;
            }
        }

        /// <summary>
        /// Returns true if the given object can be a routing restriction.
        /// </summary>
        /// <param name="type"></param>
        /// <param name="tags"></param>
        /// <returns></returns>
        public bool IsRestriction(OsmGeoType type, TagsCollectionBase tags)
        { // at least there need to be some tags.
            if (type == OsmGeoType.Relation)
            { // filter out relation-based turn-restrictions.
                if (tags != null &&
                    tags.ContainsKeyValue("type", "restriction"))
                { // yep, there's a restriction here!
                    return true;
                }
            }
            else if(type == OsmGeoType.Node)
            { // a node is possibly a restriction too.
                if(tags != null)
                {
                    return tags.ContainsKey("barrier");
                }
            }
            return false;
        }

        /// <summary>
        /// Returns all restrictions that are represented by the given node.
        /// </summary>
        /// <param name="node"></param>
        /// <returns></returns>
        public List<string> CalculateRestrictions(Node node)
        {
            var restrictedVehicles = new List<string>(2);
            if(node != null &&
                node.Tags != null)
            {
                string barrierValue;
                if (node.Tags.TryGetValue("barrier", out barrierValue))
                { // there is a barrier: http://wiki.openstreetmap.org/wiki/Key:barrier
                    if(barrierValue == "bollard")
                    { // there is a bollard, implies a restriction for all motor vehicles.
                        // http://wiki.openstreetmap.org/wiki/Tag:barrier%3Dbollard
                        restrictedVehicles.Add(VehicleType.MotorVehicle); // all motor vehicles are restricted.
                    }
                }
                string bicyleValue;
                if (node.Tags.TryGetValue("bicycle", out bicyleValue) &&
                    bicyleValue == "no")
                {
                    restrictedVehicles.Add(VehicleType.Bicycle);
                }
                string pedestrianValue;
                if (node.Tags.TryGetValue("foot", out pedestrianValue) &&
                    bicyleValue == "no")
                {
                    restrictedVehicles.Add(VehicleType.Bicycle);
                }
            }
            return restrictedVehicles;
        }

        /// <summary>
        /// Returns all restrictions that are represented by the given node.
        /// </summary>
        /// <param name="relation"></param>
        /// <param name="source"></param>
        /// <returns></returns>
        public List<KeyValuePair<string, long[]>> CalculateRestrictions(Relation relation, IOsmGeoSource source)
        {
            var restrictions = new List<KeyValuePair<string, long[]>>();
            if(relation.Tags.ContainsKeyValue("type", "restriction") &&
                relation.Members != null)
            { // regular restriction.
                Way fromWay = null, toWay = null, viaWay = null;
                Node viaNode = null;
                for(int idx = 0; idx < relation.Members.Count; idx++)
                {
                    var member = relation.Members[idx];
                    if(member.MemberRole == "from")
                    { // the from-way.
                        fromWay = source.GetWay(member.MemberId.Value);
                    }
                    else if(member.MemberRole == "to")
                    { // the to-way.
                        toWay = source.GetWay(member.MemberId.Value);
                    }
                    else if(member.MemberRole == "via")
                    { // the via node/way.
                        if (member.MemberType.Value == OsmGeoType.Node)
                        {
                            viaNode = source.GetNode(member.MemberId.Value);
                        }
                        else if(member.MemberType.Value == OsmGeoType.Way)
                        {
                            viaWay = source.GetWay(member.MemberId.Value);
                        }
                    }
                }

                // check if all data was found and type of restriction.
                if(fromWay != null && toWay != null &&
                    fromWay.Nodes.Count > 1 && toWay.Nodes.Count > 1)
                { // ok, from-to is there and has enough nodes.
                    if(viaWay != null)
                    { // via-part is a way.

                    }
                    else if (viaNode != null)
                    { // via-node is a node.
                        var restriction = new long[3];
                        // get from.
                        if (fromWay.Nodes[0] == viaNode.Id)
                        { // next node is from-node.
                            restriction[0] = fromWay.Nodes[1];
                        }
                        else if(fromWay.Nodes[fromWay.Nodes.Count - 1] == viaNode.Id)
                        { // previous node is from-node.
                            restriction[0] = fromWay.Nodes[fromWay.Nodes.Count - 2];
                        }
                        else
                        { // not found!
                            return restrictions;
                        }
                        restriction[1] = viaNode.Id.Value;
                        // get to.
                        if (toWay.Nodes[0] == viaNode.Id)
                        { // next node is to-node.
                            restriction[2] = toWay.Nodes[1];
                        }
                        else if (toWay.Nodes[toWay.Nodes.Count - 1] == viaNode.Id)
                        { // previous node is to-node.
                            restriction[2] = toWay.Nodes[toWay.Nodes.Count - 2];
                        }
                        else
                        { // not found!
                            return restrictions;
                        }
                        restrictions.Add(new KeyValuePair<string, long[]>(VehicleType.Vehicle, restriction));
                    }
                }
            }
            return restrictions;
        }
    }
}