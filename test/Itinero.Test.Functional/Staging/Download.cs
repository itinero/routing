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

using System.IO;
using System.IO.Compression;
using System.Net;

namespace Itinero.Test.Functional.Staging
{
    /// <summary>
    /// Downloads all data needed for testing.
    /// </summary>
    public static class Download
    {
        public static string BelgiumAllSource = "ftp://build.osmsharp.com/data/OSM/routing/planet/europe/belgium.a.routing.zip";
        public static string BelgiumPBF = "ftp://build.osmsharp.com/data/OSM/planet/europe/belgium-latest.osm.pbf";
        public static string BelgiumLocal = "belgium-latest.osm.pbf";
        public static string LuxembourgPBF = "ftp://build.osmsharp.com/data/OSM/planet/europe/luxembourg-latest.osm.pbf";
        public static string LuxembourgLocal = "luxembourg-latest.osm.pbf";

        /// <summary>
        /// Downloads the belgium data.
        /// </summary>
        public static void DownloadBelgiumAll()
        {
            if (!File.Exists("belgium.a.routing"))
            {
                var client = new WebClient();
                client.DownloadFile(Download.BelgiumAllSource,
                    "belgium.a.routing.zip");
                ZipFile.ExtractToDirectory("belgium.a.routing.zip", ".");
                File.Delete("belgium.a.routing.zip");
            }
            if(!File.Exists(Download.BelgiumLocal))
            {
                var client = new WebClient();
                client.DownloadFile(Download.BelgiumPBF,
                    Download.BelgiumLocal);
            }
        }

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
    }
}
