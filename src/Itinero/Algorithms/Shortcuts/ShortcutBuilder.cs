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

 using Itinero.Algorithms.Weights;
using Itinero.Attributes;
using Itinero.Data.Shortcuts;
using Itinero.LocalGeo;
using Itinero.Logging;
using Itinero.Profiles;
using System.Collections.Generic;
using System.Threading;

namespace Itinero.Algorithms.Shortcuts
{
    /// <summary>
    /// A shortcut builder.
    /// </summary>
    public class ShortcutBuilder : AlgorithmBase
    {
        private readonly RouterDb _db;
        private readonly Profile _profile;
        private readonly Coordinate[] _locations;
        private readonly IAttributeCollection[] _locationsMeta;
        private readonly string _name;
        private readonly float _switchPentaly;
        private readonly float _minShortcutSize;
        private readonly float _maxShortcutDuration;

        /// <summary>
        /// Creates a new shortcut builder.
        /// </summary>
        public ShortcutBuilder(RouterDb routerDb, Profile profile, string name, Coordinate[] locations, IAttributeCollection[] locationsMeta, float switchPenalty,
            float minShortcutSize, float maxShortcutDuration)
        {
            _db = routerDb;
            _profile = profile;
            _locations = locations;
            _locationsMeta = locationsMeta;
            _name = name;
            _switchPentaly = switchPenalty;
            _minShortcutSize = minShortcutSize;
            _maxShortcutDuration = maxShortcutDuration;
        }

        private ShortcutsDb _shortcutsDb;
        private uint[][] _shortcutIds;
        
        /// <summary>
        /// Executes the actual run.
        /// </summary>
        protected override void DoRun(CancellationToken cancellationToken)
        {
            var router = new Router(_db);
            router.VerifyAllStoppable = true;

            // resolve locations as vertices.
            // WARNING: use RouterPointEmbedder first.
            var points = new RouterPoint[_locations.Length];
            for (var i = 0; i < points.Length; i++)
            {
                var resolver = new Algorithms.Search.ResolveVertexAlgorithm(_db.Network.GeometricGraph, _locations[i].Latitude, _locations[i].Longitude, 
                    500, 3000, router.GetIsAcceptable(_profile));
                resolver.Run(cancellationToken);
                if (!resolver.HasSucceeded)
                {
                    throw new System.Exception(string.Format("Could not resolve shortcut location at index {1}: {0}.", resolver.ErrorMessage, i));
                }
                points[i] = resolver.Result;
            }

            // use non-contracted calculation.
            var weightHandler = _profile.AugmentedWeightHandlerCached(_db);
            var algorithm = new Itinero.Algorithms.Default.ManyToMany<Weight>(_db, weightHandler, points, points, new Weight()
            {
                Distance = float.MaxValue,
                Time = _maxShortcutDuration,
                Value = _maxShortcutDuration
            });
            algorithm.Run(cancellationToken);
            if (!algorithm.HasSucceeded)
            {
                this.ErrorMessage = "Many to many calculation failed: " + algorithm.ErrorMessage;
            }

            // build shortcuts db.
            _shortcutsDb = new ShortcutsDb(_profile);
            for(var i = 0; i < points.Length; i++)
            {
                _shortcutsDb.AddStop(points[i].VertexId(_db), _locationsMeta[i]);
            }
            var routes = new EdgePath<float>[_locations.Length][];
            _shortcutIds = new uint[_locations.Length][];
            var pathList = new List<uint>();
            var shortcutProfile = _db.EdgeProfiles.Add(new AttributeCollection(
                new Attribute(ShortcutExtensions.SHORTCUT_KEY, _name)));
            for (var i = 0; i < _locations.Length; i++)
            {
                _shortcutIds[i] = new uint[_locations.Length];
                for (var j = 0; j < _locations.Length; j++)
                {
                    _shortcutIds[i][j] = uint.MaxValue;
                    if (i == j)
                    {
                        continue;
                    }
                    EdgePath<Weight> path;
                    if (algorithm.TryGetPath(i, j, out path))
                    {
                        pathList.Clear();
                        path.AddToListAsVertices(pathList);

                        if (path.Weight.Time < _minShortcutSize)
                        { // don't add very short shortcuts.
                            continue;
                        }

                        if(pathList.Count < 2)
                        {
                            Itinero.Logging.Logger.Log("ShortcutBuilder", TraceEventType.Warning, "Shortcut consists of only one vertex from {0}@{1} -> {2}@{3}!",
                                i, _locations[i], j, _locations[j]);
                            continue;
                        }
                        if (pathList[0] == pathList[pathList.Count - 1])
                        {
                            Itinero.Logging.Logger.Log("ShortcutBuilder", TraceEventType.Warning, "Shortcut has the same start and end vertex from {0}@{1} -> {2}@{3}!",
                                i, _locations[i], j, _locations[j]);
                            continue;
                        }

                        // add the shortcut and keep the id.
                        var shortcutId = _shortcutsDb.Add(pathList.ToArray(), null);
                        _shortcutIds[i][j] = shortcutId;

                        // add the shortcut as an edge.
                        _db.Network.AddEdge(pathList[0], pathList[pathList.Count - 1], new Data.Network.Edges.EdgeData()
                        {
                            Distance = path.Weight.Time + _switchPentaly,
                            MetaId = 0,
                            Profile = (ushort)shortcutProfile
                        }, null);
                    }
                    else
                    {
                        Itinero.Logging.Logger.Log("ShortcutBuilder", TraceEventType.Warning, "Shortcut not found from {0}@{1} -> {2}@{3}!",
                            i, _locations[i], j, _locations[j]);
                    }
                }
            }
        }

        /// <summary>
        /// Gets the shortcut ids.
        /// </summary>
        public uint[][] ShortcutIds
        {
            get
            {
                return _shortcutIds;
            }
        }

        /// <summary>
        /// Gets the shortcuts db.
        /// </summary>
        public ShortcutsDb ShortcutsDb
        {
            get
            {
                return _shortcutsDb;
            }
        }
    }
}