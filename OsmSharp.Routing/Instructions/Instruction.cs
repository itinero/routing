// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2014 Abelshausen Ben
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

using OsmSharp.Math.Geo;
using OsmSharp.Routing.Instructions.ArcAggregation.Output;
using System.Collections.Generic;

namespace OsmSharp.Routing.Instructions
{
    /// <summary>
    /// Represents an instruction.
    /// </summary>
    public class Instruction
    {
        /// <summary>
        /// Creates a new instruction with only a location.
        /// </summary>
        /// <param name="metaData"></param>
        /// <param name="firstSegmentIdx"></param>
        /// <param name="lastSegmentIdx"></param>
        /// <param name="location"></param>
        /// <param name="text"></param>
        public Instruction(Dictionary<string, object> metaData, int firstSegmentIdx, int lastSegmentIdx, GeoCoordinateBox location, string text)
            : this(metaData, firstSegmentIdx, lastSegmentIdx, location, text, new List<PointPoi>())
        {

        }

        /// <summary>
        /// Creates a new instruction with a location and points of interest.
        /// </summary>
        /// <param name="metaData"></param>
        /// <param name="firstSegmentIdx"></param>
        /// <param name="lastSegmentIdx"></param>
        /// <param name="location"></param>
        /// <param name="text"></param>
        /// <param name="pois"></param>
        public Instruction(Dictionary<string, object> metaData, int firstSegmentIdx, int lastSegmentIdx, GeoCoordinateBox location, string text, List<PointPoi> pois)
        {
            this.FirstSegmentIdx = firstSegmentIdx;
            this.LastSegmentIdx = lastSegmentIdx;
            this.Location = location;
            this.Pois = pois;
            this.MetaData = metaData;
            this.Text = text;

            this.Pois = new List<PointPoi>(pois);
        }

        /// <summary>
        /// The points of interest for this instruction.
        /// </summary>
        public virtual List<PointPoi> Pois { get; protected set; }

        /// <summary>
        /// The location of this instruction.
        /// </summary>
        public virtual GeoCoordinateBox Location { get; protected set; }

        /// <summary>
        /// Gets the first segment idx.
        /// </summary>
        public virtual int FirstSegmentIdx { get; protected set; }

        /// <summary>
        /// Gets the last segment idx.
        /// </summary>
        public virtual int LastSegmentIdx { get; protected set; }

        /// <summary>
        /// Gets the instruction text.
        /// </summary>
        public virtual string Text { get; protected set; }

        /// <summary>
        /// Gets or sets the meta-data.
        /// </summary>
        public Dictionary<string, object> MetaData { get; private set; }

        /// <summary>
        /// Returns a string that represents the current coordinate.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.Text;
        }
    }
}
