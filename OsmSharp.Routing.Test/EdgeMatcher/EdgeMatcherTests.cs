using NUnit.Framework;
using OsmSharp.Collections.Tags;
using OsmSharp.Collections.Tags.Index;
using OsmSharp.Math.Geo;
using OsmSharp.Routing;
using OsmSharp.Routing.Graph;
using OsmSharp.Routing.Graph.Routing;
using OsmSharp.Routing.Interpreter;
using OsmSharp.Routing.Osm.Interpreter;
using OsmSharp.Routing.Vehicles;

namespace OsmSharp.Test.Unittests.Routing.EdgeMatcher
{
    /// <summary>
    /// Tests the OsmRoutingInterpreter.
    /// </summary>
    [TestFixture]
    public class EdgeMatcherTests
    {
        /// <summary>
        /// Tests the edge matcher function.
        /// </summary>
        [Test]
        public void TestEdgeMatcher()
        {
            IEdgeMatcher matcher = new DefaultEdgeMatcher();

            // create edge tags.
            var edgeTags = new TagsCollection();
            //edge_tags["highway"] = "footway";

            // create point tags.
            var pointTags = new TagsCollection();
            //point_tags["highway"] = "footway";

            // test with empty point tags.
            Assert.IsTrue(matcher.MatchWithEdge(Vehicle.Car, null, null));
            Assert.IsTrue(matcher.MatchWithEdge(Vehicle.Car, pointTags, null));

            // test with empty edge tags.
            pointTags["name"] = "Ben Abelshausen Boulevard";
            Assert.IsFalse(matcher.MatchWithEdge(Vehicle.Car, pointTags, null));
            Assert.IsFalse(matcher.MatchWithEdge(Vehicle.Car, pointTags, edgeTags));

            // test with matching name.
            edgeTags["name"] = "Ben Abelshausen Boulevard";
            Assert.IsTrue(matcher.MatchWithEdge(Vehicle.Car, pointTags, edgeTags));

            // test with none-matching name.
            edgeTags["name"] = "Jorieke Vyncke Boulevard";
            Assert.IsFalse(matcher.MatchWithEdge(Vehicle.Car, pointTags, edgeTags));
        }

