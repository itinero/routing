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
using OsmSharp.Units.Speed;
using OsmSharp.Units.Time;
using OsmSharp.Routing;
using OsmSharp.Routing.Instructions.ArcAggregation.Output;
using OsmSharp.Routing.Instructions.ArcAggregation;
using OsmSharp.Routing.Interpreter;
using OsmSharp;
using OsmSharp.Routing.Vehicles;

namespace OsmSharp.Routing.Metrics.Time
{
    /// <summary>
    /// A calculator to accurately estimate timings of a route.
    /// </summary>
    public class TimeCalculator : RouteMetricCalculator
    {
        /// <summary>
        /// Constant identifier for Time.
        /// </summary>
        public const string TIME_KEY = "Time_in_seconds";

        /// <summary>
        /// Constant identifier for Distance.
        /// </summary>
        public const string DISTANCE_KEY = "Distance_in_meter";

        /// <summary>
        /// Creates a new TimeCalculator.
        /// </summary>
        /// <param name="interpreter"></param>
        public TimeCalculator(IRoutingInterpreter interpreter)
             :base(interpreter)
        {
            
        }

        /// <summary>
        /// Calculcates the metrics.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="p"></param>
        /// <returns></returns>
        public override Dictionary<string, double> Calculate(Vehicle vehicle, AggregatedPoint p)
        {
            Dictionary<string, double> result = new Dictionary<string, double>();
            result.Add(DISTANCE_KEY, 0);
            result.Add(TIME_KEY, 0);

            Aggregated next = p;
            while (next != null)
            {
                if (next is AggregatedPoint)
                {
                    AggregatedPoint point = (next as AggregatedPoint);
                    this.CalculatePointMetrics(vehicle, result, point);
                }
                if (next is AggregatedArc)
                {
                    AggregatedArc arc = (next as AggregatedArc);
                    this.CalculateArcMetrics(vehicle, result, arc);
                }

                next = next.GetNext();
            }

            return result;
        }

        /// <summary>
        /// Calculate metrics for a given turn.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="result"></param>
        /// <param name="point"></param>
        private void CalculatePointMetrics(Vehicle vehicle, Dictionary<string, double> result, AggregatedPoint point)
        {
            if (point.Angle != null)
            {
                if (AggregatedHelper.IsTurn(point.Angle.Direction))
                {
                    // no calculations for distance.

                    // update the time.
                    Second second = 0;
                    // ESTIMATE THE INCREASE IN TIME.
                    // TODO: ASSUMED DRIVING ON THE RIGHT; UPDATE TO MAKE CONFIGURABLE.
                    switch (point.Angle.Direction)
                    {
                        case OsmSharp.Math.Geo.Meta.RelativeDirectionEnum.Left:
                        case OsmSharp.Math.Geo.Meta.RelativeDirectionEnum.SharpLeft:
                        case OsmSharp.Math.Geo.Meta.RelativeDirectionEnum.SlightlyLeft:
                            second = 25;
                            break;
                        case OsmSharp.Math.Geo.Meta.RelativeDirectionEnum.Right:
                        case OsmSharp.Math.Geo.Meta.RelativeDirectionEnum.SharpRight:
                        case OsmSharp.Math.Geo.Meta.RelativeDirectionEnum.SlightlyRight:
                            second = 5;
                            break;
                        case OsmSharp.Math.Geo.Meta.RelativeDirectionEnum.TurnBack:
                            second = 30;
                            break;
                    }
                    result[TIME_KEY] = result[TIME_KEY] + second.Value;
                }
                else
                {
                    if (point.ArcsNotTaken != null && point.ArcsNotTaken.Count > 0)
                    { // very simple estimate.
                        Second second = 0;

                        second = 5;

                        result[TIME_KEY] = result[TIME_KEY] + second.Value;

                    }
                }
            }
        }

        /// <summary>
        /// Calculate metrics for a given arc.
        /// </summary>
        /// <param name="vehicle"></param>
        /// <param name="result"></param>
        /// <param name="arc"></param>
        private void CalculateArcMetrics(Vehicle vehicle, Dictionary<string, double> result, AggregatedArc arc)
        {
            // update the distance.
            result[DISTANCE_KEY] = result[DISTANCE_KEY] + arc.Distance.Value;

            // update the time.
            KilometerPerHour speed = vehicle.ProbableSpeed(arc.Tags);
            Second time = arc.Distance / speed;

            // FOR NOW USE A METRIC OF 75% MAX SPEED.
            // TODO: improve this for a more realistic estimated based on the type of road.
            result[TIME_KEY] = result[TIME_KEY] + time.Value;
        }
    }
}
