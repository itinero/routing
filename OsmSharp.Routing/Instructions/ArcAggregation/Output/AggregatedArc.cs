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
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OsmSharp.Collections.Tags;
using OsmSharp.Units.Distance;

namespace OsmSharp.Routing.Instructions.ArcAggregation.Output
{
    /// <summary>
    /// Represents an arc in the aggregated route.
    /// </summary>
    public class AggregatedArc : Aggregated
    {
        /// <summary>
        /// Holds the next point.
        /// </summary>
        private AggregatedPoint _next;

        /// <summary>
        /// The arc following this point.
        /// </summary>
        public AggregatedPoint Next
        {
            get
            {
                return _next;
            }
            set
            {
                _next = value;
                _next.Previous = this;
            }
        }

        /// <summary>
        /// The arc before this point.
        /// </summary>
        public AggregatedPoint Previous { get; internal set; }

        /// <summary>
        /// Returns the next aggregated.
        /// </summary>
        /// <returns></returns>
        public override Aggregated GetNext()
        {
            return this.Next;
        }

        /// <summary>
        /// The distance in meter.
        /// </summary>
        public Meter Distance { get; set; }

        #region Properties

        /// <summary>
        /// The default name.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The vehicle unique name.
        /// </summary>
        public string Vehicle { get; set; }
        
        /// <summary>
        /// The name in different languages.
        /// </summary>
        public List<KeyValuePair<string, string>> Names { get; set; }

        /// <summary>
        /// The tags/properties.
        /// </summary>
        public TagsCollectionBase Tags { get; set; }

        #endregion
    }
}
