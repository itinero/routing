// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2016 Abelshausen Ben
// 
// This file is part of Itinero.
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
// along with Itinero. If not, see <http://www.gnu.org/licenses/>.

using Itinero.Algorithms;
using Itinero.Algorithms.Search;

namespace Itinero.Test.Algorithms.Search
{
    /// <summary>
    /// A mock resolver.
    /// </summary>
    class MockResolver : AlgorithmBase, IResolver
    {
        private readonly RouterPoint _result;

        public MockResolver(RouterPoint result)
        {
            _result = result;
        }

        public RouterPoint Result
        {
            get { return _result; }
        }

        protected override void DoRun()
        {
            if(_result != null)
            {
                this.HasSucceeded = true;
                return;
            }
            this.ErrorMessage = "Cannot resolve.";
            this.HasSucceeded = false;
            return;
        }
    }
}