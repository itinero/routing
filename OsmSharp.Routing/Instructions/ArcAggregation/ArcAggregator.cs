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

using OsmSharp.Collections.Tags;
using OsmSharp.Math.Geo;
using OsmSharp.Math.Geo.Meta;
using OsmSharp.Routing.Instructions.ArcAggregation.Output;
using OsmSharp.Routing.Interpreter;
using OsmSharp.Routing.Vehicles;
using OsmSharp.Units.Distance;
using System.Collections.Generic;

namespace OsmSharp.Routing.Instructions.ArcAggregation
{
    /// <summary>
    /// An arc aggregator.
    /// </summary>
    public class ArcAggregator
    {
        /// <summary>
        /// Holds the routing interpreter.
        /// </summary>
        private IRoutingInterpreter _interpreter;

        /// <summary>
        /// Creates a new arc aggregator.
        /// </summary>
        /// <param name="interpreter"></param>
        public ArcAggregator(IRoutingInterpreter interpreter)
        {
            _interpreter = interpreter;
        }

        /// <summary>
        /// Aggregates a route by remove information useless to the generation of routing instructions.
        /// </summary>
        /// <param name="route"></param>
        /// <returns></returns>
        public AggregatedPoint Aggregate(Route route)
        {
            // create the enumerator.
            var enumerator = new AggregatedPointEnumerator(route);

            AggregatedRoutePoint previous = null;
            AggregatedRoutePoint current = null;
            AggregatedRoutePoint next = null;
            AggregatedPoint previousPoint = null;
            AggregatedArc previousArc = null;
            AggregatedPoint p = null;

            // loop over all aggregated points.
            while (enumerator.MoveNext())
            {
                // get the next point.
                next = enumerator.Current;

                // process 
                this.Process(route, previous, current, next, ref p, ref previousArc, ref previousPoint);

                // make the next, current and the current previous.
                previous = current;
                current = next;
                next = null;
            }

            // process once more, the current current has not been processed.
            this.Process(route, previous, current, next, ref p, ref previousArc, ref previousPoint);

            return p;
        }

        /// <summary>
        /// Processes a part of the route.
        /// </summary>
        private void Process(Route route, AggregatedRoutePoint previous, AggregatedRoutePoint current, 
            AggregatedRoutePoint next, ref AggregatedPoint p, ref AggregatedArc previousArc, ref AggregatedPoint previousPoint)
        {
            // process the current point.
            if (current != null)
            {
                if (previous == null)
                { // point is always significant, it is the starting point!
                    // create point.
                    p = new AggregatedPoint();
                    p.Angle = null;
                    p.ArcsNotTaken = null;
                    p.Location = new GeoCoordinate(current.Segment.Latitude, current.Segment.Longitude);
                    p.Points = new List<PointPoi>();
                    p.SegmentIdx = current.SegmentIndex;

                    if (current.Segment.Points != null)
                    {
                        foreach (var routePoint in current.Segment.Points)
                        {
                            var poi = new PointPoi();
                            poi.Name = routePoint.Name;
                            poi.Tags = routePoint.Tags.ConvertTo();
                            poi.Location = new GeoCoordinate(routePoint.Latitude, routePoint.Longitude);
                            poi.Angle = null; // there is no previous point; no angle is specified.
                            p.Points.Add(poi);
                        }
                    }

                    previousPoint = p;
                }
                else
                { // test if point is significant.
                    var nextArc = this.CreateArcAndPoint(route, previous, current, next);

                    // test if the next point is significant.
                    if (previousArc == null)
                    { // this arc is always significant; it is the first arc.
                        previousPoint.Next = nextArc;
                        previousArc = nextArc;
                    }
                    else
                    { // there is a previous arc; a test can be done if the current point is significant.
                        if (this.IsSignificant(previousArc, nextArc))
                        { // the arc is significant; append it to the previous arc.
                            previousArc.Next.Next = nextArc;
                            previousArc = nextArc;
                            previousPoint = nextArc.Next;
                        }
                        else
                        { // if the arc is not significant compared to the previous one, the previous one can extend until the next point.
                            // THIS IS THE AGGREGATION STEP!

                            // add distance.
                            var distanceToNext = previousArc.Next.Location.DistanceReal(nextArc.Next.Location);
                            previousArc.Distance = previousArc.Distance + distanceToNext;

                            // set point.
                            previousArc.Next = nextArc.Next;
                        }
                    }
                }
            }
        }

