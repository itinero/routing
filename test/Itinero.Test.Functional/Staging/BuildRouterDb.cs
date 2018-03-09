using System.IO;
using Itinero.Elevation;
using Itinero.IO.Osm;
using Itinero.LocalGeo;
using SRTM;

namespace Itinero.Test.Functional.Staging
{
    public static class BuildRouterDb
    {
        public static void Build(string filename)
        {
            var routerDb = new RouterDb();

            routerDb.LoadOsmDataFromOverpass(new Box(51.25380399985758f, 4.809179306030273f,
                51.273138772415194f, 4.765233993530273f), Itinero.Osm.Vehicles.Vehicle.Car);

            // create a new srtm data instance.
            // it accepts a folder to download and cache data into.
            var srtmCache = new DirectoryInfo("srtm-cache");
            if (!srtmCache.Exists)
            {
                srtmCache.Create();
            }
            var srtmData = new SRTMData("srtm-cache");
            LocalGeo.Elevation.ElevationHandler.GetElevation = (lat, lon) =>
            {
                return (short)srtmData.GetElevation(lat, lon);
            };
            routerDb.AddElevation();

            routerDb.AddContracted(Itinero.Osm.Vehicles.Vehicle.Car.Fastest(), true);

            using (var stream = File.Open(filename, FileMode.Create))
            {
                routerDb.Serialize(stream);
            }
        }
    }
}