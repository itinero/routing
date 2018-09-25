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
using System.IO;
using Itinero.IO.Json;
using Itinero.Algorithms.Restrictions;
using Itinero.Algorithms.Dual;
using Itinero.Data.Network.Edges;
using Itinero.Algorithms.Networks;
using Itinero.Graphs.Geometric;
using Itinero.LocalGeo.IO;
using System.Threading;

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
            db.AddContracted(profile, forceEdgeBased, CancellationToken.None);
        }

        /// <summary>
        /// Creates a new contracted graph and adds it to the router db for the given profile.
        /// </summary>
        public static void AddContracted(this RouterDb db, Profiles.Profile profile, bool forceEdgeBased,
            CancellationToken cancellationToken)
        {
            var weightHandler = profile.DefaultWeightHandlerCached(db);

            // create the raw directed graph.
            ContractedDb contractedDb = null;

            if (!forceEdgeBased)
            {
                // check if there are complex restrictions in the routerdb forcing edge-based contraction.
                if (db.HasComplexRestrictions(profile))
                {
                    // there are complex restrictions, use edge-based contraction, the only way to support these in a contracted graph.
                    forceEdgeBased = true;
                }
            }

            if (forceEdgeBased)
            {
                // edge-based is needed when complex restrictions found.
                Itinero.Logging.Logger.Log("RouterDb", Logging.TraceEventType.Information,
                    "Contracting into an edge-based graph for profile {0}...",
                    profile.FullName);

                var contracted = new DirectedMetaGraph(ContractedEdgeDataSerializer.Size,
                    weightHandler.MetaSize);
                var restrictions = db.GetRestrictions(profile);
                var dualGraphBuilder = new DualGraphBuilder<float>(db.Network.GeometricGraph.Graph, contracted,
                    weightHandler, restrictions);
                dualGraphBuilder.Run(cancellationToken);

                var hierarchyBuilder =
                    new Itinero.Algorithms.Contracted.Dual.FastHierarchyBuilder(contracted, weightHandler)
                    {
                        DifferenceFactor = 5,
                        DepthFactor = 14,
                        ContractedFactor = 1
                    };
                hierarchyBuilder.Run(cancellationToken);

                contractedDb = new ContractedDb(contracted, true);
            }
            else
            {
                // vertex-based is ok when no complex restrictions found.
                Itinero.Logging.Logger.Log("RouterDb", Logging.TraceEventType.Information,
                    "Contracting into an vertex-based graph for profile {0}...",
                    profile.FullName);

                var contracted = new DirectedMetaGraph(ContractedEdgeDataSerializer.Size, weightHandler.MetaSize);
                var directedGraphBuilder =
                    new DirectedGraphBuilder<float>(db.Network.GeometricGraph.Graph, contracted, weightHandler);
                directedGraphBuilder.Run(cancellationToken);

                // contract the graph.
                var hierarchyBuilder =
                    new FastHierarchyBuilder<float>(contracted, weightHandler)
                    {
                        DifferenceFactor = 8,
                        DepthFactor = 14,
                        ContractedFactor = 1
                    };
                hierarchyBuilder.Run(cancellationToken);

                contractedDb = new ContractedDb(contracted, false);
            }

            // add the graph.
            lock (db)
            {
                db.AddContracted(profile, contractedDb);
            }
        }

        /// <summary>
        /// Creates a new contracted graph and adds it to the router db for the given profile.
        /// </summary>
        public static void AddContracted<T>(this RouterDb db, Profiles.Profile profile, WeightHandler<T> weightHandler, bool forceEdgeBased = false)
            where T : struct
        {
            db.AddContracted(profile, weightHandler, forceEdgeBased, CancellationToken.None);
        }

        /// <summary>
        /// Creates a new contracted graph and adds it to the router db for the given profile.
        /// </summary>
        public static void AddContracted<T>(this RouterDb db, Profiles.Profile profile, WeightHandler<T> weightHandler, bool forceEdgeBased, CancellationToken cancellationToken)
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

            lock (db) // TODO: reevaluate this lock, not needed around this entire block!
            {
                if (forceEdgeBased)
                { // edge-based is needed when complex restrictions found.
                    Itinero.Logging.Logger.Log("RouterDb", Logging.TraceEventType.Information, "Contracting into an edge-based graph for profile {0}...",
                        profile.FullName);

                    var contracted = new DirectedMetaGraph(ContractedEdgeDataSerializer.Size,
                        weightHandler.MetaSize);
                    var restrictions = db.GetRestrictions(profile);
                    var dualGraphBuilder = new DualGraphBuilder<T>(db.Network.GeometricGraph.Graph, contracted,
                        weightHandler, restrictions);
                    dualGraphBuilder.Run(cancellationToken);

                    // contract the graph.
                    var hierarchyBuilder = new Itinero.Algorithms.Contracted.Dual.HierarchyBuilder<T>(contracted,
                        new Itinero.Algorithms.Contracted.Dual.Witness.DykstraWitnessCalculator<T>(contracted.Graph, weightHandler, 
                            8, 1024), weightHandler);
                    hierarchyBuilder.DifferenceFactor = 5;
                    hierarchyBuilder.DepthFactor = 5;
                    hierarchyBuilder.ContractedFactor = 8;
                    hierarchyBuilder.Run(cancellationToken);

                    //// contract the graph.
                    //var priorityCalculator = new EdgeDifferencePriorityCalculator(contracted,
                    //    new DykstraWitnessCalculator(int.MaxValue));
                    //priorityCalculator.DifferenceFactor = 5;
                    //priorityCalculator.DepthFactor = 5;
                    //priorityCalculator.ContractedFactor = 8;
                    //var hierarchyBuilder = new HierarchyBuilder<T>(contracted, priorityCalculator,
                    //        new DykstraWitnessCalculator(int.MaxValue), weightHandler);
                    //hierarchyBuilder.Run();

                    contractedDb = new ContractedDb(contracted, true);
                }
                else
                { // vertex-based is ok when no complex restrictions found.
                    Itinero.Logging.Logger.Log("RouterDb", Logging.TraceEventType.Information, "Contracting into an edge-based graph for profile {0}...",
                        profile.FullName);

                    var contracted = new DirectedMetaGraph(ContractedEdgeDataSerializer.Size, weightHandler.MetaSize);
                    var directedGraphBuilder = new DirectedGraphBuilder<T>(db.Network.GeometricGraph.Graph, contracted, weightHandler);
                    directedGraphBuilder.Run(cancellationToken);

                    // contract the graph.
                    var priorityCalculator = new EdgeDifferencePriorityCalculator(contracted,
                        new DykstraWitnessCalculator(int.MaxValue));
                    priorityCalculator.DifferenceFactor = 5;
                    priorityCalculator.DepthFactor = 5;
                    priorityCalculator.ContractedFactor = 8;
                    var hierarchyBuilder = new HierarchyBuilder<T>(contracted, db.GetRestrictions(profile), priorityCalculator,
                            new DykstraWitnessCalculator(int.MaxValue), weightHandler);
                    hierarchyBuilder.Run(cancellationToken);

                    contractedDb = new ContractedDb(contracted, false);
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
        /// Gets the simple restriction function for the given profile.
        /// </summary>
        /// <param name="db">The router db.</param>
        /// <param name="profile">The vehicle profile.</param>
        /// <returns></returns>
        public static Func<uint, uint> GetGetSimpleRestrictions(this RouterDb db, Profile profile)
        {
            var getRestrictions = db.GetGetRestrictions(profile, true);
            return (v) =>
            {
                var r = getRestrictions(v);
                if (r != null)
                {
                    var rEnum = r.GetEnumerator();
                    while (rEnum.MoveNext())
                    {
                        var c = rEnum.Current;
                        if (c.Length > 0)
                        {
                            return c[0];
                        }
                    }
                }
                return Constants.NO_VERTEX;
            };
        }

        /// <summary>
        /// Gets the has any restriction function.
        /// </summary>
        /// <param name="db">The router db.</param>
        public static Func<uint, bool> GetHasAnyRestriction(this RouterDb db)
        {
            return (vertex) =>
            {
                foreach(var restrictionsDb in db.RestrictionDbs)
                {
                    if (restrictionsDb == null)
                    {
                        continue;
                    }

                    var enumerator = restrictionsDb.RestrictionsDb.GetEnumerator();
                    if (enumerator.MoveTo(vertex))
                    {
                        return true;
                    }
                }
                return false;
            };
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
        /// Gets the restriction collection for the given profile.
        /// </summary>
        /// <param name="db">The router db.</param>
        /// <param name="profile">The vehicle profile.</param>
        public static RestrictionCollection GetRestrictions(this RouterDb db, Profiles.Profile profile)
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

            return new RestrictionCollection((collection, vertex) =>
            {
                var f = false;
                collection.Clear();
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
                            var c = enumerator.Count;
                            switch (c)
                            {
                                case 0:
                                    break;
                                case 1:
                                    f = true;
                                    collection.Add(enumerator[0]);
                                    break;
                                case 2:
                                    f = true;
                                    collection.Add(enumerator[0], enumerator[1]);
                                    break;
                                default:
                                    f = true;
                                    collection.Add(enumerator[0], enumerator[1], enumerator[2]);
                                    break;
                            }
                        }
                    }
                }
                return f;
            });
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

            if (vertexPath.Count == 2 &&
                vertexPath[0] == Constants.NO_VERTEX &&
                vertexPath[1] == Constants.NO_VERTEX)
            {
                if (source.EdgeId != target.EdgeId)
                {
                    throw new Exception("Invalid path: the path represents a path inside one edge but source and target don't match.");
                }
                return source.EdgePathTo<T>(routerDb, weightHandler, target);
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
        /// Builds a non-dual edge path from a dual edge path taking into account the original router points.
        /// </summary>
        public static EdgePath<T> BuildDualEdgePath<T>(this RouterDb routerDb, WeightHandler<T> weightHandler, EdgePath<T> dualPath)
            where T : struct
        {
            EdgePath<T> top = null;
            EdgePath<T> path = null;
            var edgeEnumerator = routerDb.Network.GeometricGraph.Graph.GetEdgeEnumerator();
            uint other = Constants.NO_VERTEX;
            while (dualPath != null)
            {
                var directedEdgeId = DirectedEdgeId.FromRaw(dualPath.Vertex);
                edgeEnumerator.MoveToEdge(directedEdgeId.EdgeId);
                var vertex = edgeEnumerator.To;
                other = edgeEnumerator.From;
                if (!directedEdgeId.Forward)
                {
                    var t = other;
                    other = vertex;
                    vertex = t;
                }
                var next = new EdgePath<T>(vertex);
                if (path == null)
                {
                    path = next;
                }
                else
                {
                    path.From = next;
                    path = path.From;
                }
                if (top == null)
                {
                    top = path;
                }
                dualPath = dualPath.From;
            }
            path.From = new EdgePath<T>(other);
            return top;
        }

        /// <summary>
        /// Builds a non-dual edge path from a dual edge path taking into account the original router points.
        /// </summary>
        public static EdgePath<T> BuildDualEdgePath<T>(this RouterDb routerDb, WeightHandler<T> weightHandler, RouterPoint source, RouterPoint target, EdgePath<T> dualPath)
            where T : struct
        {
            EdgePath<T> top = null;
            EdgePath<T> path = null;
            var edgeEnumerator = routerDb.Network.GeometricGraph.Graph.GetEdgeEnumerator();
            uint other = Constants.NO_VERTEX;
            while (dualPath != null)
            {
                var directedEdgeId = DirectedEdgeId.FromRaw(dualPath.Vertex);
                edgeEnumerator.MoveToEdge(directedEdgeId.EdgeId);
                var vertex = edgeEnumerator.To;
                other = edgeEnumerator.From;
                if (!directedEdgeId.Forward)
                {
                    var t = other;
                    other = vertex;
                    vertex = t;
                }
                var next = new EdgePath<T>(vertex);
                if (path == null)
                {
                    path = next;
                }
                else
                {
                    path.From = next;
                    path = path.From;
                }
                if (top == null)
                {
                    top = path;
                }
                dualPath = dualPath.From;
            }
            if (target.IsVertex(routerDb, other))
            {
                path.From = new EdgePath<T>(other);
            }
            else
            {
                path.From = new EdgePath<T>(Constants.NO_VERTEX);
            }
            if (!source.IsVertex(routerDb, top.Vertex))
            {
                top.Vertex = Constants.NO_VERTEX;
            }
            return top;
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
        public static EdgePath<T> GetPathForEdge<T>(this RouterDb routerDb, WeightHandler<T> weightHandler, DirectedEdgeId directedEdgeId, bool asSource)
            where T : struct
        {
            var edge = routerDb.Network.GetEdge(directedEdgeId.EdgeId);

            return routerDb.GetPathForEdge(weightHandler, edge, directedEdgeId.Forward, asSource);
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
            var weight = weightHandler.GetEdgeWeight(edge);

            if (asSource)
            {
                if (edgeForward)
                {
                    if (!weight.Direction.F)
                    {
                        return null;
                    }
                    return new EdgePath<T>(edge.To, weight.Weight, edge.IdDirected(), new EdgePath<T>(edge.From));
                }
                if (!weight.Direction.B)
                {
                    return null;
                }
                return new EdgePath<T>(edge.From, weight.Weight, -edge.IdDirected(), new EdgePath<T>(edge.To));
            }
            else
            {
                if (edgeForward)
                {
                    if (!weight.Direction.F)
                    {
                        return null;
                    }
                    return new EdgePath<T>(edge.From, weight.Weight, -edge.IdDirected(), new EdgePath<T>(edge.To));
                }
                if (!weight.Direction.B)
                {
                    return null;
                }
                return new EdgePath<T>(edge.To, weight.Weight, edge.IdDirected(), new EdgePath<T>(edge.From));
            }
        }

        /// <summary>
        /// Gets all features around the given vertex as geojson.
        /// </summary>
        public static string GetGeoJsonAround(this RouterDb db, uint vertex, float distanceInMeter = 250,
            bool includeEdges = true, bool includeVertices = true)
        {
            var coordinate = db.Network.GetVertex(vertex);

            return db.GetGeoJsonAround(coordinate.Latitude, coordinate.Longitude, distanceInMeter,
                includeEdges, includeVertices);
        }

        /// <summary>
        /// Gets all features around the given location as geojson.
        /// </summary>
        public static string GetGeoJsonAround(this RouterDb db, float latitude, float longitude, float distanceInMeter = 250, 
            bool includeEdges = true, bool includeVertices = true)
        {
            var coordinate = new Coordinate(latitude, longitude);
            var north = coordinate.OffsetWithDirection(distanceInMeter, Navigation.Directions.DirectionEnum.North);
            var south = coordinate.OffsetWithDirection(distanceInMeter, Navigation.Directions.DirectionEnum.South);
            var east = coordinate.OffsetWithDirection(distanceInMeter, Navigation.Directions.DirectionEnum.East);
            var west = coordinate.OffsetWithDirection(distanceInMeter, Navigation.Directions.DirectionEnum.West);

            return db.GetGeoJsonIn(south.Latitude, west.Longitude, north.Latitude, east.Longitude,
                includeEdges, includeVertices);
        }


        /// <summary>
        /// Gets all features inside the given bounding box and builds a geojson string.
        /// </summary>
        public static string GetGeoJson(this RouterDb db, bool includeEdges = true, bool includeVertices = true)
        {
            var stringWriter = new StringWriter();
            db.WriteGeoJson(stringWriter, includeEdges, includeVertices);
            return stringWriter.ToInvariantString();
        }

        /// <summary>
        /// Gets all features inside the given bounding box and writes them as a geojson string.
        /// </summary>
        public static void WriteGeoJson(this RouterDb db, TextWriter writer, bool includeEdges = true, bool includeVertices = true)
        {
            if (db == null) { throw new ArgumentNullException("db"); }
            if (writer == null) { throw new ArgumentNullException("writer"); }

            var jsonWriter = new JsonWriter(writer);
            jsonWriter.WriteOpen();
            jsonWriter.WriteProperty("type", "FeatureCollection", true, false);
            jsonWriter.WritePropertyName("features", false);
            jsonWriter.WriteArrayOpen();

            var edges = new HashSet<long>();

            var edgeEnumerator = db.Network.GetEdgeEnumerator();
            for (uint vertex = 0; vertex < db.Network.VertexCount; vertex++)
            {
                if (includeVertices)
                {
                    db.WriteVertex(jsonWriter, vertex);
                }

                if (includeEdges)
                {
                    edgeEnumerator.MoveTo(vertex);
                    edgeEnumerator.Reset();
                    while (edgeEnumerator.MoveNext())
                    {
                        if (edges.Contains(edgeEnumerator.Id))
                        {
                            continue;
                        }
                        if (edgeEnumerator.DataInverted)
                        {
                            continue;
                        }
                        edges.Add(edgeEnumerator.Id);

                        db.WriteEdge(jsonWriter, edgeEnumerator);
                    }
                }
            }

            jsonWriter.WriteArrayClose();
            jsonWriter.WriteClose();
        }

        /// <summary>
        /// Gets all features inside the given bounding box and builds a geojson string.
        /// </summary>
        public static string GetGeoJsonIn(this RouterDb db, float minLatitude, float minLongitude,
            float maxLatitude, float maxLongitude, bool includeEdges = true, bool includeVertices = true)
        {
            var stringWriter = new StringWriter();
            db.WriteGeoJson(stringWriter, minLatitude, minLongitude, maxLatitude, maxLongitude, includeEdges, includeVertices);
            return stringWriter.ToInvariantString();
        }

        /// <summary>
        /// Gets all features inside the given bounding box and writes them as a geojson string.
        /// </summary>
        public static void WriteGeoJson(this RouterDb db, Stream stream, float minLatitude, float minLongitude,
            float maxLatitude, float maxLongitude, bool includeEdges = true, bool includeVertices = true)
        {
            db.WriteGeoJson(new StreamWriter(stream), minLatitude, minLongitude, maxLatitude, maxLongitude, includeEdges, includeVertices);
        }

        /// <summary>
        /// Gets all features inside the given bounding box and writes them as a geojson string.
        /// </summary>
        public static void WriteGeoJson(this RouterDb db, TextWriter writer, float minLatitude, float minLongitude,
            float maxLatitude, float maxLongitude, bool includeEdges = true, bool includeVertices = true)
        {
            if (db == null) { throw new ArgumentNullException("db"); }
            if (writer == null) { throw new ArgumentNullException("writer"); }

            var jsonWriter = new JsonWriter(writer);
            jsonWriter.WriteOpen();
            jsonWriter.WriteProperty("type", "FeatureCollection", true, false);
            jsonWriter.WritePropertyName("features", false);
            jsonWriter.WriteArrayOpen();

            var vertices = HilbertExtensions.Search(db.Network.GeometricGraph, minLatitude, minLongitude,
                maxLatitude, maxLongitude);
            var edges = new HashSet<long>();

            var edgeEnumerator = db.Network.GetEdgeEnumerator();
            foreach (var vertex in vertices)
            {
                if (includeVertices)
                {
                    db.WriteVertex(jsonWriter, vertex);
                }

                if (includeEdges)
                {
                    edgeEnumerator.MoveTo(vertex);
                    edgeEnumerator.Reset();
                    while (edgeEnumerator.MoveNext())
                    {
                        if (edges.Contains(edgeEnumerator.Id))
                        {
                            continue;
                        }
                        
                        edges.Add(edgeEnumerator.Id);
                        if (edgeEnumerator.DataInverted)
                        {
                            edgeEnumerator.MoveToEdge(edgeEnumerator.Id);
                            db.WriteEdge(jsonWriter, edgeEnumerator);
                            edgeEnumerator.MoveTo(vertex);
                        }
                        else
                        {
                            db.WriteEdge(jsonWriter, edgeEnumerator);
                        }
                    }
                }
            }

            jsonWriter.WriteArrayClose();
            jsonWriter.WriteClose();
        }

        /// <summary>
        /// Gets a geojson containing the given edge and optionally it's neighbours.
        /// </summary>
        /// <param name="db">The router db.</param>
        /// <param name="vertexIds">The vertices to get.</param>
        /// <param name="neighbours">Flag to get neighbours or not.</param>
        public static string GetGeoJsonVertices(this RouterDb db, bool neighbours = false, params uint[] vertexIds)
        {
            var edgeEnumerator = db.Network.GetEdgeEnumerator();

            var writer = new StringWriter();

            var jsonWriter = new JsonWriter(writer);
            jsonWriter.WriteOpen();
            jsonWriter.WriteProperty("type", "FeatureCollection", true, false);
            jsonWriter.WritePropertyName("features", false);
            jsonWriter.WriteArrayOpen();

            // collect all edges and vertices to write.
            var vertices = new HashSet<uint>();
            var originalVertices = new HashSet<uint>(vertexIds);
            var edges = new HashSet<uint>();
            foreach (var vertex in vertexIds)
            {
                vertices.Add(vertex);

                if (neighbours)
                {
                    if (edgeEnumerator.MoveTo(vertex))
                    {
                        while (edgeEnumerator.MoveNext())
                        {
                            vertices.Add(edgeEnumerator.To);

                            edges.Add(edgeEnumerator.Id);
                        }
                    }
                }
                else
                {
                    if (edgeEnumerator.MoveTo(vertex))
                    {
                        while (edgeEnumerator.MoveNext())
                        {
                            if (originalVertices.Contains(edgeEnumerator.To))
                            {
                                edges.Add(edgeEnumerator.Id);
                            }
                        }
                    }
                }
            }

            foreach (var edgeId in edges)
            {
                edgeEnumerator.MoveToEdge(edgeId);
                db.WriteEdge(jsonWriter, edgeEnumerator);
            }

            foreach (var vertex in vertices)
            {
                db.WriteVertex(jsonWriter, vertex);
            }

            jsonWriter.WriteArrayClose();
            jsonWriter.WriteClose();

            return writer.ToString();
        }

        /// <summary>
        /// Gets a geojson containing the given edge and optionally it's neighbours.
        /// </summary>
        /// <param name="db">The router db.</param>
        /// <param name="edgeIds">The edge id's to get.</param>
        /// <param name="neighbours">Flag to get neighbours or not.</param>
        /// <param name="includeVertices">Flag to get vertices or not.</param>
        public static string GetGeoJsonEdges(this RouterDb db, bool neighbours = false,
            bool includeVertices = true, params uint[] edgeIds)
        {
            var edgeEnumerator = db.Network.GetEdgeEnumerator();
            
            var writer = new StringWriter();
            
            var jsonWriter = new JsonWriter(writer);
            jsonWriter.WriteOpen();
            jsonWriter.WriteProperty("type", "FeatureCollection", true, false);
            jsonWriter.WritePropertyName("features", false);
            jsonWriter.WriteArrayOpen();

            // collect all edges and vertices to write.
            HashSet<uint> vertices = null;
            var edges = new HashSet<uint>();
            foreach (var edgeId in edgeIds)
            {
                edges.Add(edgeId);
                
                if (includeVertices)
                {
                    edgeEnumerator.MoveToEdge(edgeId);
                    if (vertices == null)
                    {
                        vertices = new HashSet<uint>();
                    }

                    vertices.Add(edgeEnumerator.From);
                    vertices.Add(edgeEnumerator.To);
                }

                if (!neighbours) continue;

                edgeEnumerator.MoveToEdge(edgeId);

                var from = edgeEnumerator.From;
                var to = edgeEnumerator.To;

                if (edgeEnumerator.MoveTo(@from))
                {
                    while (edgeEnumerator.MoveNext())
                    {
                        if (edges.Contains(edgeEnumerator.Id)) continue;
                        edges.Add(edgeEnumerator.Id);

                        if (!includeVertices) continue;
                            
                        vertices.Add(edgeEnumerator.From);
                        vertices.Add(edgeEnumerator.To);
                    }
                }

                if (edgeEnumerator.MoveTo(to))
                {
                    while (edgeEnumerator.MoveNext())
                    {
                        if (edges.Contains(edgeEnumerator.Id)) continue;
                        edges.Add(edgeEnumerator.Id);

                        if (!includeVertices) continue;
                            
                        vertices.Add(edgeEnumerator.From);
                        vertices.Add(edgeEnumerator.To);
                    }
                }
            }

            foreach (var edgeId in edges)
            {
                edgeEnumerator.MoveToEdge(edgeId);
                db.WriteEdge(jsonWriter, edgeEnumerator);
            }

            if (vertices != null)
            {
                foreach (var vertex in vertices)
                {
                    db.WriteVertex(jsonWriter, vertex);
                }
            }

            jsonWriter.WriteArrayClose();
            jsonWriter.WriteClose();
            
            return writer.ToString();
        }

        /// <summary>
        /// Writes a point-geometry for the given vertex.
        /// </summary>
        internal static void WriteVertex(this RouterDb db, JsonWriter jsonWriter, uint vertex)
        {
            var coordinate = db.Network.GetVertex(vertex);
            
            jsonWriter.WriteOpen();
            jsonWriter.WriteProperty("type", "Feature", true, false);
            jsonWriter.WritePropertyName("geometry", false);

            jsonWriter.WriteOpen();
            jsonWriter.WriteProperty("type", "Point", true, false);
            jsonWriter.WritePropertyName("coordinates", false);
            jsonWriter.WriteArrayOpen();
            jsonWriter.WriteArrayValue(coordinate.Longitude.ToInvariantString());
            jsonWriter.WriteArrayValue(coordinate.Latitude.ToInvariantString());
            jsonWriter.WriteArrayClose();
            jsonWriter.WriteClose();

            jsonWriter.WritePropertyName("properties");
            jsonWriter.WriteOpen();
            jsonWriter.WriteProperty("id", vertex.ToInvariantString());

            if (db.VertexData != null)
            {
                foreach (var dataName in db.VertexData.Names)
                {
                    var dataCollection = db.VertexData.Get(dataName);
                    if (vertex >= dataCollection.Count)
                    {
                        continue;
                    }
                    var data = dataCollection.GetRaw(vertex);
                    if (data != null)
                    {
                        jsonWriter.WriteProperty(dataName, data.ToInvariantString());
                    }
                }
            }

            var vertexMeta = db.VertexMeta?[vertex];
            if (vertexMeta != null)
            {
                foreach (var meta in vertexMeta)
                {
                    jsonWriter.WriteProperty(meta.Key, meta.Value, true, true);
                }
            }

            jsonWriter.WriteClose();

            jsonWriter.WriteClose();
        }

        /// <summary>
        /// Writes a linestring-geometry for the edge currently in the enumerator.
        /// </summary>
        internal static void WriteEdge(this RouterDb db, JsonWriter jsonWriter, RoutingNetwork.EdgeEnumerator edgeEnumerator)
        {
            var edgeAttributes = new Itinero.Attributes.AttributeCollection(db.EdgeMeta.Get(edgeEnumerator.Data.MetaId));
            edgeAttributes.AddOrReplace(db.EdgeProfiles.Get(edgeEnumerator.Data.Profile));

            var shape = db.Network.GetShape(edgeEnumerator.Current);
            
            jsonWriter.WriteOpen();
            jsonWriter.WriteProperty("type", "Feature", true, false);
            jsonWriter.WritePropertyName("geometry", false);

            jsonWriter.WriteOpen();
            jsonWriter.WriteProperty("type", "LineString", true, false);
            jsonWriter.WritePropertyName("coordinates", false);
            jsonWriter.WriteArrayOpen();

            foreach (var coordinate in shape)
            {
                jsonWriter.WriteArrayOpen();
                jsonWriter.WriteArrayValue(coordinate.Longitude.ToInvariantString());
                jsonWriter.WriteArrayValue(coordinate.Latitude.ToInvariantString());
                jsonWriter.WriteArrayClose();
            }

            jsonWriter.WriteArrayClose();
            jsonWriter.WriteClose();

            jsonWriter.WritePropertyName("properties");
            jsonWriter.WriteOpen();
            if (edgeAttributes != null)
            {
                foreach (var attribute in edgeAttributes)
                {
                    jsonWriter.WriteProperty(attribute.Key, attribute.Value, true, true);
                }
            }
            jsonWriter.WriteProperty("edgeid", edgeEnumerator.Id.ToInvariantString());
            jsonWriter.WriteProperty("vertex1", edgeEnumerator.From.ToInvariantString());
            jsonWriter.WriteProperty("vertex2", edgeEnumerator.To.ToInvariantString());

            if (db.EdgeData != null)
            {
                foreach (var dataName in db.EdgeData.Names)
                {
                    var edgeId = edgeEnumerator.Id;
                    var dataCollection = db.EdgeData.Get(dataName);
                    if (edgeId >= dataCollection.Count)
                    {
                        continue;
                    }
                    var data = dataCollection.GetRaw(edgeId);
                    if (data != null)
                    {
                        jsonWriter.WriteProperty(dataName, data.ToInvariantString());
                    }
                }
            }

            jsonWriter.WriteClose();

            jsonWriter.WriteClose();
        }
        
        /// <summary>
        /// Extracts part of the routerdb defined by the isInside function.
        /// </summary>
        /// <param name="db">The routerdb to extract from.</param>
        /// <returns></returns>
        public static RouterDb ExtractArea(this RouterDb db, float minLatitude, float minLongitude,
            float maxLatitude, float maxLongitude)
        {
            var box = new Box(minLatitude, minLongitude, maxLatitude, maxLongitude);
            var extract = db.ExtractArea((l) => box.Overlaps(l.Latitude, l.Longitude));

            var boxJson = box.ToPolygon().ToGeoJson();
            extract.Meta.AddOrReplace("bounds", boxJson);

            return extract;
        }

        /// <summary>
        /// Extracts part of the routerdb defined by the isInside function.
        /// </summary>
        /// <param name="db">The routerdb to extract from.</param>
        /// <param name="isInside">The is inside function.</param>
        /// <param name="insideOnly">Don't keep vertices outside, even if they have a neighbour that is inside.</param>
        /// <returns></returns>
        public static RouterDb ExtractArea(this RouterDb db, Func<Coordinate, bool> isInside, bool insideOnly = false)
        {
            return db.ExtractArea((v) =>
            {
                var l = db.Network.GeometricGraph.GetVertex(v);
                return isInside(l);
            }, insideOnly);
        }

        /// <summary>
        /// Extracts part of the routerdb defined by the isInside function.
        /// </summary>
        /// <param name="db">The routerdb to extract from.</param>
        /// <param name="isInside">The is inside function.</param>
        /// <param name="insideOnly">Don't keep vertices outside, even if they have a neighbour that is inside.</param>
        /// <returns></returns>
        public static RouterDb ExtractArea(this RouterDb db, Func<uint, bool> isInside, bool insideOnly = false)
        {
            var newDb = new RouterDb(db.Network.MaxEdgeDistance);
            // maps vertices old -> new.
            var idMap = new Dictionary<uint, uint>();
            // maps edges old -> new.
            var edgeIdMap = new Dictionary<uint, uint>();
            // keeps a set of vertices not inside but are needed for an edge that is partially inside.
            var boundaryVertices = new HashSet<uint>();

            // copy over all profiles.
            for (uint p = 0; p < db.EdgeProfiles.Count; p++)
            {
                var newP = newDb.EdgeProfiles.Add(db.EdgeProfiles.Get(p));

                System.Diagnostics.Debug.Assert(p == newP);
            }

            // loop over vertices and copy over relevant vertices and edges.
            var edgeEnumerator = db.Network.GetEdgeEnumerator();
            uint newV = 0;
            for (uint v = 0; v < db.Network.VertexCount; v++)
            {
                if (!isInside(v))
                {
                    // check if this vertex has neighbours that are inside.
                    // if so it needs to be kept.
                    if (!edgeEnumerator.MoveTo(v))
                    {
                        continue;
                    }

                    var keep = false;
                    while (edgeEnumerator.MoveNext())
                    {
                        var to = edgeEnumerator.To;

                        if (idMap.ContainsKey(to))
                        {
                            if (!boundaryVertices.Contains(to))
                            {
                                keep = true;
                                break;
                            }
                        }
                        else
                        {
                            if (isInside(to))
                            {
                                keep = true;
                                break;
                            }
                        }
                    }

                    if (!keep)
                    {
                        continue;
                    }

                    // this vertex is not inside but needs to be kept anyway.
                    boundaryVertices.Add(v);
                }

                // keep the mapping.
                idMap[v] = newV;

                // add the vertex.
                var vLocation = db.Network.GetVertex(v);
                newDb.Network.AddVertex(newV, vLocation.Latitude, vLocation.Longitude, vLocation.Elevation);

                // move the enumerator to the correct vertex.
                if (!edgeEnumerator.MoveTo(v))
                {
                    continue;
                }

                // add the edges if they lead to lower vertices.
                while (edgeEnumerator.MoveNext())
                {
                    var to = edgeEnumerator.To;

                    if (to > v)
                    {
                        continue;
                    }

                    // lower vertices should always have a mapping already.
                    uint newTo;
                    if (!idMap.TryGetValue(to, out newTo))
                    { // edge not inside.
                        continue;
                    }

                    if (insideOnly && !isInside(to))
                    { // keep only edge that have both inside.
                        continue;
                    }

                    // build edge data, only keep meta for copied edges.
                    var currentEdgeData = edgeEnumerator.Data;
                    var newEdgeData = new EdgeData()
                    {
                        Distance = currentEdgeData.Distance,
                        Profile = currentEdgeData.Profile,
                        MetaId = newDb.EdgeMeta.Add(
                            db.EdgeMeta.Get(currentEdgeData.MetaId))
                    };
                    var shape = edgeEnumerator.Shape;
                    uint newEdgeId;
                    if (!edgeEnumerator.DataInverted)
                    {
                        newEdgeId = newDb.Network.AddEdge(newV, newTo, newEdgeData, shape);
                    }
                    else
                    {
                        newEdgeId = newDb.Network.AddEdge(newTo, newV, newEdgeData, shape);
                    }

                    edgeIdMap[edgeEnumerator.Id] = newEdgeId;
                }

                newV++;
            }

            // copy over all supported vehicles.
            var supportedVehicles = db.GetSupportedVehicles();
            foreach (var vehicle in supportedVehicles)
            {
                newDb.AddSupportedVehicle(vehicle);
            }

            // copy over all vertex meta.
            var vertexData = db.VertexData;
            if (vertexData != null)
            {
                var vertexDataNames = db.VertexData.Names;
                foreach (var name in vertexDataNames)
                {
                    var collection = db.VertexData.Get(name);
                    var newCollection = newDb.VertexData.Add(name, collection.ElementType);

                    for (uint v = 0; v < db.Network.VertexCount; v++)
                    {
                        if (idMap.TryGetValue(v, out newV))
                        {
                            newCollection.CopyFrom(collection, newV, v);
                        }
                    }
                }
                foreach (var v in db.VertexMeta)
                {
                    if (idMap.TryGetValue(v, out newV))
                    {
                        newDb.VertexMeta[newV] = db.VertexMeta[v];
                    }
                }
            }

            // copy over all edge data.
            var edgeData = db.EdgeData;
            if (edgeData != null)
            {
                var edgeDataNames = db.EdgeData.Names;
                foreach (var name in edgeDataNames)
                {
                    var collection = db.EdgeData.Get(name);
                    var newCollection = newDb.EdgeData.Add(name, collection.ElementType);

                    for (uint e = 0; e < db.Network.EdgeCount; e++)
                    {
                        uint newEdgeId;
                        if (edgeIdMap.TryGetValue(e, out newEdgeId))
                        {
                            newCollection.CopyFrom(collection, newEdgeId, e);
                        }
                    }
                }
            }

            // copy over all restrictions.
            foreach (var r in db.RestrictionDbs)
            {
                var newR = new RestrictionsDb();
                newDb.AddRestrictions(r.Vehicle, newR);

                var enumerator = r.RestrictionsDb.GetEnumerator();
                var restrictionsSet = new HashSet<uint>();
                foreach(var pair in idMap)
                {
                    if (!enumerator.MoveTo(pair.Key))
                    {
                        continue;
                    }
    
                    while (enumerator.MoveNext())
                    {
                        if (restrictionsSet.Contains(enumerator.Id))
                        {
                            continue;
                        }
                        restrictionsSet.Add(enumerator.Id);

                        var restriction = enumerator.ToArray();
                        var newRestriction = new uint[restriction.Length];
                        for (var i = 0; i < restriction.Length; i++)
                        {
                            if (!idMap.TryGetValue(restriction[i], out newV))
                            {
                                newRestriction = null;
                                break;
                            }

                            newRestriction[i] = newV;
                        }

                        if (newRestriction != null)
                        {
                            newR.Add(newRestriction);
                        }
                    }
                }
            }

            // copy over the contracted graph(s).
            var graphEnumerator = db.Network.GeometricGraph.Graph.GetEdgeEnumerator();
            var newGraphEnumerator = newDb.Network.GeometricGraph.Graph.GetEdgeEnumerator();
            foreach (var profileName in db.GetContractedProfiles())
            {
                var profile = db.GetSupportedProfile(profileName);

                ContractedDb contractedDb;
                if (!db.TryGetContracted(profile, out contractedDb))
                {
                    continue;
                }

                // three options:
                // - vertex-based.
                // - dual vertex-based.

                if (contractedDb.HasNodeBasedGraph &&
                    contractedDb.NodeBasedIsEdgedBased)
                { // dual vertex-based
                    var graph = contractedDb.NodeBasedGraph;

                    var newGraph = graph.Extract(v =>
                    {
                        var edgeId = DirectedEdgeId.FromRaw(v);

                        uint newEdgeId;
                        if (!edgeIdMap.TryGetValue(edgeId.EdgeId, out newEdgeId))
                        {
                            return Constants.NO_VERTEX;
                        }
                        return new DirectedEdgeId(newEdgeId, edgeId.Forward).Raw;
                    });

                    var newContractedDb = new ContractedDb(newGraph, true);

                    newDb.AddContracted(profile, newContractedDb);
                }
                else if (contractedDb.HasNodeBasedGraph)
                { // vertex-based.
                    var graph = contractedDb.NodeBasedGraph;

                    var newGraph = graph.Extract(v =>
                    {
                        if (!idMap.TryGetValue(v, out newV))
                        {
                            return Constants.NO_VERTEX;
                        }
                        return newV;
                    });

                    var newContractedDb = new ContractedDb(newGraph, false);

                    newDb.AddContracted(profile, newContractedDb);
                }
            }

            return newDb;
        }

        /// <summary>
        /// Extracts part of the routerdb defined by the isInside function.
        /// </summary>
        /// <param name="db">The routerdb to extract from.</param>
        /// <param name="isInside">The is inside function, is the vertex to be included. Vertex won't be included if none of it's edge are.</param>
        /// <param name="isEdgeInside">The is inside function, is the edge to be included.</param>
        /// <returns></returns>
        public static RouterDb ExtractArea(this RouterDb db, Func<uint, bool> isInside, 
            Func<uint, uint, bool> isEdgeInside)
        {
            var newDb = new RouterDb(db.Network.MaxEdgeDistance);
            // maps vertices old -> new.
            var idMap = new Dictionary<uint, uint>();
            // maps edges old -> new.
            var edgeIdMap = new Dictionary<uint, uint>();
            // keeps a set of vertices not inside but are needed for an edge that is partially inside.
            var boundaryVertices = new HashSet<uint>();

            // copy over all profiles.
            for (uint p = 0; p < db.EdgeProfiles.Count; p++)
            {
                var newP = newDb.EdgeProfiles.Add(db.EdgeProfiles.Get(p));

                System.Diagnostics.Debug.Assert(p == newP);
            }

            // loop over vertices and copy over relevant vertices and edges.
            var edgeEnumerator = db.Network.GetEdgeEnumerator();
            uint newV = 0;
            for (uint v = 0; v < db.Network.VertexCount; v++)
            {
                if (!isInside(v))
                {
                    // check if this vertex has neighbours that are inside.
                    // if so it needs to be kept.
                    if (!edgeEnumerator.MoveTo(v))
                    {
                        continue;
                    }

                    var keep = false;
                    while (edgeEnumerator.MoveNext())
                    {
                        var to = edgeEnumerator.To;

                        if (idMap.ContainsKey(to))
                        {
                            if (!boundaryVertices.Contains(to))
                            {
                                keep = true;
                                break;
                            }
                        }
                        else
                        {
                            if (isInside(to))
                            {
                                keep = true;
                                break;
                            }
                        }
                    }

                    if (!keep)
                    {
                        continue;
                    }

                    // this vertex is not inside but needs to be kept anyway.
                    boundaryVertices.Add(v);
                }

                // keep the mapping.
                idMap[v] = newV;

                // add the vertex.
                var vLocation = db.Network.GetVertex(v);
                newDb.Network.AddVertex(newV, vLocation.Latitude, vLocation.Longitude);

                // move the enumerator to the correct vertex.
                if (!edgeEnumerator.MoveTo(v))
                {
                    continue;
                }

                // add the edges if they lead to lower vertices.
                while (edgeEnumerator.MoveNext())
                {
                    var to = edgeEnumerator.To;

                    if (to > v)
                    {
                        continue;
                    }

                    // lower vertices should always have a mapping already.
                    uint newTo;
                    if (!idMap.TryGetValue(to, out newTo))
                    { // edge not inside.
                        continue;
                    }

                    if (!isEdgeInside(v, to))
                    { // keep only edge that have both inside.
                        continue;
                    }

                    // build edge data, only keep meta for copied edges.
                    var edgeData = edgeEnumerator.Data;
                    var newEdgeData = new EdgeData()
                    {
                        Distance = edgeData.Distance,
                        Profile = edgeData.Profile,
                        MetaId = newDb.EdgeMeta.Add(
                            db.EdgeMeta.Get(edgeData.MetaId))
                    };
                    var shape = edgeEnumerator.Shape;
                    uint newEdgeId;
                    if (!edgeEnumerator.DataInverted)
                    {
                        newEdgeId = newDb.Network.AddEdge(newV, newTo, newEdgeData, shape);
                    }
                    else
                    {
                        newEdgeId = newDb.Network.AddEdge(newTo, newV, newEdgeData, shape);
                    }

                    edgeIdMap[edgeEnumerator.Id] = newEdgeId;
                }

                newV++;
            }

            // copy over all supported vehicles.
            var supportedVehicles = db.GetSupportedVehicles();
            foreach (var vehicle in supportedVehicles)
            {
                newDb.AddSupportedVehicle(vehicle);
            }

            // copy over all vertex meta.
            var vertexData = db.VertexData;
            if (vertexData != null)
            {
                var vertexDataNames = db.VertexData.Names;
                foreach (var name in vertexDataNames)
                {
                    var collection = db.VertexData.Get(name);
                    var newCollection = newDb.VertexData.Add(name, collection.ElementType);

                    for (uint v = 0; v < db.Network.VertexCount; v++)
                    {
                        if (idMap.TryGetValue(v, out newV))
                        {
                            newCollection.CopyFrom(collection, newV, v);
                        }
                    }
                }
                foreach (var v in db.VertexMeta)
                {
                    if (idMap.TryGetValue(v, out newV))
                    {
                        newDb.VertexMeta[newV] = db.VertexMeta[v];
                    }
                }
            }

            // copy over all edge data.
            if (db.EdgeData != null)
            {
                var edgeDataNames = db.EdgeData.Names;
                foreach (var name in edgeDataNames)
                {
                    var collection = db.EdgeData.Get(name);
                    var newCollection = newDb.EdgeData.Add(name, collection.ElementType);

                    for (uint e = 0; e < db.Network.EdgeCount; e++)
                    {
                        uint newEdgeId;
                        if (edgeIdMap.TryGetValue(e, out newEdgeId))
                        {
                            newCollection.CopyFrom(collection, newEdgeId, e);
                        }
                    }
                }
            }

            // copy over all restrictions.
            foreach (var r in db.RestrictionDbs)
            {
                var newR = new RestrictionsDb();
                newDb.AddRestrictions(r.Vehicle, newR);

                var enumerator = r.RestrictionsDb.GetEnumerator();
                var restrictionsSet = new HashSet<uint>();
                foreach (var pair in idMap)
                {
                    if (!enumerator.MoveTo(pair.Key))
                    {
                        continue;
                    }

                    while (enumerator.MoveNext())
                    {
                        if (restrictionsSet.Contains(enumerator.Id))
                        {
                            continue;
                        }
                        restrictionsSet.Add(enumerator.Id);

                        var restriction = enumerator.ToArray();
                        var newRestriction = new uint[restriction.Length];
                        for (var i = 0; i < restriction.Length; i++)
                        {
                            if (!idMap.TryGetValue(restriction[i], out newV))
                            {
                                newRestriction = null;
                                break;
                            }

                            newRestriction[i] = newV;
                        }

                        if (newRestriction != null)
                        {
                            newR.Add(newRestriction);
                        }
                    }
                }
            }

            // copy over the contracted graph(s).
            var graphEnumerator = db.Network.GeometricGraph.Graph.GetEdgeEnumerator();
            var newGraphEnumerator = newDb.Network.GeometricGraph.Graph.GetEdgeEnumerator();
            foreach (var profileName in db.GetContractedProfiles())
            {
                var profile = db.GetSupportedProfile(profileName);

                ContractedDb contractedDb;
                if (!db.TryGetContracted(profile, out contractedDb))
                {
                    continue;
                }

                // three options:
                // - vertex-based.
                // - dual vertex-based.

                if (contractedDb.HasNodeBasedGraph &&
                    contractedDb.NodeBasedIsEdgedBased)
                { // dual vertex-based
                    var graph = contractedDb.NodeBasedGraph;

                    var newGraph = graph.Extract(v =>
                    {
                        var edgeId = DirectedEdgeId.FromRaw(v);

                        uint newEdgeId;
                        if (!edgeIdMap.TryGetValue(edgeId.EdgeId, out newEdgeId))
                        {
                            return Constants.NO_VERTEX;
                        }
                        return new DirectedEdgeId(newEdgeId, edgeId.Forward).Raw;
                    });

                    var newContractedDb = new ContractedDb(newGraph, true);

                    newDb.AddContracted(profile, newContractedDb);
                }
                else if (contractedDb.HasNodeBasedGraph)
                { // vertex-based.
                    var graph = contractedDb.NodeBasedGraph;

                    var newGraph = graph.Extract(v =>
                    {
                        if (!idMap.TryGetValue(v, out newV))
                        {
                            return Constants.NO_VERTEX;
                        }
                        return newV;
                    });

                    var newContractedDb = new ContractedDb(newGraph, false);

                    newDb.AddContracted(profile, newContractedDb);
                }
            }

            return newDb;
        }

        /// <summary>
        /// Returns true if the routerdb already has island data for the given profile.
        /// </summary>
        /// <param name="db">The router db.</param>
        /// <param name="profile">The profile.</param>
        /// <returns>True if there is island data.</returns>
        public static bool HasIslandData(this RouterDb db, Profile profile)
        {
            var name = "islands_" + profile.FullName;
            return db.VertexData.TryGet(name, out _);
        }

        /// <summary>
        /// Adds and detects island data to improve resolving.
        /// </summary>
        public static void AddIslandData(this RouterDb db, Profile profile)
        {
            db.AddIslandData(profile, false, CancellationToken.None);
        }

        /// <summary>
        /// Adds and detects island data to improve resolving.
        /// </summary>
        public static void AddIslandData(this RouterDb db, Profile profile, bool buildEdgeMeta)
        {
            db.AddIslandData(profile, buildEdgeMeta, CancellationToken.None);
        }

        /// <summary>
        /// Adds and detects island data to improve resolving.
        /// </summary>
        public static void AddIslandData(this RouterDb db, Profile profile, bool buildEdgeMeta, CancellationToken cancellationToken)
        {
            if (!db.Supports(profile))
            {
                throw new ArgumentException(string.Format("Cannot build island data for an unsupported profile: {0}", profile.FullName));
            }

            var router = new Router(db);

            // run island detection.
            var islandDetector = new IslandDetector(db,
                new Func<ushort, Factor>[] { router.GetDefaultGetFactor(profile) },
                    db.GetRestrictions(profile));
            islandDetector.Run(cancellationToken);

            // properly format islands.
            var islands = islandDetector.Islands;
            
            // add the data to the routerdb.
            var name = "islands_" + profile.FullName;
            var meta = db.VertexData.AddUInt16(name);
            for (uint i = 0; i < islands.Length; i++)
            {
                var island = islands[i];
                if (island == IslandDetector.SINGLETON_ISLAND)
                { // these vertices can be removed in preprocessing but when using multiple profiles they can't.
                    meta[i] = Constants.ISLAND_SINGLETON;
                }
                else if(island == IslandDetector.RESTRICTED)
                {
                    meta[i] = Constants.ISLAND_RESTRICTED;
                }
                else
                {
                    uint size;
                    if (islandDetector.IslandSizes.TryGetValue(island, out size))
                    {
                        var originalSize = size;
                        size = size + 10; // below 10 is space reserved for special types of islands.
                        if (originalSize > ushort.MaxValue)
                        {
                            size = ushort.MaxValue;
                        }
                        if (originalSize > (islands.Length / 2))
                        {
                            size = ushort.MaxValue;
                        }
                        meta[i] = (ushort)size;
                    }
                    else
                    { // hmm, there should be at least something here, but no reason to fail on it.
                        meta[i] = 0;
                    }
                }
            }

            if (buildEdgeMeta)
            { // also add edge-meta.
                var edgeMeta = db.EdgeData.AddUInt16(name);

                var enumerator = db.Network.GeometricGraph.Graph.GetEdgeEnumerator();
                for (uint v = 0; v < db.Network.VertexCount; v++)
                {
                    if (!enumerator.MoveTo(v))
                    {
                        continue;
                    }

                    var vCount = meta[v];
                    var vIsland = false;
                    if (vCount < Algorithms.Search.ResolveSettings.DefaultMinIslandSize &&
                        vCount != 0 && vCount != Constants.ISLAND_RESTRICTED)
                    { 
                        vIsland = true;
                    }

                    while (enumerator.MoveNext())
                    {
                        if (v > enumerator.To)
                        { // do every edge once.
                            continue;
                        }

                        var to = enumerator.To;
                        var toCount = meta[to];
                        var toIsland = false;
                        if (toCount < Algorithms.Search.ResolveSettings.DefaultMinIslandSize &&
                            toCount != 0 && toCount != Constants.ISLAND_RESTRICTED)
                        {
                            toIsland = true;
                        }

                        if ((toIsland || vIsland) ||
                            (toCount == Constants.ISLAND_RESTRICTED && 
                            vCount == Constants.ISLAND_RESTRICTED))
                        {
                            edgeMeta[enumerator.Id] = (ushort)System.Math.Max(Constants.ISLAND_RESTRICTED,
                                System.Math.Max(vCount, toCount));
                        }
                        else
                        {
                            edgeMeta[enumerator.Id] = 0;
                        }
                    }
                }
            }
        }
        
        /// <summary>
        /// Returns the location on the network.
        /// </summary>
        public static Coordinate LocationOnNetwork(this RouterDb db, uint edgeId, ushort offset)
        {
            return db.Network.LocationOnNetwork(edgeId, offset);
        }
    }
}