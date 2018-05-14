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

using Itinero.Algorithms;
using Itinero.Algorithms.Search;
using System.Threading;

namespace Itinero.Test.Algorithms.Search
{
    /// <summary>
    /// A mock resolver.
    /// </summary>
    class MockResolver : AlgorithmBase, IResolver
    {
        private readonly RouterPoint _result;

        public MockResolver(RouterPoint result)
        {
            _result = result;
        }

        public RouterPoint Result
        {
            get { return _result; }
        }

        protected override void DoRun(CancellationToken cancellationToken)
        {
            if(_result != null)
            {
                this.HasSucceeded = true;
                return;
            }
            this.ErrorMessage = "Cannot resolve.";
            this.HasSucceeded = false;
            return;
        }
    }
}