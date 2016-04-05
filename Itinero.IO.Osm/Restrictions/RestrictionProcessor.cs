// Itinero - OpenStreetMap (OSM) SDK
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

using Itinero.IO.Osm.Streams;
using System;
using System.Collections.Generic;
using OsmSharp;
using Itinero.Algorithms.Collections;

namespace Itinero.IO.Osm.Restrictions
{
    /// <summary>
    /// An osm-data processor to process restrictions.
    /// </summary>
    public class RestrictionProcessor : ITwoPassProcessor
    {
        private readonly Func<long, uint> _getVertex; // holds a function that gets a vertex for a given node.
        private readonly HashSet<string> _vehicleTypes; // holds the set of vehicle types to take into account.
        private readonly SparseLongIndex _restrictedWayIds; // ways to keep to process restrictions.
        private readonly Dictionary<long, Way> _restrictedWays; // ways kept to process restrictions.
        private readonly Action<string, List<uint>> _foundRestriction; // restriction found action.

        /// <summary>
        /// Creates a new restriction processor.
        /// </summary>
        public RestrictionProcessor(IEnumerable<string> vehicleTypes, Func<long, uint> getVertex, Action<string, List<uint>> foundRestriction)
        {
            _vehicleTypes = new HashSet<string>(vehicleTypes);
            _getVertex = getVertex;
            _foundRestriction = foundRestriction;
            
            _restrictedWayIds = new SparseLongIndex();
            _restrictedWays = new Dictionary<long, Way>();
        }

        /// <summary>
        /// Processes the given node in the first pass.
        /// </summary>
        public void FirstPass(Node node)
        {

        }

        /// <summary>
        /// Processes the given way in the first pass.
        /// </summary>
        public void FirstPass(Way way)
        {

        }

        /// <summary>
        /// Processes the given relation in the first pass.
        /// </summary>
        public void FirstPass(Relation relation)
        {
            var vehicleType = string.Empty;
            if (!relation.IsRestriction(out vehicleType) ||
                relation.Members == null)
            {
                return;
            }

            long? from = null;
            long? to = null;
            long? via = null;
            bool viaIsWay = false;
            foreach (var member in relation.Members)
            {
                if (member.Role == "via")
                {
                    viaIsWay = member.Type == OsmGeoType.Way;
                    via = member.Id;
                }
                else if (member.Role == "from")
                {
                    from = member.Id;
                }
                else if (member.Role == "to")
                {
                    to = member.Id;
                }
            }

            if (from.HasValue && to.HasValue && via.HasValue)
            {
                _restrictedWayIds.Add(from.Value);
                _restrictedWayIds.Add(to.Value);
                if (viaIsWay)
                {
                    _restrictedWayIds.Add(via.Value);
                }
            }
        }

        /// <summary>
        /// Processes the given node in the second pass.
        /// </summary>
        public void SecondPass(Node node)
        {

        }

        /// <summary>
        /// Processes the given way in the second pass.
        /// </summary>
        public void SecondPass(Way way)
        {
            if (_restrictedWayIds.Contains(way.Id.Value))
            {
                _restrictedWays.Add(way.Id.Value, way);
            }
        }

