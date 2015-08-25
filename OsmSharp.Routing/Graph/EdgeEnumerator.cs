// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2014 Abelshausen Ben
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

using OsmSharp.Collections.Coordinates.Collections;
using System.Collections.Generic;

namespace OsmSharp.Routing.Graph
{
    /// <summary>
    /// Represents an abstract edge enumerator, enumerable and edge.
    /// </summary>
    public abstract class EdgeEnumerator<TEdgeData> : IEnumerable<Edge<TEdgeData>>, IEnumerator<Edge<TEdgeData>>
        where TEdgeData : IGraphEdgeData
    {
        /// <summary>
        /// Moves this enumerator to the given vertex.
        /// </summary>
        /// <param name="vertex">The vertex to enumerate edges for.</param>
        public abstract void MoveTo(uint vertex);

        /// <summary>
        /// Moves this enumerator to the given vertex1 and enumerate only edges that lead to vertex2.
        /// </summary>
        /// <param name="vertex1">The vertex to enumerate edges for.</param>
        /// <param name="vertex2">The neighbour.</param>
        public abstract void MoveTo(uint vertex1, uint vertex2);

        /// <summary>
        /// Returns the current neighbour.
        /// </summary>
        public abstract uint Neighbour
        {
            get;
        }

        /// <summary>
        /// Returns the edge data.
        /// </summary>
        public abstract TEdgeData EdgeData
        {
            get;
        }

        /// <summary>
        /// The edge data is inverted by default.
        /// </summary>
        public abstract bool isInverted
        {
            get;
        }

        public abstract TEdgeData InvertedEdgeData
        {
            get;
        }

        /// <summary>
        /// Returns the intermediates.
        /// </summary>
        public abstract ICoordinateCollection Intermediates
        {
            get;
        }

        /// <summary>
        /// Returns true if the count is known without enumeration.
        /// </summary>
        public abstract bool HasCount
        {
            get;
        }

        /// <summary>
        /// Returns the count if known.
        /// </summary>
        public abstract int Count
        {
            get;
        }

        /// <summary>
        /// Returns the enumerator.
        /// </summary>
        /// <returns></returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            this.Reset();
            return this;
        }

        public abstract IEnumerator<Edge<TEdgeData>> GetEnumerator();

        public abstract Edge<TEdgeData> Current
        {
            get;
        }


        public abstract bool MoveNext();

        public abstract void Reset();

        public abstract void Dispose();

        IEnumerator<Edge<TEdgeData>> IEnumerable<Edge<TEdgeData>>.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        object System.Collections.IEnumerator.Current
        {
            get { return this.Current; }
        }
    }

    /// <summary>
    /// Abstract representation of an edge.
    /// </summary>
    /// <typeparam name="TEdgeData"></typeparam>
    public class Edge<TEdgeData>
        where TEdgeData : IGraphEdgeData
    {
        /// <summary>
        /// Creates a new edge.
        /// </summary>
        public Edge()
        {

        }

        /// <summary>
        /// Creates a new edge by copying the given edge.
        /// </summary>
        /// <param name="neighbour"></param>
        /// <param name="edgeData"></param>
        /// <param name="intermediates"></param>
        public Edge(uint neighbour, TEdgeData edgeData, ICoordinateCollection intermediates)
        {
            this.Neighbour = neighbour;
            this.EdgeData = edgeData;
            this.Intermediates = intermediates;
        }

        /// <summary>
        /// Creates a new edge by copying the given edge.
        /// </summary>
        /// <param name="edge"></param>
        public Edge(Edge<TEdgeData> edge)
        {
            this.Neighbour = edge.Neighbour;
            this.EdgeData = edge.EdgeData;
            this.Intermediates = edge.Intermediates;
        }

        /// <summary>
        /// Creates a new edge keeping the current state of the given enumerator.
        /// </summary>
        /// <param name="enumerator"></param>
        public Edge(EdgeEnumerator<TEdgeData> enumerator)
        {
            this.Neighbour = enumerator.Neighbour;
            this.EdgeData = enumerator.EdgeData;
            this.Intermediates = enumerator.Intermediates;
        }

        /// <summary>
        /// Returns the current neighbour.
        /// </summary>
        public uint Neighbour
        {
            get;
            set;
        }

        /// <summary>
        /// Returns the edge data.
        /// </summary>
        public TEdgeData EdgeData
        {
            get;
            set;
        }

        /// <summary>
        /// Returns the intermediates.
        /// </summary>
        public ICoordinateCollection Intermediates
        {
            get;
            set;
        }

        /// <summary>
        /// Returns a string representing this edge.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("{0} - {1}",
                this.Neighbour,
                this.EdgeData.ToInvariantString());
        }
    }

    /// <summary>
    /// Holds extensions methods for the edge enumerator.
    /// </summary>
    public static class EdgeEnumeratorExtensions
    {
        /// <summary>
        /// Converts the given edge enumerator to an array of key-value pairs.
        /// </summary>
        /// <typeparam name="TEdgeData"></typeparam>
        /// <param name="enumerator"></param>
        /// <returns></returns>
        public static KeyValuePair<uint, TEdgeData>[] ToKeyValuePairs<TEdgeData>(this EdgeEnumerator<TEdgeData> enumerator)
            where TEdgeData : IGraphEdgeData
        {
            enumerator.Reset();
            var pairs = new List<KeyValuePair<uint, TEdgeData>>();
            while (enumerator.MoveNext())
            {
                pairs.Add(new KeyValuePair<uint, TEdgeData>(enumerator.Neighbour, enumerator.EdgeData));
            }
            return pairs.ToArray();
        }


        /// <summary>
        /// Converts the given edge enumerator to an array of key-value pairs.
        /// </summary>
        /// <typeparam name="TEdgeData"></typeparam>
        /// <param name="enumerator"></param>
        /// <returns></returns>
        public static KeyValuePair<uint, KeyValuePair<TEdgeData, ICoordinateCollection>>[] ToKeyValuePairsAndShapes<TEdgeData>(this EdgeEnumerator<TEdgeData> enumerator)
            where TEdgeData : IGraphEdgeData
        {
            enumerator.Reset();
            var pairs = new List<KeyValuePair<uint, KeyValuePair<TEdgeData, ICoordinateCollection>>>();
            while (enumerator.MoveNext())
            {
                pairs.Add(new KeyValuePair<uint, KeyValuePair<TEdgeData, ICoordinateCollection>>(enumerator.Neighbour, 
                    new KeyValuePair<TEdgeData, ICoordinateCollection>(enumerator.EdgeData, enumerator.Intermediates)));
            }
            return pairs.ToArray();
        }
    }
}