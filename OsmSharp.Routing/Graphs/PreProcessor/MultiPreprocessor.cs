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

namespace OsmSharp.Routing.Graph.PreProcessor
{
    /// <summary>
    /// A preprocessor that represents a sequence of other processors.
    /// </summary>
    public class MultiPreprocessor : IPreprocessor
    {
        private readonly IPreprocessor[] _preprocessors;

        /// <summary>
        /// Creates a new multi-preprocessor.
        /// </summary>
        /// <param name="preprocessors"></param>
        public MultiPreprocessor(params IPreprocessor[] preprocessors)
        {
            _preprocessors = preprocessors;
        }

        /// <summary>
        /// Starts this preprocessor.
        /// </summary>
        public void Start()
        {
            for(var i = 0; i < _preprocessors.Length; i++)
            {
                _preprocessors[i].Start();
            }
        }
    }
}