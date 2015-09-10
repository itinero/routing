//// OsmSharp - OpenStreetMap (OSM) SDK
//// Copyright (C) 2015 Abelshausen Ben
//// 
//// This file is part of OsmSharp.
//// 
//// OsmSharp is free software: you can redistribute it and/or modify
//// it under the terms of the GNU General Public License as published by
//// the Free Software Foundation, either version 2 of the License, or
//// (at your option) any later version.
//// 
//// OsmSharp is distributed in the hope that it will be useful,
//// but WITHOUT ANY WARRANTY; without even the implied warranty of
//// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//// GNU General Public License for more details.
//// 
//// You should have received a copy of the GNU General Public License
//// along with OsmSharp. If not, see <http://www.gnu.org/licenses/>.

//using OsmSharp.Collections.Tags.Index;
//using OsmSharp.Routing.Data;
//using OsmSharp.Routing.Data.Contracted;
//using OsmSharp.Routing.Graphs.Directed;
//using OsmSharp.Routing.Graphs.Geometric;
//using OsmSharp.Routing.Vehicles;
//using System.Collections.Generic;

//namespace OsmSharp.Routing.Data
//{
//    /// <summary>
//    /// Represents a routing network.
//    /// </summary>
//    public class RoutingNetwork
//    {
//        private readonly GeometricGraph<EdgeData> _graph;
//        private readonly Dictionary<string, DirectedGraph<ContractedEdgeData>> _contractedGraphs;
//        private readonly TagsIndex _tagsIndex;

//        /// <summary>
//        /// Creates a new routing network.
//        /// </summary>
//        public RoutingNetwork(GeometricGraph<EdgeData> graph, TagsIndex tagsIndex)
//        {
//            _graph = graph;
//            _contractedGraphs = new Dictionary<string, DirectedGraph<ContractedEdgeData>>();
//            _tagsIndex = tagsIndex;
//        }

//        /// <summary>
//        /// Returns the base graph.
//        /// </summary>
//        public GeometricGraph<EdgeData> Graph
//        {
//            get
//            {
//                return _graph;
//            }
//        }

//        /// <summary>
//        /// Returns the tags index.
//        /// </summary>
//        public TagsIndex TagsIndex
//        {
//            get
//            {
//                return _tagsIndex;
//            }
//        }

//        /// <summary>
//        /// Returns true if this network contains a contracted graph for the given vehicle.
//        /// </summary>
//        /// <returns></returns>
//        public bool HasContractedFor(Vehicle vehicle)
//        {
//            return _contractedGraphs.ContainsKey(vehicle.UniqueName);
//        }

//        /// <summary>
//        /// Returns true if this network contains a contracted graph for the given vehicle.
//        /// </summary>
//        /// <returns></returns>
//        public bool TryGetContractedFor(Vehicle vehicle, out DirectedGraph<ContractedEdgeData> contractedGraph)
//        {
//            return _contractedGraphs.TryGetValue(vehicle.UniqueName, out contractedGraph);
//        }

//        /// <summary>
//        /// Adds a contracted graph for the given vehicle profile.
//        /// </summary>
//        public void AddContracted(Vehicle vehicle, DirectedGraph<ContractedEdgeData> contractedGraph)
//        {
//            _contractedGraphs.Add(vehicle.UniqueName, contractedGraph);
//        }
//    }
//}