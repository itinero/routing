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

using OsmSharp.Routing.Instructions.ArcAggregation.Output;

namespace OsmSharp.Routing.Instructions.MicroPlanning
{
    /// <summary>
    /// A micro message that holds and arc.
    /// </summary>
    public class MicroPlannerMessageArc : MicroPlannerMessage
    {
        /// <summary>
        /// Creates a new microplanner message.
        /// </summary>
        /// <param name="route"></param>
        public MicroPlannerMessageArc(Route route)
            : base(route)
        {

        }

        /// <summary>
        /// Gets or sets the aggregated arc.
        /// </summary>
        public AggregatedArc Arc { get; set; }

        /// <summary>
        /// Returns a System.String that represents the current System.Object.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("Arc:Distance={0},Name={1},Tags={2}", this.Arc.Distance, this.Arc.Name, this.Arc.Tags);
        }
    }
}