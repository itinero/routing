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

namespace Itinero.Profiles
{
    /// <summary>
    /// Represents a dynamic routing profile that is based on a lua function.
    /// </summary>
    public class DynamicProfile : Profile
    {
        private readonly Script _script;
        private readonly object _function;
        private readonly string _name;
        private readonly ProfileMetric _metric;
        private readonly string[] _vehicleTypes;
        private readonly Table _attributesTable;
        private readonly Table _resultsTable;

        /// <summary>
        /// Creates a new dynamic profile.
        /// </summary>
        internal DynamicProfile(string name, ProfileMetric metric, string[] vehicleTypes, Vehicle parent, Script script, object factor_and_speed)
            : base(name, metric, vehicleTypes, null, parent)
        {
            _name = name;
            _metric = metric;
            _vehicleTypes = vehicleTypes;
            _script = script;
            _function = factor_and_speed;
            
            _attributesTable = new Table(_script);
            _resultsTable = new Table(_script);
        }

        /// <summary>
        /// Gets the name.
        /// </summary>
        public override string Name
        {
            get
            {
                return _name;
            }
        }

        /// <summary>
        /// Gets the metric.
        /// </summary>
        public override ProfileMetric Metric
        {
            get
            {
                return _metric;
            }
        }

        /// <summary>
        /// Gets the vehicle types.
        /// </summary>
        public override string[] VehicleTypes
        {
            get
            {
                return _vehicleTypes;
            }
        }

        /// <summary>
        /// Get a function to calculate properties for a set given edge attributes.
        /// </summary>
        /// <returns></returns>
        public sealed override FactorAndSpeed FactorAndSpeed(IAttributeCollection attributes)
        {
            lock (_script)
            {
                // build lua table.
                _attributesTable.Clear();
                if (attributes == null || attributes.Count == 0)
                {
                    return Profiles.FactorAndSpeed.NoFactor;
                }
                foreach (var attribute in attributes)
                {
                    _attributesTable.Set(attribute.Key, DynValue.NewString(attribute.Value));
                }

                // call factor_and_speed function.
                _resultsTable.Clear();
                var function = _script.Globals["factor_and_speed"];
                _script.Call(function, _attributesTable, _resultsTable);

                // get the results.
                var result = new FactorAndSpeed();
                float val;
                if (!_resultsTable.TryGetFloat("speed", out val))
                {
                    val = 0;
                }
                result.SpeedFactor = 1.0f / (val / 3.6f); // 1/m/s
                if (_metric == ProfileMetric.TimeInSeconds)
                { // use 1/speed as factor.
                    result.Value = result.SpeedFactor;
                }
                else if (_metric == ProfileMetric.DistanceInMeters)
                { // use 1 as factor.
                    result.Value = 1;
                }
                else
                { // use a custom factor.
                    if (!_resultsTable.TryGetFloat("factor", out val))
                    {
                        val = 0;
                    }
                    result.Value = val;
                }
                if (!_resultsTable.TryGetFloat("direction", out val))
                {
                    val = 0;
                }
                result.Direction = (short)val;
                bool boolVal;
                if (!_resultsTable.TryGetBool("canstop", out boolVal))
                { // default stopping everywhere.
                    boolVal = true;
                }
                if (!boolVal)
                {
                    result.Direction += 3;
                }

                return result;
            }
        }

        /// <summary>
        /// Returns true if the two edges with the given attributes are identical as far as this profile is concerned.
        /// </summary>
        /// <remarks>
        /// Default implementation compares attributes one-by-one.
        /// </remarks>
        public override sealed bool Equals(IAttributeCollection attributes1, IAttributeCollection attributes2)
        {
            return attributes1.ContainsSame(attributes2);
        }
    }
}