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

using Itinero.Graphs.Geometric;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Itinero.Profiles
{
    /// <summary>
    /// A profile factor and speed cache.
    /// </summary>
    public class ProfileFactorAndSpeedCache
    {
        private readonly RouterDb _db;
        private readonly Dictionary<string, CachedAndSpeedFactor[]> _edgeProfileFactors;

        /// <summary>
        /// A profile factor cache.
        /// </summary>
        public ProfileFactorAndSpeedCache(RouterDb db)
        {
            _db = db;
            _edgeProfileFactors = new Dictionary<string, CachedAndSpeedFactor[]>();
        }

        /// <summary>
        /// Gets the router db.
        /// </summary>
        public RouterDb RouterDb
        {
            get
            {
                return _db;
            }
        }

        /// <summary>
        /// Returns true if all the given profiles are cached and supported.
        /// </summary>
        public bool ContainsAll(params ProfileDefinition[] profileDefinitions)
        {
            for(var p = 0; p < profileDefinitions.Length; p++)
            {
                if(!_edgeProfileFactors.ContainsKey(profileDefinitions[p].Name))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Returns true if all the given profiles are cached and supported.
        /// </summary>
        public bool ContainsAll(params Profile[] profiles)
        {
            for (var p = 0; p < profiles.Length; p++)
            {
                if (!_edgeProfileFactors.ContainsKey(profiles[p].Definition.Name))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Calculates for all registered profiles.
        /// </summary>
        public void CalculateForAll()
        {
            this.CalculateFor(ProfileDefinition.GetAllRegistered().ToArray());
        }

        /// <summary>
        /// Precalculates speed factors for all the given profiles.
        /// </summary>
        public void CalculateFor(params Profile[] profiles)
        {
            var dictionary = new Dictionary<string, ProfileDefinition>();
            foreach(var profile in profiles)
            {
                dictionary[profile.Definition.Name] = profile.Definition;
            }
            this.CalculateFor(dictionary.Values.ToArray());
        }

        /// <summary>
        /// Precalculates speed factors for all the given profiles.
        /// </summary>
        public void CalculateFor(params ProfileDefinition[] profileDefinitions)
        {
            var edgeProfileFactors = new CachedAndSpeedFactor[profileDefinitions.Length][];
            for (var p = 0; p < profileDefinitions.Length; p++)
            {
                edgeProfileFactors[p] = new CachedAndSpeedFactor[(int)_db.EdgeProfiles.Count];
            }
            for (uint edgeProfile = 0; edgeProfile < _db.EdgeProfiles.Count; edgeProfile++)
            {
                var edgeProfileTags = _db.EdgeProfiles.Get(edgeProfile);
                for (var p = 0; p < profileDefinitions.Length; p++)
                {
                    var factor = profileDefinitions[p].Factor(edgeProfileTags);
                    var speed = profileDefinitions[p].Speed(edgeProfileTags);
                    var stoppable = profileDefinitions[p].CanStopOn(edgeProfileTags);
                    if(stoppable)
                    {
                        edgeProfileFactors[p][edgeProfile] = new CachedAndSpeedFactor()
                        {
                            Type = factor.Item1.Direction,
                            SpeedFactor = 1 / speed.Item1.Value,
                            Value = factor.Item1.Value,
                            Constraints = factor.Item2
                        };
                    }
                    else
                    {
                        edgeProfileFactors[p][edgeProfile] = new CachedAndSpeedFactor()
                        {
                            Type = (short)(factor.Item1.Direction + 4),
                            SpeedFactor = 1 / speed.Item1.Value,
                            Value = factor.Item1.Value,
                            Constraints = factor.Item2
                        };
                    }
                }
            }

            for (var p = 0; p < profileDefinitions.Length; p++)
            {
                _edgeProfileFactors[profileDefinitions[p].Name] = edgeProfileFactors[p];
            }
        }

        /// <summary>
        /// Returns an isacceptable function using the cached data.
        /// </summary>
        public Func<GeometricEdge, bool> GetIsAcceptable(bool verifyCanStopOn, params Profile[] profiles)
        {
            if(!this.ContainsAll(profiles)) { throw new ArgumentException("Not all given profiles are supported."); }

            var cachedFactors = new CachedAndSpeedFactor[profiles.Length][];
            for(var p = 0; p < profiles.Length; p++)
            {
                cachedFactors[p] = _edgeProfileFactors[profiles[p].Name];
            }

            return (edge) =>
            {
                float distance;
                ushort edgeProfileId;
                Data.Edges.EdgeDataSerializer.Deserialize(edge.Data[0],
                    out distance, out edgeProfileId);
                for (var i = 0; i < profiles.Length; i++)
                {
                    var cachedFactor = cachedFactors[i][edgeProfileId];
                    var profileFactor = profiles[i].Factor(cachedFactor.GetFactor(), cachedFactor.Constraints);
                    if ((verifyCanStopOn && cachedFactor.Type > 4) ||
                        profileFactor.Value <= 0)
                    {
                        return false;
                    }
                }
                return true;
            };
        }

        /// <summary>
        /// Gets the get factor function for the given profile.
        /// </summary>
        public Func<ushort, Factor> GetGetFactor(Profile profile)
        {
            if (!this.ContainsAll(profile)) { throw new ArgumentException("Given profile not supported."); }

            var cachedFactors = _edgeProfileFactors[profile.Name];
            return (p) =>
            {
                var cachedFactor = cachedFactors[p];
                return profile.Factor(cachedFactor.GetFactor(), cachedFactor.Constraints);
            };
        }

        /// <summary>
        /// Gets the get factor function for the given profile.
        /// </summary>
        public Func<ushort, FactorAndSpeed> GetGetFactorAndSpeed(Profile profile)
        {
            if (!this.ContainsAll(profile)) { throw new ArgumentException("Given profile not supported."); }

            var cachedFactors = _edgeProfileFactors[profile.Name];
            return (p) =>
            {
                var cachedFactor = cachedFactors[p];
                return profile.FactorAndSpeed(cachedFactor.GetFactorAndSpeed(), cachedFactor.Constraints);
            };
        }

        /// <summary>
        /// Returns the cached factor.
        /// </summary>
        public Factor GetFactor(ushort edgeProfile, Profile profile)
        {
            CachedAndSpeedFactor[] factorsForProfile;
            if (!_edgeProfileFactors.TryGetValue(profile.Definition.Name, out factorsForProfile))
            {
                throw new ArgumentOutOfRangeException(string.Format("{0} not found.", profile.Definition.Name));
            }
            if (edgeProfile < factorsForProfile.Length)
            {
                var cachedFactor = factorsForProfile[edgeProfile];
                return profile.Factor(cachedFactor.GetFactor(), cachedFactor.Constraints);
            }
            throw new ArgumentOutOfRangeException("Edgeprofile invalid.");
        }

        /// <summary>
        /// Returns the cached factor and speed.
        /// </summary>
        public FactorAndSpeed GetFactorAndSpeed(ushort edgeProfile, Profile profile)
        {
            CachedAndSpeedFactor[] factorsForProfile;
            if (!_edgeProfileFactors.TryGetValue(profile.Definition.Name, out factorsForProfile))
            {
                throw new ArgumentOutOfRangeException(string.Format("{0} not found.", profile.Definition.Name));
            }
            if (edgeProfile < factorsForProfile.Length)
            {
                var cachedFactor = factorsForProfile[edgeProfile];
                return profile.FactorAndSpeed(cachedFactor.GetFactorAndSpeed(), cachedFactor.Constraints);
            }
            throw new ArgumentOutOfRangeException("Edgeprofile invalid.");
        }

        /// <summary>
        /// Returns true if the given edge can be stopped on.
        /// </summary>
        public bool CanStopOn(ushort edgeProfile, Profile profile)
        {
            CachedAndSpeedFactor[] factorsForProfile;
            if(!_edgeProfileFactors.TryGetValue(profile.Definition.Name, out factorsForProfile))
            {
                throw new ArgumentOutOfRangeException(string.Format("{0} not found.", profile.Definition.Name));
            }
            if (edgeProfile < factorsForProfile.Length)
            {
                var cachedProfile = factorsForProfile[edgeProfile];
                var profileFactor = profile.Factor(cachedProfile.GetFactor(), cachedProfile.Constraints);
                return factorsForProfile[edgeProfile].Type < 4 &&
                    profileFactor.Value > 0;
            }
            throw new ArgumentOutOfRangeException("Edgeprofile invalid.");
        }

        private struct CachedAndSpeedFactor
        {
            /// <summary>
            /// Gets or sets the actual factor.
            /// </summary>
            public float Value { get; set; }

            /// <summary>
            /// Gets or sets the speed factor.
            /// </summary>
            public float SpeedFactor { get; set; }

            /// <summary>
            /// Gets or sets the direction.
            /// </summary>
            /// 0=bidirectional and stoppable, 1=forward and stoppable, 2=backward and stoppable,
            /// 4=bidirectional and not stoppable, 5=forward and not stoppable, 6=backward and not stoppable
            public short Type { get; set; }

            /// <summary>
            /// Gets or sets the constraints.
            /// </summary>
            public float[] Constraints { get; set; }

            /// <summary>
            /// Gets the factor.
            /// </summary>
            public Factor GetFactor()
            {
                if (this.Type >= 4)
                {
                    return new Factor()
                    {
                        Direction = (short)(this.Type << 2),
                        Value = this.Value
                    };
                }
                return new Factor()
                {
                    Direction = this.Type,
                    Value = this.Value
                };
            }

            /// <summary>
            /// Gets the factor and speed.
            /// </summary>
            /// <returns></returns>
            public FactorAndSpeed GetFactorAndSpeed()
            {
                if (this.Type >= 4)
                {
                    return new FactorAndSpeed()
                    {
                        Direction = (short)(this.Type << 2),
                        SpeedFactor = this.SpeedFactor,
                        Value = this.Value
                    };
                }
                return new FactorAndSpeed()
                {
                    Direction = this.Type,
                    SpeedFactor = this.SpeedFactor,
                    Value = this.Value
                };
            }
        }
    }
}