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

using OsmSharp.Collections.Coordinates.Collections;
using OsmSharp.Collections.Tags;
using OsmSharp.Routing.Network;
using OsmSharp.Routing.Profiles;
using System.Collections.Generic;
using System.Linq;

namespace OsmSharp.Routing.Algorithms
{
    /// <summary>
    /// An algorithm to build a route from a path.
    /// </summary>
    public class RouteBuilder : AlgorithmBase
    {
        private readonly RouterDb _routerDb;
        private readonly List<uint> _path;
        private readonly Profile _vehicleProfile;
        private readonly RouterPoint _source;
        private readonly RouterPoint _target;

        /// <summary>
        /// Creates a router builder.
        /// </summary>
        public RouteBuilder(RouterDb routerDb, Profile vehicleProfile, RouterPoint source, RouterPoint target, List<uint> path)
        {
            _routerDb = routerDb;
            _path = path;
            _source = source;
            _target = target;
            _vehicleProfile = vehicleProfile;
        }

        private Route _route;

        /// <summary>
        /// Executes the actual run of the algorithm.
        /// </summary>
        protected override void DoRun()
        {
            // build the route.
            _route = new Route();
            _route.Segments = new List<RouteSegment>();
            if (_path.Count == 1)
            { // only one vertex, route has length of zero.
                if (_source.EdgeId != _target.EdgeId ||
                    _source.Offset != _target.Offset)
                { // a route of one vertex but source and target do not match.
                    this.ErrorMessage = "Target and source have to be indentical with a route with only one vertex.";
                    return;
                }
                var segment = RouteSegment.CreateNew(_source.Location(), _vehicleProfile);
                segment.SetStop(new ICoordinate[] { _source.Location(), _target.Location() },
                    new TagsCollectionBase[] { _source.Tags, _target.Tags });
                _route.Segments.Add(segment);
                this.HasSucceeded = true;
                return;
            }
            if (_path.Count == 2 && (
                _path[1] == Constants.NO_VERTEX ||
                _target.IsVertex(_routerDb, _path[1])))
            { // this path represents a short path located within one edge or the edge max.
                this.AddSourceTarget();
                this.HasSucceeded = true;
                return;
            }

            // path is longer, at least one real vertex in there.
            // build the route by expanding the edges/shapes.

            // add the source.
            if (_path.Count ==  3 &&
                _path[2] == Constants.NO_VERTEX ||
                _target.IsVertex(_routerDb, _path[2]))
            { // the next one is the next one of the target.
                this.AddSource(_path[1], _routerDb.Network.GetEdge(_source.EdgeId).GetOther(_path[1]),
                    _routerDb.Network.GetEdge(_target.EdgeId).GetOther(_path[1]));
                this.AddTarget(_path[1]);
                this.HasSucceeded = true;
                return;
            }

            // add source.
            var offset = 1;
            if(_source.IsVertex(_routerDb, _path[0]))
            { // just add the first edge as normal.
                this.AddSource(_path[0]);
                offset = 0;
            }
            else
            { // add the source.
                this.AddSource(_path[1], _routerDb.Network.GetEdge(_source.EdgeId).GetOther(_path[1]),
                    _path[2]);
            }

            // add intermediate points.
            while(offset < _path.Count - 2)
            {
                if(_path[offset + 2] == Constants.NO_VERTEX)
                { // next is target.
                    this.Add(_path[offset + 1], _path[offset + 0], _routerDb.Network.GetEdge(_target.EdgeId).GetOther(
                        _path[offset + 1]));

                    // add the target.
                    this.AddTarget(_path[_path.Count - 2]);
                    this.HasSucceeded = true;
                    break;
                }
                else if(_target.IsVertex(_routerDb, _path[offset + 2]))
                { // next is the target, last is a vertex.
                    this.Add(_path[offset + 1], _path[offset + 0], _path[offset + 2]);

                    // add the target.
                    this.AddTarget(_path[_path.Count - 1], _path[_path.Count - 2]);
                    this.HasSucceeded = true;
                    break;
                }
                else
                { // next is regular edge.
                    this.Add(_path[offset + 1], _path[offset + 0], _path[offset + 2]);
                    offset++; // move to next vertex.
                }
            }
        }

        /// <summary>
        /// Gets the route.
        /// </summary>
        public Route Route
        {
            get
            {
                return _route;
            }
        }

