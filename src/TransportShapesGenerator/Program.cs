using Itinero;
using Itinero.IO.Osm;
using Itinero.LocalGeo;
using Itinero.Osm.Vehicles;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace TransportShapesGenerator
{
    public class Program
    {
        public static void Main(string[] args)
        {
        }

		public void TestTrainLanka()
		{

			// build router db.
			var routerDb = new RouterDb();


			using (var stream = new FileInfo(@"C:\IMPORT\OSM\sri-lanka-latest.osm.pbf").OpenRead())
			{
				routerDb.LoadOsmData(stream, Itinero.Osm.Vehicles.Vehicle.Train);
			}


			// test some routes.
			var router = new Router(routerDb);



			var route = router.TryCalculate(Vehicle.Train.Fastest(),
				new Coordinate(6.032639f, 80.240112f),
				new Coordinate(5.994057f, 80.307286f));

		}
	}
}
