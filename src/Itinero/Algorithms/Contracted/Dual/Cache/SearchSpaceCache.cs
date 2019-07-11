using System;
using Itinero.Algorithms.Collections;

namespace Itinero.Algorithms.Contracted.Dual.Cache
{
    /// <summary>
    /// A cache for search spaces.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class SearchSpaceCache<T>
        where T : struct
    {
        private readonly LRUCache<Tuple<uint, T, uint, T>, SearchSpace<T>> _forwardCache = new LRUCache<Tuple<uint, T, uint, T>, SearchSpace<T>>(65536);
        private readonly LRUCache<Tuple<uint, T, uint, T>, SearchSpace<T>> _backwardCache = new LRUCache<Tuple<uint, T, uint, T>, SearchSpace<T>>(65536);

        /// <summary>
        /// Tries to get a search space from cache.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="backward">The backward flag.</param>
        /// <param name="space">The search space, if any.</param>
        /// <returns>True if available.</returns>
        public bool TryGet(DykstraSource<T> source, bool backward, out SearchSpace<T> space)
        {
            var sourceTuple =
                new Tuple<uint, T, uint, T>(source.Vertex1, source.Weight1, source.Vertex2, source.Weight2);
            if (backward)
            {
                return _backwardCache.TryGet(sourceTuple, out space);
            }
            return _forwardCache.TryGet(sourceTuple, out space);
        }

        /// <summary>
        /// Adds a new search space to the cache.
        /// </summary>
        /// <param name="source">The source.</param>
        /// <param name="backward">The backward flag.</param>
        /// <param name="space">The search space.</param>
        public void Add(DykstraSource<T> source, bool backward, SearchSpace<T> space)
        {
            var sourceTuple =
                new Tuple<uint, T, uint, T>(source.Vertex1, source.Weight1, source.Vertex2, source.Weight2);
            if (backward)
            {
                _backwardCache.Add(sourceTuple, space);
                return;
            }
            _forwardCache.Add(sourceTuple, space);
        }
    }
}