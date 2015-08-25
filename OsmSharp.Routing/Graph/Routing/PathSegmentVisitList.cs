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

using OsmSharp.Collections;
using System.Collections.Generic;
using System.Linq;

namespace OsmSharp.Routing.Graph.Routing
{
    /// <summary>
    /// Internal data structure reprenting a visit list,
    /// </summary>
    public class PathSegmentVisitList : ICloneable
    {
        /// <summary>
        /// Holds all visited nodes sorted by weight.
        /// </summary>
        private SortedList<double, Dictionary<long, PathSegment<long>>> _visit_list;

        /// <summary>
        /// Holds all visited vertices.
        /// </summary>
        private Dictionary<long, double> _visited;

        /// <summary>
        /// Creates a new visit list.
        /// </summary>
        public PathSegmentVisitList()
        {
            _visit_list = new SortedList<double, Dictionary<long, PathSegment<long>>>();
            _visited = new Dictionary<long, double>();

            this.Neighbour1 = -1;
            this.Neighbour2 = -1;
        }

        /// <summary>
        /// Creates a new visit list.
        /// </summary>
        public PathSegmentVisitList(PathSegment<long> route)
            : this()
        {
            this.UpdateVertex(route);
        }

        /// <summary>
        /// Creates a new visit list with just one vertex.
        /// </summary>
        /// <param name="vertex"></param>
        public PathSegmentVisitList(long vertex)
            : this(new PathSegment<long>(vertex))
        {

        }

        /// <summary>
        /// Creates a new visit list.
        /// </summary>
        /// <param name="neighbour1"></param>
        /// <param name="neighbour2"></param>
        public PathSegmentVisitList(long neighbour1, long neighbour2)
        {
            _visit_list = new SortedList<double, Dictionary<long, PathSegment<long>>>();
            _visited = new Dictionary<long, double>();

            this.Neighbour1 = neighbour1;
            this.Neighbour2 = neighbour2;
        }

        /// <summary>
        /// Creates a new visit list by copying an existing visit list.
        /// </summary>
        /// <param name="source"></param>
        public PathSegmentVisitList(PathSegmentVisitList source)
        {
            _visit_list = new SortedList<double, Dictionary<long, PathSegment<long>>>();
            _visited = new Dictionary<long, double>();

            foreach (KeyValuePair<double, Dictionary<long, PathSegment<long>>> pair in source._visit_list)
            {
                Dictionary<long, PathSegment<long>> dic = new Dictionary<long, PathSegment<long>>();
                foreach (KeyValuePair<long, PathSegment<long>> path_pair in pair.Value)
                {
                    dic.Add(path_pair.Key, path_pair.Value);
                }
                _visit_list.Add(pair.Key, dic);
            }

            foreach (KeyValuePair<long, double> pair in source._visited)
            {
                _visited.Add(pair.Key, pair.Value);
            }

            this.Neighbour1 = source.Neighbour1;
            this.Neighbour2 = source.Neighbour2;
        }

        /// <summary>
        /// Updates a vertex in this visit list.
        /// </summary>
        /// <param name="route"></param>
        public void UpdateVertex(PathSegment<long> route)
        {
            double current_weight;
            if (_visited.TryGetValue(route.VertexId, out current_weight))
            { // the vertex was already in this list.
                if (current_weight > route.Weight)
                { // replace the existing.
                    Dictionary<long, PathSegment<long>> current_weight_vertices = _visit_list[current_weight];
                    current_weight_vertices.Remove(route.VertexId);
                    if (current_weight_vertices.Count == 0)
                    {
                        _visit_list.Remove(current_weight);
                    }
                }
                else
                { // do nothing, the existing weight is better.
                    return;
                }
            }

            // add/update everthing.
            Dictionary<long, PathSegment<long>> vertices_at_weight;
            if (!_visit_list.TryGetValue(route.Weight, out vertices_at_weight))
            {
                vertices_at_weight = new Dictionary<long, PathSegment<long>>();
                _visit_list.Add(route.Weight, vertices_at_weight);
            }
            vertices_at_weight.Add(route.VertexId, route);
            _visited[route.VertexId] = route.Weight;
        }

        /// <summary>
        /// Returns the vertex with the lowest weight and removes it.
        /// </summary>
        /// <returns></returns>
        public PathSegment<long> GetFirst()
        {
            if (_visit_list.Count > 0)
            {
                double weight = _visit_list.Keys[0];
                Dictionary<long, PathSegment<long>> first_set = _visit_list[weight];
                KeyValuePair<long, PathSegment<long>> first_pair =
                    first_set.First<KeyValuePair<long, PathSegment<long>>>();
                long vertex_id = first_pair.Key;

                // remove the vertex.
                first_set.Remove(vertex_id);
                if (first_set.Count == 0)
                {
                    _visit_list.Remove(weight);
                }
                _visited.Remove(vertex_id);

                return first_pair.Value;
            }
            return null;
        }

        /// <summary>
        /// Returns the vertex with the lowest weight.
        /// </summary>
        /// <returns></returns>
        public PathSegment<long> PeekFirst()
        {
            if (_visit_list.Count > 0)
            {
                double weight = _visit_list.Keys[0];
                Dictionary<long, PathSegment<long>> first_set = _visit_list[weight];
                KeyValuePair<long, PathSegment<long>> first_pair =
                    first_set.First<KeyValuePair<long, PathSegment<long>>>();
                return first_pair.Value;
            }
            return null;
        }

        /// <summary>
        /// Returns true if the vertex is in this visit list.
        /// </summary>
        /// <param name="vertex"></param>
        /// <returns></returns>
        public bool Contains(long vertex)
        {
            return _visited.ContainsKey(vertex);
        }

        /// <summary>
        /// Returns the path to the given vertex.
        /// </summary>
        /// <param name="vertex"></param>
        /// <returns></returns>
        public PathSegment<long> GetPathTo(long vertex)
        {
            double weight = _visited[vertex];
            Dictionary<long, PathSegment<long>> paths_at_weight =
                _visit_list[weight];
            return paths_at_weight[vertex];
        }

        /// <summary>
        /// Removes the path to the given vertex.
        /// </summary>
        /// <param name="vertex"></param>
        /// <returns></returns>
        public void Remove(long vertex)
        {
            double weight = _visited[vertex];
            _visited.Remove(vertex);
            Dictionary<long, PathSegment<long>> paths_at_weight =
                _visit_list[weight];
            paths_at_weight.Remove(vertex);
            if (paths_at_weight.Count == 0)
            {
                _visit_list.Remove(weight);
            }
        }

        /// <summary>
        /// Returns the element count in this list.
        /// </summary>
        public int Count
        {
            get
            {
                return _visited.Count;
            }
        }

        /// <summary>
        /// Gets/sets the Neighbour1.
        /// </summary>
        public long Neighbour1
        {
            get;
            set;
        }

        /// <summary>
        /// Gets/sets the Neighbour2.
        /// </summary>
        public long Neighbour2
        {
            get;
            set;
        }

        /// <summary>
        /// Returns a collection of vertices.
        /// </summary>
        /// <returns></returns>
        public IEnumerable<long> GetVertices()
        {
            return _visited.Keys;
        }

        /// <summary>
        /// Creates a copy of this path segment visit list.
        /// </summary>
        /// <returns></returns>
        public object Clone()
        {
            return new PathSegmentVisitList(this);
        }
    }
}
