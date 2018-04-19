using NUnit.Framework;
using Itinero.LocalGeo;

namespace Itinero.Test.LocalGeo
{
    /// <summary>
    /// Tests local geo tools.
    /// </summary>
    [TestFixture]
    public class ToolsTests
    {
        /// <summary>
        /// Tests the smallest difference in degrees.
        /// </summary>
        [Test]
        public void TestSmallestDiffDegrees()
        {
            Assert.AreEqual(0, 180.0.SmallestDiffDegrees(180));
            Assert.AreEqual(180, 180.0.SmallestDiffDegrees(0));
            Assert.AreEqual(180, 0.0.SmallestDiffDegrees(180));

            Assert.AreEqual(45, 45.0.SmallestDiffDegrees(0));
            Assert.AreEqual(0, 45.0.SmallestDiffDegrees(45));
            Assert.AreEqual(-45, 45.0.SmallestDiffDegrees(90));
            Assert.AreEqual(-90, 45.0.SmallestDiffDegrees(135));
            Assert.AreEqual(-135, 45.0.SmallestDiffDegrees(180));
            Assert.AreEqual(180, 45.0.SmallestDiffDegrees(225));
            Assert.AreEqual(135, 45.0.SmallestDiffDegrees(270));
            Assert.AreEqual(90, 45.0.SmallestDiffDegrees(315));

            Assert.AreEqual(-45, 315.0.SmallestDiffDegrees(0));
            Assert.AreEqual(-90, 315.0.SmallestDiffDegrees(45));
            Assert.AreEqual(-135, 315.0.SmallestDiffDegrees(90));
            Assert.AreEqual(180, 315.0.SmallestDiffDegrees(135));
            Assert.AreEqual(135, 315.0.SmallestDiffDegrees(180));
            Assert.AreEqual(90, 315.0.SmallestDiffDegrees(225));
            Assert.AreEqual(45, 315.0.SmallestDiffDegrees(270));
            Assert.AreEqual(0, 315.0.SmallestDiffDegrees(315));
        }
    }
}