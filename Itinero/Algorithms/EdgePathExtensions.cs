// Itinero - OpenStreetMap (OSM) SDK
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
        public static EdgePath Append(this EdgePath path, EdgePath reversePath)
        {
            if (path.Vertex != reversePath.Vertex)
            {
                throw new System.Exception("Cannot append path that ends with a different vertex.");
            }

            while(reversePath.From != null)
            {
                var localWeight = reversePath.Weight - reversePath.From.Weight;
                path = new EdgePath(reversePath.From.Vertex, path.Weight + localWeight, -reversePath.Edge, path);
                reversePath = reversePath.From;
            }
            return path;
        }

        /// <summary>
        /// Returns true if this path contains the given vertex.
        /// </summary>
        public static bool HasVertex(this EdgePath path, uint vertex)
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
        public static void AddToListReverse(this EdgePath path, List<uint> vertices)
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
        public static void AddToList(this EdgePath path, List<uint> vertices)
        {
            var reversed = new List<uint>();
            path.AddToListReverse(reversed);
            for (var i = reversed.Count - 1; i >= 0; i--)
            {
                vertices.Add(reversed[i]);
            }
        }

        /// <summary>
        /// Strips all edge-id's.
        /// </summary>
        public static void StripEdges(this EdgePath path)
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
    }
}