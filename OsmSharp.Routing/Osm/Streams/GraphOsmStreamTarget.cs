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
using OsmSharp.Collections.Coordinates;
using OsmSharp.Collections.Tags;
using OsmSharp.Collections.Tags.Index;
using OsmSharp.Math.Geo;
using OsmSharp.Math.Geo.Simple;
using OsmSharp.Osm.Streams;
using OsmSharp.Routing.Graph;
using OsmSharp.Routing.Graph.PreProcessor;
using OsmSharp.Routing.Interpreter.Roads;
using OsmSharp.Routing.Osm.Interpreter;
using OsmSharp.Routing.Vehicles;
using System;
using System.Collections.Generic;

namespace OsmSharp.Routing.Osm.Streams
{
    /// <summary>
    /// A data processing target containing edges with the orignal OSM-tags and their original OSM-direction.
    /// </summary>
    public class GraphOsmStreamTarget : GraphOsmStreamTargetBase<Edge>
    {
        /// <summary>
        /// Holds the list of vehicle profiles to build routing information for.
        /// </summary>
        private HashSet<Vehicle> _vehicles;

        /// <summary>
        /// Creates a new osm edge data processing target.
        /// </summary>
        public GraphOsmStreamTarget(RouterDataSourceBase<Edge> graph,
            IOsmRoutingInterpreter interpreter, ITagsIndex tagsIndex)
            : this(graph, interpreter, tagsIndex, null, true, new CoordinateIndex(), (g) =>
            {
                return new HilbertSortingPreprocessor<Edge>(g);
            })
        {

        }

        /// <summary>
        /// Creates a new osm edge data processing target.
        /// </summary>
        public GraphOsmStreamTarget(RouterDataSourceBase<Edge> graph,
            IOsmRoutingInterpreter interpreter, ITagsIndex tagsIndex, Func<GraphBase<Edge>, IPreprocessor> createPreprocessor)
            : this(graph, interpreter, tagsIndex, null, true, new CoordinateIndex(), createPreprocessor)
        {

        }

        /// <summary>
        /// Creates a new osm edge data processing target.
        /// </summary>
        public GraphOsmStreamTarget(RouterDataSourceBase<Edge> graph,
            IOsmRoutingInterpreter interpreter, ITagsIndex tagsIndex, ICoordinateIndex coordinates)
            : this(graph, interpreter, tagsIndex, null, true, coordinates, (g) =>
            {
                return new HilbertSortingPreprocessor<Edge>(g);
            })
        {

        }

        /// <summary>
        /// Creates a new osm edge data processing target.
        /// </summary>
        /// <param name="graph">The graph.</param>
        /// <param name="interpreter">Inteprets the OSM-data.</param>
        /// <param name="tagsIndex">Holds all the tags.</param>
        /// <param name="vehicles">The vehicle profiles to build routing information for.</param>
        public GraphOsmStreamTarget(RouterDataSourceBase<Edge> graph,
            IOsmRoutingInterpreter interpreter, ITagsIndex tagsIndex,
            IEnumerable<Vehicle> vehicles)
            : this(graph, interpreter, tagsIndex, vehicles, true, new CoordinateIndex(), (g) =>
            {
                return new HilbertSortingPreprocessor<Edge>(g);
            })
        {

        }

        /// <summary>
        /// Creates a new osm edge data processing target.
        /// </summary>
        public GraphOsmStreamTarget(RouterDataSourceBase<Edge> graph,
            IOsmRoutingInterpreter interpreter, ITagsIndex tagsIndex,
            IEnumerable<Vehicle> vehicles, bool collectIntermediates)
            : this(graph, interpreter, tagsIndex, vehicles, collectIntermediates, new CoordinateIndex(), (g) =>
            {
                return new HilbertSortingPreprocessor<Edge>(g);
            })
        {

        }

