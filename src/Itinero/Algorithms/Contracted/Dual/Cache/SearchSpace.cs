using System;
using System.Collections.Generic;
using Itinero.Algorithms.Collections;

namespace Itinero.Algorithms.Contracted.Dual.Cache
{
    /// <summary>
    /// A search space.
    /// </summary>
    public class SearchSpace<T>
    {
        /// <summary>
        /// The visit tree.
        /// </summary>
        public PathTree Tree { get; set; }
        
        /// <summary>
        /// Gets or sets the visits per vertex.
        /// </summary>
        public Dictionary<uint, Tuple<uint, T>> Visits { get; set; }    
        
        /// <summary>
        /// Gets or sets the visits in one set.
        /// </summary>
        public HashSet<uint> VisitSet { get; set; }      
    }
}