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
        [TestCase(default(byte))]
        [TestCase(default(sbyte))]
        [TestCase(default(short))]
        [TestCase(default(ushort))]
        [TestCase(default(char))]
        [TestCase(default(int))]
        [TestCase(default(uint))]
        [TestCase(default(long))]
        [TestCase(default(ulong))]
        [TestCase(default(double))]
        [TestCase(default(float))]
        public void DefaultArrayFactoryShouldReturnNativeMemoryArrayWhenAppropriate<T>(T exemplar)
        {
            const long SmallSize = 592;
            const long LargeSize = 1 << 20 + 1;

            if (!(Context.ArrayFactory is DefaultArrayFactory defaultArrayFactory))
            {
                Assert.Inconclusive("Need the default to be a DefaultArrayFactory.");
                return;
            }

            Type nativeMemoryArrayType = typeof(NativeMemoryArray<>).MakeGenericType(typeof(T));

            bool old32BitProcessValue = defaultArrayFactory.Is32BitProcess;
            try
            {
                // test behavior in a 32-bit process for the first round of tests.
                defaultArrayFactory.Is32BitProcess = true;

                // test 32-bit with a huge block
                using (var array = defaultArrayFactory.CreateMemoryBackedArray<T>(LargeSize))
                {
                    Assert.AreEqual(LargeSize, array.Length);
                    Assert.IsInstanceOf<MemoryArray<T>>(array, "32-bit processes are expected to have virtual memory that's more cramped, so we should use MemoryArray<T>'s chunked allocator.");
                }

                // test 32-bit with a small block
                using (var array = defaultArrayFactory.CreateMemoryBackedArray<T>(SmallSize))
                {
                    Assert.AreEqual(SmallSize, array.Length);
                    Assert.IsInstanceOf(nativeMemoryArrayType, array, "MemoryArray<T> would allocate this in a single block anyway, so there's no reason not to use NativeMemoryArray<T>.");
                }

                // test behavior in a 64-bit process for the next round of tests.
                defaultArrayFactory.Is32BitProcess = false;

                // test 64-bit with a small block
                using (var array = defaultArrayFactory.CreateMemoryBackedArray<T>(SmallSize))
                {
                    Assert.AreEqual(SmallSize, array.Length);
                    Assert.IsInstanceOf(nativeMemoryArrayType, array, "64-bit should always use NativeMemoryArray<T> for compatible types when available.");
                }

                // test 64-bit with a huge block
                using (var array = defaultArrayFactory.CreateMemoryBackedArray<T>(LargeSize))
                {
                    Assert.AreEqual(LargeSize, array.Length);
                    Assert.IsInstanceOf(nativeMemoryArrayType, array, "64-bit should always use NativeMemoryArray<T> for compatible types when available.");
                }
            }
            finally
            {
                defaultArrayFactory.Is32BitProcess = old32BitProcessValue;
            }
        }

        [Test]
        public void DefaultArrayFactoryShouldNeverUseNativeMemoryArrayForIncompatibleType()
        {
            using (var array = Context.ArrayFactory.CreateMemoryBackedArray<object>(1))
            {
                Assert.IsInstanceOf<MemoryArray<object>>(array, "Never use NativeMemoryArray<T> for arbitrary reference types.");
            }

            using (var array = Context.ArrayFactory.CreateMemoryBackedArray<StructWithReferences>(1))
            {
                Assert.IsInstanceOf<MemoryArray<StructWithReferences>>(array, "Never use NativeMemoryArray<T> for structs that embed arbitrary reference types.");
            }
        }

        private struct StructWithReferences
        {
            private object obj;
        }
    }
}
