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

using Itinero.Algorithms.Collections;
using Itinero.Algorithms.Contracted;
using System.Collections.Generic;

namespace Itinero.Test.Algorithms.Contracted
{
    class MockPriorityCalculator : IPriorityCalculator
    {
        private readonly Dictionary<uint, float> _priorities;

        public MockPriorityCalculator(Dictionary<uint, float> priorities)
        {
            _priorities = priorities;
        }

        public float Calculate(BitArray32 contractedFlags, uint vertex)
        {
            return _priorities[vertex];
        }

        public void NotifyContracted(uint vertex)
        {

        }
    }
}