        /// <summary>
        /// Adds all segments between source and target when they are on the same edge.
        /// </summary>
        private void AddSourceTarget()
        {
            if (_source.EdgeId != _target.EdgeId)
            { // a route of one vertex but source and target do not match.
                this.ErrorMessage = "Target and source have to be on the same vertex with a route with only virtual vertices.";
                return;
            }

            // get edge details.
            var edge = _routerDb.Network.GetEdge(_source.EdgeId);
            var profile = _routerDb.EdgeProfiles.Get(edge.Data.Profile);
            var speed = _vehicleProfile.Speed(profile);
            var meta = _routerDb.EdgeMeta.Get(edge.Data.MetaId);
            var tags = new TagsCollection(profile);
            if (meta != null) { tags.AddOrReplace(meta); }

            // build segments along the shape between the two virtual points.
            var segment = RouteSegment.CreateNew(_source.Location(), _vehicleProfile);
            segment.SetStop(_source.Location(), _source.Tags);
            _route.Segments.Add(segment);
            var shapePoints = _source.ShapePointsTo(_routerDb, _target);

            for (var i = 0; i < shapePoints.Count; i++)
            {
                segment = RouteSegment.CreateNew(shapePoints[i], _vehicleProfile);
                segment.Set(_route.Segments[_route.Segments.Count - 1], _vehicleProfile, tags, speed);
                _route.Segments.Add(segment);
            }

            // add the target.
            segment = RouteSegment.CreateNew(_target.Location(), _vehicleProfile);
            segment.Set(_route.Segments[_route.Segments.Count - 1], _vehicleProfile, tags, speed);
            segment.SetStop(_target.Location(), _target.Tags);
            _route.Segments.Add(segment);
        }

        /// <summary>
        /// Adds the segments from source to the given vertex.
        /// </summary>
        private void AddSource(uint vertex)
        {
            // add source.
            var segment = RouteSegment.CreateNew(_source.Location(), _vehicleProfile);
            segment.SetStop(_source.Location(), _source.Tags);
            _route.Segments.Add(segment);
        }

        /// <summary>
        /// Adds the segments from source to the given vertex.
        /// </summary>
        private void AddSource(uint vertex, uint previousVertex, uint nextVertex)
        {
            // add source.
            var segment = RouteSegment.CreateNew(_source.Location(), _vehicleProfile);
            segment.SetStop(_source.Location(), _source.Tags);
            _route.Segments.Add(segment);

            // expand source->first core vertex.
            var edge = _routerDb.Network.GetEdge(_source.EdgeId);
            var profile = _routerDb.EdgeProfiles.Get(edge.Data.Profile);
            var speed = _vehicleProfile.Speed(profile);
            var meta = _routerDb.EdgeMeta.Get(edge.Data.MetaId);
            var tags = new TagsCollection(profile);
            if (meta != null) { tags.AddOrReplace(meta); }

            // shapepoints.
            var shapePoints = _source.ShapePointsTo(_routerDb, vertex);
            for (var i = 0; i < shapePoints.Count; i++)
            {
                segment = RouteSegment.CreateNew(shapePoints[i], _vehicleProfile);
                segment.Set(_route.Segments[_route.Segments.Count - 1], _vehicleProfile, tags, speed);
                _route.Segments.Add(segment);
            }
            segment = RouteSegment.CreateNew(_routerDb.Network.GetVertex(vertex), _vehicleProfile);
            segment.Set(_route.Segments[_route.Segments.Count - 1], _vehicleProfile, tags, speed);
            segment.SetSideStreets(_routerDb, vertex, _source.EdgeId, nextVertex);
            _route.Segments.Add(segment);
        }

        /// <summary>
        /// Adds the segments from the previous vertex to the given vertex.
        /// </summary>
        private void Add(uint vertex, uint previousVertex, uint nextVertex)
        {
            RouteSegment segment;

            // get edge.
            var edge = _routerDb.Network.GetEdgeEnumerator(previousVertex).First(x => x.To == vertex);
            var profile = _routerDb.EdgeProfiles.Get(edge.Data.Profile);
            var speed = _vehicleProfile.Speed(profile);
            var meta = _routerDb.EdgeMeta.Get(edge.Data.MetaId);
            var tags = new TagsCollection(profile);
            if (meta != null) { tags.AddOrReplace(meta); }

            // expand shape between previous and vertex.
            var shape = edge.Shape;
            if (shape != null)
            {
                if (edge.DataInverted)
                { // invert if data is inverted.
                    shape = shape.Reverse();
                }
                var shapeEnumerator = shape.GetEnumerator();
                shapeEnumerator.Reset();
                while (shapeEnumerator.MoveNext())
                { // create the segment and set details.
                    segment = RouteSegment.CreateNew(shapeEnumerator.Current, _vehicleProfile);
                    segment.Set(_route.Segments[_route.Segments.Count - 1], _vehicleProfile, tags, speed);
                    _route.Segments.Add(segment);
                }
            }

            // add the actual vertex.
            segment = RouteSegment.CreateNew(_routerDb.Network.GetVertex(vertex), _vehicleProfile);
            segment.Set(_route.Segments[_route.Segments.Count - 1], _vehicleProfile, tags, speed);
            segment.SetSideStreets(_routerDb, vertex, _source.EdgeId, nextVertex);
            _route.Segments.Add(segment);
        }

