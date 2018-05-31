using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using Itinero.LocalGeo;
using NUnit.Framework;
using Itinero.LocalGeo.Operations;

namespace Itinero.Test.LocalGeo.Operations
{
    [TestFixture]
    public class PolygonAreaCalculatorTest
    {
        [Test]
        public void TestArea()
        {
            var points = "Itinero.Test.test_data.points.polyArea.hull.geojson".LoadAsStream().LoadTestPoints();
            var pointsL = new List<Coordinate> (points);
            var area = pointsL.SurfaceArea();

            Assert.AreEqual(4.46114922f, area);
        }
        
        [Test]
        public void TestSimpleArea1()
        {
            var points = "Itinero.Test.test_data.points.polyArea1.hull.geojson".LoadAsStream().LoadTestPoints();
            var pointsL = new List<Coordinate> (points);
            var area = pointsL.SurfaceArea();

            Assert.AreEqual(1f, area);
        }
        
        [Test]
        public void TestSimpleArea2()
        {
            var points = "Itinero.Test.test_data.points.polyArea1.hull.geojson".LoadAsStream().LoadTestPoints();
            var pointsL = new List<Coordinate> (points);
            var area = pointsL.SurfaceArea();

            Assert.AreEqual(1f, area);
        }
    }
}