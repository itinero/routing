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

using Itinero.Profiles;

namespace Itinero.Algorithms.Weights
{
    /// <summary>
    /// Represents a direction.
    /// </summary>
    public struct Dir
    {
        // 0: false, false.
        // 1: true, false.
        // 2: false, true.
        // 3: true, true
        public byte _val;

        /// <summary>
        /// Creates a new direction from it's raw value.
        /// </summary>
        /// <param name="val"></param>
        public Dir(byte val)
        {
            _val = val;
        }

        /// <summary>
        /// Creates a new direction.
        /// </summary>
        public Dir(bool f, bool b)
        {
            _val = 0;
            if (f)
            {
                _val = (byte)(_val | 1);
            }
            if (b)
            {
                _val = (byte)(_val | 2);
            }
        }

        /// <summary>
        /// Retuns a forward direction struct.
        /// </summary>
        public static Dir Forward
        {
            get
            {
                return new Dir(1);
            }
        }

        /// <summary>
        /// Retuns a backward direction struct.
        /// </summary>
        public static Dir Backward
        {
            get
            {
                return new Dir(1);
            }
        }

        /// <summary>
        /// Retuns a bidirectional direction struct.
        /// </summary>
        public static Dir Bi
        {
            get
            {
                return new Dir(3);
            }
        }

        /// <summary>
        /// Returns the forward flag.
        /// </summary>
        public bool F
        {
            get
            {
                return (_val & 1) == 1;
            }
            set
            {
                if (value)
                {
                    _val = (byte)(_val | 1);
                }
                else
                {
                    _val = (byte)(_val & 2);
                }
            }
        }

        /// <summary>
        /// Returns the bacward flag.
        /// </summary>
        public bool B
        {
            get
            {
                return (_val & 2) == 2;
            }
            set
            {
                if (value)
                {
                    _val = (byte)(_val | 2);
                }
                else
                {
                    _val = (byte)(_val & 1);
                }
            }
        }

        /// <summary>
        /// Reverses this direction.
        /// </summary>
        public void Reverse()
        {
            if (_val == 2)
            {
                _val = 1;
            }
            else if (_val == 1)
            {
                _val = 2;
            }
        }

        /// <summary>
        /// Returns the directional information as a nullable bool.
        /// </summary>
        /// <returns></returns>
        public bool? AsNullableBool()
        {
            switch (_val)
            {
                case 1:
                    return true;
                case 2:
                    return false;
                case 3:
                    return null;
            }
            throw new System.Exception("Cannot describe this directional information as a nullable bool.");
        }

        /// <summary>
        /// Returns an identical copy of this direction.
        /// </summary>
        /// <returns></returns>
        public Dir Clone()
        {
            return new Dir()
            {
                _val = this._val
            };
        }

        /// <summary>
        /// Returns a direction from a given factor.
        /// </summary>
        public static Dir FromFactor(Factor factor, bool inverted)
        {
            if (factor.Direction == 0)
            {
                return new Dir(true, true);
            }
            else
            {
                Dir dir;
                if (factor.Direction == 1)
                {
                    dir = new Dir(true, false);
                }
                else
                {
                    dir = new Dir(false, true);
                }
                if (inverted)
                {
                    dir.Reverse();
                }
                return dir;
            }
        }

        /// <summary>
        /// Combines two directions leaving only the possible directions flags.
        /// </summary>
        public static Dir Combine(Dir dir1, Dir dir2)
        {
            return new Dir((byte)(dir1._val & dir2._val));
        }

        /// <summary>
        /// Returns a description of this struct.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            switch (_val)
            {
                case 1:
                    return "->";
                case 2:
                    return "<-";
                case 3:
                    return "<->";
            }
            return "x";
        }
    }
}