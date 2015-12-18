 //OsmSharp - OpenStreetMap (OSM) SDK
 //Copyright (C) 2015 Abelshausen Ben
 
 //This file is part of OsmSharp.
 
 //OsmSharp is free software: you can redistribute it and/or modify
 //it under the terms of the GNU General Public License as published by
 //the Free Software Foundation, either version 2 of the License, or
 //(at your option) any later version.
 
 //OsmSharp is distributed in the hope that it will be useful,
 //but WITHOUT ANY WARRANTY; without even the implied warranty of
 //MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
 //GNU General Public License for more details.
 
 //You should have received a copy of the GNU General Public License
 //along with OsmSharp. If not, see <http://www.gnu.org/licenses/>.

using System;

namespace OsmSharp.Routing.Data.Contracted
{
     ///<summary>
     ///Represents the data on a contracted edge.
     ///</summary>
    public struct ContractedEdgeData
    {
        ///<summary>
        ///Gets or sets the weight.
        ///</summary>
        public float Weight { get; set; }

        ///<summary>
        ///Gets or sets the direction.
        ///</summary>
        public bool? Direction { get; set; }

        ///<summary>
        ///Gets or sets the id of the contracted vertex.
        ///</summary>
        public uint ContractedId { get; set; }
    }
}