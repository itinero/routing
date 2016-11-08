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

namespace Itinero.Profiles
{
    /// <summary>
    /// Defines a constraint variable.
    /// </summary>
    public class Constraint
    {
        private readonly string _name;
        private readonly bool _isMax;
        private readonly float _defaultValue;

        /// <summary>
        /// Creates a new constraint variable.
        /// </summary>
        public Constraint(string name, bool isMax, float defaultValue)
        {
            _name = name;
            _defaultValue = defaultValue;
            _isMax = isMax;
        }

        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }
        }

        /// <summary>
        /// Gets the is max boolean.
        /// </summary>
        public bool IsMax
        {
            get
            {
                return _isMax;
            }
        }

        /// <summary>
        /// Gets the default value.
        /// </summary>
        public float DefaultValue
        {
            get
            {
                return _defaultValue;
            }
        }
    }
}