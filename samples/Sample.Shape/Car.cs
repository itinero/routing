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
        public override string Name
        {
            get
            {
                return "NWB.Car";
            }
        }

        /// <summary>
        /// Gets the profile whitelist.
        /// </summary>
        public override HashSet<string> ProfileWhiteList
        {
            get
            {
                return new HashSet<string>(new string[] { "BST_CODE", "BAANSUBSRT", "RIJRICHTNG", "WEGBEHSRT", "HECTO_LTTR" });
            }
        }

        /// <summary>
        /// Adds a number of keys to the given whitelist when they are relevant for this vehicle.
        /// </summary>
        /// <returns>True if the edge with the given attributes is usefull for this vehicle.</returns>
        public override bool AddToWhiteList(IAttributeCollection attributes, Whitelist whitelist)
        {
            return true;
        }

        /// <summary>
        /// Gets series of attributes and returns the factor and speed that applies. Adds relevant tags to a whitelist.
        /// </summary>
        public override FactorAndSpeed FactorAndSpeed(IAttributeCollection attributes, Whitelist whiteList)
        {
            string highwayType;
            if (attributes == null || !attributes.TryGetValue("BST_CODE", out highwayType))
            {
                return Itinero.Profiles.FactorAndSpeed.NoFactor;
            }
            float speed = 70;
            switch (highwayType)
            {
                case "BVD":
                    speed = 50;
                    break;
                case "AF":
                case "OP":
                    speed = 70;
                    break;
                case "HR":
                    speed = 120;
                    break;
            }
            string oneway;
            short direction = 0;
            if (attributes.TryGetValue("RIJRICHTNG", out oneway))
            {
                if (oneway == "H")
                {
                    direction = 1;
                }
                else if (oneway == "T")
                {
                    direction = 2;
                }
            }

            return new Itinero.Profiles.FactorAndSpeed()
            {
                Constraints = null,
                Direction = direction,
                SpeedFactor = 1.0f / speed,
                Value = 1.0f / speed
            };
        }
    }
}