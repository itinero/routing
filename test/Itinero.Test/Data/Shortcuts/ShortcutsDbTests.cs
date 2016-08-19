using Itinero.Attributes;
using Itinero.Data.Shortcuts;
using Itinero.Osm.Vehicles;
using NUnit.Framework;

namespace Itinero.Test.Data.Shortcuts
{
    /// <summary>
    /// Contains tests for the shortcuts db.
    /// </summary>
    [TestFixture]
    public class ShortcutsDbTests
    {
        /// <summary>
        /// Tests adding stops.
        /// </summary>
        [Test]
        public void TestAddStops()
        {
            var db = new ShortcutsDb(Vehicle.Bicycle.Fastest());

            db.AddStop(10, new AttributeCollection(new Attribute()
            {
                Key = "some_key",
                Value = "some_value"
            }));
            db.AddStop(11, new AttributeCollection(new Attribute()
            {
                Key = "some_key",
                Value = "some_value"
            }));
            db.AddStop(12, new AttributeCollection(new Attribute()
            {
                Key = "some_key1",
                Value = "some_value1"
            }));
            db.AddStop(13, new AttributeCollection(new Attribute()
            {
                Key = "some_key1",
                Value = "some_value1"
            }));
        }

        /// <summary>
        /// Tests getting stops.
        /// </summary>
        [Test]
        public void TestGetStop()
        {
            var db = new ShortcutsDb(Vehicle.Bicycle.Fastest());

            db.AddStop(10, new AttributeCollection(new Attribute()
            {
                Key = "some_key",
                Value = "some_value"
            }));
            db.AddStop(11, new AttributeCollection(new Attribute()
            {
                Key = "some_key",
                Value = "some_value"
            }));
            db.AddStop(12, new AttributeCollection(new Attribute()
            {
                Key = "some_key1",
                Value = "some_value1"
            }));
            db.AddStop(13, new AttributeCollection(new Attribute()
            {
                Key = "some_key1",
                Value = "some_value1"
            }));

            var a = db.GetStop(10);
            Assert.IsNotNull(a);
            Assert.AreEqual(1, a.Count);
        }

        /// <summary>
        /// Tests adding shortcuts.
        /// </summary>
        [Test]
        public void TestAddShortcuts()
        {
            var db = new ShortcutsDb(Vehicle.Bicycle.Fastest());

            db.AddStop(10, null);
            db.AddStop(11, null);
            db.AddStop(12, null);
            db.AddStop(13, null);

            db.Add(new uint[] { 10, 100, 101, 102, 103, 11 }, new AttributeCollection(new Attribute()
            {
                Key = "some_key1",
                Value = "some_value1"
            }));

            db.Add(new uint[] { 12, 110, 111, 112, 113, 13 }, new AttributeCollection(new Attribute()
            {
                Key = "some_key1",
                Value = "some_value1"
            }));
        }

        /// <summary>
        /// Tests getting shortcuts.
        /// </summary>
        [Test]
        public void TestGetShortcut()
        {
            var db = new ShortcutsDb(Vehicle.Bicycle.Fastest());

            db.AddStop(10, null);
            db.AddStop(11, null);
            db.AddStop(12, null);
            db.AddStop(13, null);

            var s1 = db.Add(new uint[] { 10, 100, 101, 102, 103, 11 }, new AttributeCollection(new Attribute()
            {
                Key = "some_key1",
                Value = "some_value1"
            }));

            var s2 = db.Add(new uint[] { 12, 110, 111, 112, 113, 13 }, new AttributeCollection(new Attribute()
            {
                Key = "some_key1",
                Value = "some_value1"
            }));

            IAttributeCollection meta;
            var s = db.Get(s1, out meta);
            Assert.IsNotNull(s);
            Assert.AreEqual(6, s.Length);
            Assert.IsNotNull(meta);
            Assert.AreEqual(1, meta.Count);
        }
    }
}