/*
 *  Licensed to SharpSoftware under one or more contributor
 *  license agreements. See the NOTICE file distributed with this work for 
 *  additional information regarding copyright ownership.
 * 
 *  SharpSoftware licenses this file to you under the Apache License, 
 *  Version 2.0 (the "License"); you may not use this file except in 
 *  compliance with the License. You may obtain a copy of the License at
 * 
 *       http://www.apache.org/licenses/LICENSE-2.0
 * 
 *  Unless required by applicable law or agreed to in writing, software
 *  distributed under the License is distributed on an "AS IS" BASIS,
 *  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *  See the License for the specific language governing permissions and
 *  limitations under the License.
 */

using Itinero.Attributes;
using Itinero.LocalGeo;
using Itinero.Data.Network;
using Itinero.Profiles;
using System.Collections.Generic;
using System.Linq;
using Itinero.Algorithms.Shortcuts;
using Itinero.Data.Shortcuts;
using System.Threading;

namespace Itinero.Algorithms.Routes
{
    /// <summary>
    /// An algorithm to build a route from a path.
    /// </summary>
    public class CompleteRouteBuilder : AlgorithmBase
    {
        private readonly RouterDb _routerDb;
        private readonly List<uint> _path;
        private readonly Profile _profile;
        private readonly RouterPoint _source;
        private readonly RouterPoint _target;

        /// <summary>
        /// Creates a router builder.
        /// </summary>
        public CompleteRouteBuilder(RouterDb routerDb, Profile profile,
            RouterPoint source, RouterPoint target, List<uint> path)
        {
            _routerDb = routerDb;
            _path = path;
            _source = source;
            _target = target;
            _profile = profile;
        }

        private Route _route;
        private List<Coordinate> _shape;
        private List<Route.Meta> _shapeMeta;
        private List<Route.Branch> _branches;

        /// <summary>
        /// Executes the actual run of the algorithm.
        /// </summary>
        protected override void DoRun(CancellationToken cancellationToken)
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
            _branches = new List<Route.Branch>();

            // add source.
            lock (_routerDb)
            {
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
                        this.Add(_path[i], _path[i + 1], _path[i + 2], cancellationToken);
                    }
                    this.Add(_path[i], _path[i + 1]);
                    this.HasSucceeded = true;
                }
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
            stops[0].Distance = 0;
            stops[0].Time = 0;
            stops[1].Distance = _shapeMeta.Last().Distance;
            stops[1].Time = _shapeMeta.Last().Time;

            // build route.
            _route = new Route()
            {
                Shape = _shape.ToArray(),
                ShapeMeta = _shapeMeta.ToArray(),
                Stops = stops,
				Branches = _branches.ToArray(),
                TotalDistance = _shapeMeta.Last().Distance,
                TotalTime = _shapeMeta.Last().Time,
                Profile = _profile.FullName
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
            _shape.Add(_source.LocationOnNetwork(_routerDb));
            _shapeMeta.Add(new Route.Meta()
            {
                Attributes = new AttributeCollection(
                    new Attributes.Attribute("profile", _profile.FullName)),
                AttributesDirection = true,
                Shape = _shape.Count - 1
            });
        }

