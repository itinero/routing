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
using OsmSharp.Units.Angle;
using OsmSharp.Math.Geo;
using OsmSharp.Math.Geo.Meta;

namespace OsmSharp.Routing.Instructions.ArcAggregation.Output
{
    /// <summary>
    /// Represents a point in an aggregated route.
    /// </summary>
    public class AggregatedPoint : Aggregated
    {
        /// <summary>
        /// Holds the next arc.
        /// </summary>
        private AggregatedArc _next;

        /// <summary>
        /// The physical location of this point.
        /// </summary>
        public GeoCoordinate Location { get; set; }

        /// <summary>
        /// The arc following this point.
        /// </summary>
        public AggregatedArc Next
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
        public AggregatedArc Previous { get; internal set; }

        /// <summary>
        /// Returns the next aggregated.
        /// </summary>
        /// <returns></returns>
        public override Aggregated GetNext()
        {
            return this.Next;
        }

        #region Properties

        /// <summary>
        /// The angle between the end of the previous arc and the beginning of the next arc.
        /// </summary>
        public RelativeDirection Angle { get; set; }

        /// <summary>
        /// The segment index.
        /// </summary>
        public int SegmentIdx { get; set; }

        /// <summary>
        /// The point of points at this location.
        /// </summary>
        public List<PointPoi> Points { get; set; }

        #endregion

        #region Arcs-not-taken

        /// <summary>
        /// List of the arcs not taken and their angle with respect to the end of the previous arc.
        /// </summary>
        public List<KeyValuePair<RelativeDirection, AggregatedArc>> ArcsNotTaken { get; set; }

        #endregion
    }

    /// <summary>
    /// Represents a point that is being routed to/from and it's properties.
    /// </summary>
    public class PointPoi
    {
        /// <summary>
        /// The angle between the direction of the latest arc and the direction of this poi.
        /// </summary>
        public RelativeDirection Angle { get; set; }

        /// <summary>
        /// The physical location of this point.
        /// </summary>
        public GeoCoordinate Location { get; set; }

        /// <summary>
        /// The name of the point.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The tags/properties.
        /// </summary>
        public List<KeyValuePair<string, string>> Tags { get; set; }
    }
}
