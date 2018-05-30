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
        /// Tests converting to radians.
        /// </summary>
        [Test]
        public void TestToRadians()
        {
            Assert.AreEqual(System.Math.PI * 0 / 2, 000.0f.ToRadians(), 0.00001);
            Assert.AreEqual(System.Math.PI * 1 / 2, 090.0f.ToRadians(), 0.00001);
            Assert.AreEqual(System.Math.PI * 2 / 2, 180.0f.ToRadians(), 0.00001);
            Assert.AreEqual(System.Math.PI * 3 / 2, 270.0f.ToRadians(), 0.00001);
            Assert.AreEqual(System.Math.PI * 4 / 2, 360.0f.ToRadians(), 0.00001);

            Assert.AreEqual(System.Math.PI * 0 / 2, 000.0.ToRadians(), 0.00001);
            Assert.AreEqual(System.Math.PI * 1 / 2, 090.0.ToRadians(), 0.00001);
            Assert.AreEqual(System.Math.PI * 2 / 2, 180.0.ToRadians(), 0.00001);
            Assert.AreEqual(System.Math.PI * 3 / 2, 270.0.ToRadians(), 0.00001);
            Assert.AreEqual(System.Math.PI * 4 / 2, 360.0.ToRadians(), 0.00001);
        }

        /// <summary>
        /// Tests converting to degrees.
        /// </summary>
        [Test]
        public void TestToDegrees()
        {
            Assert.AreEqual(000.0, (System.Math.PI * 0 / 2).ToDegrees(), 0.00001);
            Assert.AreEqual(090.0, (System.Math.PI * 1 / 2).ToDegrees(), 0.00001);
            Assert.AreEqual(180.0, (System.Math.PI * 2 / 2).ToDegrees(), 0.00001);
            Assert.AreEqual(270.0, (System.Math.PI * 3 / 2).ToDegrees(), 0.00001);
            Assert.AreEqual(360.0, (System.Math.PI * 4 / 2).ToDegrees(), 0.00001);

            Assert.AreEqual(000.0f, ((float)System.Math.PI * 0 / 2).ToDegrees(), 0.0001);
            Assert.AreEqual(090.0f, ((float)System.Math.PI * 1 / 2).ToDegrees(), 0.0001);
            Assert.AreEqual(180.0f, ((float)System.Math.PI * 2 / 2).ToDegrees(), 0.0001);
            Assert.AreEqual(270.0f, ((float)System.Math.PI * 3 / 2).ToDegrees(), 0.0001);
            Assert.AreEqual(360.0f, ((float)System.Math.PI * 4 / 2).ToDegrees(), 0.0001);
        }

        /// <summary>
        /// Tests normalizing degrees.
        /// </summary>
        [Test]
        public void TestNormalizeDegrees()
        {
            Assert.AreEqual(0, (0 + (1 * 360.0)).NormalizeDegrees(), 0.00001);
            Assert.AreEqual(36, (36 + (10 * 360.0)).NormalizeDegrees(), 0.00001);
            Assert.AreEqual(359, (359 + (11 * 360.0)).NormalizeDegrees(), 0.00001);
            
            Assert.AreEqual(360 - 36, (-36.0).NormalizeDegrees(), 0.00001);
            Assert.AreEqual(360 - 359, (-359.0).NormalizeDegrees(), 0.00001);
        }
        
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