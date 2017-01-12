// Itinero - Routing for .NET
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

using Itinero.Algorithms.Contracted;
using Itinero.Algorithms.Contracted.Witness;
using Itinero.Attributes;
using Itinero.Graphs.Directed;
using Itinero.Data.Network.Restrictions;
using Itinero.Data.Contracted.Edges;
using Itinero.Data.Contracted;
using System.Collections.Generic;
using System;
using Itinero.Profiles;
using Itinero.Algorithms.Weights;
using Itinero.Algorithms;
using Itinero.Graphs;
using Itinero.Data.Network;
using Itinero.Algorithms.Search.Hilbert;
using Itinero.LocalGeo;

namespace Itinero
{
    /// <summary>
    /// Contains extension methods for the router db.
    /// </summary>
    public static class RouterDbExtensions
    {
        /// <summary>
        /// Creates a new contracted graph and adds it to the router db for the given profile.
        /// </summary>
        public static void AddContracted(this RouterDb db, Profiles.Profile profile, bool forceEdgeBased = false)
        {
            db.AddContracted<float>(profile, profile.DefaultWeightHandlerCached(db), forceEdgeBased);
        }

        /// <summary>
        /// Creates a new contracted graph and adds it to the router db for the given profile.
        /// </summary>
        public static void AddContracted<T>(this RouterDb db, Profiles.Profile profile, WeightHandler<T> weightHandler, bool forceEdgeBased = false)
            where T : struct
        {
            // create the raw directed graph.
            ContractedDb contractedDb = null;

            if (!forceEdgeBased)
            { // check if there are complex restrictions in the routerdb forcing edge-based contraction.
                if (db.HasComplexRestrictions(profile))
                { // there are complex restrictions, use edge-based contraction, the only way to support these in a contracted graph.
                    forceEdgeBased = true;
                }
            }

            lock (db)
            {
                if (forceEdgeBased)
                { // edge-based is needed when complex restrictions found.
                    var contracted = new DirectedDynamicGraph(weightHandler.DynamicSize);
                    var directedGraphBuilder = new Itinero.Algorithms.Contracted.EdgeBased.DirectedGraphBuilder<T>(db.Network.GeometricGraph.Graph, contracted,
                        weightHandler);
                    directedGraphBuilder.Run();

                    // contract the graph.
                    var priorityCalculator = new Itinero.Algorithms.Contracted.EdgeBased.EdgeDifferencePriorityCalculator<T>(contracted, weightHandler,
                        new Itinero.Algorithms.Contracted.EdgeBased.Witness.DykstraWitnessCalculator<T>(weightHandler, 4, 64));
                    priorityCalculator.DifferenceFactor = 5;
                    priorityCalculator.DepthFactor = 5;
                    priorityCalculator.ContractedFactor = 8;
                    var hierarchyBuilder = new Itinero.Algorithms.Contracted.EdgeBased.HierarchyBuilder<T>(contracted, priorityCalculator,
                            new Itinero.Algorithms.Contracted.EdgeBased.Witness.DykstraWitnessCalculator<T>(weightHandler, int.MaxValue, 64), weightHandler, db.GetGetRestrictions(profile, null));
                    hierarchyBuilder.Run();

                    contractedDb = new ContractedDb(contracted);
                }
                else
                { // vertex-based is ok when no complex restrictions found.
                    var contracted = new DirectedMetaGraph(ContractedEdgeDataSerializer.Size, weightHandler.MetaSize);
                    var directedGraphBuilder = new DirectedGraphBuilder<T>(db.Network.GeometricGraph.Graph, contracted, weightHandler);
                    directedGraphBuilder.Run();

                    // contract the graph.
                    var priorityCalculator = new EdgeDifferencePriorityCalculator(contracted,
                        new DykstraWitnessCalculator(int.MaxValue));
                    priorityCalculator.DifferenceFactor = 5;
                    priorityCalculator.DepthFactor = 5;
                    priorityCalculator.ContractedFactor = 8;
                    var hierarchyBuilder = new HierarchyBuilder<T>(contracted, priorityCalculator,
                            new DykstraWitnessCalculator(int.MaxValue), weightHandler);
                    hierarchyBuilder.Run();

                    contractedDb = new ContractedDb(contracted);
                }
            }

            // add the graph.
            lock (db)
            {
                db.AddContracted(profile, contractedDb);
            }
        }

