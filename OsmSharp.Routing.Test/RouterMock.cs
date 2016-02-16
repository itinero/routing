// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2016 Abelshausen Ben
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

using OsmSharp.Routing.Attributes;
using OsmSharp.Routing.Geo;
using OsmSharp.Routing.Network;
using OsmSharp.Routing.Profiles;
using System.Collections.Generic;

namespace OsmSharp.Routing.Test
{
    class RouterMock : IRouter
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

        public Result<Route[][]> TryCalculate(OsmSharp.Routing.Profiles.Profile profile, RouterPoint[] sources, RouterPoint[] targets, 
            ISet<int> invalidSources, ISet<int> invalidTargets)
        {
            throw new System.NotImplementedException();
        }

        public Result<Route> TryCalculate(OsmSharp.Routing.Profiles.Profile profile, RouterPoint source, RouterPoint target)
        {
            var route = new Route();
            route.Shape = new Coordinate[]
            {
                source.Location(),
                target.Location()
            };
            return new Result<Route>(route);
        }

        public Result<float[][]> TryCalculateWeight(Profile profile,
            RouterPoint[] sources, RouterPoint[] targets, ISet<int> invalidSources, ISet<int> invalidTargets)
        {
            var weights = new float[sources.Length][];
            for (var s = 0; s < sources.Length; s++)
            {
                weights[s] = new float[targets.Length];
                for (var t = 0; t < sources.Length; t++)
                {
                    weights[s][t] = Coordinate.DistanceEstimateInMeter(
                        new Coordinate(sources[s].Latitude, sources[s].Longitude),
                        new Coordinate(targets[t].Latitude, targets[t].Longitude));
                }
            }

            foreach (var invalid in _invalidSet)
            {
                invalidSources.Add(invalid);
                invalidTargets.Add(invalid);
            }

            return new Result<float[][]>(weights);
        }

        public Result<float> TryCalculateWeight(Profile profile, RouterPoint source, RouterPoint target)
        {
            throw new System.NotImplementedException();
        }

        public Result<bool> TryCheckConnectivity(Profile profile, RouterPoint point, float radiusInMeters)
        {
            throw new System.NotImplementedException();
        }

        public Result<RouterPoint> TryResolve(Profile[] profiles,
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

        public bool SupportsAll(params Profile[] profiles)
        {
            return true;
        }
    }
}