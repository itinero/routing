using NUnit.Framework;
using OsmSharp.Routing.Geo;

namespace OsmSharp.Routing.Test.Geo
{
    /// <summary>
    /// Contains tests for the line class.
    /// </summary>
    [TestFixture]
    public class LineTests
    {
        /// <summary>
        /// Tests project on.
        /// </summary>
        [Test]
        public void TestProjectOne()
        {
            var line = new Line(new Coordinate(0, 0), new Coordinate(0, 2));
            var coordinate = new Coordinate(1, 1);

            var result = line.ProjectOn(coordinate);
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Value.Latitude, 0.001);
            Assert.AreEqual(1, result.Value.Longitude, 0.001);

            line = new Line(new Coordinate(0, 0), new Coordinate(2, 0));
            coordinate = new Coordinate(1, 1);

            result = line.ProjectOn(coordinate);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Value.Latitude, 0.001);
            Assert.AreEqual(0, result.Value.Longitude, 0.001);

            line = new Line(new Coordinate(0, 0), new Coordinate(2, 2));
            coordinate = new Coordinate(1, 1);

            result = line.ProjectOn(coordinate);
            Assert.IsNotNull(result);
            Assert.AreEqual(1, result.Value.Latitude, 0.001);
            Assert.AreEqual(1, result.Value.Longitude, 0.001);
            line = new Line(new Coordinate(0, 0), new Coordinate(0, -2));
            coordinate = new Coordinate(-1, -1);

            result = line.ProjectOn(coordinate);
            Assert.IsNotNull(result);
            Assert.AreEqual(0, result.Value.Latitude, 0.001);
            Assert.AreEqual(-1, result.Value.Longitude, 0.001);

            line = new Line(new Coordinate(0, 0), new Coordinate(-2, 0));
            coordinate = new Coordinate(-1, -1);

            result = line.ProjectOn(coordinate);
            Assert.IsNotNull(result);
            Assert.AreEqual(-1, result.Value.Latitude, 0.001);
            Assert.AreEqual(0, result.Value.Longitude, 0.001);

            line = new Line(new Coordinate(0, 0), new Coordinate(-2, -2));
            coordinate = new Coordinate(-1, -1);

            result = line.ProjectOn(coordinate);
            Assert.IsNotNull(result);
            Assert.AreEqual(-1, result.Value.Latitude, 0.001);
            Assert.AreEqual(-1, result.Value.Longitude, 0.001);
        }
    }
}
