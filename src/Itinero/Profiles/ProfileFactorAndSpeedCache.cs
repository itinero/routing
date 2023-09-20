﻿/*
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
        private Dictionary<string, FactorAndSpeed[]> _edgeProfileFactors;

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
            for (var p = 0; p < profiles.Length; p++)
            {
                if (!_edgeProfileFactors.ContainsKey(profiles[p].FullName))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Returns true if all the given profiles are cached and supported.
        /// </summary>
        public bool ContainsAll(params IProfileInstance[] profileInstances)
        {
            for (var p = 0; p < profileInstances.Length; p++)
            {
                if (!_edgeProfileFactors.TryGetValue(profileInstances[p].Profile.FullName, out _))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        /// Calculates for all registered profiles.
        /// </summary>
        [Obsolete]
        public void CalculateForAll()
        {
            this.CalculateFor(Profile.GetRegistered().ToArray());
        }

        /// <summary>
        /// Precalculates speed factors for all the given profiles.
        /// </summary>
        public void CalculateFor(params Profile[] profiles)
        {
            lock (this)
            { // don't allow multiple threads to fill this cache at the same time.
                var newEdgeProfileFactors = new Dictionary<string, FactorAndSpeed[]>(_edgeProfileFactors);

                var edgeProfileFactors = new FactorAndSpeed[profiles.Length][];
                for (var p = 0; p < profiles.Length; p++)
                {
                    edgeProfileFactors[p] = new FactorAndSpeed[(int) _db.EdgeProfiles.Count];
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
                    newEdgeProfileFactors[profiles[p].FullName] = edgeProfileFactors[p];
                }

                _edgeProfileFactors = newEdgeProfileFactors;
            }
        }

        private void CalculateFor(Profile profile)
        {
            // don't allow multiple threads to fill this cache at the same time.
            var factors = new FactorAndSpeed[(int) _db.EdgeProfiles.Count];
            lock (this)
            {
                // don't allow multiple threads to fill this cache at the same time.
                for (uint edgeProfile = 0; edgeProfile < _db.EdgeProfiles.Count; edgeProfile++)
                {
                    var edgeProfileTags = _db.EdgeProfiles.Get(edgeProfile);
                    factors[edgeProfile]
                        = profile.FactorAndSpeed(edgeProfileTags);
                }
            }
            _edgeProfileFactors[profile.FullName] = factors;
        }

        /// <summary>
        /// Returns an isacceptable function using the cached data.
        /// </summary>
        public Func<GeometricEdge, bool> GetIsAcceptable(bool verifyCanStopOn, params IProfileInstance[] profileInstances)
        {
            var edgeProfileFactors = _edgeProfileFactors;  
            
            var cachedFactors = new FactorAndSpeed[profileInstances.Length][];
            for (var p = 0; p < profileInstances.Length; p++)
            {
                if (!edgeProfileFactors.TryGetValue(profileInstances[p].Profile.FullName, out var cache)) throw new ArgumentException("Not all given profiles are supported.");
                cachedFactors[p] = cache;
            }

            return (edge) =>
            {
                float distance;
                ushort edgeProfileId;
                Data.Edges.EdgeDataSerializer.Deserialize(edge.Data[0],
                    out distance, out edgeProfileId);
                
                for (var i = 0; i < profileInstances.Length; i++)
                {
                    if (edgeProfileId >= cachedFactors[i].Length)
                    {
                        this.CalculateFor(profileInstances[i].Profile);
                        if (!_edgeProfileFactors.TryGetValue(profileInstances[i].Profile.FullName, out var newCachedFactors)) throw new ArgumentException("Given profile not supported.");
                        cachedFactors[i] = newCachedFactors;
                    }
                    
                    var cachedFactor = cachedFactors[i][edgeProfileId];
                    if (cachedFactor.Value <= 0)
                    { // edge is not accessible to this profile.
                        return false;
                    }

                    if (verifyCanStopOn)
                    { // check for a stopping point.
                        if (!cachedFactor.CanStopOn())
                        { // can't use this edge as stopping point. 
                            return false;
                        }
                    }

                    if (profileInstances[i].IsConstrained(cachedFactor.Constraints))
                    { // edge is constrained, vehicle too heavy or too big for example.
                        return false;
                    }
                }
                return true;
            };
        }

        /// <summary>
        /// Gets the get factor function for the given profile.
        /// </summary>
        public Func<ushort, Factor> GetGetFactor(IProfileInstance profileInstance)
        {
            if (!_edgeProfileFactors.TryGetValue(profileInstance.Profile.FullName, out var cache)) throw new ArgumentException("Given profile not supported.");
            var cachedFactors = cache;
            if (profileInstance.Constraints != null)
            {
                return (p) =>
                {
                    if (p >= cachedFactors.Length)
                    {
                        this.CalculateFor(profileInstance.Profile);
                        if (!_edgeProfileFactors.TryGetValue(profileInstance.Profile.FullName, out cachedFactors)) throw new ArgumentException("Given profile not supported.");
                    }
                    
                    var cachedFactor = cachedFactors[p];
                    if (profileInstance.IsConstrained(cachedFactor.Constraints))
                    {
                        return Factor.NoFactor;
                    }
                    return cachedFactor.ToFactor();
                };
            }
            return (p) =>
            {
                if (p >= cachedFactors.Length)
                {
                    this.CalculateFor(profileInstance.Profile);
                    if (!_edgeProfileFactors.TryGetValue(profileInstance.Profile.FullName, out cachedFactors)) throw new ArgumentException("Given profile not supported.");
                }
                
                return cachedFactors[p].ToFactor();
            };
        }

        /// <summary>
        /// Gets the get factor function for the given profile.
        /// </summary>
        public Func<ushort, FactorAndSpeed> GetGetFactorAndSpeed(IProfileInstance profileInstance)
        {
            if (!_edgeProfileFactors.TryGetValue(profileInstance.Profile.FullName, out var cache)) throw new ArgumentException("Given profile not supported.");
            var cachedFactors = cache;
            if (profileInstance.Constraints != null)
            {
                return (p) =>
                {
                    if (p >= cachedFactors.Length)
                    {
                        this.CalculateFor(profileInstance.Profile);
                        if (!_edgeProfileFactors.TryGetValue(profileInstance.Profile.FullName, out cachedFactors)) throw new ArgumentException("Given profile not supported.");
                    }
                    
                    var cachedFactor = cachedFactors[p];
                    if (profileInstance.IsConstrained(cachedFactor.Constraints))
                    {
                        return FactorAndSpeed.NoFactor;
                    }
                    return cachedFactor;
                };
            }
            return (p) =>
            {
                if (p >= cachedFactors.Length)
                {
                    this.CalculateFor(profileInstance.Profile);
                    if (!_edgeProfileFactors.TryGetValue(profileInstance.Profile.FullName, out cachedFactors)) throw new ArgumentException("Given profile not supported.");
                }
                
                return cachedFactors[p];
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
                return factorsForProfile[edgeProfile].ToFactor();
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
                return factorsForProfile[edgeProfile];
            }
            throw new ArgumentOutOfRangeException("Edgeprofile invalid.");
        }

        /// <summary>
        /// Returns true if the given edge can be stopped on.
        /// </summary>
        public bool CanStopOn(ushort edgeProfile, string profileName)
        {
            FactorAndSpeed[] factorsForProfile;
            if (!_edgeProfileFactors.TryGetValue(profileName, out factorsForProfile))
            {
                throw new ArgumentOutOfRangeException(string.Format("{0} not found.", profileName));
            }
            if (edgeProfile < factorsForProfile.Length)
            {
                return factorsForProfile[edgeProfile].CanStopOn();
            }
            throw new ArgumentOutOfRangeException("Edgeprofile invalid.");
        }
    }
}
