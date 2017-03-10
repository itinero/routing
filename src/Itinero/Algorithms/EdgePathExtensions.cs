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
using System.Collections.Generic;

namespace Itinero.Algorithms
{
    /// <summary>
    /// Contains extension methods from the edge path.
    /// </summary>
    public static class EdgePathExtensions
    {        
        /// <summary>
        /// Appends the given path in reverse to the edge path.
        /// </summary>
        public static EdgePath<float> Append(this EdgePath<float> path, EdgePath<float> reversePath)
        {
            if (path.Vertex != reversePath.Vertex)
            {
                throw new System.Exception("Cannot append path that ends with a different vertex.");
            }

            while(reversePath.From != null)
            {
                var localWeight = reversePath.Weight - reversePath.From.Weight;
                path = new EdgePath<float>(reversePath.From.Vertex, path.Weight + localWeight, -reversePath.Edge, path);
                reversePath = reversePath.From;
            }
            return path;
        }

        /// <summary>
        /// Appends the given path in reverse to the edge path.
        /// </summary>
        public static EdgePath<T> Append<T>(this EdgePath<T> path, EdgePath<T> reversePath, WeightHandler<T> weightHandler)
            where T : struct
        {
            if (path.Vertex != reversePath.Vertex)
            {
                throw new System.Exception("Cannot append path that ends with a different vertex.");
            }

            while (reversePath.From != null)
            {
                var localWeight = weightHandler.Subtract(reversePath.Weight, reversePath.From.Weight);
                path = new EdgePath<T>(reversePath.From.Vertex, weightHandler.Add(path.Weight, localWeight), -reversePath.Edge, path);
                reversePath = reversePath.From;
            }
            return path;
        }

        /// <summary>
        /// Returns true if this path contains the given vertex.
        /// </summary>
        public static bool HasVertex<T>(this EdgePath<T> path, uint vertex)
        {
            while(path != null)
            {
                if (path.Vertex == vertex)
                {
                    return true;
                }
                path = path.From;
            }
            return false;
        }
        
        /// <summary>
        /// Adds the vertices in this path to the given list.
        /// </summary>
        public static void AddToListReverse<T>(this EdgePath<T> path, List<EdgePath<T>> pathsList)
        {
            while (path != null)
            {
                pathsList.Add(path);
                path = path.From;
            }
        }

        /// <summary>
        /// Adds the vertices in this path to the given list.
        /// </summary>
        public static void AddToListReverseAsVertices<T>(this EdgePath<T> path, List<uint> vertices)
        {
            while (path != null)
            {
                vertices.Add(path.Vertex);
                path = path.From;
            }
        }

        /// <summary>
        /// Adds the vertices in this path to the given list.
        /// </summary>
        public static void AddToList<T>(this EdgePath<T> path, List<EdgePath<T>> pathList)
        {
            var reversed = new List<EdgePath<T>>();
            path.AddToListReverse(reversed);
            for (var i = reversed.Count - 1; i >= 0; i--)
            {
                pathList.Add(reversed[i]);
            }
        }

        /// <summary>
        /// Adds the vertices in this path to the given list.
        /// </summary>
        public static void AddToListAsVertices<T>(this EdgePath<T> path, List<uint> vertices)
        {
            var reversed = new List<uint>();
            path.AddToListReverseAsVertices(reversed);
            for (var i = reversed.Count - 1; i >= 0; i--)
            {
                vertices.Add(reversed[i]);
            }
        }

        /// <summary>
        /// Strips all edge-id's.
        /// </summary>
        public static void StripEdges<T>(this EdgePath<T> path)
        {
            while(path != null)
            {
                if (path.Edge != Constants.NO_EDGE)
                {
                    path.Edge = Constants.NO_EDGE;
                }
                path = path.From;
            }
        }

        /// <summary>
        /// Strips the source vertex.
        /// </summary>
        public static void StripSource<T>(this EdgePath<T> path)
        {
            while(path.From != null)
            {
                path = path.From;
            }
            path.Vertex = Constants.NO_VERTEX;
        }

        /// <summary>
        /// Strips the target vertex.
        /// </summary>
        public static void StripTarget<T>(this EdgePath<T> path)
        {
            path.Vertex = Constants.NO_VERTEX;
        }

        /// <summary>
        /// Converts the path to a list.
        /// </summary>
        public static List<EdgePath<T>> ToList<T>(this EdgePath<T> path)
        {
            var pathAsList = new List<EdgePath<T>>();
            path.AddToList(pathAsList);
            return pathAsList;
        }

        /// <summary>
        /// Converts the path to a list of vertices.
        /// </summary>
        public static List<uint> ToListAsVertices<T>(this EdgePath<T> path)
        {
            var pathAsList = new List<uint> ();
            path.AddToListAsVertices(pathAsList);
            return pathAsList;
        }
    }
}