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

namespace Itinero.Osm.Vehicles
{
    /// <summary>
    ///     Vehicle class contains routing info
    /// </summary>
    public abstract class Vehicle
    {
        /// <summary>
        /// Default Car
        /// </summary>
        public static readonly Car Car = new Car();

        /// <summary>
        /// Default Pedestrian
        /// </summary>
        public static readonly Pedestrian Pedestrian = new Pedestrian();

        /// <summary>
        /// Default Bicycle
        /// </summary>
        public static readonly Bicycle Bicycle = new Bicycle();

        /// <summary>
        /// Default Moped
        /// </summary>
        public static readonly Moped Moped = new Moped();

        /// <summary>
        /// Default MotorCycle
        /// </summary>
        public static readonly MotorCycle MotorCycle = new MotorCycle();

        /// <summary>
        /// Default SmallTruck
        /// </summary>
        public static readonly SmallTruck SmallTruck = new SmallTruck();

        /// <summary>
        /// Default BigTruck
        /// </summary>
        public static readonly BigTruck BigTruck = new BigTruck();

        /// <summary>
        /// Default BigTruck
        /// </summary>
        public static readonly Bus Bus = new Bus();

        /// <summary>
        /// Registers all default vehicles.
        /// </summary>
        public static void RegisterVehicles()
        {
            Car.Register();
            Pedestrian.Register();
            Bicycle.Register();
            Moped.Register();
            MotorCycle.Register();
            SmallTruck.Register();
            BigTruck.Register();
            Bus.Register();
        }

        /// <summary>
        /// Holds the vehicles by name.
        /// </summary>
        private static Dictionary<string, Vehicle> VehiclesByName = null;

        /// <summary>
        /// Creates a new vehicle.
        /// </summary>
        public Vehicle()
        {

        }

        /// <summary>
        /// Registers this vehicle by name.
        /// </summary>
        public virtual void Register()
        {
            if (VehiclesByName == null)
            {
                VehiclesByName = new Dictionary<string, Vehicle>();
            }
            VehiclesByName[this.UniqueName.ToLowerInvariant()] = this;

            foreach (var profile in this.GetProfileDefinitions())
            {
                Itinero.Profiles.ProfileDefinition.Register(profile);
            }
        }

        /// <summary>
        /// Returns the vehicle with the given name.
        /// </summary>
        public static Vehicle GetByUniqueName(string uniqueName)
        {
            Vehicle vehicle;
            if (!Vehicle.TryGetByUniqueName(uniqueName, out vehicle))
            { // vehicle name not found.
                throw new ArgumentOutOfRangeException(string.Format("Vehicle profile with name {0} not found or not registered.", uniqueName));
            }
            return vehicle;
        }

        /// <summary>
        /// Gets all registered vehicles.
        /// </summary>
        public static IEnumerable<Vehicle> GetAllRegistered()
        {
            return VehiclesByName.Values;
        }

        /// <summary>
        /// Tries to the the vehicle given it's unique name.
        /// </summary>
        public static bool TryGetByUniqueName(string uniqueName, out Vehicle vehicle)
        {
            if (uniqueName == null) { throw new ArgumentNullException("uniqueName"); }

            if (VehiclesByName == null)
            { // no vehicles have been registered.
                Vehicle.RegisterVehicles();
            }
            uniqueName = uniqueName.ToLowerInvariant();
            if (!VehiclesByName.TryGetValue(uniqueName, out vehicle))
            { // vehicle name not registered.
                return false;
            }
            return true;
        }

