using System;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
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
            var a = "polygons.polygon1.geojson".LoadTestStream().LoadTestPolygon();
            Assert.IsTrue(a.IsClockwise());
            Assert.IsTrue(a.ExteriorRing[0].Equals(a.ExteriorRing.Last()));
            var b = "polygons.polygon2.geojson".LoadTestStream().LoadTestPolygon();
            Assert.IsFalse(b.IsClockwise());
            var intersections = PolygonIntersection.IntersectionsBetween(a.ExteriorRing, b.ExteriorRing);


            var expected =
                new List<Coordinate>("polygons.Intersections_Polygon1_Polygon2.geojson".LoadTestStream()
                    .LoadTestPoints());
            for (var i = 0; i < intersections.Count; i++)
            {
                Assert.AreEqual(intersections[i].Item3.ToString(), expected[i].ToString());
            }
        }


        [Test]
        public void TestClockwise()
        {
            var a = "polygons.polygon1.geojson".LoadTestStream().LoadTestPolygon();
            Assert.IsTrue(a.IsClockwise());
            Assert.IsTrue(a.ExteriorRing[0].Equals(a.ExteriorRing.Last())); // is closed
            a.ExteriorRing.Reverse();
            Assert.IsFalse(a.IsClockwise());
            // B is constructed COUNTERclockwise
            var b = "polygons.polygon2.geojson".LoadTestStream().LoadTestPolygon();
            Assert.IsFalse(b.IsClockwise());
            b.ExteriorRing.Reverse();
            Assert.IsTrue(b.IsClockwise());
        }

        [Test]
        public void TestSum()
        {
            var c = "polygons.polygon3.geojson".LoadTestStream().LoadTestPolygon();
            var sa = c.ExteriorRing.SignedSurfaceArea();
            Assert.IsTrue(sa > 0);
            c.ExteriorRing.Reverse();
            sa = c.ExteriorRing.SignedSurfaceArea();
            Assert.IsFalse(sa > 0);
        }

        public void Test(string poly1, string poly2, string expInter, string expUnion, bool dump = false)
        {
            var a = poly1.LoadTestStream().LoadTestPolygon();
            var b = poly2.LoadTestStream().LoadTestPolygon();
            var c = a.IntersectionsWith(b);

            if (!dump)
            {
                Assert.IsTrue(c.Count == 1);
                var poly = c[0];
                poly.MakeClosed();
                var exp = expInter.LoadTestStream().LoadTestPolygon();
                Assert.AreEqual(poly.ToGeoJson(), exp.ToGeoJson());
            }
            else
            {
                File.WriteAllText("/home/pietervdvn/Desktop/Intersection.geojson", c.ToGeoJson());
            }

            var union = a.UnionWith(b);
            union.MakeClosed();
            if (!dump)
            {
                var exp = expUnion.LoadTestStream().LoadTestPolygon();
                Assert.AreEqual(union.ToGeoJson(), exp.ToGeoJson());
            }
            else
            {
                File.WriteAllText("/home/pietervdvn/Desktop/Union.geojson", union.ToGeoJson());
            }
        }

        [Test]
        public void TestSimple()
        {
            Test("polygons.polygon1.geojson", "polygons.polygon2.geojson", "polygons.Intersect_1_2.geojson",
                "polygons.Union1_2.geojson");
            Test("polygons.polygon6.geojson", "polygons.polygon7.geojson", "polygons.Intersect_6_7.geojson",
                "polygons.Union_6_7.geojson");
            Test("polygons.polygon4.geojson", "polygons.polygon5.geojson", "polygons.Intersect_4_5.geojson",
                "polygons.Union_4_5.geojson");
        }

        [Test]
        public void TestInternals()
        {
            var a = "polygons.polygon6.geojson".LoadTestStream().LoadTestPolygon();
            var b = "polygons.polygon7.geojson".LoadTestStream().LoadTestPolygon();


            var intersections = PolygonIntersection.IntersectionsBetween(a.ExteriorRing, b.ExteriorRing);
            PolygonIntersection.PrepareIntersectionMatrix(intersections, out var aInt,
                out var bInt);

            Assert.AreEqual(aInt[0][1], bInt[1][0]);

            // What happens if we start walking clockwise, from the most northern intersection point on poly a?
            // By chance the most northern point is also the first one

            var visits = new List<Coordinate>();
            var nxtIntersection = PolygonIntersection.FollowAlong(visits, a, 0, 1, aInt);
            Assert.AreEqual(nxtIntersection, Tuple.Create(1, 1));

            var c = a.IntersectionsWith(b);

            Assert.AreEqual(1, c.Count);
        }
    }
}