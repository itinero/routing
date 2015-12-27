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

using OsmSharp.Geo.Attributes;
using OsmSharp.Geo.Features;
using OsmSharp.Geo.Geometries;
using OsmSharp.Math.Geo;
using System;
using System.Collections.Generic;

namespace OsmSharp.Routing.Algorithms.Routes
{
    /// <summary>
    /// A route segment aggregator. Groups together segments and converts them to features.
    /// </summary>
    public class RouteSegmentAggregator : AlgorithmBase
    {
        private readonly Route _route;
        private readonly Func<RouteSegment, RouteSegment, RouteSegment> _aggregate;

        /// <summary>
        /// Creates a new route segment aggregator.
        /// </summary>
        public RouteSegmentAggregator(Route route, Func<RouteSegment, RouteSegment, RouteSegment> aggregate)
        {
            if (route == null) { throw new ArgumentNullException("route"); }
            if (aggregate == null) { throw new ArgumentNullException("aggregate"); }

            _route = route;
            _aggregate = aggregate;
        }

        private FeatureCollection _features;

        /// <summary>
        /// Gets the features.
        /// </summary>
        public FeatureCollection Features
        {
            get
            {
                return _features;
            }
        }

        /// <summary>
        /// Exectes the actual run of the algorithm.
        /// </summary>
        protected override void DoRun()
        {
            _features = new FeatureCollection();
            if (_route.Segments == null ||
                _route.Segments.Count == 0)
            {
                return;
            }
            RouteSegment current = null;
            var currentShape = new List<GeoCoordinate>();
            currentShape.Add(new GeoCoordinate(_route.Segments[0].Latitude, _route.Segments[0].Longitude));
            this.AddPoints(_route.Segments[0]);
            for (int i = 1; i < _route.Segments.Count; i++)
            {
                // try to aggregate.
                if (current == null)
                { // there is no current yet, set it.
                    current = _route.Segments[i];
                }
                else
                { // try to merge the current segment with the next one.
                    var aggregated = _aggregate(current, _route.Segments[i]);
                    if (aggregated == null)
                    { // convert the current segment.
                        this.AddSegment(current, currentShape);

                        currentShape.Clear();
                        currentShape.Add(new GeoCoordinate(current.Latitude, current.Longitude));
                        current = _route.Segments[i];
                    }
                    else
                    { // keep the aggregated as current.
                        current = aggregated;
                    }
                }

                // add to shape.
                currentShape.Add(new GeoCoordinate(_route.Segments[i].Latitude, _route.Segments[i].Longitude));

                // add points for the current segment.
                this.AddPoints(_route.Segments[i]);
            }

            if (current != null)
            { // add the final segment.
                this.AddSegment(_route.Segments[_route.Segments.Count - 1], currentShape);
            }

            this.HasSucceeded = true;
        }

        /// <summary>
        /// Adds a segment with the given shape.
        /// </summary>
        private void AddSegment(RouteSegment segment, List<GeoCoordinate> shape)
        {
            var segmentLineString = new LineString(shape);

            var segmentTags = segment.Tags;
            var attributesTable = new SimpleGeometryAttributeCollection();
            if (segmentTags != null)
            { // there are tags.
                foreach (var tag in segmentTags)
                {
                    attributesTable.Add(tag.Key, tag.Value);
                }
            }
            attributesTable.Add("time", segment.Time);
            attributesTable.Add("distance", segment.Distance);
            attributesTable.Add("profile", segment.Profile);
            _features.Add(new Feature(segmentLineString, attributesTable));
        }

        /// <summary>
        /// Adds the points for the given segment if any.
        /// </summary>
        private void AddPoints(RouteSegment segment)
        {
            // create points.
            if (segment.Points != null)
            {
                foreach (var point in segment.Points)
                {
                    // build attributes.
                    var currentPointTags = point.Tags;
                    var attributesTable = new SimpleGeometryAttributeCollection();
                    if (currentPointTags != null)
                    { // there are tags.
                        foreach (var tag in currentPointTags)
                        {
                            attributesTable.Add(tag.Key, tag.Value);
                        }
                    }

                    // build feature.
                    var pointGeometry = new Point(new GeoCoordinate(point.Latitude, point.Longitude));
                    _features.Add(new Feature(pointGeometry, attributesTable));
                }
            }
        }

        /// <summary>
        /// A default function to aggregate per mode (or profile).
        /// </summary>
        public static Func<RouteSegment, RouteSegment, RouteSegment> ModalAggregator = (x, y) =>
            {
                if (x.Profile == y.Profile)
                {
                    var tags = x.Tags;
                    if (tags != null && y.Tags != null)
                    {
                        RouteTagsExtensions.AddOrReplace(ref tags, y.Tags);
                    }
                    return new RouteSegment()
                    {
                        Distance = y.Distance,
                        Latitude = y.Latitude,
                        Longitude = y.Longitude,
                        Profile = y.Profile,
                        Time = y.Time,
                        Tags = tags,
                        Metrics = y.Metrics,
                        SideStreets = y.SideStreets
                    };
                }
                return null;
            };
    }
}