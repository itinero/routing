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

namespace Itinero.Algorithms.Networks.Analytics
{
    /// <summary>
    /// Abstract representation of an algorithm that generates edge visits.
    /// </summary>
    public interface IEdgeVisitor<T> : IAlgorithm
    {
        /// <summary>
        /// Gets or sets the visit delegate.
        /// </summary>
        VisitDelegate<T> Visit { get; set; }
    }

    /// <summary>
    /// A delegate that defines a visit.
    /// </summary>
    public delegate bool VisitDelegate<T>(EdgePath<T> path);
}