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

using Itinero.Attributes;
using Itinero.Profiles.Lua;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace Itinero.Profiles
{
    /// <summary>
    /// A dynamic vehicle based on single lua script.
    /// </summary>
    public class DynamicVehicle : Vehicle
    {
        private readonly Script _script;
        private readonly string _source;
        private readonly string[] _vehicleTypes;
        private readonly string _name;
        private readonly HashSet<string> _metaWhiteList;
        private readonly HashSet<string> _profileWhiteList;
        private readonly IReadonlyAttributeCollection _parameters;

        /// <summary>
        /// Creates a new dynamic profile based on the given lua script.
        /// </summary>
        public DynamicVehicle(string script)
        {
            _profileFunctions = new Dictionary<string, object>();
            _source = script;

            _script = new Script();
            _script.DoString(script);

            var dynName = _script.Globals.Get("name");
            if (dynName == null)
            {
                throw new Exception("Dynamic profile doesn't define a name.");
            }
            _name = dynName.String;

            var dynVehicleTypes = _script.Globals.Get("vehicle_types");
            if (dynVehicleTypes != null &&
                dynVehicleTypes.Type == DataType.Table)
            {
                _vehicleTypes = dynVehicleTypes.Table.Values.Select(x => x.String).ToArray();
            }
            else
            {
                _vehicleTypes = new string[0];
            }

            var dynProfiles = _script.Globals.Get("profiles");
            if (dynProfiles == null)
            {
                throw new ArgumentException("No profiles defined in lua script.");
            }
            foreach (var dynProfile in dynProfiles.Table.Pairs)
            {
                var profileDefinition = dynProfile.Value;
                var profileName = profileDefinition.Table.Get("name").String;
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
                var profile = new DynamicProfile(profileName, metric, _vehicleTypes, this, _script, function);
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

            var dynParameters = _script.Globals.Get("parameters");
            var parameters = new AttributeCollection();
            if (dynParameters != null && dynParameters.Table != null)
            {
                foreach (var dynParameter in dynParameters.Table.Pairs)
                {
                    var parameterName = dynParameter.Key;
                    var parameterValue = dynParameter.Value;

                    parameters.AddOrReplace(parameterName.String, parameterValue.String);
                }
            }
            _parameters = parameters;
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
        public Script Script
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
        public override HashSet<string> MetaWhiteList
        {
            get
            {
                return _metaWhiteList;
            }
        }

        /// <summary>
        /// Gets the attributes whitelist.
        /// </summary>
        public override HashSet<string> ProfileWhiteList
        {
            get
            {
                return _profileWhiteList;
            }
        }

        /// <summary>
        /// 
        /// </summary>
        public override bool AddToWhiteList(IAttributeCollection attributes, Whitelist whitelist)
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
                    whitelist.Add(attribute);
                }
            }
            return traversable;
        }

        /// <summary>
        /// Gets the parameters.
        /// </summary>
        public override IReadonlyAttributeCollection Parameters
        {
            get
            {
                return _parameters;
            }
        }

        /// <summary>
        /// Pushes the attributes through this profiles and adds used keys in the given whitelist.
        /// </summary>
        public override FactorAndSpeed FactorAndSpeed(IAttributeCollection attributes, Whitelist whiteList)
        {
            throw new NotImplementedException("Not used and unavailable with dynamic vehicles.");
        }

        /// <summary>
        /// Serializes the content of this vehicle.
        /// </summary>
        protected override long DoSerialize(Stream stream)
        {
            return stream.WriteWithSize(this._source);
        }

        /// <summary>
        /// Loads the vehicle from the given script.
        /// </summary>
        public static DynamicVehicle Load(string script)
        {
            var vehicle = new DynamicVehicle(script);
            return vehicle;
        }

        /// <summary>
        /// Loads the vehicle from the given stream using the current position as the size.
        /// </summary>
        public static DynamicVehicle LoadWithSize(Stream stream)
        {
            return DynamicVehicle.Load(stream.ReadWithSizeString());
        }

        /// <summary>
        /// Loads the vehicle from the given stream.
        /// </summary>
        public static DynamicVehicle LoadFromStream(Stream stream)
        {
            return DynamicVehicle.Load(stream.ReadToEnd());
        }

        /// <summary>
        /// Loads the vehicle from the embedded resources.
        /// </summary>
        public static DynamicVehicle LoadFromEmbeddedResource(Assembly assembly, string embeddedResource)
        {
            return DynamicVehicle.Load(assembly.GetManifestResourceStream(embeddedResource).ReadToEnd());
        }
    }
}