using Reminiscence.Arrays;

namespace Itinero
{
    /// <summary>
    /// Factory for creating arrays.  These "arrays" are not your typical CLR
    /// <c>T[]</c>.  They act the same for the most part, but they can be much
    /// larger and do not necessarily have to be stored contiguously, nor even
    /// fully in main memory.
    /// </summary>
    public interface IArrayFactory
    {
        /// <summary>
        /// Creates an <see cref="ArrayBase{T}"/> fully backed by main memory,
        /// with an initial capacity set to a given number of elements.
        /// </summary>
        /// <typeparam name="T">
        /// The type of element stored in the array.
        /// </typeparam>
        /// <param name="size">
        /// The number of elements that the array needs to fit.
        /// </param>
        /// <returns>
        /// An <see cref="ArrayBase{T}"/> fully backed by main memory, with its
        /// size set to <paramref name="size"/> elements.
        /// </returns>
        ArrayBase<T> CreateMemoryBackedArray<T>(long size);
    }
}
