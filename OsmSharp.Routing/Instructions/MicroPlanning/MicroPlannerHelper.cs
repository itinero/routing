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
using OsmSharp.Routing.Instructions.ArcAggregation.Output;
using OsmSharp.Routing.Interpreter;
using System.Collections.Generic;

namespace OsmSharp.Routing.Instructions.MicroPlanning
{
    /// <summary>
    /// Holds helper methods for microplanners.
    /// </summary>
    public static class MicroPlannerHelper
    {
        /// <summary>
        /// Returns true if the given direction can be considered 'left'.
        /// </summary>
        /// <param name="direction">The direction.</param>
        /// <param name="interpreter">The routing interpreter.</param>
        /// <returns></returns>
        public static bool IsLeft(RelativeDirectionEnum direction, IRoutingInterpreter interpreter)
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
        /// Returns true if the given direction can be considered 'right'.
        /// </summary>
        /// <param name="direction">The direction.</param>
        /// <param name="interpreter">The routing interpreter.</param>
        /// <returns></returns>
        public static bool IsRight(RelativeDirectionEnum direction, IRoutingInterpreter interpreter)
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
        /// Returns true if the given direction can be considered 'turning'.
        /// </summary>
        /// <param name="direction">The direction.</param>
        /// <param name="interpreter">The routing interpreter.</param>
        /// <returns></returns>
        public static bool IsTurn(RelativeDirectionEnum direction, IRoutingInterpreter interpreter)
        {
            switch (direction)
            {
                case RelativeDirectionEnum.StraightOn:
                    return false;
            }
            return true;
        }

        public static int GetStraightOn(MicroPlannerMessagePoint point, IRoutingInterpreter interpreter)
        {
            int straight = 0;
            if (point.Point.ArcsNotTaken != null)
            {
                foreach (KeyValuePair<RelativeDirection, AggregatedArc> arc_pair in point.Point.ArcsNotTaken)
                {
                    if (!MicroPlannerHelper.IsTurn(arc_pair.Key.Direction, interpreter))
                    {
                        if (interpreter.EdgeInterpreter.IsRoutable(arc_pair.Value.Tags))
                        {
                            straight++;
                        }
                    }
                }
            }
            return straight;
        }

        public static int GetLeft(IList<MicroPlannerMessage> messages, IRoutingInterpreter interpreter)
        {
            int left = 0;
            foreach (MicroPlannerMessage message in messages)
            {
                if (message is MicroPlannerMessagePoint)
                {
                    MicroPlannerMessagePoint point = (message as MicroPlannerMessagePoint);
                    left = left + MicroPlannerHelper.GetLeft(point, interpreter);
                }
            }
            return left;
        }

        public static int GetLeft(MicroPlannerMessagePoint point, IRoutingInterpreter interpreter)
        {
            int left = 0;
            if (point.Point.ArcsNotTaken != null)
            {
                foreach (KeyValuePair<RelativeDirection, AggregatedArc> arc_pair in point.Point.ArcsNotTaken)
                {
                    if (MicroPlannerHelper.IsLeft(arc_pair.Key.Direction, interpreter))
                    {
                        if (interpreter.EdgeInterpreter.IsRoutable(arc_pair.Value.Tags))
                        {
                            left++;
                        }
                    }
                }
            }
            return left;
        }

        public static int GetRight(IList<MicroPlannerMessage> messages, IRoutingInterpreter interpreter)
        {
            int right = 0;
            foreach (MicroPlannerMessage message in messages)
            {
                if (message is MicroPlannerMessagePoint)
                {
                    MicroPlannerMessagePoint point = (message as MicroPlannerMessagePoint);
                    right = right + MicroPlannerHelper.GetRight(point, interpreter);
                }
            }
            return right;
        }

        public static int GetRight(MicroPlannerMessagePoint point, IRoutingInterpreter interpreter)
        {
            int right = 0;
            if (point.Point.ArcsNotTaken != null)
            {
                foreach (KeyValuePair<RelativeDirection, AggregatedArc> arc_pair in point.Point.ArcsNotTaken)
                {
                    if (MicroPlannerHelper.IsRight(arc_pair.Key.Direction, interpreter))
                    {
                        if (interpreter.EdgeInterpreter.IsRoutable(arc_pair.Value.Tags))
                        {
                            right++;
                        }
                    }
                }
            }
            return right;
        }
    }
}
