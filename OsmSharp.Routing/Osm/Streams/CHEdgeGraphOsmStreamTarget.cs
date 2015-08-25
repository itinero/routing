// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2015 Abelshausen Ben
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
using OsmSharp.Routing.CH;
using OsmSharp.Routing.CH.Preprocessing;
using OsmSharp.Routing.CH.Preprocessing.Ordering;
using OsmSharp.Routing.CH.Preprocessing.Witnesses;
using OsmSharp.Routing.Graph;
using OsmSharp.Routing.Graph.PreProcessor;
using OsmSharp.Routing.Graph.Routing;
using OsmSharp.Routing.Interpreter.Roads;
using OsmSharp.Routing.Osm.Interpreter;
using OsmSharp.Routing.Vehicles;
using System;
using System.Collections.Generic;

namespace OsmSharp.Routing.Osm.Streams
{
    /// <summary>
    /// A pre-processing target for OSM-data to a CH data structure.
    /// </summary>
    public class CHEdgeGraphOsmStreamTarget : GraphOsmStreamTargetBase<CHEdgeData>
    {
        /// <summary>
        /// Holds the vehicle profile this pre-processing target is for.
        /// </summary>
        private readonly Vehicle _vehicle;
        
        /// <summary>
        /// Creates a CH data processing target.
        /// </summary>
        public CHEdgeGraphOsmStreamTarget(RouterDataSourceBase<CHEdgeData> graph,
            IOsmRoutingInterpreter interpreter, ITagsIndex tagsIndex, Vehicle vehicle)
            : this(graph, interpreter, tagsIndex, vehicle, (g) =>
            {
                return new DefaultPreprocessor(g);
            })
        {

        }

        /// <summary>
        /// Creates a CH data processing target.
        /// </summary>
        public CHEdgeGraphOsmStreamTarget(RouterDataSourceBase<CHEdgeData> graph,
            IOsmRoutingInterpreter interpreter, ITagsIndex tagsIndex, Vehicle vehicle, Func<GraphBase<CHEdgeData>, IPreprocessor> createPreprocessor)
            : base(graph, interpreter, tagsIndex, createPreprocessor)
        {
            if (!graph.IsDirected) { throw new ArgumentOutOfRangeException("Only directed graphs can be used for contraction hiearchies."); }

            _vehicle = vehicle;
        }
        
        /// <summary>
        /// Initializes the processing.
        /// </summary>
        public override void Initialize()
        {
            base.Initialize();

            base.Graph.AddSupportedProfile(_vehicle);
        }

        /// <summary>
        /// Calculates edge data.
        /// </summary>
        /// <param name="edgeInterpreter"></param>
        /// <param name="tagsIndex"></param>
        /// <param name="tags"></param>
        /// <param name="tagsForward"></param>
        /// <param name="from"></param>
        /// <param name="to"></param>
        /// <param name="intermediates"></param>
        /// <returns></returns>
        protected override CHEdgeData CalculateEdgeData(IEdgeInterpreter edgeInterpreter, ITagsIndex tagsIndex,
            TagsCollectionBase tags, bool tagsForward, GeoCoordinate from, GeoCoordinate to, List<GeoCoordinateSimple> intermediates)
        {
            var direction = _vehicle.IsOneWay(tags);
            var forward = false;
            var backward = false;
            if (!direction.HasValue)
            { // both directions.
                forward = true;
                backward = true;
            }
            else
            { // define back/forward.
                if (tagsForward)
                { // relatively same direction.
                    forward = direction.Value;
                    backward = !direction.Value;
                }
                else
                { // relatively opposite direction.
                    forward = !direction.Value;
                    backward = direction.Value;
                }
            }

            // add tags.
            uint tagsId = 0;
            if (_storeTags)
            { // yes, store the tags!
                tagsId = tagsIndex.Add(tags);
            }

            // calculate weight including intermediates.
            var shapeInBox = true;
            float weight = 0;
            var previous = from;
            if (intermediates != null)
            {
                var box = new GeoCoordinateBox(from, to);
                for (int idx = 0; idx < intermediates.Count; idx++)
                {
                    var current = new GeoCoordinate(intermediates[idx].Latitude, intermediates[idx].Longitude);
                    weight = weight + (float)_vehicle.Weight(tags, previous, current);
                    previous = current;

                    if (!box.Contains(intermediates[idx].Longitude, intermediates[idx].Latitude))
                    { // shape not in box.
                        shapeInBox = false;
                    }
                }
            }
            weight = weight + (float)_vehicle.Weight(tags, previous, to);

            // initialize the edge data.
            var edge = new CHEdgeData(tagsId, tagsForward, forward, backward, weight);
            edge.ShapeInBox = shapeInBox;
            return edge;
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
            return _vehicle.CanTraverse(tags);
        }

        #region Static Processing Functions

        /// <summary>
        /// Preprocesses the data from the given OsmStreamReader and converts it directly to a routable data source.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="tagsIndex"></param>
        /// <param name="interpreter"></param>
        /// <param name="vehicle"></param>
        /// <returns></returns>
        public static RouterDataSource<CHEdgeData> Preprocess(OsmStreamSource reader,
            ITagsIndex tagsIndex, IOsmRoutingInterpreter interpreter, Vehicle vehicle)
        {
            // pull in the data.
            var graph = new RouterDataSource<CHEdgeData>(new DirectedGraph<CHEdgeData>(), tagsIndex);
            var targetData = new CHEdgeGraphOsmStreamTarget(
                graph, interpreter, tagsIndex, vehicle);
            targetData.RegisterSource(reader);
            targetData.Pull();

            return graph;
        }

        /// <summary>
        /// Preprocesses the data from the given OsmStreamReader and converts it directly to a routable data source.
        /// </summary>
        /// <param name="reader"></param>
        /// <param name="interpreter"></param>
        /// <param name="vehicle"></param>
        /// <returns></returns>
        public static RouterDataSource<CHEdgeData> Preprocess(OsmStreamSource reader,
            IOsmRoutingInterpreter interpreter, Vehicle vehicle)
        {
            return CHEdgeGraphOsmStreamTarget.Preprocess(reader, new TagsIndex(), interpreter, vehicle);
        }

        #endregion
    }
}