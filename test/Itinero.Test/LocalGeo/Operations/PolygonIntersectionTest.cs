using System.IO;
using System.Linq;
using System.Collections.Generic;
using Itinero.Geo;
using Itinero.LocalGeo;
using Itinero.LocalGeo.IO;
using Itinero.LocalGeo.Operations;
using NUnit.Framework;

namespace Itinero.Test.LocalGeo.Operations
{
    [TestFixture]
    public class PolygonIntersectionTest
    {
        [Test]
        public void TestIntersectionsBetween()
        {
            var a = "points.polygon1.geojson".LoadTestStream().LoadTestPolygon();
            Assert.IsTrue(a.IsClockwise());
            Assert.IsTrue(a.ExteriorRing[0].Equals(a.ExteriorRing.Last()));
            var b = "points.polygon2.geojson".LoadTestStream().LoadTestPolygon();
            var intersections = PolygonIntersection.IntersectionsBetween(a.ExteriorRing, b.ExteriorRing);

            var expected = new List<Coordinate>("points.Intersections_Polygon1_Polygon2.geojson".LoadTestStream().LoadTestPoints());
            for (var i = 0; i < intersections.Count; i++)
            {
                Assert.AreEqual(intersections[i].Item3.ToString(), expected[i].ToString());
            }

        }

        [Test]
        public void TestSimple()
        {
            var a = "points.polygon1.geojson".LoadTestStream().LoadTestPolygon();
            Assert.IsTrue(a.IsClockwise());
            Assert.IsTrue(a.ExteriorRing[0].Equals(a.ExteriorRing.Last()));
            var b = "points.polygon2.geojson".LoadTestStream().LoadTestPolygon();
            var c = a.IntersectionsWith(b);
            File.WriteAllText("/home/pietervdvn/Desktop/output.geojson", c.ToGeoJson());
        }
    }
}