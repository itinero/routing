// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2013 Abelshausen Ben
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

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OsmSharp.Routing;
using OsmSharp.Routing.Graph;

namespace OsmSharp.Routing.Routers
{
    /// <summary>
    /// An implementation of an in-memory dynamic graph.
    /// </summary>
    public class TypedRouterResolvedGraph
    {
        /// <summary>
        /// Holds all graph data.
        /// </summary>
        private readonly Dictionary<long, RouterResolvedGraphVertex> _vertices;

        /// <summary>
        /// Creates a new in-memory graph.
        /// </summary>
        public TypedRouterResolvedGraph()
        {
            _vertices = new Dictionary<long, RouterResolvedGraphVertex>();
        }

        /// <summary>
        /// Adds a new vertex.
        /// </summary>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <param name="id"></param>
        /// <returns></returns>
        public void AddVertex(long id, float latitude, float longitude)
        {
            var vertex = new RouterResolvedGraphVertex();
            vertex.Id = id;
            vertex.Latitude = latitude;
            vertex.Longitude = longitude;

            // create vertex.
            _vertices.Add(id, vertex);
        }

        /// <summary>
        /// Returns the information in the current vertex.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="latitude"></param>
        /// <param name="longitude"></param>
        /// <returns></returns>
        public bool GetVertex(long id, out float latitude, out float longitude)
        {
            RouterResolvedGraphVertex vertex;
            if (_vertices.TryGetValue(id, out vertex))
            {
                latitude = vertex.Latitude;
                longitude = vertex.Longitude;
                return true;
            }
            latitude = float.MaxValue;
            longitude = float.MaxValue;
            return false;
        }

        /// <summary>
        /// Returns an enumerable of all vertices.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<long> GetVertices()
        {
            return _vertices.Keys;
        }

        /// <summary>
        /// Adds and arc to an existing vertex.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="data"></param>
        public void AddEdge(long from, long to, RouterResolvedGraphEdge data)
        {
            RouterResolvedGraphVertex vertex;
            if (_vertices.TryGetValue(from, out vertex))
            {
                KeyValuePair<long, RouterResolvedGraphEdge>[] arcs =
                    vertex.Arcs;
                int idx = -1;
                if (arcs == null)
                {
                    arcs = new KeyValuePair<long, RouterResolvedGraphEdge>[1];
                    idx = 0;
                    vertex.Arcs = arcs;
                }
                else
                {
                    idx = arcs.Length;
                    Array.Resize<KeyValuePair<long, RouterResolvedGraphEdge>>(ref arcs, arcs.Length + 1);
                    vertex.Arcs = arcs;
                }
                arcs[idx] = new KeyValuePair<long, RouterResolvedGraphEdge>(
                    to, data);
                _vertices[from] = vertex;
                return;
            }
            throw new ArgumentOutOfRangeException("from");
        }

        /// <summary>
        /// Removes all arcs starting at from ending at to.
        /// </summary>
        /// <param name="from"></param>
        /// <param name="to"></param>
        public void DeleteEdge(long from, long to)
        {
            RouterResolvedGraphVertex vertex;
            if (_vertices.TryGetValue(from, out vertex))
            {
                KeyValuePair<long, RouterResolvedGraphEdge>[] arcs =
                    _vertices[from].Arcs;
                if (arcs != null && arcs.Length > 0)
                {
                    var arcsList =
                        new List<KeyValuePair<long, RouterResolvedGraphEdge>>(arcs);
                    foreach (KeyValuePair<long, RouterResolvedGraphEdge> arc in arcs)
                    {
                        if (arc.Key == to)
                        {
                            arcsList.Remove(arc);
                        }
                    }
                    vertex.Arcs = arcsList.ToArray();
                }
                _vertices[from] = vertex;
                return;
            }
            throw new ArgumentOutOfRangeException("from");
        }

        /// <summary>
        /// Returns all arcs starting at the given vertex.
        /// </summary>
        /// <param name="vertex"></param>
        /// <returns></returns>
        public KeyValuePair<long, RouterResolvedGraphEdge>[] GetEdges(long vertex)
        {
            if (_vertices.ContainsKey(vertex))
            {
                if (_vertices[vertex].Arcs == null)
                {
                    return new KeyValuePair<long, RouterResolvedGraphEdge>[0];
                }
                return _vertices[vertex].Arcs;
            }
            return new KeyValuePair<long, RouterResolvedGraphEdge>[0]; // return empty data if the vertex does not exist!
        }

        /// <summary>
        /// Represents a simple vertex.
        /// </summary>
        private struct RouterResolvedGraphVertex
        {
            /// <summary>
            /// The id of this vertex.
            /// </summary>
            public long Id { get; set; }

