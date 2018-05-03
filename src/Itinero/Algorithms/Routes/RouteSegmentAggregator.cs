/*
 *  Licensed to SharpSoftware under one or more contributor
 *  license agreements. See the NOTICE file distributed with this work for 
 *  additional information regarding copyright ownership.
 * 
 *  SharpSoftware licenses this file to you under the Apache License, 
 *  Version 2.0 (the "License"); you may not use this file except in 
 *  compliance with the License. You may obtain a copy of the License at
 * 
 *       http://www.apache.org/licenses/LICENSE-2.0
 * 
 *  Unless required by applicable law or agreed to in writing, software
 *  distributed under the License is distributed on an "AS IS" BASIS,
 *  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *  See the License for the specific language governing permissions and
 *  limitations under the License.
 */

using Itinero.Attributes;
using Itinero.LocalGeo;
using System;
using System.Collections.Generic;
using System.Threading;

namespace Itinero.Algorithms.Routes
{
    /// <summary>
    /// A route segment aggregator. Groups together segments and converts them to features.
    /// </summary>
    public class RouteSegmentAggregator : AlgorithmBase
    {
        private readonly Route _route;
        private readonly Func<Route.Meta, Route.Meta, Route.Meta> _aggregate;

        /// <summary>
        /// Creates a new route segment aggregator.
        /// </summary>
        public RouteSegmentAggregator(Route route, Func<Route.Meta, Route.Meta, Route.Meta> aggregate)
        {
            if (route == null) { throw new ArgumentNullException("route"); }
            if (aggregate == null) { throw new ArgumentNullException("aggregate"); }

            _route = route;
            _aggregate = aggregate;
        }

        private Route _aggregatedRoute;

        /// <summary>
        /// Gets the aggregated route.
        /// </summary>
        public Route AggregatedRoute
        {
            get
            {
                return _aggregatedRoute;
            }
        }

        /// <summary>
        /// Exectes the actual run of the algorithm.
        /// </summary>
        protected override void DoRun(CancellationToken cancellationToken)
        {
            if (_route.Shape == null || 
                _route.Shape.Length == 0 ||
                _route.ShapeMeta == null ||
                _route.ShapeMeta.Length == 0)
            {
                return;
            }

            _aggregatedRoute = new Route();
            if (_route.Attributes != null)
            {
                _aggregatedRoute.Attributes = new AttributeCollection(_route.Attributes);
            }
            _aggregatedRoute.Shape = _route.Shape.Clone() as Coordinate[];
            if (_route.Stops != null)
            {
                _aggregatedRoute.Stops = new Route.Stop[_route.Stops.Length];
                for(var s = 0; s < _route.Stops.Length; s++)
                {
                    _aggregatedRoute.Stops[s] = _route.Stops[s].Clone();
                }
            }
            if (_route.Branches != null)
            {
                _aggregatedRoute.Branches = new Route.Branch[_route.Branches.Length];
                for (var s = 0; s < _route.Branches.Length; s++)
                {
                    _aggregatedRoute.Branches[s] = _route.Branches[s].Clone();
                }
            }

            Route.Meta current = null;
            var metas = new List<Route.Meta>();
            if (_route.ShapeMeta[0].Shape == 0)
            {
                metas.Add(_route.ShapeMeta[0].Clone());
            }
            for (int i = 1; i < _route.ShapeMeta.Length; i++)
            {
                // try to aggregate.
                if (current == null)
                { // there is no current yet, set it.
                    current = _route.ShapeMeta[i].Clone();
                }
                else
                { // try to merge the current segment with the next one.
                    var aggregated = _aggregate(current, _route.ShapeMeta[i]); // expecting an already new object.
                    if (aggregated == null)
                    { // the current segment could not be merged with the next, add it to the final route.
                        metas.Add(current);
                        
                        current = _route.ShapeMeta[i].Clone();
                    }
                    else
                    { // keep the aggregated as current.
                        aggregated.Shape = _route.ShapeMeta[i].Shape; // make sure to set the shape-index correctly.
                        current = aggregated;
                    }
                }
            }

            if (current != null)
            { // add the final segment.
                metas.Add(current);
            }

            _aggregatedRoute.ShapeMeta = metas.ToArray();

            this.HasSucceeded = true;
        }
        
        /// <summary>
        /// A default function to aggregate per mode (or profile).
        /// </summary>
        public static Func<Route.Meta, Route.Meta, Route.Meta> ModalAggregator = (x, y) =>
            {
                var xProfile = string.Empty;
                var yProfile = string.Empty;
                if (x.Profile == y.Profile)
                {
                    var attributes = new AttributeCollection(x.Attributes);
                    attributes.AddOrReplace(y.Attributes);

                    return new Route.Meta()
                    {
                        Shape = y.Shape,
                        Attributes = attributes
                    };
                }
                return null;
            };
    }
}