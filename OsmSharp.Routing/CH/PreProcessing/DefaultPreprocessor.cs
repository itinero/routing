// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2015 Abelshausen Ben
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

using OsmSharp.Routing.CH.Preprocessing.Ordering;
using OsmSharp.Routing.CH.Preprocessing.Witnesses;
using OsmSharp.Routing.Graph;
using OsmSharp.Routing.Graph.PreProcessor;

namespace OsmSharp.Routing.CH.Preprocessing
{
    /// <summary>
    /// The default preprocessor to use on all to-be-contracted graphs.
    /// </summary>
    public class DefaultPreprocessor : IPreprocessor
    {
        private readonly CHPreprocessor _preprocessor;

        /// <summary>
        /// Creates a new preprocessor.
        /// </summary>
        public DefaultPreprocessor(GraphBase<CHEdgeData> graph)
        {
            var witnessCalculator = new DykstraWitnessCalculator();
            var edgeDifference = new EdgeDifferenceContractedSearchSpace(graph, witnessCalculator);
            _preprocessor = new CHPreprocessor(graph, edgeDifference, witnessCalculator);
        }

        /// <summary>
        /// Creates a new preprocessor.
        /// </summary>
        public DefaultPreprocessor(CHPreprocessor preprocessor)
        {
            _preprocessor = preprocessor;
        }

        /// <summary>
        /// Starts this preprocessor.
        /// </summary>
        public void Start()
        {
            // first sort the graph ...
            var sortingPreprocessor = new HilbertSortingPreprocessor<CHEdgeData>(_preprocessor.Target);
            sortingPreprocessor.Start();

            // ... and the build the hierarchy.
            _preprocessor.Start();
        }
    }
}