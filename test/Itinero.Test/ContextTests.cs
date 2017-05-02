using System;

using NUnit.Framework;
using Reminiscence.Arrays;

namespace Itinero.Test
{
    /// <summary>
    /// Tests which ensure that our <see cref="Context"/> is set up to ensure,
    /// by default, maximum backwards-compatibility at call sites that used to
    /// used previously inline the default implementations; anything that wants
    /// to make interesting use of <see cref="Context"/> needs to opt in.
    /// </summary>
    [TestFixture]
    public class ContextTests
    {
        /// <summary>
        /// The default factory in the context should be initialized properly.
        /// </summary>
        [Test]
        public void DefaultArrayFactoryShouldReturnProperMemoryBackedArray()
        {
            const long SizeToUse = 592;
            using (var array = Context.ArrayFactory.CreateMemoryBackedArray<Guid>(SizeToUse))
            {
                Assert.AreEqual(SizeToUse, array.Length, "Default returns an array that doesn't even obey the contract.");
                Assert.IsInstanceOf<MemoryArray<Guid>>(array, "Default should return a standard MemoryArray<T> to ensure perfect backwards-compatibility.");
            }
        }
    }
}
