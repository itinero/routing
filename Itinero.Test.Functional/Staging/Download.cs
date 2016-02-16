// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2016 Abelshausen Ben
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

using System.IO;
using System.IO.Compression;
using System.Net;

namespace OsmSharp.Routing.Test.Functional.Staging
{
    /// <summary>
    /// Downloads all data needed for testing.
    /// </summary>
    public static class Download
    {
        public static string BelgiumAllSource = "ftp://build.osmsharp.com/data/OSM/routing/planet/europe/belgium.a.routing.zip";
        public static string BelgiumPBF = "ftp://build.osmsharp.com/data/OSM/planet/europe/belgium-latest.osm.pbf";

        /// <summary>
        /// Downloads the belgium routing file.
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
            if(!File.Exists("belgium-latest.osm.pbf"))
            {
                var client = new WebClient();
                client.DownloadFile(Download.BelgiumAllSource,
                    "belgium-latest.osm.pbf");
            }
        }
    }
}
