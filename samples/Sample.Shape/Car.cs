// The MIT License (MIT)

// Copyright (c) 2016 Ben Abelshausen

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using System;
using System.Collections.Generic;
using Itinero.Attributes;
using Itinero.IO.Shape.Vehicles;
using Itinero.Profiles;

namespace Sample.Shape
{
    /// <summary>
    /// A vehicle profile for NWB-based shapefiles.
    /// </summary>
    public class Car : Vehicle
    {
        /// <summary>
        /// Gets the unique name.
        /// </summary>
        public override string UniqueName
        {
            get
            {
                return "NWB.Car";
            }
        }

        /// <summary>
        /// Gets the vehicle types.
        /// </summary>
        public override List<string> VehicleTypes
        {
            get
            {
                return new List<string>(new[] { "motor_vehicle", "car" });
            }
        }

        /// <summary>
        /// Returns true if an attribute with the given key is relevant for the profile.
        /// </summary>
        public override bool IsRelevantForProfile(string key)
        {
            return key == "BST_CODE" || 
                key == "BAANSUBSRT" ||
                key == "RIJRICHTNG" ||
                key == "WEGBEHSRT" ||
                key == "HECTO_LTTR";
        }

        /// <summary>
        /// Returns the maximum speed.
        /// </summary>
        /// <returns></returns>
        public override float MaxSpeed()
        {
            return 130;
        }

        /// <summary>
        /// Returns the minimum speed.
        /// </summary>
        /// <returns></returns>
        public override float MinSpeed()
        {
            return 5;
        }

        /// <summary>
        /// Returns the probable speed.
        /// </summary>
        public override float ProbableSpeed(IAttributeCollection tags)
        {
            string highwayType;
            if (tags.TryGetValue("BAANSUBSRT", out highwayType))
            {
                switch (highwayType)
                {
                    case "BVD":
                        return 50;
                    case "AF":
                    case "OP":
                        return 70;
                    case "HR":
                        return 120;
                    default:
                        return 70;
                }
            }
            return 0;
        }

        /// <summary>
        /// Returns true if the edge is oneway forward, false if backward, null if bidirectional.
        /// </summary>
        protected override bool? IsOneWay(IAttributeCollection tags)
        {
            string oneway;
            if (tags.TryGetValue("RIJRICHTNG", out oneway))
            {
                if (oneway == "H")
                {
                    return true;
                }
                else if (oneway == "T")
                {
                    return false;
                }
            }
            return null;
        }
    }
}