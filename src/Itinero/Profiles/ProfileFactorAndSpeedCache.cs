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
        private readonly Dictionary<string, FactorAndSpeed[]> _edgeProfileFactors;

        /// <summary>
        /// A profile factor cache.
        /// </summary>
        public ProfileFactorAndSpeedCache(RouterDb db)
        {
            _db = db;
            _edgeProfileFactors = new Dictionary<string, FactorAndSpeed[]>();
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
        public bool ContainsAll(params Profile[] profiles)
        {
            for(var p = 0; p< profiles.Length; p++)
            {
                if(!_edgeProfileFactors.ContainsKey(profiles[p].Name))
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
            this.CalculateFor(Profile.GetRegistered().ToArray());
        }

        /// <summary>
        /// Precalculates speed factors for all the given profiles.
        /// </summary>
        public void CalculateFor(params Profile[] profiles)
        {
            var edgeProfileFactors = new FactorAndSpeed[profiles.Length][];
            for (var p = 0; p < profiles.Length; p++)
            {
                edgeProfileFactors[p] = new FactorAndSpeed[(int)_db.EdgeProfiles.Count];
            }
            for (uint edgeProfile = 0; edgeProfile < _db.EdgeProfiles.Count; edgeProfile++)
            {
                var edgeProfileTags = _db.EdgeProfiles.Get(edgeProfile);
                for (var p = 0; p < profiles.Length; p++)
                {
                    edgeProfileFactors[p][edgeProfile] 
                        = profiles[p].FactorAndSpeed(edgeProfileTags);
                }
            }

            for (var p = 0; p < profiles.Length; p++)
            {
                _edgeProfileFactors[profiles[p].Name] = edgeProfileFactors[p];
            }
        }

        /// <summary>
        /// Returns an isacceptable function using the cached data.
        /// </summary>
        public Func<GeometricEdge, bool> GetIsAcceptable(bool verifyCanStopOn, params Profile[] profiles)
        {
            if(!this.ContainsAll(profiles)) { throw new ArgumentException("Not all given profiles are supported."); }

            var cachedFactors = new FactorAndSpeed[profiles.Length][];
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
                    if ((verifyCanStopOn && cachedFactor.Direction > 4) || 
                        cachedFactor.Value <= 0)
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
                if (cachedFactor.Direction >= 4)
                {
                    return new Factor()
                    {
                        Direction = (short)(cachedFactor.Direction - 4),
                        Value = cachedFactor.Value
                    };
                }
                return new Factor()
                {
                    Direction = cachedFactor.Direction,
                    Value = cachedFactor.Value
                };
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
                if (cachedFactor.Direction >= 4)
                {
                    return new FactorAndSpeed()
                    {
                        Direction = (short)(cachedFactor.Direction - 4),
                        SpeedFactor = cachedFactor.SpeedFactor,
                        Value = cachedFactor.Value
                    };
                }
                return new FactorAndSpeed()
                {
                    Direction = cachedFactor.Direction,
                    SpeedFactor = cachedFactor.SpeedFactor,
                    Value = cachedFactor.Value
                };
            };
        }

        /// <summary>
        /// Returns the cached factor.
        /// </summary>
        public Factor GetFactor(ushort edgeProfile, string profileName)
        {
            FactorAndSpeed[] factorsForProfile;
            if (!_edgeProfileFactors.TryGetValue(profileName, out factorsForProfile))
            {
                throw new ArgumentOutOfRangeException(string.Format("{0} not found.", profileName));
            }
            if (edgeProfile < factorsForProfile.Length)
            {
                var cachedFactor = factorsForProfile[edgeProfile];
                if(cachedFactor.Direction >= 4)
                {
                    return new Factor()
                    {
                        Direction = (short)(cachedFactor.Direction << 2),
                        Value = cachedFactor.Value
                    };
                }
                return new Factor()
                {
                    Direction = cachedFactor.Direction,
                    Value = cachedFactor.Value
                };
            }
            throw new ArgumentOutOfRangeException("Edgeprofile invalid.");
        }

        /// <summary>
        /// Returns the cached factor and speed.
        /// </summary>
        public FactorAndSpeed GetFactorAndSpeed(ushort edgeProfile, string profileName)
        {
            FactorAndSpeed[] factorsForProfile;
            if (!_edgeProfileFactors.TryGetValue(profileName, out factorsForProfile))
            {
                throw new ArgumentOutOfRangeException(string.Format("{0} not found.", profileName));
            }
            if (edgeProfile < factorsForProfile.Length)
            {
                var cachedFactor = factorsForProfile[edgeProfile];
                if (cachedFactor.Direction >= 4)
                {
                    return new FactorAndSpeed()
                    {
                        Direction = (short)(cachedFactor.Direction << 2),
                        SpeedFactor = cachedFactor.SpeedFactor,
                        Value = cachedFactor.Value
                    };
                }
                return new FactorAndSpeed()
                {
                    Direction = cachedFactor.Direction,
                    SpeedFactor = cachedFactor.SpeedFactor,
                    Value = cachedFactor.Value
                };
            }
            throw new ArgumentOutOfRangeException("Edgeprofile invalid.");
        }

        /// <summary>
        /// Returns true if the given edge can be stopped on.
        /// </summary>
        public bool CanStopOn(ushort edgeProfile, string profileName)
        {
            FactorAndSpeed[] factorsForProfile;
            if(!_edgeProfileFactors.TryGetValue(profileName, out factorsForProfile))
            {
                throw new ArgumentOutOfRangeException(string.Format("{0} not found.", profileName));
            }
            if (edgeProfile < factorsForProfile.Length)
            {
                return factorsForProfile[edgeProfile].Direction < 4;
            }
            throw new ArgumentOutOfRangeException("Edgeprofile invalid.");
        }
    }
}