        /// <summary>
        /// Returns true if the point between the two arcs represents a significant step in the route.
        /// </summary>
        /// <param name="previousArc"></param>
        /// <param name="nextArc"></param>
        /// <returns></returns>
        protected virtual bool IsSignificant(AggregatedArc previousArc, AggregatedArc nextArc)
        {
            if (previousArc.Next.Points != null && previousArc.Next.Points.Count > 0)
            { // the point has at least one important point.
                return true;
            }
            if (previousArc.Next.ArcsNotTaken != null && previousArc.Next.ArcsNotTaken.Count > 0)
            { // the point has at least one arc not taken.
                return true;
            }

            // a vehicle change is also always significant.
            if(previousArc.Vehicle != nextArc.Vehicle)
            { // there is a vehicle change.
                return true;
            }

            // create tag interpreters for arcs to try and work out if the arcs are different for the given vehicle.
            var previousTagsDic = new TagsCollection();
            if (previousArc.Tags != null)
            {
                foreach (var pair in previousArc.Tags)
                {
                    previousTagsDic.Add(pair.Key, pair.Value);
                }
            }
            var nextTagsDic = new TagsCollection();
            if (nextArc.Tags != null)
            {
                foreach (var pair in nextArc.Tags)
                {
                    nextTagsDic.Add(pair.Key, pair.Value);
                }
            }
            if (!string.IsNullOrWhiteSpace(previousArc.Vehicle))
            { // there is a vehicle set on the previous arc.
                var vehicle = Vehicle.GetByUniqueName(previousArc.Vehicle);
                if (!vehicle.IsEqualFor(previousTagsDic, nextTagsDic))
                { // the previous and the next edge do not represent a change for the given vehicle.
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Generates an arc and it's next point from the current aggregated point.
        /// </summary>
        /// <param name="route"></param>
        /// <param name="previous"></param>
        /// <param name="current"></param>
        /// <param name="next"></param>
        /// <returns></returns>
        internal AggregatedArc CreateArcAndPoint(Route route, AggregatedRoutePoint previous, 
            AggregatedRoutePoint current, AggregatedRoutePoint next)
        {
            // create the arc.
            var a = new AggregatedArc();
            a.Name = current.Segment.Name;
            a.Names = current.Segment.Names.ConvertTo();
            a.Tags = current.Segment.Tags.ConvertToTagsCollection();
            a.Vehicle = string.IsNullOrWhiteSpace(route.Vehicle) ? current.Segment.Vehicle : route.Vehicle;
            if (previous != null)
            {
                var previousCoordinate = new GeoCoordinate(previous.Segment.Latitude, previous.Segment.Longitude);
                var currentCoordinate = new GeoCoordinate(current.Segment.Latitude, current.Segment.Longitude);

                var distance = previousCoordinate.DistanceReal(currentCoordinate);
                a.Distance = distance;
            }

            // create the point.
            var p = new AggregatedPoint();
            p.Location = new GeoCoordinate(current.Segment.Latitude, current.Segment.Longitude);
            p.Points = new List<PointPoi>();
            p.SegmentIdx = current.SegmentIndex;
            if (previous != null && next != null && next.Segment != null)
            {
                var previousCoordinate = new GeoCoordinate(previous.Segment.Latitude, previous.Segment.Longitude);
                var nextCoordinate = new GeoCoordinate(next.Segment.Latitude, next.Segment.Longitude);

                p.Angle = RelativeDirectionCalculator.Calculate(previousCoordinate, p.Location, nextCoordinate);
            }
            if (current.Segment.SideStreets != null && current.Segment.SideStreets.Length > 0)
            {
                p.ArcsNotTaken = new List<KeyValuePair<RelativeDirection, AggregatedArc>>();
                foreach (var sideStreet in current.Segment.SideStreets)
                {
                    var side = new AggregatedArc();
                    side.Name = sideStreet.Name;
                    side.Names = sideStreet.Names.ConvertTo();
                    side.Tags = sideStreet.Tags.ConvertToTagsCollection();

                    RelativeDirection sideDirection = null;
                    if (previous != null)
                    {
                        var previousCoordinate = new GeoCoordinate(previous.Segment.Latitude, previous.Segment.Longitude);
                        var nextCoordinate = new GeoCoordinate(sideStreet.Latitude, sideStreet.Longitude);

                        sideDirection = RelativeDirectionCalculator.Calculate(previousCoordinate, p.Location, nextCoordinate);
                    }

                    p.ArcsNotTaken.Add(new KeyValuePair<RelativeDirection, AggregatedArc>(sideDirection, side));
                }
            }
            if (current.Segment.Points != null)
            {
                foreach (var routePoint in current.Segment.Points)
                {
                    var poi = new PointPoi();
                    poi.Name = routePoint.Name;
                    poi.Tags = routePoint.Tags.ConvertTo();
                    poi.Location = new GeoCoordinate(routePoint.Latitude, routePoint.Longitude);

                    var previousCoordinate = new GeoCoordinate(previous.Segment.Latitude, previous.Segment.Longitude);
                    var currentCoordinate = new GeoCoordinate(current.Segment.Latitude, current.Segment.Longitude);
                    poi.Angle = RelativeDirectionCalculator.Calculate(previousCoordinate, currentCoordinate, poi.Location);

                    p.Points.Add(poi);
                }
            }

            // link the arc to the point.
            a.Next = p;

            return a;
        }
    }

    /// <summary>
    /// Enumerates all aggregated points.
    /// </summary>
    internal class AggregatedPointEnumerator : IEnumerator<AggregatedRoutePoint>
    {
        /// <summary>
        /// Holds the entry index.
        /// </summary>
        private int _segmentIdx;

        /// <summary>
        /// Holds the route.
        /// </summary>
        private Route _route;

        /// <summary>
        /// Creates a new agregrated point enumerator.
        /// </summary>
        /// <param name="route"></param>
        public AggregatedPointEnumerator(Route route)
        {
            _segmentIdx = -1;

            _route = route;
            _current = null;
        }

        #region IEnumerator<AggregatedRoutePoint> Members

        /// <summary>
        /// Holds the current point.
        /// </summary>
        private AggregatedRoutePoint _current;

        /// <summary>
        /// Returns the current point.
        /// </summary>
        public AggregatedRoutePoint Current
        {
            get 
            {
                if (_current == null)
                {
                    _current = new AggregatedRoutePoint();
                    _current.SegmentIndex = _segmentIdx;
                    _current.Segment = _route.Segments[_segmentIdx];                    
                }
                return _current;
            }
        }

        #endregion

        #region IDisposable Members

        /// <summary>
        /// Disposes all resources associated with this enumerator.
        /// </summary>
        public void Dispose()
        {

        }

        #endregion

        #region IEnumerator Members

        /// <summary>
        /// Returns the current point.
        /// </summary>
        object System.Collections.IEnumerator.Current
        {
            get { return this.Current; }
        }

        /// <summary>
        /// Moves to the next point.
        /// </summary>
        /// <returns></returns>
        public bool MoveNext()
        {
            _current = null;
            if (_route.Segments == null)
            {
                return false;
            }
            _segmentIdx++;
            return _route.Segments.Length > _segmentIdx;
        }

        /// <summary>
        /// Resets this enumerator.
        /// </summary>
        public void Reset()
        {
            _current = null;

            _segmentIdx = -1;
        }

        #endregion
    }

    /// <summary>
    /// Represents an aggregated point.
    /// </summary>
    internal class AggregatedRoutePoint
    {
        /// <summary>
        /// Gets or sets the entry index.
        /// </summary>
        public int SegmentIndex { get; set; }

        /// <summary>
        /// Gets or sets the route point entry.
        /// </summary>
        public RouteSegment Segment { get; set; }
    }
}