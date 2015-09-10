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

namespace OsmSharp.Routing.Algorithms
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
        /// Checks if HasRun is true and throw an exception if not.
        /// </summary>
        protected void CheckHasRun()
        {
            if (!this.HasRun)
            {
                throw new Exception("No results available, Algorithm has not run yet!");
            }
        }

        /// <summary>
        /// Checks if HasRun and HasSucceeded is true and throws exception if not.
        /// </summary>
        protected void CheckHasRunAndHasSucceeded()
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
            if (this.HasRun)
            {
                throw new Exception("Algorithm has run already, use a new instance for each run. Use HasRun to check.");
            }
            this.DoRun();
            this.HasRun = true;
        }

        /// <summary>
        /// Executes the actual run of the algorithm.
        /// </summary>
        protected abstract void DoRun();
    }
}