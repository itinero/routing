using Itinero.IO.Osm.Streams;
using System;
using System.Collections.Generic;
using OsmSharp;
using Itinero.Algorithms.Collections;
using OsmSharp.Tags;
using System.Linq;

namespace Itinero.IO.Osm.Restrictions
{
    /// <summary>
    /// An osm-data processor to process node based restrictions.
    /// </summary>
    /// <remarks>
    /// This is here to ensure backwards compat with non-lua based profiles nothing else.
    /// </remarks>
    public class NodeRestrictionProcessor : ITwoPassProcessor
    {
        private readonly Action<string, List<uint>> _foundRestriction; // restriction found action.
        private readonly Func<Node, uint> _markCore; // marks the node as core.

        /// <summary>
        /// Creates a new restriction processor.
        /// </summary>
        public NodeRestrictionProcessor(Func<Node, uint> markCore, Action<string, List<uint>> foundRestriction)
        {
            _foundRestriction = foundRestriction;
            _markCore = markCore;
        }
        
        /// <summary>
        /// Processes the given way in the first pass.
        /// </summary>
        public void FirstPass(Way way)
        {

        }

        /// <summary>
        /// Processes the given relation in the first pass.
        /// </summary>
        public bool FirstPass(Relation relation)
        {
            return false;
        }

        /// <summary>
        /// Processes the given node in the second pass.
        /// </summary>
        public void SecondPass(Node node)
        {
            if (node.Tags != null &&
                (node.Tags.Contains("barrier", "bollard") ||
                 node.Tags.Contains("barrier", "fence") ||
                 node.Tags.Contains("barrier", "gate")))
            {
                var vertex = _markCore(node);
                if (vertex != Itinero.Constants.NO_VERTEX)
                {
                    var r = new List<uint>();
                    r.Add(vertex);
                    _foundRestriction("motorcar", r);
                }
            }
        }

        /// <summary>
        /// Processes the given way in the second pass.
        /// </summary>
        public void SecondPass(Way way)
        {
            
        }

        /// <summary>
        /// Processes the given relation in the second pass.
        /// </summary>
        public void SecondPass(Relation relation)
        {
            
        }
    }
}