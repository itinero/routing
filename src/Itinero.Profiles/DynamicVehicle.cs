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

using Itinero.Attributes;
using MoonSharp.Interpreter;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Itinero.Profiles
{
    /// <summary>
    /// A dynamic vehicle based on single lua script.
    /// </summary>
    public class DynamicVehicle : Vehicle
    {
        private readonly Script _script;
        private readonly string[] _vehicleTypes;
        private readonly string _name;
        private readonly HashSet<string> _metaWhiteList;
        private readonly HashSet<string> _profileWhiteList;
        
        /// <summary>
        /// Creates a new dynamic profile based on the given lua script.
        /// </summary>
        public DynamicVehicle(string script)
        {
            _profileFunctions = new Dictionary<string, object>();

            _script = new Script();
            _script.DoString(script);

            var dynName = _script.Globals.Get("name");
            if (dynName == null)
            {
                throw new Exception("Dynamic profile doesn't define a name.");
            }
            _name = dynName.String;

            var dynVehicleTypes = _script.Globals.Get("vehicle_types");
            if (dynVehicleTypes != null)
            {
                _vehicleTypes = dynVehicleTypes.Table.Values.Select(x => x.String).ToArray();
            }

            var dynProfiles = _script.Globals.Get("profiles");
            if (dynProfiles == null)
            {
                throw new ArgumentException("No profiles defined in lua script.");
            }
            foreach(var dynProfile in dynProfiles.Table.Pairs)
            {
                var profileName = dynProfile.Key.String;
                var profileDefinition = dynProfile.Value;
                var functionName = profileDefinition.Table.Get("function_name").String;
                var function = _script.Globals[functionName];
                if (function == null)
                {
                    throw new ArgumentException(string.Format("Function {0} not found in lua script.", functionName));
                }

                var metric = ProfileMetric.Custom;
                var dynMetric = profileDefinition.Table.Get("metric");
                if (dynMetric != null)
                {
                    switch (dynMetric.String)
                    {
                        case "time":
                            metric = ProfileMetric.TimeInSeconds;
                            break;
                        case "distance":
                            metric = ProfileMetric.DistanceInMeters;
                            break;
                    }
                }

                if (!_profileFunctions.ContainsKey(functionName))
                {
                    _profileFunctions[functionName] = function;
                }
                var profile = new DynamicProfile(this.Name + "." + profileName, metric, _vehicleTypes, _script, function);
                this.Register(profile);
            }
            
            var dynAttributesWhitelist = this.Script.Globals.Get("meta_whitelist");
            _metaWhiteList = new HashSet<string>();
            if (dynAttributesWhitelist != null)
            {
                foreach (var attribute in dynAttributesWhitelist.Table.Values.Select(x => x.String))
                {
                    _metaWhiteList.Add(attribute);
                }
            }

            dynAttributesWhitelist = this.Script.Globals.Get("profile_whitelist");
            _profileWhiteList = new HashSet<string>();
            if (dynAttributesWhitelist != null)
            {
                foreach (var attribute in dynAttributesWhitelist.Table.Values.Select(x => x.String))
                {
                    _profileWhiteList.Add(attribute);
                }
            }
        }

        private readonly Dictionary<string, object> _profileFunctions;

        /// <summary>
        /// Gets all the profile functions.
        /// </summary>
        protected IEnumerable<object> ProfileFunctions
        {
            get
            {
                return _profileFunctions.Values;
            }
        }

        /// <summary>
        /// Gets the script.
        /// </summary>
        protected Script Script
        {
            get
            {
                return _script;
            }
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        public sealed override string Name
        {
            get
            {
                return _name;
            }
        }

        /// <summary>
        /// Gets the vehicle types.
        /// </summary>
        public sealed override string[] VehicleTypes
        {
            get
            {
                return _vehicleTypes;
            }
        }

        private Table _attributesTable;
        private Table _resultsTable;

        /// <summary>
        /// Gets the attributes whitelist.
        /// </summary>
        public HashSet<string> MetaWhiteList
        {
            get
            {
                return _metaWhiteList;
            }
        }

        /// <summary>
        /// Gets the attributes whitelist.
        /// </summary>
        public HashSet<string> ProfileWhiteList
        {
            get
            {
                return _profileWhiteList;
            }
        }

        /// <summary>
        /// Pushes the attributes through this profiles and returns only those that are used in routing.
        /// </summary>
        public bool AddToProfileWhiteList(HashSet<string> whiteList, IAttributeCollection attributes)
        {
            if (_attributesTable == null)
            {
                _attributesTable = new Table(this.Script);
                _resultsTable = new Table(this.Script);
            }

            var traversable = false;

            // build lua table.
            _attributesTable.Clear();
            foreach (var attribute in attributes)
            {
                _attributesTable.Set(attribute.Key, DynValue.NewString(attribute.Value));
            }

            // call each function once and build the list of attributes to keep.
            foreach (var function in this.ProfileFunctions)
            {
                // call factor_and_speed function.
                _resultsTable.Clear();
                this.Script.Call(function, _attributesTable, _resultsTable);

                float val;
                if (_resultsTable.TryGetFloat("speed", out val))
                {
                    if (val != 0)
                    {
                        traversable = true;
                    }
                }

                // get the result.
                var dynAttributesToKeep = _resultsTable.Get("attributes_to_keep");
                if (dynAttributesToKeep == null)
                {
                    continue;
                }
                foreach (var attribute in dynAttributesToKeep.Table.Keys.Select(x => x.String))
                {
                    whiteList.Add(attribute);
                }
            }
            return traversable;
        }
    }
}