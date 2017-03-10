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

namespace Itinero.Profiles
{
    /// <summary>
    /// Abstract definition of a profile instance.
    /// </summary>
    public interface IProfileInstance
    {
        /// <summary>
        /// Gets the profile.
        /// </summary>
        Profile Profile
        {
            get;
        }

        /// <summary>
        /// Gets the constraint boundaries.
        /// </summary>
        float[] Constraints
        {
            get;
        }
    }

    class ProfileInstance : IProfileInstance
    {
        private readonly Profile _profile;
        private readonly float[] _constraints;

        /// <summary>
        /// Creates a new profile instance.
        /// </summary>
        public ProfileInstance(Profile profile, float[] constraints)
        {
            _profile = profile;
            _constraints = constraints;
        }

        /// <summary>
        /// Gets the constraints.
        /// </summary>
        public float[] Constraints
        {
            get
            {
                return _constraints;
            }
        }

        /// <summary>
        /// Gets the profile.
        /// </summary>
        public Profile Profile
        {
            get
            {
                return _profile;
            }
        }
    }
}