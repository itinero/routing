// Itinero - OpenStreetMap (OSM) SDK
// Copyright (C) 2016 Abelshausen Ben
// 
// This file is part of Itinero.
// 
// Itinero is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// Itinero is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Itinero. If not, see <http://www.gnu.org/licenses/>.

using Itinero.Attributes;
using Itinero.LocalGeo;
using Itinero.Data.Network;
using Itinero.Profiles;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Itinero.Algorithms.Routes
{
    /// <summary>
    /// An algorithm to build a route from a path.
    /// </summary>
    public class FastRouteBuilder : AlgorithmBase
    {
        private readonly RouterDb _routerDb;
        private readonly List<uint> _path;
        private readonly Profile _profile;
        private readonly Func<ushort, Factor> _getFactor;
        private readonly RouterPoint _source;
        private readonly RouterPoint _target;

        /// <summary>
        /// Creates a router builder.
        /// </summary>
        public FastRouteBuilder(RouterDb routerDb, Profile profile, Func<ushort, Factor> getFactor, RouterPoint source, RouterPoint target, 
            List<uint> path)
        {
            _routerDb = routerDb;
            _path = path;
            _source = source;
            _target = target;
            _profile = profile;
            _getFactor = getFactor;
        }

        private Route _route;
        private List<Coordinate> _shape;
        private List<Route.Meta> _shapeMeta;

        /// <summary>
        /// Executes the actual run of the algorithm.
        /// </summary>
        protected override void DoRun()
        {
            if (_path.Count == 0)
            { // an empty path.
                this.ErrorMessage = "Path was empty.";
                this.HasSucceeded = false;
                return;
            }

            // check source.
            var source = _path[0];
            if (source != Constants.NO_VERTEX &&
                !_source.IsVertex(_routerDb, source))
            {
                this.ErrorMessage = "The source is a vertex but the source is not a match.";
                this.HasSucceeded = false;
                return;
            }

            // check target.
            var target = _path[_path.Count - 1];
            if (target != Constants.NO_VERTEX &&
                !_target.IsVertex(_routerDb, target))
            {
                this.ErrorMessage = "The target is a vertex but the target is not a match.";
                this.HasSucceeded = false;
                return;
            }

            // build the route.
            _shape = new List<Coordinate>();
            _shapeMeta = new List<Route.Meta>();

            // add source.
            this.AddSource();

            if (_path.Count == 1)
            { // there is only the source/target location.
                this.HasSucceeded = true;
            }
            else
            { // there are at least two points.
                var i = 0;
                for (i = 0; i < _path.Count - 2; i++)
                {
                    this.Add(_path[i], _path[i + 1], _path[i + 2]);
                }
                this.Add(_path[i], _path[i + 1]);
                this.HasSucceeded = true;
            }

            // set stops.
            var stops = new Route.Stop[]
            {
                new Route.Stop()
                {
                    Shape = 0,
                    Attributes = new AttributeCollection(_source.Attributes),
                    Coordinate = _source.Location()
                },
                new Route.Stop()
                {
                    Shape = _shape.Count - 1,
                    Attributes = new AttributeCollection(_target.Attributes),
                    Coordinate = _target.Location()
                }
            };

            // build route.
            _route = new Route()
            {
                Shape = _shape.ToArray(),
                ShapeMeta = _shapeMeta.ToArray(),
                Stops = stops,
                TotalDistance = _shapeMeta.Last().Distance,
                TotalTime = _shapeMeta.Last().Time
            };
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
        /// Adds the source.
        /// </summary>
        private void AddSource()
        {
            _shape.Add(_source.Location());
            _shapeMeta.Add(new Route.Meta()
            {
                Attributes = new AttributeCollection(
                    new Attributes.Attribute("profile", _profile.Name)),
                Shape = _shape.Count - 1
            });
        }

        /// <summary>
        /// Adds the shape point between from and to and the target location itself.
        /// </summary>
        private void Add(uint from, uint to)
        {
            if (from == Constants.NO_VERTEX &&
                _source.IsVertex())
            { // replace from with the vertex.
                from = _source.VertexId(_routerDb);
            }
            if (to == Constants.NO_VERTEX &&
                _target.IsVertex())
            { // replace to with the vertex.
                to = _target.VertexId(_routerDb);
            }

            // get shapepoints and edge.
            var shape = new List<Coordinate>(0);
            RoutingEdge edge = null;
            Coordinate? targetLocation = null;
            var distance = 0f;
            if (from == Constants.NO_VERTEX &&
               to == Constants.NO_VERTEX)
            { // from is the source and to is the target.
                if (_source.EdgeId != _target.EdgeId)
                { // a route inside one edge but source and target do not match.
                    this.ErrorMessage = "Target and source have to be on the same vertex with a route with only virtual vertices.";
                    return;
                }
                shape = _source.ShapePointsTo(_routerDb, _target);
                edge = _routerDb.Network.GetEdge(_source.EdgeId);
                distance = _source.DistanceTo(_routerDb, _target);
                targetLocation = _target.Location();
                shape.Add(targetLocation.Value);
            }
            else if (from == Constants.NO_VERTEX)
            { // from is the source and to is a regular vertex.
                edge = _routerDb.Network.GetEdge(_source.EdgeId);
                var toOnEdge = _routerDb.Network.CreateRouterPointForVertex(to,
                    edge.GetOther(to));
                shape = _source.ShapePointsTo(_routerDb, toOnEdge);
                distance = _source.DistanceTo(_routerDb, toOnEdge);
                targetLocation = _routerDb.Network.GetVertex(to);
                shape.Add(targetLocation.Value);
            }
            else if (to == Constants.NO_VERTEX)
            { // from is a regular vertex and to is the target.
                edge = _routerDb.Network.GetEdge(_target.EdgeId);
                var fromOnEdge = _routerDb.Network.CreateRouterPointForVertex(from, 
                    edge.GetOther(from));
                shape = fromOnEdge.ShapePointsTo(_routerDb, _target);
                distance = fromOnEdge.DistanceTo(_routerDb, _target);
                targetLocation = _target.Location();
                shape.Add(targetLocation.Value);
            }
            else
            { // both are just regular vertices.
                edge = _routerDb.Network.GetEdgeEnumerator(from).First(x => x.To == to);
                distance = edge.Data.Distance;
                var shapeEnumerable = edge.Shape;
                if (shapeEnumerable != null)
                {
                    if (edge.DataInverted)
                    {
                        shapeEnumerable = shapeEnumerable.Reverse();
                    }
                    shape.AddRange(shapeEnumerable);
                }
                targetLocation = _routerDb.Network.GetVertex(to);
                shape.Add(targetLocation.Value);
            }

            // get speed.
            var speed = this.GetSpeedFor(edge.Data.Profile);
            var time = 0f;
            if (speed.Value != 0)
            {
                time = distance / speed.Value;
            }

            // build attributes.
            var attributes = new AttributeCollection(
                new Attributes.Attribute("profile", _profile.Name));

            // add shape and meta.
            _shape.AddRange(shape);
            var previousMeta = _shapeMeta[_shapeMeta.Count - 1];
            var shapeMeta = new Route.Meta()
            {
                Shape = _shape.Count - 1,
                Attributes = attributes
            };
            shapeMeta.Distance = distance + previousMeta.Distance;
            shapeMeta.Time = time + previousMeta.Time;
            _shapeMeta.Add(shapeMeta);
        }

        /// <summary>
        /// Adds the shape point between from and to and the target location itself.
        /// </summary>
        private void Add(uint from, uint to, uint next)
        {
            if (from == Constants.NO_VERTEX &&
                _source.IsVertex())
            { // replace from with the vertex.
                from = _source.VertexId(_routerDb);
            }
            if (next == Constants.NO_VERTEX &&
                _target.IsVertex())
            { // replace next with the vertex.
                next = _target.VertexId(_routerDb);
            }

            // get shapepoints and edge.
            var shape = new List<Coordinate>(0);
            RoutingEdge edge = null;
            Coordinate? targetLocation = null;
            var distance = 0f;
            if (from == Constants.NO_VERTEX &&
               to == Constants.NO_VERTEX)
            { // from is the source and to is the target.
                if (_source.EdgeId != _target.EdgeId)
                { // a route inside one edge but source and target do not match.
                    this.ErrorMessage = "Target and source have to be on the same vertex with a route with only virtual vertices.";
                    return;
                }
                shape = _source.ShapePointsTo(_routerDb, _target);
                edge = _routerDb.Network.GetEdge(_source.EdgeId);
                distance = _source.DistanceTo(_routerDb, _target);
                targetLocation = _target.Location();
                shape.Add(targetLocation.Value);
            }
            else if (from == Constants.NO_VERTEX)
            { // from is the source and to is a regular vertex.
                edge = _routerDb.Network.GetEdge(_source.EdgeId);
                var toOnEdge = _routerDb.Network.CreateRouterPointForVertex(to,
                    edge.GetOther(to));
                shape = _source.ShapePointsTo(_routerDb, toOnEdge);
                distance = _source.DistanceTo(_routerDb, toOnEdge);
                targetLocation = _routerDb.Network.GetVertex(to);
                shape.Add(targetLocation.Value);
            }
            else if (to == Constants.NO_VERTEX)
            { // from is a regular vertex and to is the target.
                edge = _routerDb.Network.GetEdge(_target.EdgeId);
                var fromOnEdge = _routerDb.Network.CreateRouterPointForVertex(from,
                    edge.GetOther(from));
                shape = fromOnEdge.ShapePointsTo(_routerDb, _target);
                distance = fromOnEdge.DistanceTo(_routerDb, _target);
                targetLocation = _target.Location();
                shape.Add(targetLocation.Value);
            }
            else
            { // both are just regular vertices.
                edge = _routerDb.Network.GetEdgeEnumerator(from).First(x => x.To == to);
                distance = edge.Data.Distance;
                var shapeEnumerable = edge.Shape;
                if (shapeEnumerable != null)
                {
                    if (edge.DataInverted)
                    {
                        shapeEnumerable = shapeEnumerable.Reverse();
                    }
                    shape.AddRange(shapeEnumerable);
                }
                targetLocation = _routerDb.Network.GetVertex(to);
                shape.Add(targetLocation.Value);
            }

            // get speed.
            var speed = this.GetSpeedFor(edge.Data.Profile);
            var time = 0f;
            if (speed.Value != 0)
            {
                time = distance / speed.Value;
            }

            // build attributes.
            var attributes = new AttributeCollection(
                new Attributes.Attribute("profile", _profile.Name));

            // add shape and meta.
            _shape.AddRange(shape);
            var previousMeta = _shapeMeta[_shapeMeta.Count - 1];
            var shapeMeta = new Route.Meta()
            {
                Shape = _shape.Count - 1,
                Attributes = attributes
            };
            shapeMeta.Distance = distance + previousMeta.Distance;
            shapeMeta.Time = time + previousMeta.Time;
            _shapeMeta.Add(shapeMeta);
        }

        /// <summary>
        /// Gets the speed for the given profile.
        /// </summary>
        private Speed GetSpeedFor(ushort profileId)
        {
            var speed = new Speed()
            {
                Direction = 0,
                Value = 1.0f
            };
            if (_profile.Metric == ProfileMetric.TimeInSeconds)
            { // in this case factor is 1/speed so just reuse.
                var factor = _getFactor(profileId).Value;
                if (factor != 0)
                {
                    speed = new Speed()
                    {
                        Direction = 0,
                        Value = 1.0f / factor
                    };
                }
            }
            else
            { // here we need to use the slower option of getting the speed from the profile.
                // factor has nothing to do with the actual speed.
                var edgeProfile = _routerDb.EdgeProfiles.Get(profileId);
                speed = _profile.Speed(edgeProfile);
            }
            return speed;
        }

        /// <summary>
        /// Builds a route.
        /// </summary>
        public static Route Build(RouterDb db, Profile profile, Func<ushort, Profiles.Factor> getFactor, RouterPoint source, RouterPoint target, Path path)
        {
            return FastRouteBuilder.TryBuild(db, profile, getFactor, source, target, path).Value;
        }

        /// <summary>
        /// Builds a route.
        /// </summary>
        public static Result<Route> TryBuild(RouterDb db, Profile profile, Func<ushort, Profiles.Factor> getFactor, RouterPoint source, RouterPoint target, Path path)
        {
            var pathList = new List<uint>();
            path.AddToList(pathList);
            return FastRouteBuilder.TryBuild(db, profile, getFactor, source, target, pathList);
        }

        /// <summary>
        /// Builds a route.
        /// </summary>
        public static Route Build(RouterDb db, Profile profile, Func<ushort, Profiles.Factor> getFactor, RouterPoint source, RouterPoint target, List<uint> path)
        {
            return FastRouteBuilder.TryBuild(db, profile, getFactor, source, target, path).Value;
        }

        /// <summary>
        /// Builds a route.
        /// </summary>
        public static Result<Route> TryBuild(RouterDb db, Profile profile, Func<ushort, Profiles.Factor> getFactor, 
            RouterPoint source, RouterPoint target, List<uint> path)
        {
            var routeBuilder = new FastRouteBuilder(db, profile, getFactor, source, target, path);
            routeBuilder.Run();
            if (!routeBuilder.HasSucceeded)
            {
                return new Result<Route>(
                    string.Format("Failed to build route: {0}", routeBuilder.ErrorMessage));
            }
            return new Result<Route>(routeBuilder.Route);
        }
    }
}