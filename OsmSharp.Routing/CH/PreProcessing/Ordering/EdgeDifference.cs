//// OsmSharp - OpenStreetMap (OSM) SDK
//// Copyright (C) 2013 Abelshausen Ben
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

//using OsmSharp.Routing.Graph;
//using OsmSharp.Routing.Graph.Routing;
//using System.Collections.Generic;

//namespace OsmSharp.Routing.CH.Preprocessing.Ordering
//{
//    /// <summary>
//    /// The edge difference calculator.
//    /// </summary>
//    public class EdgeDifference : INodeWeightCalculator
//    {
//        /// <summary>
//        /// Holds the witness calculator.
//        /// </summary>
//        private INodeWitnessCalculator _witnessCalculator;

//        /// <summary>
//        /// Holds the data.
//        /// </summary>
//        private IRouterDataSource<CHEdgeData> _data;

//        /// <summary>
//        /// Creates a new edge difference calculator.
//        /// </summary>
//        /// <param name="data"></param>
//        /// <param name="witnessCalculator"></param>
//        public EdgeDifference(IRouterDataSource<CHEdgeData> data, INodeWitnessCalculator witnessCalculator)
//        {
//            _data = data;
//            _witnessCalculator = witnessCalculator;
//        }

//        /// <summary>
//        /// Calculates the edge-difference if u would be contracted.
//        /// </summary>
//        /// <param name="vertex"></param>
//        /// <returns></returns>
//        public float Calculate(uint vertex)
//        {
//            // get the neighbours.
//            var neighbours = _data.GetEdges(vertex);

//            // simulate the construction of new edges.
//            int newEdges = 0;
//            int removed = 0;
//            //var edgesForContractions = new List<Edge<CHEdgeData>>();
//            //var tos = new List<uint>();
//            //foreach (var neighbour in neighbours)
//            //{
//            //    edgesForContractions.Add(neighbour);
//            //    tos.Add(neighbour.Neighbour);
//            //    removed++;
//            //}

//            //// loop over all neighbours and check for witnesses.
//            //// loop over each combination of edges just once.
//            //var witnesses = new bool[edgesForContractions.Count];
//            //var tosWeights = new List<float>(edgesForContractions.Count);
//            //for (int x = 0; x < edgesForContractions.Count; x++)
//            //{ // loop over all elements first.
//            //    var xEdge = edgesForContractions[x];

//            //    // calculate max weight.
//            //    tosWeights.Clear();
//            //    for (int idx = 0; idx < edgesForContractions.Count; idx++)
//            //    {
//            //        // update maxWeight.
//            //        var yEdge = edgesForContractions[idx];
//            //        if (xEdge.Neighbour != yEdge.Neighbour)
//            //        {
//            //            // reset witnesses.
//            //            float weight = (float)xEdge.EdgeData.Weight + (float)yEdge.EdgeData.Weight;
//            //            witnesses[idx] = false;
//            //            tosWeights.Add(weight);
//            //        }
//            //        else
//            //        { // already set this to true, not use calculating it's witness.
//            //            witnesses[idx] = true;
//            //            tosWeights.Add(0);
//            //        }
//            //    }

//            //    _witnessCalculator.Exists(_data, xEdge.Neighbour, tos, tosWeights, int.MaxValue, ref witnesses);
//            //    for (int y = 0; y < edgesForContractions.Count; y++)
//            //    { // loop over all elements.
//            //        var yEdge = edgesForContractions[y];

//            //        if (yEdge.Neighbour != xEdge.Neighbour &&
//            //            yEdge.EdgeData.CanMoveForward &&
//            //            !witnesses[y])
//            //        { // the neighbours point to different vertices.
//            //            // a new edge is needed.
//            //            // no witness exists.
//            //            newEdges++;
//            //        }
//            //    }
//            //}

//            return (2 * newEdges) + removed;
//        }

//        /// <summary>
//        /// Notifies this calculator that the vertex was contracted.
//        /// </summary>
//        /// <param name="vertex"></param>
//        public void NotifyContracted(uint vertex)
//        {

//        }
//    }
//}