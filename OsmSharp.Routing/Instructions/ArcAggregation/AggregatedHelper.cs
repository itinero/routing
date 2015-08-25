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

using OsmSharp.Math.Geo.Meta;

namespace OsmSharp.Routing.Instructions.ArcAggregation
{
    /// <summary>
    /// Contains some helper functions for arc aggregation.
    /// </summary>
    public class AggregatedHelper
    {
        /// <summary>
        /// Returns true if the given direction is left.
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public static bool IsLeft(RelativeDirectionEnum direction)
        {
            switch (direction)
            {
                case RelativeDirectionEnum.Left:
                case RelativeDirectionEnum.SharpLeft:
                case RelativeDirectionEnum.SlightlyLeft:
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Returns true if the given direction is right.
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public static bool IsRight(RelativeDirectionEnum direction)
        {
            switch (direction)
            {
                case RelativeDirectionEnum.Right:
                case RelativeDirectionEnum.SharpRight:
                case RelativeDirectionEnum.SlightlyRight:
                    return true;
            }
            return false;
        }

        /// <summary>
        /// Returns true if the given direction is a turn.
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        public static bool IsTurn(RelativeDirectionEnum direction)
        {
            switch (direction)
            {
                case RelativeDirectionEnum.StraightOn:
                    return false;
            }
            return true;
        }
    }
}
