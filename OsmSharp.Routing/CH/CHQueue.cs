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
using OsmSharp.Routing.Graph.Routing;

namespace OsmSharp.Routing.CH.Routing
{
    /// <summary>
    /// A CH queue.
    /// </summary>
    public class CHQueue
    {
        /// <summary>
        /// The forward queue.
        /// </summary>
        private Dictionary<long, PathSegment<long>> _forward;

        /// <summary>
        /// The backward queue.
        /// </summary>
        private Dictionary<long, PathSegment<long>> _backward;

        /// <summary>
        /// The backward-forward intersection.
        /// </summary>
        private Dictionary<long, double> _intersection;

        /// <summary>
        /// Creates a new CH queue.
        /// </summary>
        public CHQueue()
        {
            _intersection = new Dictionary<long, double>();

            _forward = new Dictionary<long, PathSegment<long>>();
            _backward = new Dictionary<long, PathSegment<long>>();
        }

        /// <summary>
        /// Returns the intersection.
        /// </summary>
        public Dictionary<long, double> Intersection
        {
            get
            {
                return _intersection;
            }
        }

        /// <summary>
        /// Returns the forward queue.
        /// </summary>
        public Dictionary<long, PathSegment<long>> Forward
        {
            get
            {
                return _forward;
            }
        }

        /// <summary>
        /// Returns the backward queue.
        /// </summary>
        public Dictionary<long, PathSegment<long>> Backward
        {
            get
            {
                return _backward;
            }
        }

        /// <summary>
        /// Adds a path segment to the forward queue.
        /// </summary>
        /// <param name="segment"></param>
        public void AddForward(PathSegment<long> segment)
        {
            _forward[segment.VertexId] = segment;

            PathSegment<long> backward;
            if (_backward.TryGetValue(segment.VertexId, out backward))
            {
                _intersection[segment.VertexId] = backward.Weight + segment.Weight;
            }
        }

        /// <summary>
        /// Adds a path segment to the backward queue.
        /// </summary>
        /// <param name="segment"></param>
        public void AddBackward(PathSegment<long> segment)
        {
            _backward[segment.VertexId] = segment;

            PathSegment<long> forward;
            if (_forward.TryGetValue(segment.VertexId, out forward))
            {
                _intersection[segment.VertexId] = forward.Weight + segment.Weight;
            }
        }
    }
}