        /// <summary>
        /// Adds the shape point between from and to and the target location itself.
        /// </summary>
        private void Add(uint from, uint to)
        { 
            if(from == Constants.NO_VERTEX &&
                _source.IsVertex())
            { // replace from with the vertex.
                from = _source.VertexId(_routerDb);
            }
            if (to == Constants.NO_VERTEX &&
                _target.IsVertex())
            { // replace to with the vertex.
                to = _target.VertexId(_routerDb);
            }

            if (to == from && to != Constants.NO_VERTEX)
            { // nothing to be done.
                return;
            }

            // get shapepoints and edge.
            var shape = new List<Coordinate>(0);
            RoutingEdge edge = null;
            Coordinate? targetLocation = null;
            var distance = 0f;
            var direction = true;
            if(from == Constants.NO_VERTEX &&
               to == Constants.NO_VERTEX)
            { // from is the source and to is the target.
                if (_source.EdgeId != _target.EdgeId)
                { // a route inside one edge but source and target do not match.
                    this.ErrorMessage = "Target and source have to be on the same vertex with a route with only virtual vertices.";
                    return;
                }
                shape = _source.ShapePointsTo(_routerDb, _target);
                distance = _source.DistanceTo(_routerDb, _target);
                edge = _routerDb.Network.GetEdge(_source.EdgeId);
                targetLocation = _target.LocationOnNetwork(_routerDb);
                shape.Add(targetLocation.Value);
            }
            else if(from == Constants.NO_VERTEX)
            { // from is the source and to is a regular vertex.
                edge = _routerDb.Network.GetEdge(_source.EdgeId);
                var toOnEdge = _routerDb.Network.CreateRouterPointForVertex(to,
                    edge.GetOther(to));
                shape = _source.ShapePointsTo(_routerDb, toOnEdge);
                distance = _source.DistanceTo(_routerDb, toOnEdge);
                targetLocation = _routerDb.Network.GetVertex(to);
                shape.Add(targetLocation.Value);
            }
            else if(to == Constants.NO_VERTEX)
            { // from is a regular vertex and to is the target.
                edge = _routerDb.Network.GetEdge(_target.EdgeId);
                var fromOnEdge = _routerDb.Network.CreateRouterPointForVertex(from,
                    edge.GetOther(from));
                shape = fromOnEdge.ShapePointsTo(_routerDb, _target);
                distance = fromOnEdge.DistanceTo(_routerDb, _target);
                targetLocation = _target.LocationOnNetwork(_routerDb);
                shape.Add(targetLocation.Value);
            }
            else
            { // both are just regular vertices.
                edge = _routerDb.Network.GetEdgeEnumerator(from).First(x => x.To == to);
                direction = !edge.DataInverted;
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

            // get edge details.
            var profile = _routerDb.EdgeProfiles.Get(edge.Data.Profile);
            var speed = this._profile.Speed(profile);
            var time = 0f;
            if (speed.Value != 0)
            {
                time = distance / speed.Value;
            }
            var meta = _routerDb.EdgeMeta.Get(edge.Data.MetaId);
            var attributes = new AttributeCollection(meta);
            attributes.AddOrReplace(profile);
            attributes.AddOrReplace("profile", _profile.FullName);
            
            // add shape and meta.
            _shape.AddRange(shape);
            var previousMeta = _shapeMeta[_shapeMeta.Count - 1];
            var shapeMeta = new Route.Meta()
            {
                Shape = _shape.Count - 1,
                Attributes = attributes,
                AttributesDirection = direction
            };
            shapeMeta.Distance = distance + previousMeta.Distance;
            shapeMeta.Time = time + previousMeta.Time;
            _shapeMeta.Add(shapeMeta);
        }

        /// <summary>
        /// Adds the shape point between from and to and the target location itself.
        /// </summary>
        private void Add(uint from, uint to, uint next, CancellationToken cancellationToken)
        {
            if (from == Constants.NO_VERTEX &&
                _source.IsVertex())
            { // replace from with the vertex.
                from = _source.VertexId(_routerDb);

                if (from == to)
                { // nothing to be done.
                    return;
                }
            }
            if (next == Constants.NO_VERTEX &&
                _target.IsVertex())
            { // replace next with the vertex.
                next = _target.VertexId(_routerDb);

                //if (to == next)
                //{ // nothing to be done.
                //    return;
                //}
            }


            // get shapepoints and edge.
            var shape = new List<Coordinate>(0);
            RoutingEdge edge = null;
            Coordinate? targetLocation = null;
            var distance = 0f;
            var direction = true;
            if (from == Constants.NO_VERTEX &&
               to == Constants.NO_VERTEX)
            { // from is the source and to is the target.
                if (_source.EdgeId != _target.EdgeId)
                { // a route inside one edge but source and target do not match.
                    this.ErrorMessage = "Target and source have to be on the same vertex with a route with only virtual vertices.";
                    return;
                }
                shape = _source.ShapePointsTo(_routerDb, _target);
                distance = _source.DistanceTo(_routerDb, _target);
                edge = _routerDb.Network.GetEdge(_source.EdgeId);
                targetLocation = _target.LocationOnNetwork(_routerDb);
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
                targetLocation = _target.LocationOnNetwork(_routerDb);
                shape.Add(targetLocation.Value);
            }
            else
            { // both are just regular vertices.
                edge = _routerDb.Network.GetEdgeEnumerator(from).First(x => x.To == to);
                direction = !edge.DataInverted;
                if (this.AddShorcut(edge, cancellationToken))
                {
                    return;
                }

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

            // get edge details.
            var profile = _routerDb.EdgeProfiles.Get(edge.Data.Profile);
            var speed = this._profile.Speed(profile);
            var time = 0f;
            if (speed.Value != 0)
            {
                time = distance / speed.Value;
            }
            var meta = _routerDb.EdgeMeta.Get(edge.Data.MetaId);
            var attributes = new AttributeCollection(meta);
            attributes.AddOrReplace(profile);
            attributes.AddOrReplace("profile", _profile.FullName);

            // add shape and meta.
            _shape.AddRange(shape);
            var previousMeta = _shapeMeta[_shapeMeta.Count - 1];
            var shapeMeta = new Route.Meta()
            {
                Shape = _shape.Count - 1,
                Attributes = attributes,
                AttributesDirection = direction
            };
            shapeMeta.Distance = distance + previousMeta.Distance;
            shapeMeta.Time = time + previousMeta.Time;
            _shapeMeta.Add(shapeMeta);

            // add sidestreets.
            if (to != Constants.NO_VERTEX)
            {
                _branches.AddBranches(_routerDb, _shape.Count - 1, to, edge.Id, next);
            }
        }

        /// <summary>
        /// Add shortcut if the current edge is one.
        /// </summary>
        private bool AddShorcut(RoutingEdge edge, CancellationToken cancellationToken)
        {
            // when both are just regular vertices, this could be a shortcut.
            var edgeProfile = _routerDb.EdgeProfiles.Get(edge.Data.Profile);
            var shortcutName = string.Empty;
            if (edgeProfile.IsShortcut(out shortcutName))
            { // ok profile is a shortcut, get the actual path, expand and build route.
                ShortcutsDb shortcutsDb;
                if (_routerDb.TryGetShortcuts(shortcutName, out shortcutsDb))
                {
                    var shortcutProfile = shortcutsDb.Profile;
                    IAttributeCollection shortcutMeta;
                    var shortcut = shortcutsDb.Get(edge.From, edge.To, out shortcutMeta);
                    if (shortcut != null && shortcut.Length >= 2)
                    {
                        var route = CompleteRouteBuilder.Build(_routerDb, shortcutProfile,
                            _routerDb.CreateRouterPointForVertex(shortcut[0], shortcutProfile, _profile),
                            _routerDb.CreateRouterPointForVertex(shortcut[shortcut.Length - 1], shortcutProfile, _profile),
                                new List<uint>(shortcut), cancellationToken);
                        if (shortcutMeta == null)
                        {
                            shortcutMeta = new AttributeCollection();
                        }
                        shortcutMeta.AddOrReplace(ShortcutExtensions.SHORTCUT_KEY, shortcutName);

                        this.AppendRoute(route, shortcutMeta);
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Appends a route to the existing data.
        /// </summary>
        private void AppendRoute(Route route, IAttributeCollection shortcutMeta)
        {
            var previousMeta = _shapeMeta[_shapeMeta.Count - 1];
            var shapeOffset = _shape.Count;
            for(var i = 0; i < route.Shape.Length; i++)
            {
                _shape.Add(route.Shape[i]);
            }
            for(var i = 0; i < route.ShapeMeta.Length;i++)
            {
                var meta = route.ShapeMeta[i];
                var attributes = new AttributeCollection(meta.Attributes);
                attributes.AddOrReplace(shortcutMeta);
                _shapeMeta.Add(new Route.Meta()
                {
                    Attributes = attributes,
                    Distance = meta.Distance + previousMeta.Distance,
                    Profile = meta.Profile,
                    Shape = shapeOffset + meta.Shape,
                    Time = meta.Time + previousMeta.Time
                });
            }
        }

        /// <summary>
        /// Builds a route.
        /// </summary>
        public static Route Build(RouterDb db, Profile profile, RouterPoint source, RouterPoint target, EdgePath<float> path)
        {
            return CompleteRouteBuilder.Build(db, profile, source, target, path, CancellationToken.None);
        }

        /// <summary>
        /// Builds a route.
        /// </summary>
        public static Route Build(RouterDb db, Profile profile, RouterPoint source, RouterPoint target, EdgePath<float> path, CancellationToken cancellationToken)
        {
            return CompleteRouteBuilder.TryBuild(db, profile, source, target, path, cancellationToken).Value;
        }

        /// <summary>
        /// Builds a route.
        /// </summary>
        public static Result<Route> TryBuild<T>(RouterDb db, Profile profile, RouterPoint source, RouterPoint target, EdgePath<T> path)
        {
            return CompleteRouteBuilder.TryBuild(db, profile, source, target, path, CancellationToken.None);
        }

        /// <summary>
        /// Builds a route.
        /// </summary>
        public static Result<Route> TryBuild<T>(RouterDb db, Profile profile, RouterPoint source, RouterPoint target, EdgePath<T> path, CancellationToken cancellationToken)
        {
            var pathList = new List<uint>();
            path.AddToListAsVertices(pathList);
            return CompleteRouteBuilder.TryBuild(db, profile, source, target, pathList, cancellationToken);
        }

        /// <summary>
        /// Builds a route.
        /// </summary>
        public static Route Build(RouterDb db, Profile profile, RouterPoint source, RouterPoint target, List<uint> path)
        {
            return CompleteRouteBuilder.Build(db, profile, source, target, path, CancellationToken.None);
        }

        /// <summary>
        /// Builds a route.
        /// </summary>
        public static Route Build(RouterDb db, Profile profile, RouterPoint source, RouterPoint target, List<uint> path, CancellationToken cancellationToken)
        {
            return CompleteRouteBuilder.TryBuild(db, profile, source, target, path, cancellationToken).Value;
        }

        /// <summary>
        /// Builds a route.
        /// </summary>
        public static Result<Route> TryBuild(RouterDb db, Profile profile, RouterPoint source, RouterPoint target, List<uint> path)
        {
            return CompleteRouteBuilder.TryBuild(db, profile, source, target, path, CancellationToken.None);
        }

        /// <summary>
        /// Builds a route.
        /// </summary>
        public static Result<Route> TryBuild(RouterDb db, Profile profile, RouterPoint source, RouterPoint target, List<uint> path, CancellationToken cancellationToken)
        {
            var routeBuilder = new CompleteRouteBuilder(db, profile, source, target, path);
            routeBuilder.Run(cancellationToken);
            if(!routeBuilder.HasSucceeded)
            {
                return new Result<Route>(
                    string.Format("Failed to build route: {0}", routeBuilder.ErrorMessage));
            }
            return new Result<Route>(routeBuilder.Route);
        }
    }
}