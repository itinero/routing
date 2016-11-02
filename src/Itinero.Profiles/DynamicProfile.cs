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
using MoonSharp.Interpreter;
using Itinero.Attributes;
using System.Linq;

namespace Itinero.Profiles
{
    /// <summary>
    /// A dynamic profile.
    /// </summary>
    public class DynamicProfile
    {
        private readonly Script _script;
        private readonly string[] _vehicleTypes;
        private readonly string _name;
        private readonly ProfileMetric _metric;

        /// <summary>
        /// Creates a new dynamic profile based on the given lua script.
        /// </summary>
        public DynamicProfile(string script)
        {
            _script = new Script();
            _script.DoString(script);

            _attributesTable = new Table(_script);
            _resultsTable = new Table(_script);

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

            _metric = ProfileMetric.Custom;
            var dynMetric = _script.Globals.Get("metric");
            if (dynMetric != null)
            {
                switch(dynMetric.String)
                {
                    case "time":
                        _metric = ProfileMetric.TimeInSeconds;
                        break;
                    case "distance":
                        _metric = ProfileMetric.DistanceInMeters;
                        break;
                }
            }
        }

        private readonly Table _attributesTable;
        private readonly Table _resultsTable;

        /// <summary>
        /// Gets the name.
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }
        }

        /// <summary>
        /// Get a function to calculate properties for a set given edge attributes.
        /// </summary>
        /// <returns></returns>
        public Func<IAttributeCollection, DynamicFactorAndSpeed> GetGetFactorAndSpeed()
        {
            return (attributes) =>
            {
                lock (_script)
                {
                    // build lua table.
                    _attributesTable.Clear();
                    foreach (var attribute in attributes)
                    {
                        _attributesTable.Set(attribute.Key, DynValue.NewString(attribute.Value));
                    }

                    // call factor_and_speed function.
                    _resultsTable.Clear();
                    var function = _script.Globals["factor_and_speed"];
                    _script.Call(function, _attributesTable, _resultsTable);

                    // get the results.
                    var result = new DynamicFactorAndSpeed();
                    float val;
                    if (!_resultsTable.TryGetFloat("speed", out val))
                    {
                        val = 0;
                    }                    
                    result.SpeedFactor = 1.0f / (val / 3.6f); // 1/m/s
                    if (_metric == ProfileMetric.TimeInSeconds)
                    { // use 1/speed as factor.
                        result.Factor = result.SpeedFactor;
                    }
                    else if(_metric == ProfileMetric.DistanceInMeters)
                    { // use 1 as factor.
                        result.Factor = 1;
                    }
                    else
                    { // use a custom factor.
                        if (!_resultsTable.TryGetFloat("factor", out val))
                        {
                            val = 0;
                        }
                        result.Factor = val;
                    }
                    bool boolVal;
                    if (!_resultsTable.TryGetBool("canstop", out boolVal))
                    { // default stopping everywhere.
                        boolVal = true;
                    }
                    result.CanStop = boolVal;
                    if (!_resultsTable.TryGetFloat("direction", out val))
                    {
                        val = 0;
                    }
                    result.Direction = (ushort)val;

                    return result;
                }
            };
        }
    }
}