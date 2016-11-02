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
using System.IO;
using System.Text;

namespace Itinero.Profiles
{
    /// <summary>
    /// Represents a profile.
    /// </summary>
    public class Profile
    {
        private readonly ProfileDefinition _profile;
        private readonly Constraint[] _constraints;

        /// <summary>
        /// Creates a new routing profile.
        /// </summary>
        public Profile(ProfileDefinition profile, Constraint[] constraints)
        {
            if (_constraints != null && _constraints.Length > 255) { throw new ArgumentException("Maximum 255 constraints allowed."); }

            _profile = profile;
            _constraints = constraints;
        }

        /// <summary>
        /// Verifies the constraints given the constraint values.
        /// </summary>
        public bool VerifyConstraints(float[] constraints)
        {
            if (constraints != null &&
                _constraints != null)
            {
                for (var i = 0; i < _constraints.Length && i < constraints.Length; i++)
                {
                    if (_constraints[i].Evaluate(constraints[i]))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Returns the multiplication factor taking into account the given constraints.
        /// </summary>
        public virtual Factor Factor(Factor factor, float[] constraints)
        {
            if (!VerifyConstraints(constraints))
            {
                return Profiles.Factor.NoFactor;
            }
            return factor;
        }

        /// <summary>
        /// Returns the speed taking into account the given constraints.
        /// </summary>
        public virtual FactorAndSpeed FactorAndSpeed(FactorAndSpeed factorAndSpeed, float[] constraints)
        {
            if (!VerifyConstraints(constraints))
            {
                return Profiles.FactorAndSpeed.NoFactor;
            }
            return factorAndSpeed;
        }

        /// <summary>
        /// Returns the speed taking into account the given constraints.
        /// </summary>
        public virtual Speed Speed(Speed speed, float[] constraints)
        {
            if (!VerifyConstraints(constraints))
            {
                return Profiles.Speed.NoSpeed;
            }
            return speed;
        }

        /// <summary>
        /// Gets the profile this is an instance for.
        /// </summary>
        public ProfileDefinition Definition
        {
            get
            {
                return _profile;
            }
        }

        /// <summary>
        /// Gets a unique description of this profile.
        /// </summary>
        public string Name
        {
            get
            {
                var stringBuilder = new StringBuilder();
                stringBuilder.Append(_profile.Name);
                if (_constraints != null)
                {
                    stringBuilder.Append("_[");
                    for (var i = 0; i < _constraints.Length; i++)
                    {
                        if (i > 0)
                        {
                            stringBuilder.Append(',');
                        }
                        if (_constraints[i] != null)
                        {
                            stringBuilder.Append(_constraints[i].Description);
                        }
                        else
                        {
                            stringBuilder.Append("null");
                        }
                    }
                    stringBuilder.Append(']');
                }
                return stringBuilder.ToInvariantString();
            }
        }

        /// <summary>
        /// Serializes this profile.
        /// </summary>
        public long Serialize(Stream stream)
        {
            long size = 0;
            size += stream.WriteWithSize(this.Definition.Name);
            var count = _constraints == null ? 0 : _constraints.Length;
            stream.WriteByte((byte)count);
            size += 4;
            for (var i = 0; i < count; i++)
            {
                if (_constraints[i] == null)
                {
                    size += Constraint.SerializeNull(stream);
                }
                else
                {
                    size += _constraints[i].Serialize(stream);
                }
            }
            return size;
        }

        /// <summary>
        /// Deserializes this stream.
        /// </summary>
        public static Profile Deserialize(Stream stream)
        {
            var name = stream.ReadWithSizeString();
            var profileDefinition = ProfileDefinition.Get(name);
            var count = stream.ReadByte();
            var constraints = new Constraint[count];
            for(var i = 0; i < constraints.Length; i++)
            {
                constraints[i] = Constraint.Deserialize(stream);
            }

            return new Profile(profileDefinition, constraints);
        }
    }
}