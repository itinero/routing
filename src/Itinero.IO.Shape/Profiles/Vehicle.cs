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

using Itinero.Profiles;
using MoonSharp.Interpreter;
using NetTopologySuite.IO;
using System.Collections.Generic;
using System.Linq;

namespace Itinero.IO.Shape.Profiles
{
    /// <summary>
    /// Defines an shape vehicle.
    /// </summary>
    public class Vehicle : DynamicVehicle
    {
        /// <summary>
        /// Creates a new vehicle.
        /// </summary>
        internal Vehicle(string script)
            : base(script)
        {
        }

        private Table _attributesTable;
        private Table _resultsTable;
        
        /// <summary>
        /// Pushes the attributes through this profiles and returns only those that are used in routing.
        /// </summary>
        public bool AddToProfileWhiteList(HashSet<string> whiteList, ShapefileDataReader reader)
        {
            if (_attributesTable == null)
            {
                _attributesTable = new Table(this.Script);
                _resultsTable = new Table(this.Script);
            }

            var traversable = false;

            // build lua table.
            _attributesTable.Clear();
            foreach (var field in reader.DbaseHeader.Fields)
            {
                var valueString = string.Empty;
                var value = reader[field.Name];
                if (value != null)
                {
                    valueString = value.ToInvariantString();
                }
                _attributesTable.Set(field.Name, DynValue.NewString(valueString));
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