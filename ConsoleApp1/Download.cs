using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    /// <summary>
    /// Downloads all data needed for testing.
    /// </summary>
    public static class Download
    {
        public static string LuxembourgPBF = "ftp://build.osmsharp.com/data/OSM/planet/europe/luxembourg-latest.osm.pbf";
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
    }
}
