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

using Itinero.Algorithms.Contracted;
using System;
using System.Collections.Generic;

namespace Itinero.Algorithms.Dual
{
    /// <summary>
    /// Contains extension methods related to contracted bidirectional dykstra.
    /// </summary>
    public static class BidirectionalDykstraExtensions
    {
        /// <summary>
        /// Returns the path.
        /// </summary>
        /// <returns></returns>
        public static List<uint> GetDualPath<T>(this BidirectionalDykstra<T> dykstra)
            where T : struct
        {
            dykstra.CheckHasRunAndHasSucceeded();

            EdgePath<T> fromSource;
            EdgePath<T> toTarget;
            if (dykstra.TryGetForwardVisit(dykstra.Best, out fromSource) &&
                dykstra.TryGetBackwardVisit(dykstra.Best, out toTarget))
            {
                var vertices = new List<uint>();

                // add vertices from source.
                vertices.Add(fromSource.Vertex);
                while (fromSource.From != null)
                {
                    if (fromSource.From.Vertex != Constants.NO_VERTEX)
                    { // this should be the end of the path.
                        if (fromSource.Edge == Constants.NO_EDGE)
                        { // only expand when there is no edge id.
                            dykstra.Graph.ExpandEdge(fromSource.From.Vertex, fromSource.Vertex, vertices, false, true);
                        }
                    }
                    vertices.Add(fromSource.From.Vertex);
                    fromSource = fromSource.From;
                }
                vertices.Reverse();

                // and add vertices to target.
                while (toTarget.From != null)
                {
                    if (toTarget.From.Vertex != Constants.NO_VERTEX)
                    { // this should be the end of the path.
                        if (toTarget.Edge == Constants.NO_EDGE)
                        { // only expand when there is no edge id.
                            dykstra.Graph.ExpandEdge(toTarget.From.Vertex, toTarget.Vertex, vertices, false, false);
                        }
                    }
                    vertices.Add(toTarget.From.Vertex);
                    toTarget = toTarget.From;
                }

                return vertices;
            }
            throw new InvalidOperationException("No path could be found to/from source/target.");
        }
    }
}
