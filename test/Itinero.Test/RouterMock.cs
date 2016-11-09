// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2016 Abelshausen Ben
// 
// This file is part of Itinero.
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
// along with Itinero. If not, see <http://www.gnu.org/licenses/>.

using Itinero.Attributes;
using Itinero.LocalGeo;
using Itinero.Data.Network;
using Itinero.Profiles;
using System.Collections.Generic;
using Itinero.Algorithms.Weights;
using System;
using Itinero.Algorithms;

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
            RoutingSettings<T> settings)
        {
            throw new System.NotImplementedException();
        }

        public override Result<EdgePath<T>> TryCalculateRaw<T>(Itinero.Profiles.IProfileInstance profile, WeightHandler<T> weightHandler, RouterPoint source, RouterPoint target, RoutingSettings<T> settings)
        {
            return new Result<EdgePath<T>>(new EdgePath<T>());
        }

        public override Result<EdgePath<T>> TryCalculateRaw<T>(IProfileInstance profile, WeightHandler<T> weightHandler, long sourceDirectedEdge, long targetDirectedEdge, RoutingSettings<T> settings)
        {
            return new Result<EdgePath<T>>(new EdgePath<T>());
        }

        public override Result<Route> BuildRoute<T>(IProfileInstance profile, WeightHandler<T> weightHandler, RouterPoint source, RouterPoint target, EdgePath<T> path)
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
            RouterPoint[] sources, RouterPoint[] targets, ISet<int> invalidSources, ISet<int> invalidTargets, RoutingSettings<T> settings)
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

        public override Result<bool> TryCheckConnectivity(IProfileInstance profile, RouterPoint point, float radiusInMeters)
        {
            throw new System.NotImplementedException();
        }

        public override Result<RouterPoint> TryResolve(IProfileInstance[] profiles,
            float latitude, float longitude, System.Func<RoutingEdge, bool> isBetter,
                float maxSearchDistance = Constants.SearchDistanceInMeter)
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