            /// <summary>
            /// Holds the latitude.
            /// </summary>
            public float Latitude { get; set; }

            /// <summary>
            /// Holds longitude.
            /// </summary>
            public float Longitude { get; set; }

            /// <summary>
            /// Holds an array of edges starting at this vertex.
            /// </summary>
            public KeyValuePair<long, RouterResolvedGraphEdge>[] Arcs { get; set; }
        }

        /// <summary>
        /// Represents a resolved edge.
        /// </summary>
        public class RouterResolvedGraphEdge : IGraphEdgeData
        {
            /// <summary>
            /// Creates a new resolved edge.
            /// </summary>
            /// <param name="tags"></param>
            /// <param name="forward"></param>
            public RouterResolvedGraphEdge(uint tags, bool forward)
            {
                this.Tags = tags;
                this.Forward = forward;
            }

            /// <summary>
            /// Gets or sets the tags id.
            /// </summary>
            public uint Tags
            {
                get;
                private set;
            }

            /// <summary>
            /// Gets or sets the type id.
            /// </summary>
            public uint TypeId
            {
                get;
                private set;
            }

            /// <summary>
            /// Gets/sets the forward flag.
            /// </summary>
            public bool Forward
            {
                get; private set; 
            }

            /// <summary>
            /// These edge can always be resolved on.
            /// </summary>
            public bool IsVirtual
            {
                get
                {
                    return false;
                }
            }

            /// <summary>
            /// Returns true if this edge represents a neighbour-relation.
            /// </summary>
            public bool RepresentsNeighbourRelations
            {
                get { return true; }
            }

            /// <summary>
            /// Gets or sets intermediate coordinates (if any).
            /// </summary>
            public Math.Geo.Simple.GeoCoordinateSimple[] Coordinates
            {
                get;
                set;
            }

            /// <summary>
            /// Returns the exact reverse edge.
            /// </summary>
            /// <returns></returns>
            public IGraphEdgeData Reverse()
            {
                throw new NotImplementedException();
            }

            /// <summary>
            /// Returns true if the other edge represents the same information than this edge.
            /// </summary>
            /// <param name="other"></param>
            /// <returns></returns>
            public bool Equals(IGraphEdgeData other)
            {
                var otherResolved = (other as RouterResolvedGraphEdge);
                if(otherResolved != null)
                { // the same type already!
                    if(this.Tags != otherResolved.Tags ||
                        this.Forward != otherResolved.Forward ||
                        this.IsVirtual != otherResolved.IsVirtual)
                    { // something is different!
                        return false;
                    }

                    // only the coordinates can be different now.
                    if(this.Coordinates != null)
                    { // both have to contain the same coordinates.
                        if(this.Coordinates.Length != otherResolved.Coordinates.Length)
                        { // impossible, different number of coordinates.
                            return false;
                        }

                        for(int idx = 0; idx < otherResolved.Coordinates.Length; idx++)
                        {
                            if(this.Coordinates[idx].Longitude != otherResolved.Coordinates[idx].Longitude ||
                                this.Coordinates[idx].Latitude != otherResolved.Coordinates[idx].Latitude)
                            { // oeps, coordinates are different!
                                return false;
                            }
                        }
                        return true;
                    }
                    else
                    { // both are null.
                        return otherResolved.Coordinates == null;
                    }
                }
                return false;
            }

            /// <summary>
            /// Returns true if the other edge represents the same geographical information than this edge.
            /// </summary>
            /// <param name="other"></param>
            /// <returns></returns>
            public bool EqualsGeometrically(IGraphEdgeData other)
            {
                var otherResolved = (other as RouterResolvedGraphEdge);
                if (otherResolved != null)
                { // the same type already!
                    // only the coordinates can be different now.
                    if (this.Coordinates != null)
                    { // both have to contain the same coordinates.
                        if (this.Coordinates.Length != otherResolved.Coordinates.Length)
                        { // impossible, different number of coordinates.
                            return false;
                        }

                        for (int idx = 0; idx < otherResolved.Coordinates.Length; idx++)
                        {
                            if (this.Coordinates[idx].Longitude != otherResolved.Coordinates[idx].Longitude ||
                                this.Coordinates[idx].Latitude != otherResolved.Coordinates[idx].Latitude)
                            { // oeps, coordinates are different!
                                return false;
                            }
                        }
                        return true;
                    }
                    else
                    { // both are null.
                        return otherResolved.Coordinates == null;
                    }
                }
                return false;
            }

            /// <summary>
            /// Returns true if the shape of this edge is within the bounding box formed by it's two vertices.
            /// </summary>
            /// <remarks>False by default, only true when explicitly checked.</remarks>
            public bool ShapeInBox
            {
                get { return false; }
            }
        }
    }
}