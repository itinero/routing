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

using GeoAPI.Geometries;
using Itinero.Algorithms.Collections;
using Itinero.Geo;
using Itinero.Logging;
using Itinero.Profiles;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using System;
using System.Collections;
using System.Collections.Generic;

namespace Itinero.IO.Shape.Writer
{
    public class FeaturesList : IList<IFeature>
    {
        private readonly RouterDb _routerDb;
        private readonly List<Profile> _profiles;

        /// <summary>
        /// Creates a new features list.
        /// </summary>
        public FeaturesList(RouterDb routerDb, IEnumerable<Profile> profiles)
        {
            _routerDb = routerDb;
            _profiles = new List<Profile>(profiles);
        }

        public IFeature this[int index]
        {
            get
            {
                return this.BuildFeature(index);
            }
            set
            {
                throw new NotSupportedException("List is reaonly.");
            }
        }

        public int Count
        {
            get
            {
                return (int)_routerDb.Network.EdgeCount;
            }
        }

        public bool IsReadOnly
        {
            get
            {
                return true;
            }
        }

        public void Add(IFeature item)
        {
            throw new NotSupportedException("List is reaonly.");
        }

        public void Clear()
        {
            throw new NotSupportedException("List is reaonly.");
        }

        public bool Contains(IFeature item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(IFeature[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<IFeature> GetEnumerator()
        {
            return new Enumerator(this);
        }

        private class Enumerator : IEnumerator<IFeature>
        {
            private readonly FeaturesList _list;

            public Enumerator(FeaturesList list)
            {
                _list = list;
            }

            private int _current = -1;

            public IFeature Current
            {
                get
                {
                    return _list[_current];
                }
            }

            object IEnumerator.Current
            {
                get
                {
                    return _list[_current];
                }
            }

            public void Dispose()
            {

            }

            public bool MoveNext()
            {
                _current++;
                return _current < _list.Count;
            }

            public void Reset()
            {
                _current = -1;
            }
        }

        public int IndexOf(IFeature item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, IFeature item)
        {
            throw new NotSupportedException("List is reaonly.");
        }

        public bool Remove(IFeature item)
        {
            throw new NotSupportedException("List is reaonly.");
        }

        public void RemoveAt(int index)
        {
            throw new NotSupportedException("List is reaonly.");
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        private IFeature BuildFeature(int index)
        {
            if (((index - 1) % 10000) == 0)
            {
                Itinero.Logging.Logger.Log("FeatureList", TraceEventType.Information,
                    "Building feature {0}/{1}.", index - 1, this.Count);
            }

            var edge = _routerDb.Network.GetEdge((uint)index);

            var vertexLocation1 = _routerDb.Network.GeometricGraph.GetVertex(edge.From);
            var coordinates = new List<Coordinate>();
            coordinates.Add(new Coordinate(vertexLocation1.Longitude, vertexLocation1.Latitude));
            var shape = edge.Shape;
            if (shape != null)
            {
                var shapeEnumerable = shape.GetEnumerator();
                shapeEnumerable.Reset();
                while (shapeEnumerable.MoveNext())
                {
                    coordinates.Add(new Coordinate(shapeEnumerable.Current.Longitude,
                        shapeEnumerable.Current.Latitude));
                }
            }
            var vertexLocation2 = _routerDb.Network.GeometricGraph.GetVertex(edge.To);
            coordinates.Add(new Coordinate(vertexLocation2.Longitude, vertexLocation2.Latitude));
            var geometry = new LineString(coordinates.ToArray());

            var length = 0.0f;
            for (var i = 0; i < coordinates.Count - 1; i++)
            {
                length += Itinero.LocalGeo.Coordinate.DistanceEstimateInMeter((float)coordinates[i + 0].Y, (float)coordinates[i + 0].X,
                    (float)coordinates[i + 1].Y, (float)coordinates[i + 1].X);
            }

            var tags = new Itinero.Attributes.AttributeCollection(_routerDb.EdgeProfiles.Get(edge.Data.Profile));
            foreach (var tag in _routerDb.EdgeMeta.Get(edge.Data.MetaId))
            {
                tags.AddOrReplace(tag.Key, tag.Value);
            }

            var attributes = new AttributesTable();
            attributes.AddFrom("highway", tags);
            attributes.AddFrom("route", tags);

            foreach (var p in _profiles)
            {
                var profileName = p.FullName;

                if (profileName.Length > 7)
                {
                    profileName = profileName.Substring(0, 7);
                }

                var factor = p.Factor(tags);
                attributes.Add(profileName.ToLower(), factor.Value != 0);
                attributes.Add(profileName + "_dir", factor.Direction);
            }

            attributes.Add("length", System.Math.Round(length, 3));

            string lanesString;
            var lanes = 1;
            var lanesVerified = false;
            if (tags.TryGetValue("lanes", out lanesString))
            {
                lanesVerified = true;
                if (!int.TryParse(lanesString, out lanes))
                {
                    lanes = 1;
                    lanesVerified = false;
                }
            }
            attributes.Add("lanes", lanes);
            attributes.Add("lanes_ve", lanesVerified);
            
            var name = tags.ExtractName();
            attributes.Add("name", name);

            attributes.AddFrom("way_id", tags);
            attributes.AddFrom("tunnel", tags);
            attributes.AddFrom("bridge", tags);
            
            attributes.Add("from", edge.From);
            attributes.Add("to", edge.To);
            
            return new Feature(geometry, attributes);
        }
    }
}