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

using Itinero.Algorithms.Collections;
using Itinero.Algorithms.Weights;

namespace Itinero.Algorithms.Contracted.Dual
{
    /// <summary>
    /// Contains extension methods for weight handlers.
    /// </summary>
    public static class WeightHandlerExtensions
    {
        /// <summary>
        /// Returns an edge path for the path represented by the given pointer.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <returns></returns>
        public static EdgePath<T> GetPath<T>(this WeightHandler<T> weightHandler, PathTree pathTree, uint pointer)
            where T : struct
        {
            uint vertex, previous;
            T weight;
            weightHandler.GetPathTree(pathTree, pointer, out vertex, out weight, out previous);
            if (previous == uint.MaxValue)
            {
                return new EdgePath<T>(vertex);
            }
            var previousPath = weightHandler.GetPath(pathTree, previous);
            return new EdgePath<T>(vertex, weight, previousPath);
        }
    }
}
