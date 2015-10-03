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
        private readonly Path _path;
        private readonly Profile _profile;
        private readonly RouterPoint _source;
        private readonly RouterPoint _target;

        /// <summary>
        /// Creates a router builder.
        /// </summary>
        public RouteBuilder(RouterDb routerDb, Profile profile, RouterPoint source, RouterPoint target, Path path)
        {
            _routerDb = routerDb;
            _path = path;
            _source = source;
            _target = target;
            _profile = profile;
        }

        private Route _route;

        /// <summary>
        /// Executes the actual run of the algorithm.
        /// </summary>
        protected override void DoRun()
        {
            // reverse the original path.
            var path = _path.Reverse();

            // build the route.
            _route = new Route();
            _route.Segments = new List<RouteSegment>();
            if (path.From == null)
            { // only one vertex, route has length of zero.
                if (_source.EdgeId != _target.EdgeId ||
                   _source.Offset != _target.Offset)
                { // a route of one vertex but source and target do not match.
                    this.ErrorMessage = "Target and source have to be indentical with a route with only one vertex.";
                    return;
                }
                var segment = RouteSegment.CreateNew(_source.Location(), _profile);
                segment.SetStop(new ICoordinate[] { _source.Location(), _target.Location() },
                    new TagsCollectionBase[] { _source.Tags, _target.Tags });
                _route.Segments.Add(segment);
                this.HasSucceeded = true;
                return;
            }
            if (path.From.Vertex == Constants.NO_VERTEX ||
                _target.IsVertex(_routerDb, path.From.Vertex))
            { // this path represents a short path located within one edge or the edge max.
                this.AddSourceTarget();
                this.HasSucceeded = true;
                return;
            }

            // path is longer, at least one real vertex in there.
            // build the route by expanding the edges/shapes.

            // add the source.
            if (path.From.From.Vertex == Constants.NO_VERTEX ||
                _target.IsVertex(_routerDb, path.From.From.Vertex))
            { // the next one is the next one of the target.
                this.AddSource(path.From.Vertex, _routerDb.Network.GetEdge(_source.EdgeId).GetOther(path.From.Vertex),
                    _routerDb.Network.GetEdge(_target.EdgeId).GetOther(path.From.Vertex));
                this.AddTarget(path.From.Vertex);
                this.HasSucceeded = true;
                return;
            }

            // next one is a regular vertex.
            this.AddSource(path.From.Vertex, _routerDb.Network.GetEdge(_source.EdgeId).GetOther(path.From.Vertex),
                path.From.From.Vertex);

            path = path.From; // move to next vertex.

            // add intermediate points.
            while (path != null &&
                  path.From != null &&
                  path.From.From != null)
            {
                if(path.From.From.Vertex == Constants.NO_VERTEX ||
                    _target.IsVertex(_routerDb, path.From.From.Vertex))
                { // next is target.
                    this.Add(path.From.Vertex, path.Vertex, _routerDb.Network.GetEdge(_target.EdgeId).GetOther(path.From.Vertex));
                    break;
                }
                else
                { // next is regular edge.
                    this.Add(path.From.Vertex, path.Vertex, path.From.From.Vertex);
                    path = path.From; // move to next vertex.
                }
            }

            // add the target.
            this.AddTarget(path.From.Vertex);
            this.HasSucceeded = true;
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
            var profile = _routerDb.Profiles.Get(edge.Data.Profile);
            var speed = _profile.Speed(profile);
            var meta = _routerDb.Profiles.Get(edge.Data.MetaId);
            var tags = new TagsCollection(profile);
            tags.AddOrReplace(meta);

            // build segments along the shape between the two virtual points.
            var segment = RouteSegment.CreateNew(_source.Location(), _profile);
            segment.SetStop(_source.Location(), _source.Tags);
            _route.Segments.Add(segment);
            var shapePoints = _source.ShapePointsTo(_routerDb, _target);

            for (var i = 0; i < shapePoints.Count; i++)
            {
                segment = RouteSegment.CreateNew(shapePoints[i], _profile);
                segment.Set(_route.Segments[_route.Segments.Count - 1], _profile, tags, speed);
                _route.Segments.Add(segment);
            }

            // add the target.
            segment = RouteSegment.CreateNew(_target.Location(), _profile);
            segment.Set(_route.Segments[_route.Segments.Count - 1], _profile, tags, speed);
            segment.SetStop(_target.Location(), _target.Tags);
            _route.Segments.Add(segment);
        }

        /// <summary>
        /// Adds the segments from source to the given vertex.
        /// </summary>
        private void AddSource(uint vertex, uint previousVertex, uint nextVertex)
        {
            // add source.
            var segment = RouteSegment.CreateNew(_source.Location(), _profile);
            segment.SetStop(_source.Location(), _source.Tags);
            _route.Segments.Add(segment);

            // expand source->first core vertex.
            var edge = _routerDb.Network.GetEdge(_source.EdgeId);
            var profile = _routerDb.Profiles.Get(edge.Data.Profile);
            var speed = _profile.Speed(profile);
            var meta = _routerDb.Meta.Get(edge.Data.MetaId);
            var tags = new TagsCollection(profile);
            tags.AddOrReplace(meta);

            // shapepoints.
            var shapePoints = _source.ShapePointsTo(_routerDb, vertex);
            for (var i = 0; i < shapePoints.Count; i++)
            {
                segment = RouteSegment.CreateNew(shapePoints[i], _profile);
                segment.Set(_route.Segments[_route.Segments.Count - 1], _profile, tags, speed);
                _route.Segments.Add(segment);
            }
            segment = RouteSegment.CreateNew(_routerDb.Network.GetVertex(vertex), _profile);
            segment.Set(_route.Segments[_route.Segments.Count - 1], _profile, tags, speed);
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
            var profile = _routerDb.Profiles.Get(edge.Data.Profile);
            var speed = _profile.Speed(profile);
            var meta = _routerDb.Meta.Get(edge.Data.MetaId);
            var tags = new TagsCollection(profile);
            tags.AddOrReplace(meta);

            // expand shape between previous and vertex.
            var shape = edge.Shape;
            if (shape != null)
            {
                if (edge.DataInverted)
                { // invert if data is inverted.
                    shape = shape.Reverse();
                }
                shape.Reset();
                while (shape.MoveNext())
                { // create the segment and set details.
                    segment = RouteSegment.CreateNew(shape.Current, _profile);
                    segment.Set(_route.Segments[_route.Segments.Count - 1], _profile, tags, speed);
                    _route.Segments.Add(segment);
                }
            }

            // add the actual vertex.
            segment = RouteSegment.CreateNew(_routerDb.Network.GetVertex(vertex), _profile);
            segment.Set(_route.Segments[_route.Segments.Count - 1], _profile, tags, speed);
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
            var profile = _routerDb.Profiles.Get(edge.Data.Profile);
            var speed = _profile.Speed(profile);
            var meta = _routerDb.Meta.Get(edge.Data.MetaId);
            var tags = new TagsCollection(profile);
            tags.AddOrReplace(meta);

            // shapepoints.
            var shapePoints = _target.ShapePointsTo(_routerDb, vertex);
            shapePoints.Reverse();
            for (var i = 0; i < shapePoints.Count; i++)
            {
                segment = RouteSegment.CreateNew(shapePoints[i], _profile);
                segment.Set(_route.Segments[_route.Segments.Count - 1], _profile, tags, speed);
                _route.Segments.Add(segment);
            }

            // add the actual target.
            segment = RouteSegment.CreateNew(_target.Location(), _profile);
            segment.Set(_route.Segments[_route.Segments.Count - 1], _profile, tags, speed);
            segment.SetStop(_target.Location(), _target.Tags);
            _route.Segments.Add(segment);
        }
    }
}