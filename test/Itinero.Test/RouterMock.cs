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
using Itinero.Algorithms.Weights;
using System;
using Itinero.Algorithms;
using Itinero.Algorithms.Search;
using System.Threading;

namespace Itinero.Test
{
    class RouterMock : RouterBase
    {
        private long _resolvedId = 0;
        private HashSet<int> _invalidSet = new HashSet<int>();
        private AttributeCollection _matchingTags;

        public RouterMock()
        {

        }

        public RouterMock(HashSet<int> invalidSet)
        {
            _invalidSet = invalidSet;
        }

        public RouterMock(AttributeCollection matchingTags)
        {
            _matchingTags = matchingTags;
        }

        public override RouterDb Db
        {
            get
            {
                return new RouterDb();
            }
        }

        public override Result<EdgePath<T>[][]> TryCalculateRaw<T>(Itinero.Profiles.IProfileInstance profile, WeightHandler<T> weightHandler, RouterPoint[] sources, RouterPoint[] targets, 
            RoutingSettings<T> settings, CancellationToken token)
        {
            throw new System.NotImplementedException();
        }

        public override Result<EdgePath<T>> TryCalculateRaw<T>(Itinero.Profiles.IProfileInstance profile, WeightHandler<T> weightHandler, RouterPoint source, RouterPoint target, RoutingSettings<T> settings, CancellationToken token)
        {
            return new Result<EdgePath<T>>(new EdgePath<T>());
        }

        public override Result<EdgePath<T>> TryCalculateRaw<T>(IProfileInstance profile, WeightHandler<T> weightHandler, long sourceDirectedEdge, long targetDirectedEdge, RoutingSettings<T> settings, CancellationToken token)
        {
            return new Result<EdgePath<T>>(new EdgePath<T>());
        }

        public override Result<EdgePath<T>> TryCalculateRaw<T>(IProfileInstance profileInstance, WeightHandler<T> weightHandler, RouterPoint source, bool? sourceForward, RouterPoint target, bool? targetForward, RoutingSettings<T> settings, CancellationToken cancellationToken)
        {
            return new Result<EdgePath<T>>(new EdgePath<T>());
        }

        public override Result<Route> BuildRoute<T>(IProfileInstance profile, WeightHandler<T> weightHandler, RouterPoint source, RouterPoint target, EdgePath<T> path, CancellationToken cancellationToken)
        {
            var route = new Route();
            route.Shape = new Coordinate[]
            {
                source.Location(),
                target.Location()
            };
            return new Result<Route>(route);
        }

        public override Result<T[][]> TryCalculateWeight<T>(IProfileInstance profile, WeightHandler<T> weightHandler,
            RouterPoint[] sources, RouterPoint[] targets, ISet<int> invalidSources, ISet<int> invalidTargets, RoutingSettings<T> settings, CancellationToken token)
        {
            var weights = new T[sources.Length][];
            for (var s = 0; s < sources.Length; s++)
            {
                weights[s] = new T[targets.Length];
                for (var t = 0; t < sources.Length; t++)
                {
                    weights[s][t] = weightHandler.Calculate(0, Coordinate.DistanceEstimateInMeter(
                        new Coordinate(sources[s].Latitude, sources[s].Longitude),
                        new Coordinate(targets[t].Latitude, targets[t].Longitude)));
                }
            }

            foreach (var invalid in _invalidSet)
            {
                invalidSources.Add(invalid);
                invalidTargets.Add(invalid);
            }

            return new Result<T[][]>(weights);
        }

        public override Result<bool> TryCheckConnectivity(IProfileInstance profile, RouterPoint point, float radiusInMeters, bool? forward, CancellationToken token)
        {
            throw new System.NotImplementedException();
        }

        public override Result<RouterPoint> TryResolve(IProfileInstance[] profiles,
            float latitude, float longitude, System.Func<RoutingEdge, bool> isBetter,
                float maxSearchDistance, ResolveSettings settings, CancellationToken token)
        {
            if (latitude < -90 || latitude > 90 ||
                longitude < -180 || longitude > 180)
            {
                return new Result<RouterPoint>("Outside of loaded network.");
            }
            if(isBetter != null &&
               !isBetter(null))
            {
                return new Result<RouterPoint>("Not better.");
            }
            _resolvedId++;
            return new Result<RouterPoint>(new RouterPoint(latitude, longitude, 0, 0));
        }
    }
}