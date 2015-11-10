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

using OsmSharp.Collections.Tags;
using OsmSharp.Math.Geo;
using System.Collections.Generic;

namespace OsmSharp.Routing.Test
{
    class RouterMock : IRouter
    {
        private long _resolvedId = 0;
        private HashSet<int> _invalidSet = new HashSet<int>();
        private TagsCollectionBase _matchingTags;

        public RouterMock()
        {

        }

        public RouterMock(HashSet<int> invalidSet)
        {
            _invalidSet = invalidSet;
        }

        public RouterMock(TagsCollectionBase matchingTags)
        {
            _matchingTags = matchingTags;
        }

        public Result<Route[][]> TryCalculate(OsmSharp.Routing.Profiles.Profile profile, RouterPoint[] sources, RouterPoint[] targets, ISet<int> invalidSources, ISet<int> invalidTargets)
        {
            throw new System.NotImplementedException();
        }

        public Result<Route> TryCalculate(OsmSharp.Routing.Profiles.Profile profile, RouterPoint source, RouterPoint target)
        {
            var route = new Route();
            route.Segments = new List<RouteSegment>(2);
            route.Segments.Add(new RouteSegment()
            {
                Latitude = (float)source.Latitude,
                Longitude = (float)source.Longitude,
                Profile = profile.Name
            });
            route.Segments.Add(new RouteSegment()
            {
                Latitude = (float)target.Latitude,
                Longitude = (float)target.Longitude,
                Profile = profile.Name
            });
            return new Result<Route>(route);
        }

        public Result<float[][]> TryCalculateWeight(OsmSharp.Routing.Profiles.Profile profile,
            RouterPoint[] sources, RouterPoint[] targets, ISet<int> invalidSources, ISet<int> invalidTargets)
        {
            var weights = new float[sources.Length][];
            for (var s = 0; s < sources.Length; s++)
            {
                weights[s] = new float[targets.Length];
                for (var t = 0; t < sources.Length; t++)
                {
                    weights[s][t] = (float)(new GeoCoordinate(sources[s].Latitude,
                        sources[s].Longitude)).DistanceReal(
                            (new GeoCoordinate(targets[t].Latitude,
                                targets[t].Longitude))).Value;
                }
            }

            foreach (var invalid in _invalidSet)
            {
                invalidSources.Add(invalid);
                invalidTargets.Add(invalid);
            }

            return new Result<float[][]>(weights);
        }

        public Result<float> TryCalculateWeight(OsmSharp.Routing.Profiles.Profile profile, RouterPoint source, RouterPoint target)
        {
            throw new System.NotImplementedException();
        }

        public Result<bool> TryCheckConnectivity(OsmSharp.Routing.Profiles.Profile profile, RouterPoint point, float radiusInMeters)
        {
            throw new System.NotImplementedException();
        }

        public Result<RouterPoint> TryResolve(OsmSharp.Routing.Profiles.Profile[] profiles,
            float latitude, float longitude, System.Func<OsmSharp.Routing.Network.RoutingEdge, bool> isBetter)
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