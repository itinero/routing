// Itinero - Routing for .NET
// Copyright (C) 2016 Abelshausen Ben
// 
// This file is part of Itinero.
// 
// Itinero is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// Itinero is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Itinero. If not, see <http://www.gnu.org/licenses/>.

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