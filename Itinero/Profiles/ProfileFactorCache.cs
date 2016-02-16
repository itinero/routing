// Itinero - OpenStreetMap (OSM) SDK
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
    /// A profile factor cache.
    /// </summary>
    public class ProfileFactorCache
    {
        private readonly RouterDb _db;
        private readonly Dictionary<string, CachedFactor[]> _edgeProfileFactors;

        /// <summary>
        /// A profile factor cache.
        /// </summary>
        public ProfileFactorCache(RouterDb db)
        {
            _db = db;
            _edgeProfileFactors = new Dictionary<string, CachedFactor[]>();
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
            this.CalculateFor(Profile.GetAllRegistered().ToArray());
        }

        /// <summary>
        /// Precalculates speed factors for all the given profiles.
        /// </summary>
        public void CalculateFor(params Profile[] profiles)
        {
            var edgeProfileFactors = new CachedFactor[profiles.Length][];
            for (var p = 0; p < profiles.Length; p++)
            {
                edgeProfileFactors[p] = new CachedFactor[(int)_db.EdgeProfiles.Count];
            }
            for (uint edgeProfile = 0; edgeProfile < _db.EdgeProfiles.Count; edgeProfile++)
            {
                var edgeProfileTags = _db.EdgeProfiles.Get(edgeProfile);
                for (var p = 0; p < profiles.Length; p++)
                {
                    var factor = profiles[p].Factor(edgeProfileTags);
                    var stoppable = profiles[p].CanStopOn(edgeProfileTags);
                    if(stoppable)
                    {
                        edgeProfileFactors[p][edgeProfile] = new CachedFactor()
                        {
                            Type = factor.Direction,
                            Value = factor.Value
                        };
                    }
                    else
                    {
                        edgeProfileFactors[p][edgeProfile] = new CachedFactor()
                        {
                            Type = (short)(factor.Direction + 4),
                            Value = factor.Value
                        };
                    }
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

            var cachedFactors = new CachedFactor[profiles.Length][];
            for(var p = 0; p < profiles.Length; p++)
            {
                cachedFactors[p] = _edgeProfileFactors[profiles[p].Name];
            }

            return (edge) =>
            {
                float distance;
                ushort edgeProfileId;
                Itinero.Data.EdgeDataSerializer.Deserialize(edge.Data[0],
                    out distance, out edgeProfileId);
                for (var i = 0; i < profiles.Length; i++)
                {
                    var cachedFactor = cachedFactors[i][edgeProfileId];
                    if ((verifyCanStopOn && cachedFactor.Type > 4) || 
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
                if (cachedFactor.Type >= 4)
                {
                    return new Factor()
                    {
                        Direction = (short)(cachedFactor.Type - 4),
                        Value = cachedFactor.Value
                    };
                }
                return new Factor()
                {
                    Direction = cachedFactor.Type,
                    Value = cachedFactor.Value
                };
            };
        }

        /// <summary>
        /// Returns the cached factor.
        /// </summary>
        public Factor GetFactor(ushort edgeProfile, string profileName)
        {
            CachedFactor[] factorsForProfile;
            if (!_edgeProfileFactors.TryGetValue(profileName, out factorsForProfile))
            {
                throw new ArgumentOutOfRangeException(string.Format("{0} not found.", profileName));
            }
            if (edgeProfile < factorsForProfile.Length)
            {
                var cachedFactor = factorsForProfile[edgeProfile];
                if(cachedFactor.Type >= 4)
                {
                    return new Factor()
                    {
                        Direction = (short)(cachedFactor.Type << 2),
                        Value = cachedFactor.Value
                    };
                }
                return new Factor()
                {
                    Direction = cachedFactor.Type,
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
            CachedFactor[] factorsForProfile;
            if(!_edgeProfileFactors.TryGetValue(profileName, out factorsForProfile))
            {
                throw new ArgumentOutOfRangeException(string.Format("{0} not found.", profileName));
            }
            if (edgeProfile < factorsForProfile.Length)
            {
                return factorsForProfile[edgeProfile].Type < 4;
            }
            throw new ArgumentOutOfRangeException("Edgeprofile invalid.");
        }

        private struct CachedFactor
        {
            /// <summary>
            /// Gets or sets the actual factor.
            /// </summary>
            public float Value { get; set; }

            /// <summary>
            /// Gets or sets the direction.
            /// </summary>
            /// 0=bidirectional and stoppable, 1=forward and stoppable, 2=backward and stoppable,
            /// 4=bidirectional and not stoppable, 5=forward and not stoppable, 6=backward and not stoppable
            public short Type { get; set; }
        }
    }
}
