using System.IO;
using System.Net;
using Itinero.LocalGeo;
using Itinero.LocalGeo.IO;
using NUnit.Framework;

namespace Itinero.Test.LocalGeo.Operations
{
    [TestFixture]
    public class PolygonIntersectionTest
    {
        [Test]
        public void TestSimple()
        {
            var t = "points.points1.geojson".LoadTestStream().LoadTestPoints();
            var a = "points.polygon1.geojson".LoadTestStream().LoadTestPolygon();
            var b = "points.polygon2.geojson".LoadTestStream().LoadTestPolygon();
            var c = a.IntersectionsWith(b);
            File.WriteAllText("/home/pietervdvn/Desktop/output.geojson", c.ToGeoJson());
        }
    }
}