        /// <summary>
        /// Processes the given relation in the second pass.
        /// </summary>
        public void SecondPass(Relation relation)
        {
            var vehicleType = string.Empty;
            if (!relation.IsRestriction(out vehicleType) ||
                relation.Members == null)
            {
                return;
            }

            long? from = null;
            long? to = null;
            long? via = null;
            bool viaIsWay = false;
            foreach (var member in relation.Members)
            {
                if (member.Role == "via")
                {
                    viaIsWay = member.Type == OsmGeoType.Way;
                    via = member.Id;
                }
                else if (member.Role == "from")
                {
                    from = member.Id;
                }
                else if (member.Role == "to")
                {
                    to = member.Id;
                }
            }

            if (from.HasValue && to.HasValue && via.HasValue)
            {
                var sequence = new List<uint>(3);

                // get from/to ways.
                Way fromWay = null;
                if (!_restrictedWays.TryGetValue(from.Value, out fromWay))
                {
                    Logging.Logger.Log("RouterDbStreamTarget", Logging.TraceEventType.Warning,
                        "From way not found for restriction relation {0}!", relation.Id.Value);
                    return;
                }
                var fromNodes = fromWay.Nodes.Clone() as long[];
                if (fromNodes == null ||
                    fromNodes.Length <= 1)
                {
                    Logging.Logger.Log("RouterDbStreamTarget", Logging.TraceEventType.Warning,
                        "From way has zero or one node for restriction relation {0}!", relation.Id.Value);
                    return;
                }
                Way toWay;
                if (!_restrictedWays.TryGetValue(to.Value, out toWay))
                {
                    Logging.Logger.Log("RouterDbStreamTarget", Logging.TraceEventType.Warning,
                        "To way not found for restriction relation {0}!", relation.Id.Value);
                    return;
                }
                var toNodes = toWay.Nodes.Clone() as long[];
                if (toNodes == null ||
                    toNodes.Length <= 1)
                {
                    Logging.Logger.Log("RouterDbStreamTarget", Logging.TraceEventType.Warning,
                        "To way has zero or one node for restriction relation {0}!", relation.Id.Value);
                    return;
                }

                // figure out the 'via'.
                var fromNode = long.MaxValue;
                var toNode = long.MaxValue;
                if (!viaIsWay)
                {
                    fromNode = via.Value;
                    toNode = via.Value;

                    if (fromNodes[0] == via.Value)
                    {
                        Array.Reverse(fromNodes);
                    }
                    else if(fromNodes[fromNodes.Length - 1] != via.Value)
                    {
                        Logging.Logger.Log("RouterDbStreamTarget", Logging.TraceEventType.Warning,
                            "No link could be found between from and via node for restriction relation {0}!", relation.Id.Value);
                        return;
                    }

                    if (toNodes[toNodes.Length - 1] == via.Value)
                    {
                        Array.Reverse(toNodes);
                    }
                    else if(toNodes[0] != via.Value)
                    {
                        Logging.Logger.Log("RouterDbStreamTarget", Logging.TraceEventType.Warning,
                            "No link could be found between to and via node for restriction relation {0}!", relation.Id.Value);
                        return;
                    }

                    // add via vertex.
                    var viaVertex = _getVertex(via.Value);
                    if (viaVertex == uint.MaxValue)
                    {
                        Logging.Logger.Log("RouterDbStreamTarget", Logging.TraceEventType.Warning,
                            "No vertex found for via node for restriction relation {0}!", relation.Id.Value);
                        return;
                    }
                    sequence.Add(viaVertex);
                }
                else
                {
                    Way viaWay;
                    if (!_restrictedWays.TryGetValue(via.Value, out viaWay))
                    {
                        Logging.Logger.Log("RouterDbStreamTarget", Logging.TraceEventType.Warning,
                            "Via way not found for restriction relation {0}!", relation.Id.Value);
                        return;
                    }
                    var viaNodes = viaWay.Nodes.Clone() as long[];
                    if (viaNodes == null ||
                        viaNodes.Length <= 0)
                    {
                        Logging.Logger.Log("RouterDbStreamTarget", Logging.TraceEventType.Warning,
                            "Via way has zero nodes for restriction relation {0}!", relation.Id.Value);
                        return;
                    }
                    
                    if (!(viaNodes[0] == fromNodes[0] ||
                         viaNodes[0] == fromNodes[fromNodes.Length - 1]))
                    {
                        Array.Reverse(viaNodes);
                    }
                    if (viaNodes[0] == fromNodes[0])
                    {
                        Array.Reverse(fromNodes);
                    }
                    else if(viaNodes[0] != fromNodes[fromNodes.Length - 1])
                    {
                        Logging.Logger.Log("RouterDbStreamTarget", Logging.TraceEventType.Warning,
                            "No link could be found between from and via way for restriction relation {0}!", relation.Id.Value);
                        return;
                    }

                    if (viaNodes[viaNodes.Length - 1] == toNodes[toNodes.Length - 1])
                    {
                        Array.Reverse(toNodes);
                    }
                    else if(viaNodes[viaNodes.Length - 1] != toNodes[0])
                    {
                        Logging.Logger.Log("RouterDbStreamTarget", Logging.TraceEventType.Warning,
                            "No link could be found between to and via way for restriction relation {0}!", relation.Id.Value);
                        return;
                    }


                    // add via vertices.
                    var viaVertex = _getVertex(viaNodes[0]);
                    if (viaVertex == uint.MaxValue)
                    {
                        Logging.Logger.Log("RouterDbStreamTarget", Logging.TraceEventType.Warning,
                            "No vertex found for first node of via way for restriction relation {0}!", relation.Id.Value);
                        return;
                    }
                    sequence.Add(viaVertex);
                    for (var i = 1; i < viaNodes.Length - 1; i++)
                    {
                        viaVertex = _getVertex(viaNodes[i]);
                        if (viaVertex != uint.MaxValue)
                        {
                            sequence.Add(viaVertex);
                        }
                    }
                    if (viaNodes.Length > 1)
                    {
                        viaVertex = _getVertex(viaNodes[viaNodes.Length - 1]);
                        if (viaVertex == uint.MaxValue)
                        {
                            Logging.Logger.Log("RouterDbStreamTarget", Logging.TraceEventType.Warning,
                                "No vertex found for last node of via way for restriction relation {0}!", relation.Id.Value);
                            return;
                        }
                        sequence.Add(viaVertex);
                    }
                }

                // add from vertex.
                var found = false;
                for(var i = fromNodes.Length - 2; i >= 0; i--)
                {
                    var fromVertex = _getVertex(fromNodes[i]);
                    if (fromVertex != uint.MaxValue)
                    {
                        sequence.Insert(0, fromVertex);
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    Logging.Logger.Log("RouterDbStreamTarget", Logging.TraceEventType.Warning,
                        "No before vertex found for from way for restriction relation {0}!", relation.Id.Value);
                    return;
                }

                // add to vertex.
                found = false;
                for(var i = 1; i < toNodes.Length; i++)
                {
                    var toVertex = _getVertex(toNodes[i]);
                    if (toVertex != uint.MaxValue)
                    {
                        sequence.Add(toVertex);
                        found = true;
                        break;
                    }
                }
                if (!found)
                {
                    Logging.Logger.Log("RouterDbStreamTarget", Logging.TraceEventType.Warning,
                        "No after vertex found for to way for restriction relation {0}!", relation.Id.Value);
                    return;
                }

                _foundRestriction(vehicleType, sequence);
            }
        }
    }
}