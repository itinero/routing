using System;
using Reminiscence.Arrays;

namespace Itinero
{
    /// <summary>
    /// Implementation of <see cref="IArrayFactory"/> which uses the default
    /// array types implemented and exposed in Reminiscence.
    /// </summary>
    public sealed class DefaultArrayFactory : IArrayFactory
    {
#if !HAS_NATIVE_MEMORY_ARRAY
        /// <inheritdoc />
        public ArrayBase<T> CreateMemoryBackedArray<T>(long size) => new MemoryArray<T>(size);
#else
        /// <summary>
        /// Gets or sets an <see cref="IUnmanagedMemoryAllocator"/> instance to use to allocate
        /// contiguous blocks of virtual memory.
        /// <para>
        /// The default is an allocator that uses the global heap, i.e., the trio of:
        /// <see cref="System.Runtime.InteropServices.Marshal.AllocHGlobal(IntPtr)"/>,
        /// <see cref="System.Runtime.InteropServices.Marshal.ReAllocHGlobal(IntPtr, IntPtr)"/>, and
        /// <see cref="System.Runtime.InteropServices.Marshal.FreeHGlobal(IntPtr)"/>.
        /// </para>
        /// </summary>
        public IUnmanagedMemoryAllocator UnmanagedMemoryAllocator { get; set; } = DefaultUnmanagedMemoryAllocator.Instance;

        // overwritten only by tests, temporarily, to validate behavior on 32-bit systems.
        internal bool Is32BitProcess { get; set; } = IntPtr.Size == 4;

        /// <inheritdoc />
        public ArrayBase<T> CreateMemoryBackedArray<T>(long size)
        {
            // 32-bit processes risk running out of contiguous virtual memory blocks big enough for
            // the kinds of arrays we use long before they risk running out of memory, so if the
            // native array would be larger than a single MemoryArray<T> block on a 32-bit process,
            // then fall back to MemoryArray<T> for safety.
            const long MemoryArrayThresholdFor32Bits = 1 << 20;
            if (Is32BitProcess && size > MemoryArrayThresholdFor32Bits)
            {
                return new MemoryArray<T>(size);
            }

            // NativeMemoryArray<T> is constrained to unmanaged types, so we have to do a bit of a
            // workaround in order to use it this way. Don't worry about performance of all the type
            // checks; they should happen at JIT-time, not at runtime.
            // RuntimeHelpers.IsReferenceOrContainsReferences<T>() is available in .NET Core 2.0 and
            // .NET Standard 2.1 which we could use as part of a solution that eliminates this stack
            // of copy-paste and hardcoded list of types.
            object arrayObj = null;
            if (typeof(T) == typeof(byte))
            {
                arrayObj = new NativeMemoryArray<byte>(this.UnmanagedMemoryAllocator, size);
            }
            else if (typeof(T) == typeof(sbyte))
            {
                arrayObj = new NativeMemoryArray<sbyte>(this.UnmanagedMemoryAllocator, size);
            }
            else if (typeof(T) == typeof(short))
            {
                arrayObj = new NativeMemoryArray<short>(this.UnmanagedMemoryAllocator, size);
            }
            else if (typeof(T) == typeof(ushort))
            {
                arrayObj = new NativeMemoryArray<ushort>(this.UnmanagedMemoryAllocator, size);
            }
            else if (typeof(T) == typeof(char))
            {
                arrayObj = new NativeMemoryArray<char>(this.UnmanagedMemoryAllocator, size);
            }
            else if (typeof(T) == typeof(int))
            {
                arrayObj = new NativeMemoryArray<int>(this.UnmanagedMemoryAllocator, size);
            }
            else if (typeof(T) == typeof(uint))
            {
                arrayObj = new NativeMemoryArray<uint>(this.UnmanagedMemoryAllocator, size);
            }
            else if (typeof(T) == typeof(long))
            {
                arrayObj = new NativeMemoryArray<long>(this.UnmanagedMemoryAllocator, size);
            }
            else if (typeof(T) == typeof(ulong))
            {
                arrayObj = new NativeMemoryArray<ulong>(this.UnmanagedMemoryAllocator, size);
            }
            else if (typeof(T) == typeof(float))
            {
                arrayObj = new NativeMemoryArray<float>(this.UnmanagedMemoryAllocator, size);
            }
            else if (typeof(T) == typeof(double))
            {
                arrayObj = new NativeMemoryArray<double>(this.UnmanagedMemoryAllocator, size);
            }
            // else it's either a compatible type that we never bothered to add (in which case,
            // please add it here!), or it is / contains references (in which case, we cannot return
            // NativeMemoryArray<T>, though a future specific optimization could handle it).

            return ((ArrayBase<T>)arrayObj) ?? new MemoryArray<T>(size);
        }
#endif
    }
}
