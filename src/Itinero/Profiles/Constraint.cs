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
using System.IO;

namespace Itinero.Profiles
{
    /// <summary>
    /// Abstract definition of a constraint.
    /// </summary>
    public sealed class Constraint
    {
        private readonly float _min = float.MinValue;
        private readonly float _max = float.MaxValue;
        private readonly bool _minEqualsOk = false;
        private readonly bool _maxEqualsOk = false;

        /// <summary>
        /// Creates a new constraint.
        /// </summary>
        public Constraint(float min, bool minEqualsOk, float max, bool maxEqualsOk)
        {
            _min = min;
            _max = max;
            _minEqualsOk = minEqualsOk;
            _maxEqualsOk = maxEqualsOk;

            if (_min == float.MinValue ||
                _max == float.MaxValue)
            {
                throw new ArgumentException("No reasonable range defined.");
            }
        }

        /// <summary>
        /// Evaluates this constraint, true means this constraint applies.
        /// </summary>
        public bool Evaluate(float value)
        {
            if (_minEqualsOk && _maxEqualsOk)
            {
                return _min <= value && value <= _max;
            }
            else if(_minEqualsOk)
            {
                return _min <= value && value < _max;
            }
            else if (_maxEqualsOk)
            {
                return _min < value && value <= _max;
            }
            return _min < value && value < _max;
        }

        /// <summary>
        /// Gets a string describing this constraint.
        /// </summary>
        public string Description
        {
            get
            {
                var min = "<";
                if (_minEqualsOk)
                {
                    min = "<=";
                }
                if (_min != float.MinValue)
                {
                    min = min + _min.ToInvariantString();
                }
                var max = ">";
                if (_maxEqualsOk)
                {
                    max = ">=";
                }
                if (_max != float.MaxValue)
                {
                    max = max + _min.ToInvariantString();
                }
                if (_min != float.MinValue && _max != float.MaxValue)
                {
                    return min + "&" + max;
                }
                else if (_min != float.MinValue)
                {
                    return min;
                }
                return max;
            }
        }

        /// <summary>
        /// Serializes to the given stream.
        /// </summary>
        public int Serialize(Stream stream)
        {
            return Serialize(stream, _min, _max, _minEqualsOk, _maxEqualsOk);
        }

        /// <summary>
        /// Serializes as null.
        /// </summary>
        public static int SerializeNull(Stream stream)
        {
            return Serialize(stream, float.MinValue, float.MaxValue, true, true);
        }

        /// <summary>
        /// Serializes a constraint with the given parameters.
        /// </summary>
        /// <returns></returns>
        public static int Serialize(Stream stream, float min, float max, bool minEqualsOk, bool maxEqualsOk)
        {
            byte flags = (byte)((minEqualsOk ? 1 : 0) +
                (maxEqualsOk ? 2 : 0));
            stream.WriteByte(flags);
            var bytes = BitConverter.GetBytes(min);
            stream.Write(bytes, 0, 4);
            bytes = BitConverter.GetBytes(max);
            stream.Write(bytes, 0, 4);
            return 4 + 4 + 1;
        }

        /// <summary>
        /// Deserializes a stream.
        /// </summary>
        public static Constraint Deserialize(Stream stream)
        {
            var flags = stream.ReadByte();
            var minEqualsOk = (flags & (1 << 0)) != 0;
            var maxEqualsOk = (flags & (1 << 1)) != 0;
            var bytes = new byte[8];
            stream.Read(bytes, 0, 8);
            var max = BitConverter.ToSingle(bytes, 0);
            var min = BitConverter.ToSingle(bytes, 4);
            if (max == float.MaxValue && min == float.MinValue)
            {
                return null;
            }
            return new Constraint(min, minEqualsOk, max, maxEqualsOk);
        }
    }
}