        /// <summary>
        /// Returns true if all of the given profiles are supported.
        /// </summary>
        /// <returns></returns>
        public static bool SupportsAll(this RouterDb db, params Profiles.IProfileInstance[] profiles)
        {
            for (var i = 0; i < profiles.Length; i++)
            {
                if (!db.Supports(profiles[i].Profile))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Returns true if all of the given profiles are supported.
        /// </summary>
        /// <returns></returns>
        public static bool SupportsAll(this RouterDb db, params Profiles.Profile[] profiles)
        {
            for (var i = 0; i < profiles.Length; i++)
            {
                if (!db.Supports(profiles[i]))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Returns true if the given profile is supported.
        /// </summary>
        public static bool Supports(this RouterDb db, Profiles.Profile profile)
        {
            return db.Supports(profile.Parent.Name);
        }

        /// <summary>
        /// Returns one attribute collection containing both the profile and meta tags.
        /// </summary>
        public static IAttributeCollection GetProfileAndMeta(this RouterDb db, uint profileId, uint meta)
        {
            var tags = new AttributeCollection();

            var metaTags = db.EdgeMeta.Get(meta);
            if (metaTags != null)
            {
                tags.AddOrReplace(metaTags);
            }

            var profileTags = db.EdgeProfiles.Get(profileId);
            if (profileTags != null)
            {
                tags.AddOrReplace(profileTags);
            }

            return tags;
        }

        /// <summary>
        /// Returns true if this db contains restrictions for the given vehicle type.
        /// </summary>
        public static bool HasRestrictions(this RouterDb db, string vehicleType)
        {
            RestrictionsDb restrictions;
            return db.TryGetRestrictions(vehicleType, out restrictions);
        }

        /// <summary>
        /// Returns true if this db contains complex restrictions for the given vehicle type.
        /// </summary>
        public static bool HasComplexRestrictions(this RouterDb db, string vehicleType)
        {
            RestrictionsDb restrictions;
            if (db.TryGetRestrictions(vehicleType, out restrictions))
            {
                return restrictions.HasComplexRestrictions;
            }
            return false;
        }

        /// <summary>
        /// Returns true if this db contains complex restrictions for the given vehicle types.
        /// </summary>
        public static bool HasComplexRestrictions(this RouterDb db, IEnumerable<string> vehicleTypes)
        {
            if (db.HasComplexRestrictions(string.Empty))
            {
                return true;
            }
            foreach (var vehicleType in vehicleTypes)
            {
                if (db.HasComplexRestrictions(vehicleType))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns true if this db contains complex restrictions for the given profile.
        /// </summary>
        public static bool HasComplexRestrictions(this RouterDb db, Profiles.Profile profile)
        {
            return db.HasComplexRestrictions(profile.VehicleTypes);
        }

        /// <summary>
        /// Gets the get restriction function for the given profile.
        /// </summary>
        /// <param name="db">The router db.</param>
        /// <param name="profile">The vehicle profile.</param>
        /// <param name="first">When true, only restrictions starting with given vertex, when false only restrictions ending with given vertex already reversed, when null all restrictions are returned.</param>
        public static Func<uint, IEnumerable<uint[]>> GetGetRestrictions(this RouterDb db, Profiles.Profile profile, bool? first)
        {
            var vehicleTypes = new List<string>(profile.VehicleTypes);
            vehicleTypes.Insert(0, string.Empty);

            var restrictionDbs = new RestrictionsDb[vehicleTypes.Count];
            for (var i = 0; i < vehicleTypes.Count; i++)
            {
                RestrictionsDb restrictionsDb;
                if (db.TryGetRestrictions(vehicleTypes[i], out restrictionsDb))
                {
                    restrictionDbs[i] = restrictionsDb;
                }
            }

            return (vertex) =>
            {
                var restrictionList = new List<uint[]>();
                for (var i = 0; i < restrictionDbs.Length; i++)
                {
                    var restrictionsDb = restrictionDbs[i];
                    if (restrictionsDb == null)
                    {
                        continue;
                    }

                    var enumerator = restrictionsDb.GetEnumerator();
                    if (enumerator.MoveTo(vertex))
                    {
                        while (enumerator.MoveNext())
                        {
                            if (first.HasValue && first.Value)
                            {
                                if (enumerator[0] == vertex)
                                {
                                    restrictionList.Add(enumerator.ToArray());
                                }
                            }
                            else if (first.HasValue && !first.Value)
                            {
                                if (enumerator[(int)enumerator.Count - 1] == vertex)
                                {
                                    var array = enumerator.ToArray();
                                    array.Reverse();
                                    restrictionList.Add(array);
                                }
                            }
                            else
                            {
                                restrictionList.Add(enumerator.ToArray());
                            }
                        }
                    }
                }
                return restrictionList;
            };
        }

        /// <summary>
        /// Builds an edge path from a path consisiting of only vertices.
        /// </summary>
        public static EdgePath<T> BuildEdgePath<T>(this RouterDb routerDb, WeightHandler<T> weightHandler, RouterPoint source, RouterPoint target, List<uint> vertexPath)
            where T : struct
        {
            if (vertexPath == null || vertexPath.Count == 0)
            {
                return null;
            }

            var path = new EdgePath<T>(vertexPath[0]);
            var i = 1;
            if (path.Vertex == Constants.NO_VERTEX)
            { // add first router point segment from source.
                path = source.EdgePathTo(routerDb, weightHandler, vertexPath[1]);
                i = 2;
            }

            var edgeEnumerator = routerDb.Network.GeometricGraph.Graph.GetEdgeEnumerator();
            for (; i < vertexPath.Count; i++)
            {
                var vertex = vertexPath[i];
                if (vertex == Constants.NO_VERTEX)
                {
                    if (i != vertexPath.Count - 1)
                    {
                        throw new Exception("Invalid data found in vertex path: a non-vertex id was found at an invalid location.");
                    }
                    var toTarget = target.EdgePathTo(routerDb, weightHandler, path.Vertex, true);
                    path = new EdgePath<T>(toTarget.Vertex, weightHandler.Add(toTarget.Weight, path.Weight), toTarget.Edge, path);
                    break;
                }
                T weight;
                var best = edgeEnumerator.FindBestEdge(weightHandler, path.Vertex, vertexPath[i], out weight);
                if (best == Constants.NO_EDGE)
                {
                    throw new Exception(string.Format("Cannot build vertex path, edge {0} -> {1} not found.", path.Vertex, vertexPath[i]));
                }
                path = new EdgePath<T>(vertexPath[i], weightHandler.Add(weight, path.Weight), best, path);
            }
            return path;
        }

        /// <summary>
        /// Adds the router point as a vertex.
        /// </summary>
        public static uint AddAsVertex(this RouterDb routerDb, RouterPoint point)
        {
            if (routerDb.HasContracted)
            {
                throw new InvalidOperationException("Cannot add new vertices to a routerDb with contracted versions of the network.");
            }

            if (point.IsVertex())
            { // the router point is already a vertex.
                return point.VertexId(routerDb);
            }

            // add a new vertex at the router point location.            
            var location = point.LocationOnNetwork(routerDb);
            var vertex = routerDb.Network.VertexCount;
            routerDb.Network.AddVertex(vertex, location.Latitude, location.Longitude);

            // add two new edges.
            var edge = routerDb.Network.GetEdge(point.EdgeId);
            var shapeFrom = point.ShapePointsTo(routerDb, edge.From);
            shapeFrom.Reverse(); // we need this shape from edge.From -> vertex.
            var shapeTo = point.ShapePointsTo(routerDb, edge.To);
            var distanceFrom = point.DistanceTo(routerDb, edge.From);
            var distanceTo = point.DistanceTo(routerDb, edge.To);

            // remove edge id.
            routerDb.Network.RemoveEdge(point.EdgeId);

            // split in two.
            routerDb.Network.AddEdge(edge.From, vertex, new Data.Network.Edges.EdgeData()
            {
                Distance = distanceFrom,
                MetaId = edge.Data.MetaId,
                Profile = edge.Data.Profile
            }, shapeFrom);
            routerDb.Network.AddEdge(vertex, edge.To, new Data.Network.Edges.EdgeData()
            {
                Distance = distanceTo,
                MetaId = edge.Data.MetaId,
                Profile = edge.Data.Profile
            }, shapeTo);

            // sort the vertices again.
            routerDb.Network.Sort((v1, v2) =>
            {
                if (vertex == v1)
                {
                    vertex = (uint)v2;
                }
                else if (vertex == v2)
                {
                    vertex = (uint)v1;
                }
            });
            return vertex;
        }

        /// <summary>
        /// Adds the router point as a vertex.
        /// </summary>
        public static uint[] AddAsVertices(this RouterDb routerDb, RouterPoint[] points)
        {
            if (routerDb.HasContracted)
            {
                throw new InvalidOperationException("Cannot add new vertices to a routerDb with contracted versions of the network.");
            }

            var edges = new HashSet<uint>();
            for (var i = 0; i < points.Length; i++)
            {
                var point = points[i];
                if (edges.Contains(point.EdgeId))
                {
                    point = null;
                }
                else
                {
                    edges.Add(point.EdgeId);
                }
            }

            var newVertices = new uint[points.Length];
            var newEdges = new List<EdgeToSplit>();
            for (var i = 0; i < points.Length; i++)
            {
                var point = points[i];

                if (point == null)
                { // there was a duplicate edge.
                    newVertices[i] = Constants.NO_VERTEX;
                    newEdges.Add(null);
                    continue;
                }

                if (point.IsVertex())
                { // the router point is already a vertex.
                    newVertices[i] = point.VertexId(routerDb);
                    newEdges.Add(null);
                    continue;
                }

                // add a new vertex at the router point location.            
                var location = point.LocationOnNetwork(routerDb);

                // add two new edges.
                var edge = routerDb.Network.GetEdge(point.EdgeId);
                var shapeFrom = point.ShapePointsTo(routerDb, edge.From);
                shapeFrom.Reverse(); // we need this shape from edge.From -> vertex.
                var shapeTo = point.ShapePointsTo(routerDb, edge.To);
                var distanceFrom = point.DistanceTo(routerDb, edge.From);
                var distanceTo = point.DistanceTo(routerDb, edge.To);

                // register new edge.
                newEdges.Add(new EdgeToSplit()
                {
                    Coordinate = location,
                    From = edge.From,
                    To = edge.To,
                    DistanceFrom = distanceFrom,
                    DistanceTo = distanceTo,
                    ShapeFrom = shapeFrom,
                    ShapeTo = shapeTo,
                    MetaId = edge.Data.MetaId,
                    Profile = edge.Data.Profile
                });
            }

            for (var i = 0; i < newEdges.Count; i++)
            {
                var edgeToSplit = newEdges[i];
                if (edgeToSplit == null)
                {
                    continue;
                }

                // remove original edge.
                routerDb.Network.RemoveEdges(edgeToSplit.From, edgeToSplit.To);

                // add vertex.
                var vertex = routerDb.Network.VertexCount;
                routerDb.Network.AddVertex(vertex, edgeToSplit.Coordinate.Latitude, edgeToSplit.Coordinate.Longitude);

                // add two new pieces.
                routerDb.Network.AddEdge(edgeToSplit.From, vertex, new Data.Network.Edges.EdgeData()
                {
                    Distance = edgeToSplit.DistanceFrom,
                    MetaId = edgeToSplit.MetaId,
                    Profile = edgeToSplit.Profile
                }, edgeToSplit.ShapeFrom);
                routerDb.Network.AddEdge(vertex, edgeToSplit.To, new Data.Network.Edges.EdgeData()
                {
                    Distance = edgeToSplit.DistanceTo,
                    MetaId = edgeToSplit.MetaId,
                    Profile = edgeToSplit.Profile
                }, edgeToSplit.ShapeTo);

                newVertices[i] = vertex;
            }

            return newVertices;
        }

        private class EdgeToSplit
        {
            public Coordinate Coordinate { get; set; }

            public uint From { get; set; }

            public uint To { get; set; }

            public float DistanceFrom { get; set; }

            public float DistanceTo { get; set; }

            public List<LocalGeo.Coordinate> ShapeFrom { get; set; }

            public List<LocalGeo.Coordinate> ShapeTo { get; set; }

            public uint MetaId { get; set; }

            public ushort Profile { get; set; }
        }

        /// <summary>
        /// Generates an edge path for the given edge.
        /// </summary>
        public static EdgePath<T> GetPathForEdge<T>(this RouterDb routerDb, WeightHandler<T> weightHandler, long directedEdgeId, bool asSource)
            where T : struct
        {
            var edge = routerDb.Network.GetEdge(directedEdgeId);

            return routerDb.GetPathForEdge(weightHandler, edge, directedEdgeId > 0, asSource);
        }

        /// <summary>
        /// Generates an edge path for the given edge.
        /// </summary>
        public static EdgePath<T> GetPathForEdge<T>(this RouterDb routerDb, WeightHandler<T> weightHandler, RoutingEdge edge, bool edgeForward, bool asSource)
            where T : struct
        {
            var weight = weightHandler.Calculate(edge.Data.Profile, edge.Data.Distance);

            if (asSource)
            {
                if (edgeForward)
                {
                    return new EdgePath<T>(edge.To, weight, edge.IdDirected(), new EdgePath<T>(edge.From));
                }
                return new EdgePath<T>(edge.From, weight, -edge.IdDirected(), new EdgePath<T>(edge.To));
            }
            else
            {
                if (edgeForward)
                {
                    return new EdgePath<T>(edge.From, weight, -edge.IdDirected(), new EdgePath<T>(edge.To));
                }
                return new EdgePath<T>(edge.To, weight, edge.IdDirected(), new EdgePath<T>(edge.From));
            }
        }
    }
}