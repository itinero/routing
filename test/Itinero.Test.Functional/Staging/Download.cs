// Itinero - Routing for .NET
// Copyright (C) 2015 Abelshausen Ben
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