        /// <summary>
        /// Adds the segments from the vertex to the target.
        /// </summary>
        private void AddTarget(uint vertex)
        {
            RouteSegment segment;

            // get edge.
            var edge = _routerDb.Network.GetEdge(_target.EdgeId);
            var profile = _routerDb.EdgeProfiles.Get(edge.Data.Profile);
            var speed = _vehicleProfile.Speed(profile);
            var meta = _routerDb.EdgeMeta.Get(edge.Data.MetaId);
            var tags = new TagsCollection(profile);
            if (meta != null) { tags.AddOrReplace(meta); }

            // shapepoints.
            var shapePoints = _target.ShapePointsTo(_routerDb, vertex);
            shapePoints.Reverse();
            for (var i = 0; i < shapePoints.Count; i++)
            {
                segment = RouteSegment.CreateNew(shapePoints[i], _vehicleProfile);
                segment.Set(_route.Segments[_route.Segments.Count - 1], _vehicleProfile, tags, speed);
                _route.Segments.Add(segment);
            }

            // add the actual target.
            segment = RouteSegment.CreateNew(_target.Location(), _vehicleProfile);
            segment.Set(_route.Segments[_route.Segments.Count - 1], _vehicleProfile, tags, speed);
            segment.SetStop(_target.Location(), _target.Tags);
            _route.Segments.Add(segment);
        }

        /// <summary>
        /// Adds the segments from the previous vertex to the target when the target is a vertex.
        /// </summary>
        private void AddTarget(uint vertex, uint previousVertex)
        {
            RouteSegment segment;

            // get edge.
            var edge = _routerDb.Network.GetEdgeEnumerator(previousVertex).First(x => x.To == vertex);
            var profile = _routerDb.EdgeProfiles.Get(edge.Data.Profile);
            var speed = _vehicleProfile.Speed(profile);
            var meta = _routerDb.EdgeMeta.Get(edge.Data.MetaId);
            var tags = new TagsCollection(profile);
            if (meta != null) { tags.AddOrReplace(meta); }

            // expand shape between previous and vertex.
            var shape = edge.Shape;
            if (shape != null)
            {
                if (edge.DataInverted)
                { // invert if data is inverted.
                    shape = shape.Reverse();
                }
                var shapeEnumerator = shape.GetEnumerator();
                shapeEnumerator.Reset();
                while (shapeEnumerator.MoveNext())
                { // create the segment and set details.
                    segment = RouteSegment.CreateNew(shapeEnumerator.Current, _vehicleProfile);
                    segment.Set(_route.Segments[_route.Segments.Count - 1], _vehicleProfile, tags, speed);
                    _route.Segments.Add(segment);
                }
            }

            // add the actual vertex.
            segment = RouteSegment.CreateNew(_routerDb.Network.GetVertex(vertex), _vehicleProfile);
            segment.Set(_route.Segments[_route.Segments.Count - 1], _vehicleProfile, tags, speed);
            _route.Segments.Add(segment);
        }
        
        /// <summary>
        /// Builds a route.
        /// </summary>
        public static Route Build(RouterDb db, Profile profile, RouterPoint source, RouterPoint target, Path path)
        {
            return RouteBuilder.TryBuild(db, profile, source, target, path).Value;
        }

        /// <summary>
        /// Builds a route.
        /// </summary>
        public static Result<Route> TryBuild(RouterDb db, Profile profile, RouterPoint source, RouterPoint target, Path path)
        {
            var pathList = new List<uint>();
            path.AddToList(pathList);
            return RouteBuilder.TryBuild(db, profile, source, target, pathList);
        }

        /// <summary>
        /// Builds a route.
        /// </summary>
        public static Route Build(RouterDb db, Profile profile, RouterPoint source, RouterPoint target, List<uint> path)
        {
            return RouteBuilder.TryBuild(db, profile, source, target, path).Value;
        }

        /// <summary>
        /// Builds a route.
        /// </summary>
        public static Result<Route> TryBuild(RouterDb db, Profile profile, RouterPoint source, RouterPoint target, List<uint> path)
        {
            var routeBuilder = new RouteBuilder(db, profile, source, target, path);
            routeBuilder.Run();
            if(!routeBuilder.HasSucceeded)
            {
                return new Result<Route>(
                    string.Format("Failed to build route: {0}", routeBuilder.ErrorMessage));
            }
            return new Result<Route>(routeBuilder.Route);
        }
    }
}