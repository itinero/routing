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

using System.Collections.Generic;
using System.IO;
using OsmSharp;
using OsmSharp.Streams;

namespace Itinero.IO.Osm.Overpass
{
    // TODO: move this to OsmSharp.

    /// <summary>
    /// An OSM source stream based on an overpass query.
    /// </summary>
    public class OverpassSourceStream : OsmStreamSource
    {
        private readonly string _query;
        private readonly MemoryStream _overpassData;

        /// <summary>
        /// Creates a new overpass stream.
        /// </summary>
        public OverpassSourceStream(string query)
        {
            _query = query;
            _overpassData = new MemoryStream();
        }

        private OsmStreamSource _source = null;

        /// <summary>
        /// Returns true if this stream can be reset.
        /// </summary>
        public override bool CanReset => true;

        /// <summary>
        /// Gets the current object.
        /// </summary>
        /// <returns></returns>
        public override OsmGeo Current()
        {
            return _source.Current();
        }

        /// <summary>
        /// Moves to the next object.
        /// </summary>
        public override bool MoveNext(bool ignoreNodes, bool ignoreWays, bool ignoreRelations)
        {
            if (_source == null)
            {
                if (_overpassData.Length == 0)
                { // download data.
                    using (var overpassStream = OverpassDownload.ToStream(_query))
                    {
                        overpassStream.CopyTo(_overpassData);
                        _overpassData.Seek(0, SeekOrigin.Begin);
                    }
                }

                var xmlSource = new XmlOsmStreamSource(_overpassData);
                var xmlSourceList = new List<OsmGeo>(xmlSource);
                xmlSourceList.Sort((x, y) =>
                {
                    if (x.Type == y.Type)
                    {
                        return x.Id.Value.CompareTo(y.Id.Value);
                    }
                    if (x.Type == OsmSharp.OsmGeoType.Node)
                    {
                        return -1;
                    }
                    else if (x.Type == OsmSharp.OsmGeoType.Way)
                    {
                        if (y.Type == OsmSharp.OsmGeoType.Node)
                        {
                            return 1;
                        }
                            return -1;
                    }
                    return 1;
                });
                _source = new OsmSharp.Streams.OsmEnumerableStreamSource(
                    xmlSourceList);                
            }
            return _source.MoveNext(ignoreNodes, ignoreWays, ignoreRelations);
        }

        /// <summary>
        /// Resets this stream.
        /// </summary>
        public override void Reset()
        {
            if (_source != null)
            {
                _source.Reset();
            }
        }
    }
}