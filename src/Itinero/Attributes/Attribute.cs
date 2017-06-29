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

namespace Itinero.Attributes
{
    /// <summary>
    /// Represents an attributes.
    /// </summary>
    public struct Attribute
    {
        /// <summary>
        /// Creates a new attribute.
        /// </summary>
        public Attribute(string key, string value)
        {
            this.Key = key;
            this.Value = value;
        }

        /// <summary>
        /// Gets or sets the key.
        /// </summary>
        public string Key { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public string Value { get; set; }
        
        /// <summary>
        /// Gets a proper description of this attribute.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return this.Key + "=" + this.Value;
        }
    }
}