        /// <summary>
        /// Tests the edge matcher in combination with dykstra routing.
        /// </summary>
        [Test]
        public void TestEdgeMatcherDykstra()
        {
            string name = "Ben Abelshausen Boulevard";
            IEdgeMatcher matc = new DefaultEdgeMatcher();

            this.TestResolveOnEdge(name, "footway", Vehicle.Pedestrian, matc, true);
            this.TestResolveOnEdge(name, "cycleway", Vehicle.Pedestrian, matc, true);
            this.TestResolveOnEdge(name, "bridleway", Vehicle.Pedestrian, matc, false);
            this.TestResolveOnEdge(name, "path", Vehicle.Pedestrian, matc, true);
            this.TestResolveOnEdge(name, "pedestrian", Vehicle.Pedestrian, matc, true);
            this.TestResolveOnEdge(name, "road", Vehicle.Pedestrian, matc, true);
            this.TestResolveOnEdge(name, "living_street", Vehicle.Pedestrian, matc, true);
            this.TestResolveOnEdge(name, "residential", Vehicle.Pedestrian, matc, true);
            this.TestResolveOnEdge(name, "unclassified", Vehicle.Pedestrian, matc, true);
            this.TestResolveOnEdge(name, "tertiary", Vehicle.Pedestrian, matc, true);
            this.TestResolveOnEdge(name, "secondary", Vehicle.Pedestrian, matc, true);
            this.TestResolveOnEdge(name, "primary", Vehicle.Pedestrian, matc, true);
            this.TestResolveOnEdge(name, "trunk", Vehicle.Pedestrian, matc, false);
            this.TestResolveOnEdge(name, "motorway", Vehicle.Pedestrian, matc, false);

            this.TestResolveOnEdge(name, "footway", Vehicle.Bicycle, matc, false);
            this.TestResolveOnEdge(name, "cycleway", Vehicle.Bicycle, matc, true);
            this.TestResolveOnEdge(name, "bridleway", Vehicle.Bicycle, matc, false);
            this.TestResolveOnEdge(name, "path", Vehicle.Bicycle, matc, true);
            this.TestResolveOnEdge(name, "pedestrian", Vehicle.Bicycle, matc, false);
            this.TestResolveOnEdge(name, "road", Vehicle.Bicycle, matc, true);
            this.TestResolveOnEdge(name, "living_street", Vehicle.Bicycle, matc, true);
            this.TestResolveOnEdge(name, "residential", Vehicle.Bicycle, matc, true);
            this.TestResolveOnEdge(name, "unclassified", Vehicle.Bicycle, matc, true);
            this.TestResolveOnEdge(name, "tertiary", Vehicle.Bicycle, matc, true);
            this.TestResolveOnEdge(name, "secondary", Vehicle.Bicycle, matc, true);
            this.TestResolveOnEdge(name, "primary", Vehicle.Bicycle, matc, true);
            this.TestResolveOnEdge(name, "trunk", Vehicle.Bicycle, matc, false);
            this.TestResolveOnEdge(name, "motorway", Vehicle.Bicycle, matc, false);

            this.TestResolveOnEdge(name, "footway", Vehicle.Moped, matc, false);
            this.TestResolveOnEdge(name, "cycleway", Vehicle.Moped, matc, false);
            this.TestResolveOnEdge(name, "bridleway", Vehicle.Moped, matc, false);
            this.TestResolveOnEdge(name, "path", Vehicle.Moped, matc, false);
            this.TestResolveOnEdge(name, "pedestrian", Vehicle.Moped, matc, false);
            this.TestResolveOnEdge(name, "road", Vehicle.Moped, matc, true);
            this.TestResolveOnEdge(name, "living_street", Vehicle.Moped, matc, true);
            this.TestResolveOnEdge(name, "residential", Vehicle.Moped, matc, true);
            this.TestResolveOnEdge(name, "unclassified", Vehicle.Moped, matc, true);
            this.TestResolveOnEdge(name, "tertiary", Vehicle.Moped, matc, true);
            this.TestResolveOnEdge(name, "secondary", Vehicle.Moped, matc, true);
            this.TestResolveOnEdge(name, "primary", Vehicle.Moped, matc, true);
            this.TestResolveOnEdge(name, "trunk", Vehicle.Moped, matc, true);
            this.TestResolveOnEdge(name, "motorway", Vehicle.Moped, matc, false);

            this.TestResolveOnEdge(name, "footway", Vehicle.MotorCycle, matc, false);
            this.TestResolveOnEdge(name, "cycleway", Vehicle.MotorCycle, matc, false);
            this.TestResolveOnEdge(name, "bridleway", Vehicle.MotorCycle, matc, false);
            this.TestResolveOnEdge(name, "path", Vehicle.MotorCycle, matc, false);
            this.TestResolveOnEdge(name, "pedestrian", Vehicle.MotorCycle, matc, false);
            this.TestResolveOnEdge(name, "road", Vehicle.MotorCycle, matc, true);
            this.TestResolveOnEdge(name, "living_street", Vehicle.MotorCycle, matc, true);
            this.TestResolveOnEdge(name, "residential", Vehicle.MotorCycle, matc, true);
            this.TestResolveOnEdge(name, "unclassified", Vehicle.MotorCycle, matc, true);
            this.TestResolveOnEdge(name, "tertiary", Vehicle.MotorCycle, matc, true);
            this.TestResolveOnEdge(name, "secondary", Vehicle.MotorCycle, matc, true);
            this.TestResolveOnEdge(name, "primary", Vehicle.MotorCycle, matc, true);
            this.TestResolveOnEdge(name, "trunk", Vehicle.MotorCycle, matc, true);
            this.TestResolveOnEdge(name, "motorway", Vehicle.MotorCycle, matc, true);

            this.TestResolveOnEdge(name, "footway", Vehicle.Car, matc, false);
            this.TestResolveOnEdge(name, "cycleway", Vehicle.Car, matc, false);
            this.TestResolveOnEdge(name, "bridleway", Vehicle.Car, matc, false);
            this.TestResolveOnEdge(name, "path", Vehicle.Car, matc, false);
            this.TestResolveOnEdge(name, "pedestrian", Vehicle.Car, matc, false);
            this.TestResolveOnEdge(name, "road", Vehicle.Car, matc, true);
            this.TestResolveOnEdge(name, "living_street", Vehicle.Car, matc, true);
            this.TestResolveOnEdge(name, "residential", Vehicle.Car, matc, true);
            this.TestResolveOnEdge(name, "unclassified", Vehicle.Car, matc, true);
            this.TestResolveOnEdge(name, "tertiary", Vehicle.Car, matc, true);
            this.TestResolveOnEdge(name, "secondary", Vehicle.Car, matc, true);
            this.TestResolveOnEdge(name, "primary", Vehicle.Car, matc, true);
            this.TestResolveOnEdge(name, "trunk", Vehicle.Car, matc, true);
            this.TestResolveOnEdge(name, "motorway", Vehicle.Car, matc, true);

            this.TestResolveOnEdge(name, "footway", Vehicle.SmallTruck, matc, false);
            this.TestResolveOnEdge(name, "cycleway", Vehicle.SmallTruck, matc, false);
            this.TestResolveOnEdge(name, "bridleway", Vehicle.SmallTruck, matc, false);
            this.TestResolveOnEdge(name, "path", Vehicle.SmallTruck, matc, false);
            this.TestResolveOnEdge(name, "pedestrian", Vehicle.SmallTruck, matc, false);
            this.TestResolveOnEdge(name, "road", Vehicle.SmallTruck, matc, true);
            this.TestResolveOnEdge(name, "living_street", Vehicle.SmallTruck, matc, true);
            this.TestResolveOnEdge(name, "residential", Vehicle.SmallTruck, matc, true);
            this.TestResolveOnEdge(name, "unclassified", Vehicle.SmallTruck, matc, true);
            this.TestResolveOnEdge(name, "tertiary", Vehicle.SmallTruck, matc, true);
            this.TestResolveOnEdge(name, "secondary", Vehicle.SmallTruck, matc, true);
            this.TestResolveOnEdge(name, "primary", Vehicle.SmallTruck, matc, true);
            this.TestResolveOnEdge(name, "trunk", Vehicle.SmallTruck, matc, true);
            this.TestResolveOnEdge(name, "motorway", Vehicle.SmallTruck, matc, true);

            this.TestResolveOnEdge(name, "footway", Vehicle.BigTruck, matc, false);
            this.TestResolveOnEdge(name, "cycleway", Vehicle.BigTruck, matc, false);
            this.TestResolveOnEdge(name, "bridleway", Vehicle.BigTruck, matc, false);
            this.TestResolveOnEdge(name, "path", Vehicle.BigTruck, matc, false);
            this.TestResolveOnEdge(name, "pedestrian", Vehicle.BigTruck, matc, false);
            this.TestResolveOnEdge(name, "road", Vehicle.BigTruck, matc, true);
            this.TestResolveOnEdge(name, "living_street", Vehicle.BigTruck, matc, true);
            this.TestResolveOnEdge(name, "residential", Vehicle.BigTruck, matc, true);
            this.TestResolveOnEdge(name, "unclassified", Vehicle.BigTruck, matc, true);
            this.TestResolveOnEdge(name, "tertiary", Vehicle.BigTruck, matc, true);
            this.TestResolveOnEdge(name, "secondary", Vehicle.BigTruck, matc, true);
            this.TestResolveOnEdge(name, "primary", Vehicle.BigTruck, matc, true);
            this.TestResolveOnEdge(name, "trunk", Vehicle.BigTruck, matc, true);
            this.TestResolveOnEdge(name, "motorway", Vehicle.BigTruck, matc, true);

            this.TestResolveOnEdge(name, "footway", Vehicle.Bus, matc, false);
            this.TestResolveOnEdge(name, "cycleway", Vehicle.Bus, matc, false);
            this.TestResolveOnEdge(name, "bridleway", Vehicle.Bus, matc, false);
            this.TestResolveOnEdge(name, "path", Vehicle.Bus, matc, false);
            this.TestResolveOnEdge(name, "pedestrian", Vehicle.Bus, matc, false);
            this.TestResolveOnEdge(name, "road", Vehicle.Bus, matc, true);
            this.TestResolveOnEdge(name, "living_street", Vehicle.Bus, matc, true);
            this.TestResolveOnEdge(name, "residential", Vehicle.Bus, matc, true);
            this.TestResolveOnEdge(name, "unclassified", Vehicle.Bus, matc, true);
            this.TestResolveOnEdge(name, "tertiary", Vehicle.Bus, matc, true);
            this.TestResolveOnEdge(name, "secondary", Vehicle.Bus, matc, true);
            this.TestResolveOnEdge(name, "primary", Vehicle.Bus, matc, true);
            this.TestResolveOnEdge(name, "trunk", Vehicle.Bus, matc, true);
            this.TestResolveOnEdge(name, "motorway", Vehicle.Bus, matc, true);
        }

