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

using System;
using System.Threading;

namespace Itinero.Algorithms
{
    /// <summary>
    /// Abstract representation of an algorithm.
    /// </summary>
    public abstract class AlgorithmBase : IAlgorithm
    {
        /// <summary>
        /// Returns true if this instance has run already.
        /// </summary>
        public bool HasRun
        {
            get;
            protected set;
        }

        /// <summary>
        /// Returns true if this instance has run and it was succesfull.
        /// </summary>
        public bool HasSucceeded
        {
            get;
            protected set;
        }

        /// <summary>
        /// Returns an error message when the algorithm was not successful.
        /// </summary>
        public string ErrorMessage
        {
            get;
            protected set;
        }

        /// <summary>
        /// Checks if HasRun is true and throw an exception if not.
        /// </summary>
        public void CheckHasRun()
        {
            if (!this.HasRun)
            {
                throw new Exception("No results available, Algorithm has not run yet!");
            }
        }

        /// <summary>
        /// Checks if HasRun and HasSucceeded is true and throws exception if not.
        /// </summary>
        public void CheckHasRunAndHasSucceeded()
        {
            this.CheckHasRun();

            if (!this.HasSucceeded)
            {
                throw new Exception("No results available, Algorithm was not successful!");
            }
        }

        /// <summary>
        /// Runs the algorithm.
        /// </summary>
        public void Run()
        {
            this.Run(CancellationToken.None);
        }

        /// <summary>
        /// Runs the algorithm.
        /// </summary>
        public void Run(CancellationToken cancellationToken)
        {
            if (this.HasRun)
            {
                throw new Exception("Algorithm has run already, use a new instance for each run. Use HasRun to check.");
            }
            this.DoRun(cancellationToken);
            this.HasRun = true;
        }

        /// <summary>
        /// Executes the actual run of the algorithm.
        /// </summary>
        protected abstract void DoRun(CancellationToken cancellationToken);
    }
}