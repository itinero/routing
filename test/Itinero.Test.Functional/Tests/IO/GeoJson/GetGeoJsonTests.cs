using System;
using System.IO;

namespace Itinero.Test.Functional.Tests.IO.GeoJson
{
    public static class GetGeoJsonTests
    {
        /// <summary>
        /// Runs resolving tests on the given routerDb.
        /// </summary>
        public static void Run(RouterDb routerDb)
        {
            GetTestWriteGeoJsonFile(routerDb).TestPerf("Writing geojson.");
        }

        /// <summary>
        /// Tests writing a shapefile.
        /// </summary>
        public static Action GetTestWriteGeoJsonFile(RouterDb routerDb)
        {
            return () =>
            {
                File.WriteAllText("luxembourg.geojson", 
                    routerDb.GetGeoJson(true, true, true));
            };
        }
    }
}