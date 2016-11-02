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
using Itinero.Profiles;
using System;
using System.Collections.Generic;

namespace Itinero.IO.Shape.Vehicles
{
    /// <summary>
    /// Abstract representation of a shape vehicle profile.
    /// </summary>
    public abstract class Vehicle
    {
        /// <summary>
        /// Gets a name unique for this vehicle.
        /// </summary>
        public abstract string UniqueName
        {
            get;
        }

        /// <summary>
        /// Returns the vehicle types.
        /// </summary>
        /// <remarks>
        /// This is used for restrictions, for example cars would be:
        /// motor_vehicle, car
        /// This means all restrictions restricting motor_vehicles and cars are applicable.
        /// </remarks>
        public abstract List<string> VehicleTypes
        {
            get;
        }

        /// <summary>
        /// Returns the max speed this vehicle can handle.
        /// </summary>
        public abstract float MaxSpeed();

        /// <summary>
        /// Returns the minimum speed of this vehicle.
        /// </summary>
        public abstract float MinSpeed();

        /// <summary>
        /// Returns true if an attribute with the given key is relevant for the profile.
        /// </summary>
        public abstract bool IsRelevantForProfile(string key);
        
        /// <summary>
        /// Returns true if the edge is one way forward, false if backward, null if bidirectional.
        /// </summary>
        protected abstract bool? IsOneWay(IAttributeCollection tags);

        /// <summary>
        /// Returns true if an edge with the given profile can be use for an end- or startpoint.
        /// </summary>
        public virtual bool CanStopOn(IAttributeCollection tags)
        {
            return true; // assume every edge can be used.
        }

        /// <summary>
        /// Estimates the probable speed of this vehicle on a way with the given tags.
        /// </summary>
        public abstract float ProbableSpeed(IAttributeCollection tags);

        /// <summary>
        /// Returns the name of a given edge.
        /// </summary>
        protected virtual string GetName(IAttributeCollection tags)
        {
            string name;
            if (tags.TryGetValue("name", out name))
            {
                return name;
            }
            if (tags.TryGetValue("Name", out name))
            {
                return name;
            }
            if (tags.TryGetValue("NAME", out name))
            {
                return name;
            }
            return string.Empty;
        }

        /// <summary>
        /// Returns true if an attribute with the given key is relevant as meta-data.
        /// </summary>
        public virtual bool IsRelevant(string key)
        {
            return this.IsRelevantForProfile(key);
        }
        
        /// <summary>
        /// Returns true if the edges with the given properties are equal for the vehicle.
        /// </summary>
        public virtual bool IsEqualFor(IAttributeCollection tags1, IAttributeCollection tags2)
        {
            if (this.GetName(tags1) != this.GetName(tags2))
            {
                // the name have to be equal.
                return false;
            }
            return true;
        }

        /// <summary>
        /// Gets the can stop function.
        /// </summary>
        internal Func<IAttributeCollection, bool> GetCanStop()
        {
            return (tags) =>
            {
                return this.CanStopOn(tags);
            };
        }

        /// <summary>
        /// Gets the equals function.
        /// </summary>
        internal Func<IAttributeCollection, IAttributeCollection, bool> GetEquals()
        {
            return (edge1, edge2) =>
            {
                return this.IsEqualFor(edge1, edge2);
            };
        }

        /// <summary>
        /// Gets the get factor function.
        /// </summary>
        internal Func<IAttributeCollection, Itinero.Profiles.Factor> GetGetFactor()
        {
            var getSpeed = this.GetGetSpeed();
            return (tags) =>
            {
                var speed = getSpeed(tags);
                if (speed.Value == 0)
                {
                    return new Itinero.Profiles.Factor()
                    {
                        Value = 0,
                        Direction = 0
                    };
                }
                return new Itinero.Profiles.Factor()
                {
                    Value = 1.0f / speed.Value,
                    Direction = speed.Direction
                };
            };
        }

        /// <summary>
        /// Gets the get speed function.
        /// </summary>
        internal Func<IAttributeCollection, Itinero.Profiles.Speed> GetGetSpeed()
        {
            return (tags) =>
            {
                var speed = new Itinero.Profiles.Speed()
                {
                    Value = (float)this.ProbableSpeed(tags) / 3.6f,
                    Direction = 0
                };
                if (speed.Value == 0)
                {
                    return Itinero.Profiles.Speed.NoSpeed;
                }
                var oneway = this.IsOneWay(tags);

                if (oneway.HasValue)
                {
                    if (oneway.Value)
                    {
                        speed.Direction = 1;
                    }
                    else
                    {
                        speed.Direction = 2;
                    }
                }
                return speed;
            };
        }

        /// <summary>
        /// Gets all profiles for this vehicles.
        /// </summary>
        public virtual Itinero.Profiles.ProfileDefinition[] GetProfileDefinitions()
        {
            return new Itinero.Profiles.ProfileDefinition[]
            {
                this.Fastest().Definition,
                this.Shortest().Definition
            };
        }

        /// <summary>
        /// Returns a profile for this vehicle that can be used for finding fastest routes;.
        /// </summary>
        public Itinero.Profiles.Profile Fastest()
        {
            return new Itinero.Profiles.ProfileDefinition(this.UniqueName,
                this.GetGetSpeed().ToUnconstrainedGetSpeed(),
                () => new Itinero.Profiles.Speed()
                {
                    Value = (float)this.MinSpeed() / 3.6f,
                    Direction = 0
                },
                this.GetCanStop(),
                this.GetEquals(),
                this.VehicleTypes,
                Itinero.Profiles.ProfileMetric.TimeInSeconds).Default();
        }

        /// <summary>
        /// Returns a profile for this vehicle that can be used for finding fastest routes;.
        /// </summary>
        public Itinero.Profiles.Profile Shortest()
        {
            return new Itinero.Profiles.ProfileDefinition(this.UniqueName + ".Shortest",
                this.GetGetSpeed().ToUnconstrainedGetSpeed(),
                () => new Itinero.Profiles.Speed()
                {
                    Value = (float)this.MinSpeed() / 3.6f,
                    Direction = 0
                },
                this.GetCanStop(),
                this.GetEquals(),
                this.VehicleTypes,
                Itinero.Profiles.ProfileMetric.DistanceInMeters).Default();
        }
    }
}