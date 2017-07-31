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

using Itinero.IO.Osm;
using Itinero.Profiles;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Net;
using System.Net.Http;

namespace Itinero.Test.Functional.Staging
{
    /// <summary>
    /// Downloads all data needed for testing.
    /// </summary>
    public static class Download
    {
        public static string LuxembourgPBF = "http://files.itinero.tech/data/OSM/planet/europe/luxembourg-latest.osm.pbf";
        public static string LuxembourgLocal = "luxembourg-latest.osm.pbf";
        
        /// <summary>
        /// Downloads the luxembourg data.
        /// </summary>
        public static void DownloadLuxembourgAll()
        {
            if (!File.Exists(Download.LuxembourgLocal))
            {
                var client = new WebClient();
                client.DownloadFile(Download.LuxembourgPBF,
                    Download.LuxembourgLocal);
            }
        }

        /// <summary>
        /// Downloads data from overpass.
        /// </summary>
        public static string DownloadOverpass(string query, string name)
        {
            var filename = name + ".osm";

            if (!File.Exists(filename))
            {
                var client = new HttpClient();
                var content = new StringContent("data=" + query);
                var response = client.PostAsync(@"http://overpass-api.de/api/interpreter", content);
                using (var stream = response.GetAwaiter().GetResult().Content.ReadAsStreamAsync().GetAwaiter().GetResult())
                using (var outputStream = File.OpenWrite(filename))
                {
                    stream.CopyTo(outputStream);
                }
            }
            return filename;
        }

        /// <summary>
        /// Builds a routerdb from an overpass query.
        /// </summary>
        public static RouterDb BuildRouterDbOverpass(string query, Vehicle vehicle)
        {
            var fileName = DownloadOverpass(query, "temp");

            RouterDb routerDb;
            using (var stream = File.OpenRead(fileName))
            {
                var xmlStream = new OsmSharp.Streams.XmlOsmStreamSource(stream);
                var sortedData = xmlStream.ToList();
                sortedData.Sort((x, y) =>
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

                routerDb = new RouterDb();
                routerDb.LoadOsmData(sortedData, vehicle);
            }
            return routerDb;
        }
    }
}