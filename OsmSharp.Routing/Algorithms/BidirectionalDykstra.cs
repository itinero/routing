// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2015 Abelshausen Ben
// 
// This file is part of OsmSharp.
// 
// OsmSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// OsmSharp is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with OsmSharp. If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;

namespace OsmSharp.Routing.Algorithms
{
    /// <summary>
    /// An implementation of the bi-directional dykstra algorithm.
    /// </summary>
    public class BidirectionalDykstra : AlgorithmBase
    {
        private readonly Dykstra _sourceSearch;
        private readonly Dykstra _targetSearch;

        /// <summary>
        /// Creates a new instance of search algorithm.
        /// </summary>
        public BidirectionalDykstra(Dykstra sourceSearch, Dykstra targetSearch)
        {
            _sourceSearch = sourceSearch;
            _targetSearch = targetSearch;
        }

        private uint _bestVertex = uint.MaxValue;
        private float _bestWeight = float.MaxValue;
        private float _maxForward = float.MaxValue;
        private float _maxBackward = float.MaxValue;

        /// <summary>
        /// Executes the algorithm.
        /// </summary>
        protected override void DoRun()
        {
            _bestVertex = uint.MaxValue;
            _bestWeight = float.MaxValue;
            _maxForward = float.MinValue;
            _maxBackward = float.MinValue;
            _sourceSearch.WasFound = (vertex, weight) =>
            {
                _maxForward = weight;
                return false;
            };
            _targetSearch.WasFound = (vertex, weight) =>
            {
                _maxBackward = weight;
                return this.ReachedVertexBackward((uint)vertex, weight);
            };
            _sourceSearch.Initialize();
            _targetSearch.Initialize();
            var source = true;
            var target = true;
            while (source || target)
            {
                source = false;
                if (_maxForward < _bestWeight)
                { // still a need to search, not best found or max < best.
                    source = _sourceSearch.Step();
                }
                target = false;
                if (_maxBackward < _bestWeight)
                { // still a need to search, not best found or max < best.
                    target = _targetSearch.Step();
                }
            }
        }

        /// <summary>
        /// Called when a vertex was reached during a backward search.
        /// </summary>
        /// <returns></returns>
        private bool ReachedVertexBackward(uint vertex, float weight)
        {
            // check forward search for the same vertex.
            Path forwardVisit;
            if (_sourceSearch.TryGetVisit(vertex, out forwardVisit))
            { // there is a status for this vertex in the source search.
                weight = weight + forwardVisit.Weight;
                if (weight < _bestWeight)
                { // this vertex is a better match.
                    _bestWeight = weight;
                    _bestVertex = vertex;
                    this.HasSucceeded = true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns the source-search algorithm.
        /// </summary>
        public Dykstra SourceSearch
        {
            get
            {
                return _sourceSearch;
            }
        }

        /// <summary>
        /// Returns the target-search algorithm.
        /// </summary>
        public Dykstra TargetSearch
        {
            get
            {
                return _targetSearch;
            }
        }

        /// <summary>
        /// Gets the best vertex.
        /// </summary>
        public uint BestVertex
        {
            get
            {
                this.CheckHasRunAndHasSucceeded();

                return _bestVertex;
            }
        }

        /// <summary>
        /// Gets the path from source->target.
        /// </summary>
        /// <returns></returns>
        public List<uint> GetPath()
        {
            this.CheckHasRunAndHasSucceeded();

            Path fromSource;
            Path toTarget;
            if(_sourceSearch.TryGetVisit(_bestVertex, out fromSource) &&
               _targetSearch.TryGetVisit(_bestVertex, out toTarget))
            {
                var path = new List<uint>();
                fromSource.AddToList(path);
                if (toTarget.From != null)
                {
                    toTarget.From.AddToListReverse(path);
                }
                return path;
            }
            throw new InvalidOperationException("No path could be found to/from source/target.");
        }
    }
}