        /// <summary>
        /// Tests the edge matcher in combination with dykstra routing.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="highway"></param>
        /// <param name="vehicle"></param>
        /// <param name="matcher"></param>
        /// <param name="found"></param>
        private void TestResolveOnEdge(string name, string highway,
            Vehicle vehicle, IEdgeMatcher matcher, bool found)
        {
            this.TestResolveOnEdgeSingle(name, highway, vehicle, null, null, !found);
            this.TestResolveOnEdgeSingle(name, highway, vehicle, matcher, null, !found);
            this.TestResolveOnEdgeSingle(name, highway, vehicle, matcher, name, !found);
        }

        /// <summary>
        /// Tests the edge matcher in combination with dykstra routing.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="highway"></param>
        /// <param name="vehicle"></param>
        /// <param name="matcher"></param>
        /// <param name="pointName"></param>
        /// <param name="notFound"></param>
        private void TestResolveOnEdgeSingle(string name, string highway, 
            Vehicle vehicle, IEdgeMatcher matcher, 
            string pointName, bool notFound)
        {
            var fromName = new GeoCoordinate(51.0003, 4.0007);
            var toName = new GeoCoordinate(51.0003, 4.0008);

            var fromNoname = new GeoCoordinate(51.0, 4.0007);
            var toNoname = new GeoCoordinate(51.0, 4.0008);

            TagsCollectionBase pointTags = new TagsCollection();
            pointTags["name"] = pointName;

            TagsCollectionBase tags = new TagsCollection();
            tags["highway"] = highway;
            //tags["name"] = name;

            var tagsIndex = new TagsIndex();

            // do the data processing.
            var data = new RouterDataSource<Edge>(new Graph<Edge>(), tagsIndex);
            uint vertexNoname1 = data.AddVertex((float)fromNoname.Latitude, (float)fromNoname.Longitude);
            uint vertexNoname2 = data.AddVertex((float)toNoname.Latitude, (float)toNoname.Longitude);
            data.AddEdge(vertexNoname1, vertexNoname2, new Edge()
            {
                Forward = true,
                Tags = tagsIndex.Add(tags)
            }, null);
            tags = new TagsCollection();
            tags["highway"] = highway;
            tags["name"] = name;
            uint vertexName1 = data.AddVertex((float)fromName.Latitude, (float)fromName.Longitude);
            uint vertexName2 = data.AddVertex((float)toName.Latitude, (float)toName.Longitude);
            data.AddEdge(vertexName1, vertexName2, new Edge()
            {
                Forward = true,
                Tags = tagsIndex.Add(tags)
            }, null);

            IRoutingInterpreter interpreter = new OsmRoutingInterpreter();

            // creates the data.
            IRoutingAlgorithm<Edge> router = new Dykstra();

            var nonameLocation = new GeoCoordinate(
                (fromNoname.Latitude + toNoname.Latitude) / 2.0,
                (fromNoname.Longitude + toNoname.Longitude) / 2.0);
//            var nameLocation = new GeoCoordinate(
//                (fromName.Latitude + toName.Latitude) / 2.0,
//                (fromName.Longitude + toName.Longitude) / 2.0);

            const float delta = 0.01f;
            var result = router.SearchClosest(data, interpreter, vehicle, nonameLocation, delta, matcher, pointTags, null);
            if (result.Distance < double.MaxValue)
            { // there is a result.
                Assert.IsFalse(notFound, "A result was found but was supposed not to  be found!");

                if (name == pointName)
                { // the name location was supposed to be found!
                    Assert.IsTrue(result.Vertex1 == vertexName1 || result.Vertex1 == vertexName2);
                    Assert.IsTrue(result.Vertex2 == vertexName1 || result.Vertex2 == vertexName2);
                }
                else
                { // the noname location was supposed to be found!
                    Assert.IsTrue(result.Vertex1 == vertexNoname1 || result.Vertex1 == vertexNoname2);
                    Assert.IsTrue(result.Vertex2 == vertexNoname1 || result.Vertex2 == vertexNoname2);
                }
                return;
            }
            Assert.IsTrue(notFound, "A result was not found but was supposed to be found!");
        }
    }
}