using Itinero.Algorithms.Contracted.EdgeBased;
using Itinero.Algorithms.Contracted.EdgeBased.Contraction;
using Itinero.Algorithms.PriorityQueues;
using Itinero.Algorithms.Weights;
using Itinero.Graphs.Directed;
using Itinero.IO.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Itinero
{
#if DEBUG
    /// <summary>
    /// Contains some helper debug extensions.
    /// </summary>
    public static class DebugExtensions
    {
        /// <summary>
        /// Gets the edges of the given vertex and returns a geojson description of them.
        /// </summary>
        public static string GetEdgesAsGeoJson(this DirectedDynamicGraph graph, RouterDb routerDb, uint vertex)
        {
            var edgeEnumerator = graph.GetEdgeEnumerator(vertex);
            var originalSources = new HashSet<OriginalEdge>();
            var originalTargets = new HashSet<OriginalEdge>();
            var vertices = new HashSet<uint>();
            var contractedEdges = new HashSet<Tuple<OriginalEdge, OriginalEdge>>();
            vertices.Add(vertex);

            var writer = new StringWriter();
            var jsonWriter = new JsonWriter(writer);
            jsonWriter.WriteOpen();
            jsonWriter.WriteProperty("type", "FeatureCollection", true, false);
            jsonWriter.WritePropertyName("features", false);
            jsonWriter.WriteArrayOpen();
            
            while (edgeEnumerator.MoveNext())
            {
                if (edgeEnumerator.IsOriginal())
                {
                    originalSources.Add(new OriginalEdge(vertex, edgeEnumerator.Neighbour));
                    originalTargets.Add(new OriginalEdge(vertex, edgeEnumerator.Neighbour));
                }
                else
                {
                    var s1 = edgeEnumerator.GetSequence1();
                    var s2 = edgeEnumerator.GetSequence2();

                    var source = new OriginalEdge(vertex, s1[0]);
                    var target = new OriginalEdge(s2[0], edgeEnumerator.Neighbour);

                    vertices.Add(source.Vertex2);
                    vertices.Add(target.Vertex1);

                    contractedEdges.Add(new Tuple<OriginalEdge, OriginalEdge>(source, target));
                }
                vertices.Add(edgeEnumerator.Neighbour);
            }
            
            foreach (var v in vertices)
            {
                routerDb.WriteVertex(jsonWriter, v);
            }

            foreach(var source in originalSources)
            {
                jsonWriter.WriteOpen();
                jsonWriter.WriteProperty("type", "Feature", true, false);
                jsonWriter.WritePropertyName("geometry", false);

                jsonWriter.WriteOpen();
                jsonWriter.WriteProperty("type", "LineString", true, false);
                jsonWriter.WritePropertyName("coordinates", false);
                jsonWriter.WriteArrayOpen();
                
                var vertex1Coordinate = routerDb.Network.GetVertex(source.Vertex1);
                jsonWriter.WriteArrayOpen();
                jsonWriter.WriteArrayValue(vertex1Coordinate.Longitude.ToInvariantString());
                jsonWriter.WriteArrayValue(vertex1Coordinate.Latitude.ToInvariantString());
                jsonWriter.WriteArrayClose();
                var vertex2Coordinate = routerDb.Network.GetVertex(source.Vertex2);
                jsonWriter.WriteArrayOpen();
                jsonWriter.WriteArrayValue(vertex2Coordinate.Longitude.ToInvariantString());
                jsonWriter.WriteArrayValue(vertex2Coordinate.Latitude.ToInvariantString());
                jsonWriter.WriteArrayClose();

                jsonWriter.WriteArrayClose();
                jsonWriter.WriteClose();

                jsonWriter.WritePropertyName("properties");
                jsonWriter.WriteOpen();
                if (originalTargets.Contains(source))
                {
                    jsonWriter.WriteProperty("type", "original", true);
                }
                else
                {
                    jsonWriter.WriteProperty("type", "source", true);
                }
                jsonWriter.WriteProperty("vertex1", source.Vertex1.ToInvariantString());
                jsonWriter.WriteProperty("vertex2", source.Vertex2.ToInvariantString());
                jsonWriter.WriteClose();

                jsonWriter.WriteClose();
            }
            
            foreach (var source in originalTargets)
            {
                if (!originalSources.Contains(source))
                {
                    jsonWriter.WriteOpen();
                    jsonWriter.WriteProperty("type", "Feature", true, false);
                    jsonWriter.WritePropertyName("geometry", false);

                    jsonWriter.WriteOpen();
                    jsonWriter.WriteProperty("type", "LineString", true, false);
                    jsonWriter.WritePropertyName("coordinates", false);
                    jsonWriter.WriteArrayOpen();

                    var vertex1Coordinate = routerDb.Network.GetVertex(source.Vertex1);
                    jsonWriter.WriteArrayOpen();
                    jsonWriter.WriteArrayValue(vertex1Coordinate.Longitude.ToInvariantString());
                    jsonWriter.WriteArrayValue(vertex1Coordinate.Latitude.ToInvariantString());
                    jsonWriter.WriteArrayClose();
                    var vertex2Coordinate = routerDb.Network.GetVertex(source.Vertex2);
                    jsonWriter.WriteArrayOpen();
                    jsonWriter.WriteArrayValue(vertex2Coordinate.Longitude.ToInvariantString());
                    jsonWriter.WriteArrayValue(vertex2Coordinate.Latitude.ToInvariantString());
                    jsonWriter.WriteArrayClose();

                    jsonWriter.WriteArrayClose();
                    jsonWriter.WriteClose();

                    jsonWriter.WritePropertyName("properties");
                    jsonWriter.WriteOpen();
                    jsonWriter.WriteProperty("type", "target", true);
                    jsonWriter.WriteProperty("vertex1", source.Vertex1.ToInvariantString());
                    jsonWriter.WriteProperty("vertex2", source.Vertex2.ToInvariantString());
                    jsonWriter.WriteClose();

                    jsonWriter.WriteClose();
                }
            }

            foreach (var contracted in contractedEdges)
            {
                jsonWriter.WriteOpen();
                jsonWriter.WriteProperty("type", "Feature", true, false);
                jsonWriter.WritePropertyName("geometry", false);

                jsonWriter.WriteOpen();
                jsonWriter.WriteProperty("type", "LineString", true, false);
                jsonWriter.WritePropertyName("coordinates", false);
                jsonWriter.WriteArrayOpen();

                var vertex1Coordinate = routerDb.Network.GetVertex(contracted.Item1.Vertex2);
                jsonWriter.WriteArrayOpen();
                jsonWriter.WriteArrayValue(vertex1Coordinate.Longitude.ToInvariantString());
                jsonWriter.WriteArrayValue(vertex1Coordinate.Latitude.ToInvariantString());
                jsonWriter.WriteArrayClose();
                var vertex2Coordinate = routerDb.Network.GetVertex(contracted.Item2.Vertex1);
                jsonWriter.WriteArrayOpen();
                jsonWriter.WriteArrayValue(vertex2Coordinate.Longitude.ToInvariantString());
                jsonWriter.WriteArrayValue(vertex2Coordinate.Latitude.ToInvariantString());
                jsonWriter.WriteArrayClose();

                jsonWriter.WriteArrayClose();
                jsonWriter.WriteClose();

                jsonWriter.WritePropertyName("properties");
                jsonWriter.WriteOpen();
                jsonWriter.WriteProperty("type", "contracted", true);
                jsonWriter.WriteProperty("source_vertex1", contracted.Item1.Vertex1.ToInvariantString());
                jsonWriter.WriteProperty("source_vertex2", contracted.Item1.Vertex2.ToInvariantString());
                jsonWriter.WriteProperty("target_vertex1", contracted.Item2.Vertex1.ToInvariantString());
                jsonWriter.WriteProperty("target_vertex2", contracted.Item2.Vertex2.ToInvariantString());
                jsonWriter.WriteClose();

                jsonWriter.WriteClose();
            }

            jsonWriter.WriteArrayClose();
            jsonWriter.WriteClose();

            return writer.ToInvariantString();
        }

        /// <summary>
        /// Gets the entire search space of the given vertex as geojson.
        /// </summary>
        public static string GetSearchSpaceAsGeoJson(this DirectedDynamicGraph graph, RouterDb routerDb, uint vertex, bool forward)
        {
            var weightHandler = new DefaultWeightHandler(null);
            var edgeEnumerator = graph.GetEdgeEnumerator(vertex);
            var heap = new BinaryHeap<Tuple<OriginalEdge, OriginalEdge>>();
            var settled = new HashSet<OriginalEdge>();
            var edges = new List<Tuple<OriginalEdge, OriginalEdge>>();
            heap.Push(new Tuple<OriginalEdge, OriginalEdge>(new OriginalEdge(Constants.NO_VERTEX, Constants.NO_VERTEX),
                new OriginalEdge(Constants.NO_VERTEX, vertex)), 0);

            var writer = new StringWriter();
            var jsonWriter = new JsonWriter(writer);
            jsonWriter.WriteOpen();
            jsonWriter.WriteProperty("type", "FeatureCollection", true, false);
            jsonWriter.WritePropertyName("features", false);
            jsonWriter.WriteArrayOpen();

            while (heap.Count > 0)
            {
                float currentWeight;
                var current = heap.Pop(out currentWeight);
                if (settled.Contains(current.Item2))
                {
                    continue;
                }
                settled.Add(current.Item2);

                edges.Add(current);

                edgeEnumerator.MoveTo(current.Item2.Vertex2);
                while(edgeEnumerator.MoveNext())
                {
                    var weight = weightHandler.GetEdgeWeight(edgeEnumerator);
                    if ((forward && weight.Direction.F) ||
                        (!forward && weight.Direction.B))
                    {
                        OriginalEdge neighbour;
                        OriginalEdge startEdge;
                        if(edgeEnumerator.IsOriginal())
                        {
                            neighbour = new OriginalEdge(current.Item2.Vertex2, edgeEnumerator.Neighbour);
                            startEdge = neighbour;
                        }
                        else
                        {
                            var s1 = edgeEnumerator.GetSequence1();
                            var s2 = edgeEnumerator.GetSequence2();
                            neighbour = new OriginalEdge(s2[0], edgeEnumerator.Neighbour);
                            startEdge = new OriginalEdge(current.Item1.Vertex2, s1[0]);
                        }
                        heap.Push(new Tuple<OriginalEdge, OriginalEdge>(startEdge, neighbour), currentWeight + weight.Weight);
                    }
                }
            }

            foreach (var contracted in edges)
            {
                if (contracted.Item1.Vertex1 != Constants.NO_VERTEX)
                {
                    jsonWriter.WriteOpen();
                    jsonWriter.WriteProperty("type", "Feature", true, false);
                    jsonWriter.WritePropertyName("geometry", false);

                    jsonWriter.WriteOpen();
                    jsonWriter.WriteProperty("type", "LineString", true, false);
                    jsonWriter.WritePropertyName("coordinates", false);
                    jsonWriter.WriteArrayOpen();

                    var coordinate = routerDb.Network.GetVertex(contracted.Item1.Vertex1);
                    jsonWriter.WriteArrayOpen();
                    jsonWriter.WriteArrayValue(coordinate.Longitude.ToInvariantString());
                    jsonWriter.WriteArrayValue(coordinate.Latitude.ToInvariantString());
                    jsonWriter.WriteArrayClose();
                    coordinate = routerDb.Network.GetVertex(contracted.Item1.Vertex2);
                    jsonWriter.WriteArrayOpen();
                    jsonWriter.WriteArrayValue(coordinate.Longitude.ToInvariantString());
                    jsonWriter.WriteArrayValue(coordinate.Latitude.ToInvariantString());
                    jsonWriter.WriteArrayClose();
                    coordinate = routerDb.Network.GetVertex(contracted.Item2.Vertex1);
                    jsonWriter.WriteArrayOpen();
                    jsonWriter.WriteArrayValue(coordinate.Longitude.ToInvariantString());
                    jsonWriter.WriteArrayValue(coordinate.Latitude.ToInvariantString());
                    jsonWriter.WriteArrayClose();
                    coordinate = routerDb.Network.GetVertex(contracted.Item2.Vertex2);
                    jsonWriter.WriteArrayOpen();
                    jsonWriter.WriteArrayValue(coordinate.Longitude.ToInvariantString());
                    jsonWriter.WriteArrayValue(coordinate.Latitude.ToInvariantString());
                    jsonWriter.WriteArrayClose();

                    jsonWriter.WriteArrayClose();
                    jsonWriter.WriteClose();

                    jsonWriter.WritePropertyName("properties");
                    jsonWriter.WriteOpen();
                    jsonWriter.WriteProperty("type", "target", true);
                    jsonWriter.WriteProperty("source_vertex1", contracted.Item1.Vertex1.ToInvariantString());
                    jsonWriter.WriteProperty("source_vertex2", contracted.Item1.Vertex2.ToInvariantString());
                    jsonWriter.WriteProperty("target_vertex1", contracted.Item2.Vertex1.ToInvariantString());
                    jsonWriter.WriteProperty("target_vertex2", contracted.Item2.Vertex2.ToInvariantString());
                    jsonWriter.WriteClose();

                    jsonWriter.WriteClose();
                }
            }

            jsonWriter.WriteArrayClose();
            jsonWriter.WriteClose();

            return writer.ToInvariantString();
        }
    }
#endif
}
