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

using OsmSharp.Routing.Graph;

namespace OsmSharp.Routing.Test.Graph
{
    /// <summary>
    /// A mock of graph edge data
    /// </summary>
    struct EdgeDataMock : IEdgeData
    {
        public EdgeDataMock(int id)
            : this()
        {
            this.Id = id;
        }

        public int Id { get; set; }

        public IEdgeData Reverse()
        {
            return new EdgeDataMock()
            {
                Id = -this.Id
            };
        }

        public bool Equals(IEdgeData other)
        {
            return ((EdgeDataMock)other).Id == this.Id;
        }
    }
}