        /// <summary>
        /// Creates a new osm edge data processing target.
        /// </summary>
        public GraphOsmStreamTarget(RouterDataSourceBase<Edge> graph,
            IOsmRoutingInterpreter interpreter, ITagsIndex tagsIndex,
            IEnumerable<Vehicle> vehicles, bool collectIntermediates, ICoordinateIndex coordinates, Func<GraphBase<Edge>, IPreprocessor> createPreprocessor)
            : base(graph, interpreter, tagsIndex, createPreprocessor, new HugeDictionary<long, uint>(), collectIntermediates, coordinates)
        {
            _vehicles = new HashSet<Vehicle>();
            if (vehicles != null)
            {
                foreach (Vehicle vehicle in vehicles)
                {
                    _vehicles.Add(vehicle);
                }
            }
        }

        /// <summary>
        /// Calculates edge data.
        /// </summary>
        /// <param name="tagsIndex"></param>
        /// <param name="tags"></param>
        /// <param name="tagsForward"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="edgeInterpreter"></param>
        /// <param name="intermediates"></param>
        /// <returns></returns>
        protected override Edge CalculateEdgeData(IEdgeInterpreter edgeInterpreter, ITagsIndex tagsIndex,
            TagsCollectionBase tags, bool tagsForward, GeoCoordinate from, GeoCoordinate to, List<GeoCoordinateSimple> intermediates)
        {
            if (edgeInterpreter == null) throw new ArgumentNullException("edgeInterpreter");
            if (tagsIndex == null) throw new ArgumentNullException("tagsIndex");
            if (tags == null) throw new ArgumentNullException("tags");
            if (from == null) throw new ArgumentNullException("from");
            if (to == null) throw new ArgumentNullException("to");

            uint tagsId = tagsIndex.Add(tags);

            var shapeInBox = true;
            if (intermediates != null)
            { // verify shape-in-box.
                var box = new GeoCoordinateBox(from, to);
                for (int idx = 0; idx < intermediates.Count; idx++)
                {
                    if (!box.Contains(intermediates[idx].Longitude, intermediates[idx].Latitude))
                    { // shape not in box.
                        shapeInBox = false;
                        break;
                    }
                }
            }

            return new Edge()
            {
                Forward = tagsForward,
                Tags = tagsId,
                Distance = (float)from.DistanceEstimate(to).Value,
                ShapeInBox = shapeInBox
            };
        }

        /// <summary>
        /// Returns true if the edge is traversable.
        /// </summary>
        /// <param name="edgeInterpreter"></param>
        /// <param name="tagsIndex"></param>
        /// <param name="tags"></param>
        /// <returns></returns>
        protected override bool CalculateIsTraversable(IEdgeInterpreter edgeInterpreter,
            ITagsIndex tagsIndex, TagsCollectionBase tags)
        {
            if (_vehicles.Count > 0)
            { // limit only to vehicles in this list.
                foreach (var vehicle in _vehicles)
                {
                    if (vehicle.CanTraverse(tags))
                    { // one of them is enough.
                        return true;
                    }
                }
                return false;
            }
            return edgeInterpreter.IsRoutable(tags);
        }

        #region Static Processing Functions

        /// <summary>
        /// Preprocesses the data from the given OsmStreamReader and converts it directly to a routable data source.
        /// </summary>
        /// <param name="source">The source stream.</param>
        /// <param name="tagsIndex">The tags index.</param>
        /// <param name="interpreter">The routing interpreter.</param>
        /// <returns></returns>
        public static RouterDataSource<Edge> Preprocess(OsmStreamSource source, ITagsIndex tagsIndex, IOsmRoutingInterpreter interpreter)
        {
            var routerDataSource =
                new RouterDataSource<Edge>(new Graph<Edge>(), tagsIndex);
            var targetData = new GraphOsmStreamTarget(routerDataSource, interpreter,
                tagsIndex);
            targetData.RegisterSource(source);
            targetData.Pull();

            return routerDataSource;
        }

        /// <summary>
        /// Preprocesses the data from the given OsmStreamReader and converts it directly to a routable data source.
        /// </summary>
        /// <param name="source">The source stream.</param>
        /// <param name="interpreter">The routing interpreter.</param>
        /// <returns></returns>
        public static RouterDataSource<Edge> Preprocess(OsmStreamSource source, IOsmRoutingInterpreter interpreter)
        {
            return GraphOsmStreamTarget.Preprocess(source, new TagsIndex(), interpreter);
        }

        #endregion
    }
}
