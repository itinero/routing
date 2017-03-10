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