        /// <summary>
        /// Returns true if the given tag is relevant for any registered vehicle, false otherwise.
        /// </summary>
        public static bool IsRelevantForOneOrMore(string key)
        {
            // register at least the default vehicles.
            if (VehiclesByName == null)
            { // no vehicles have been registered.
                Vehicle.RegisterVehicles();
            }

            foreach (var vehicle in VehiclesByName)
            {
                if (vehicle.Value.IsRelevant(key))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Returns true if the given key-value pair is relevant for any registered vehicle, false otherwise.
        /// </summary>
        public static bool IsRelevantForOneOrMore(string key, string value)
        {
            if (VehiclesByName == null)
            {
                Vehicle.RegisterVehicles();
            }

            foreach (var vehicle in VehiclesByName)
            {
                if (vehicle.Value.IsRelevant(key, value))
                {
                    return true;
                }
            }
            return false;
        }

        private static HashSet<string> _relevantProfileKeys = new HashSet<string> { "oneway", "highway", "vehicle", "motor_vehicle",
           "bicycle", "foot", "access", "maxspeed", "junction" };
        private static HashSet<string> _relevantMetaKeys = new HashSet<string> { "name" };

        /// <summary>
        /// Returns true if the given key is relevant for profile.
        /// </summary>
        public virtual bool IsRelevantForProfile(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
            {
                return false;
            }
            if (_relevantProfileKeys.Contains(key))
            {
                return true;
            }
            if (key.StartsWith("oneway"))
            {
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns true if the given key is relevant for meta-data.
        /// </summary>
        /// <returns></returns>
        public virtual bool IsRelevantForMeta(string key)
        {
            return _relevantMetaKeys.Contains(key);
        }

        /// <summary>
        /// Holds names of the hierarchy of vehicle types for this vehicle.
        /// </summary>
        /// <remarks>This is used to interpret restrictions.
        /// And OpenStreetMap-wiki: http://wiki.openstreetmap.org/wiki/Key:access#Transport_mode_restrictions</remarks>
        public readonly List<string> VehicleTypes = new List<string>();

        /// <summary>
        /// Contains Accessiblity Rules
        /// </summary>
        protected readonly Dictionary<string, string> AccessibleTags = new Dictionary<string, string>();

        /// <summary>
        /// Trys to return the highwaytype from the tags
        /// </summary>
        protected bool TryGetHighwayType(IAttributeCollection tags, out string highwayType)
        {
            highwayType = string.Empty;
            return tags != null && tags.TryGetValue("highway", out highwayType);
        }

        /// <summary>
        /// Returns true if the edge with the given tags can be traversed by the vehicle.
        /// </summary>
        public virtual bool CanTraverse(IAttributeCollection tags)
        {
            string highwayType;
            if (TryGetHighwayType(tags, out highwayType))
            {
                return IsVehicleAllowed(tags, highwayType);
            }
            return false;
        }
        
        /// <summary>
        /// Returns true if an edge with the given profile can be use for an end- or startpoint.
        /// </summary>
        public virtual bool CanStopOn(IAttributeCollection tags)
        {
            string highway;
            if (tags != null && tags.TryGetValue("highway", out highway))
            {
                return !string.IsNullOrWhiteSpace(highway);
            }
            return false;
        }

        /// <summary>
        /// Returns the Max Speed for the highwaytype in Km/h
        /// </summary>
        public abstract float MaxSpeedAllowed(string highwayType);

        /// <summary>
        /// Returns the max speed this vehicle can handle.
        /// </summary>
        public abstract float MaxSpeed();

        /// <summary>
        /// Returns the minimum speed of this vehicle.
        /// </summary>
        public abstract float MinSpeed();

        /// <summary>
        /// Returns the maximum speed.
        /// </summary>
        public virtual float MaxSpeedAllowed(IAttributeCollection tags)
        {
            float speed = 5;

            // get max-speed tag if any.
            if (tags.TryGetMaxSpeed(out speed))
            {
                return speed;
            }

            string highwayType;
            if (TryGetHighwayType(tags, out highwayType))
            {
                speed = this.MaxSpeedAllowed(highwayType);
            }

            return speed;
        }

        /// <summary>
        /// Estimates the probable speed of this vehicle on a way with the given tags.
        /// </summary>
        public virtual float ProbableSpeed(IAttributeCollection tags)
        {
            var maxSpeedAllowed = this.MaxSpeedAllowed(tags);
            var maxSpeed = this.MaxSpeed();
            if (maxSpeed < maxSpeedAllowed)
            {
                return maxSpeed;
            }
            return maxSpeedAllowed;
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
        /// Returns true if the edge is one way forward, false if backward, null if bidirectional.
        /// </summary>
        public virtual bool? IsOneWay(IAttributeCollection tags)
        {
            string oneway;
            if (tags.TryGetValue("oneway", out oneway))
            {
                if (oneway == "yes")
                {
                    return true;
                }
                else if (oneway == "no")
                { // explicitly tagged as not oneway.
                    return null;
                }
                return false;
            }
            string junction;
            if (tags.TryGetValue("junction", out junction))
            {
                if (junction == "roundabout")
                {
                    return true;
                }
            }
            return null;
        }

        /// <summary>
        /// Returns the name of a given way.
        /// </summary>
        private string GetName(IAttributeCollection tags)
        {
            var name = string.Empty;
            if (!tags.TryGetValue("name", out name))
            {
                return string.Empty;
            }
            return name;
        }

        /// <summary>
        /// Returns true if the vehicle is allowed on the way represented by these tags
        /// </summary>
        protected abstract bool IsVehicleAllowed(IAttributeCollection tags, string highwayType);

        /// <summary>
        /// Returns a unique name this vehicle type.
        /// </summary>
        public abstract string UniqueName
        {
            get;
        }

        /// <summary>
        /// Returns true if the key is relevant for this vehicle profile.
        /// </summary>
        public virtual bool IsRelevant(string key)
        {
            return this.IsRelevantForProfile(key) || this.IsRelevantForMeta(key);
        }

        /// <summary>
        /// Returns true if the key-value pair is relevant for this vehicle profile.
        /// </summary>
        public virtual bool IsRelevant(string key, string value)
        {
            return this.IsRelevant(key);
        }

        /// <summary>
        /// Returns a profile for this vehicle that can be used for finding fastest routes;.
        /// </summary>
        public Itinero.Profiles.Profile Fastest()
        {
            return new Itinero.Profiles.ProfileDefinition(this.UniqueName,
                this.GetGetSpeed().ToUnconstrainedGetSpeed(),
                this.GetGetMinSpeed(),
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
                this.GetGetMinSpeed(),
                this.GetCanStop(),
                this.GetEquals(), 
                this.VehicleTypes, 
                Itinero.Profiles.ProfileMetric.DistanceInMeters).Default();
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
        /// Gets the get speed function.
        /// </summary>
        internal Func<IAttributeCollection, Itinero.Profiles.Speed> GetGetSpeed()
        {
            return (tags) =>
            {
                if (this.CanTraverse(tags))
                {
                    var speed = new Itinero.Profiles.Speed()
                    {
                        Value = (float)this.ProbableSpeed(tags) / 3.6f,
                        Direction = 0
                    };
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
                }
                return Itinero.Profiles.Speed.NoSpeed;
            };
        }

        /// <summary>
        /// Gets the get minimum speed function.
        /// </summary>
        internal Func<Itinero.Profiles.Speed> GetGetMinSpeed()
        {
            return () => new Itinero.Profiles.Speed()
            {
                Value = (float)this.MinSpeed() / 3.6f,
                Direction = 0
            };
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
    }
}