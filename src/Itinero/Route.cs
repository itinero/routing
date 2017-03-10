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

using Itinero.Attributes;
using Itinero.LocalGeo;
using System.Collections.Generic;
using System;
using System.Collections;
using Itinero.Navigation.Directions;

namespace Itinero
{
    /// <summary>
    /// Represents a route.
    /// </summary>
    public partial class Route : IEnumerable<RoutePosition>
    {
        /// <summary>
        /// Gets or sets the shape.
        /// </summary>
        public Coordinate[] Shape { get; set; }

        /// <summary>
        /// Gets or sets the attributes.
        /// </summary>
        public IAttributeCollection Attributes { get; set; }

        /// <summary>
        /// Gets or sets the stops.
        /// </summary>
        public Stop[] Stops { get; set; }

        /// <summary>
        /// Gets or sets the meta data.
        /// </summary>
        public Meta[] ShapeMeta { get; set; }

        /// <summary>
        /// Represents a stop.
        /// </summary>
        public class Stop
        {
            /// <summary>
            /// Gets or sets the shape index.
            /// </summary>
            public int Shape { get; set; }

            /// <summary>
            /// Gets or sets the coordinates.
            /// </summary>
            public Coordinate Coordinate { get; set; }

            /// <summary>
            /// Gets or sets the attributes.
            /// </summary>
            public IAttributeCollection Attributes { get; set; }

            /// <summary>
            /// Creates a clone of this object.
            /// </summary>
            public Stop Clone()
            {
                AttributeCollection attributes = null;
                if (this.Attributes != null)
                {
                    attributes = new AttributeCollection(this.Attributes);
                }
                return new Stop()
                {
                    Attributes = attributes,
                    Shape = this.Shape,
                    Coordinate = this.Coordinate
                };
            }

            /// <summary>
            /// The distance in meter.
            /// </summary>
            public float Distance
            {
                get
                {
                    if (this.Attributes == null)
                    {
                        return 0;
                    }
                    float value;
                    if (!this.Attributes.TryGetSingle("distance", out value))
                    {
                        return 0;
                    }
                    return value;
                }
                set
                {
                    if (this.Attributes == null)
                    {
                        this.Attributes = new AttributeCollection();
                    }
                    this.Attributes.SetSingle("distance", value);
                }
            }

            /// <summary>
            /// The time in seconds.
            /// </summary>
            public float Time
            {
                get
                {
                    if (this.Attributes == null)
                    {
                        return 0;
                    }
                    float value;
                    if (!this.Attributes.TryGetSingle("time", out value))
                    {
                        return 0;
                    }
                    return value;
                }
                set
                {
                    if (this.Attributes == null)
                    {
                        this.Attributes = new AttributeCollection();
                    }
                    this.Attributes.SetSingle("time", value);
                }
            }
        }

        /// <summary>
        /// Represents meta-data about a part of this route.
        /// </summary>
        public class Meta
        {
            /// <summary>
            /// Gets or sets the shape index.
            /// </summary>
            public int Shape { get; set; }

            /// <summary>
            /// Gets or sets the attributes.
            /// </summary>
            public IAttributeCollection Attributes { get; set; }

            /// <summary>
            /// Gets or sets the relative direction flag of the attributes.
            /// </summary>
            public bool AttributesDirection { get; set; }

            /// <summary>
            /// Gets or sets the profile.
            /// </summary>
            public string Profile
            {
                get
                {
                    if (this.Attributes == null)
                    {
                        return string.Empty;
                    }
                    string value;
                    if (!this.Attributes.TryGetValue("profile", out value))
                    {
                        return string.Empty;
                    }
                    return value;
                }
                set
                {
                    if (this.Attributes == null)
                    {
                        this.Attributes = new AttributeCollection();
                    }
                    this.Attributes.AddOrReplace("profile", value);
                }
            }

            /// <summary>
            /// Creates a clone of this meta-object.
            /// </summary>
            /// <returns></returns>
            public Meta Clone()
            {
                AttributeCollection attributes = null;
                if (this.Attributes != null)
                {
                    attributes = new AttributeCollection(this.Attributes);
                }
                return new Meta()
                {
                    Attributes = attributes,
                    Shape = this.Shape
                };
            }

            /// <summary>
            /// The distance in meter.
            /// </summary>
            public float Distance
            {
                get
                {
                    if (this.Attributes == null)
                    {
                        return 0;
                    }
                    float value;
                    if (!this.Attributes.TryGetSingle("distance", out value))
                    {
                        return 0;
                    }
                    return value;
                }
                set
                {
                    if (this.Attributes == null)
                    {
                        this.Attributes = new AttributeCollection();
                    }
                    this.Attributes.SetSingle("distance", value);
                }
            }

            /// <summary>
            /// The time in seconds.
            /// </summary>
            public float Time
            {
                get
                {
                    if (this.Attributes == null)
                    {
                        return 0;
                    }
                    float value;
                    if (!this.Attributes.TryGetSingle("time", out value))
                    {
                        return 0;
                    }
                    return value;
                }
                set
                {
                    if (this.Attributes == null)
                    {
                        this.Attributes = new AttributeCollection();
                    }
                    this.Attributes.SetSingle("time", value);
                }
            }
        }

        /// <summary>
        /// Gets or sets the branches.
        /// </summary>
        public Branch[] Branches { get; set; }

        /// <summary>
        /// Represents a branch.
        /// </summary>
        public class Branch
        {
            /// <summary>
            /// Gets or sets the shape index.
            /// </summary>
            public int Shape { get; set; }

            /// <summary>
            /// Gets or sets the coordinates.
            /// </summary>
            public Coordinate Coordinate { get; set; }

            /// <summary>
            /// Gets or sets the attributes.
            /// </summary>
            public IAttributeCollection Attributes { get; set; }

            /// <summary>
            /// Gets or sets the relative direction flag of the attributes.
            /// </summary>
            public bool AttributesDirection { get; set; }

            /// <summary>
            /// Creates a clone of this object.
            /// </summary>
            public Branch Clone()
            {
                AttributeCollection attributes = null;
                if (this.Attributes != null)
                {
                    attributes = new AttributeCollection(this.Attributes);
                }
                return new Branch()
                {
                    Attributes = attributes,
                    Shape = this.Shape,
                    Coordinate = this.Coordinate
                };
            }
        }

        /// <summary>
        /// The distance in meter.
        /// </summary>
        public float TotalDistance
        {
            get
            {
                if (this.Attributes == null)
                {
                    return 0;
                }
                float value;
                if (!this.Attributes.TryGetSingle("distance", out value))
                {
                    return 0;
                }
                return value;
            }
            set
            {
                if (this.Attributes == null)
                {
                    this.Attributes = new AttributeCollection();
                }
                this.Attributes.SetSingle("distance", value);
            }
        }

        /// <summary>
        /// The time in seconds.
        /// </summary>
        public float TotalTime
        {
            get
            {
                if (this.Attributes == null)
                {
                    return 0;
                }
                float value;
                if (!this.Attributes.TryGetSingle("time", out value))
                {
                    return 0;
                }
                return value;
            }
            set
            {
                if (this.Attributes == null)
                {
                    this.Attributes = new AttributeCollection();
                }
                this.Attributes.SetSingle("time", value);
            }
        }

        /// <summary>
        /// Gets or sets the profile.
        /// </summary>
        public string Profile
        {
            get
            {
                if (this.Attributes == null)
                {
                    return string.Empty;
                }
                string value;
                if (!this.Attributes.TryGetValue("profile", out value))
                {
                    return string.Empty;
                }
                return value;
            }
            set
            {
                if (this.Attributes == null)
                {
                    this.Attributes = new AttributeCollection();
                }
                this.Attributes.AddOrReplace("profile", value);
            }
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<RoutePosition> GetEnumerator()
        {
            return new RouteEnumerator(this);
        }

        /// <summary>
        /// Gets the enumerator.
        /// </summary>
        /// <returns></returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return new RouteEnumerator(this);
        }
    }

    /// <summary>
    /// Represents a route enumerator.
    /// </summary>
    class RouteEnumerator : IEnumerator<RoutePosition>
    {
        private readonly Route _route;

        /// <summary>
        /// Creates a new route enumerator.
        /// </summary>
        internal RouteEnumerator(Route route)
        {
            _route = route;
        }

        private RoutePosition _current;

        /// <summary>
        /// Resets this enumerator.
        /// </summary>
        public void Reset()
        {
            _current = new RoutePosition(_route,
                -1, -1, -1, -1);
        }

        /// <summary>
        /// Returns the current object.
        /// </summary>
        public RoutePosition Current
        {
            get
            {
                return _current;
            }
        }

        /// <summary>
        /// Returns the current object.
        /// </summary>
        object IEnumerator.Current
        {
            get
            {
                return _current;
            }
        }

        /// <summary>
        /// Move next.
        /// </summary>
        public bool MoveNext()
        {
            if (_current.Route == null)
            {
                this.Reset();
            }
            return _current.MoveNext();
        }

        /// <summary>
        /// Disponses native resources associated with this enumerator.
        /// </summary>
        public void Dispose()
        {

        }
    }

    /// <summary>
    /// Abstract representation of a route position.
    /// </summary>
    public struct RoutePosition
    {
        /// <summary>
        /// Creates a new route position.
        /// </summary>
        public RoutePosition(Route route, int shape, int stopIndex, 
            int metaIndex, int branchIndex)
        {
            this.Route = route;
            this.Shape = shape;
            this.StopIndex = stopIndex;
            this.MetaIndex = metaIndex;
            this.BranchIndex = branchIndex;
        }

        /// <summary>
        /// Gets the route.
        /// </summary>
        public Route Route { get; private set; }

        /// <summary>
        /// Gets the shape index.
        /// </summary>
        public int Shape { get; private set; }

        /// <summary>
        /// Gets the stop index.
        /// </summary>
        public int StopIndex { get; private set; }

        /// <summary>
        /// Gets the meta index.
        /// </summary>
        public int MetaIndex { get; private set; }

        /// <summary>
        /// Gets the branch index.
        /// </summary>
        public int BranchIndex { get; private set; }

        /// <summary>
        /// Move to the next position.
        /// </summary>
        public bool MoveNext()
        {
            this.Shape++;
            if (this.Route.Shape == null ||
                this.Shape >= this.Route.Shape.Length)
            {
                return false;
            }
            
            if (this.Route.Stops != null)
            {
                if (this.StopIndex == -1)
                {
                    this.StopIndex = 0;
                }
                else
                {
                    while (this.StopIndex < this.Route.Stops.Length &&
                        this.Route.Stops[this.StopIndex].Shape < this.Shape)
                    {
                        this.StopIndex++;
                    }
                }
            }

            if (this.Route.ShapeMeta != null)
            {
                if (this.MetaIndex == -1)
                {
                    this.MetaIndex = 0;
                }
                else
                {
                    while (this.MetaIndex < this.Route.ShapeMeta.Length &&
                        this.Route.ShapeMeta[this.MetaIndex].Shape < this.Shape)
                    {
                        this.MetaIndex++;
                    }
                }
            }

            if (this.Route.Branches != null)
            {
                if (this.BranchIndex == -1)
                {
                    this.BranchIndex = 0;
                }
                else
                {
                    while (this.BranchIndex < this.Route.Branches.Length &&
                        this.Route.Branches[this.BranchIndex].Shape < this.Shape)
                    {
                        this.BranchIndex++;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Move to the next position.
        /// </summary>
        public bool MovePrevious()
        {
            this.Shape--;
            if (this.Route.Shape == null ||
                this.Shape < 0 ||
                this.Shape >= this.Route.Shape.Length)
            {
                return false;
            }

            while (this.Route.Stops != null &&
                this.StopIndex > 0 &&
                this.StopIndex < this.Route.Stops.Length &&
                this.Route.Stops[this.StopIndex].Shape > this.Shape)
            {
                this.StopIndex--;
            }

            while (this.Route.ShapeMeta != null &&
                this.MetaIndex > 0 &&
                this.MetaIndex < this.Route.ShapeMeta.Length &&
                this.Route.ShapeMeta[this.MetaIndex].Shape > this.Shape)
            {
                this.MetaIndex--;
            }

            while (this.Route.Branches != null &&
                this.BranchIndex > 0 &&
                this.BranchIndex < this.Route.Branches.Length &&
                this.Route.Branches[this.BranchIndex].Shape > this.Shape)
            {
                this.BranchIndex--;
            }
            return true;
        }
    }

    /// <summary>
    /// Extension methods for the IRoutePosition-interface.
    /// </summary>
    public static class IRoutePositionExtensions
    {
        /// <summary>
        /// Returns true if this position has stops.
        /// </summary>
        public static bool HasStops(this RoutePosition position)
        {
            return position.Route.Stops != null &&
                position.Route.Stops.Length > position.StopIndex &&
                position.Route.Stops[position.StopIndex].Shape == position.Shape;
        }

        /// <summary>
        /// Returns the stops at this position.
        /// </summary>
        public static IEnumerable<Route.Stop> Stops(this RoutePosition position)
        {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Returns true if this position has branches.
        /// </summary>
        public static bool HasBranches(this RoutePosition position)
        {
            return position.Route.Branches != null &&
                position.Route.Branches.Length > position.BranchIndex &&
                position.Route.Branches[position.BranchIndex].Shape == position.Shape;
        }

        /// <summary>
        /// Returns the branches at this position.
        /// </summary>
        public static IEnumerable<Route.Branch> Branches(this RoutePosition position)
        {
            var branches = new List<Route.Branch>();
            if (position.Route.Branches != null &&
                position.Route.Branches.Length > position.BranchIndex &&
                position.Route.Branches[position.BranchIndex].Shape == position.Shape)
            {
                var branchIndex = position.BranchIndex;
                while (position.Route.Branches.Length > branchIndex && 
                    position.Route.Branches[branchIndex].Shape == position.Shape)
                {
                    branches.Add(position.Route.Branches[branchIndex]);
                    branchIndex++;
                }
            }
            return branches;
        }

        /// <summary>
        /// Returns true if this position has current meta.
        /// </summary>
        public static bool HasCurrentMeta(this RoutePosition position)
        {
            return position.Route.ShapeMeta != null &&
                position.Route.ShapeMeta.Length > position.MetaIndex &&
                position.Route.ShapeMeta[position.MetaIndex].Shape == position.Shape;
        }

        /// <summary>
        /// Returns the current meta.
        /// </summary>
        public static Route.Meta CurrentMeta(this RoutePosition position)
        {
            if (position.HasCurrentMeta())
            {
                return position.Route.ShapeMeta[position.MetaIndex];
            }
            return null;
        }

        /// <summary>
        /// Returns the meta that applies to this position.
        /// </summary>
        public static Route.Meta Meta(this RoutePosition position)
        {
            if (position.Route.ShapeMeta != null &&
                position.Route.ShapeMeta.Length > position.MetaIndex)
            {
                return position.Route.ShapeMeta[position.MetaIndex];
            }
            return null;
        }

        /// <summary>
        /// Returns true if this position is the first position.
        /// </summary>
        public static bool IsFirst(this RoutePosition position)
        {
            return position.Shape == 0;
        }

        /// <summary>
        /// Returns true if this position is the last position.
        /// </summary>
        public static bool IsLast(this RoutePosition position)
        {
            return position.Route.Shape.Length - 1 == position.Shape;
        }

        /// <summary>
        /// Gets the previous location.
        /// </summary>
        public static Coordinate PreviousLocation(this RoutePosition position)
        {
            return position.Route.Shape[position.Shape - 1];
        }

        /// <summary>
        /// Gets the next location.
        /// </summary>
        public static Coordinate NextLocation(this RoutePosition position)
        {
            return position.Route.Shape[position.Shape + 1];
        }

        /// <summary>
        /// Gets the location.
        /// </summary>
        public static Coordinate Location(this RoutePosition position)
        {
            return position.Route.Shape[position.Shape];
        }

        /// <summary>
        /// Gets the relative direction at this position.
        /// </summary>
        public static RelativeDirection RelativeDirection(this RoutePosition position)
        {
            return position.Route.RelativeDirectionAt(position.Shape);
        }

        /// <summary>
        /// Gets the direction at this position.
        /// </summary>
        public static DirectionEnum Direction(this RoutePosition position)
        {
            return DirectionCalculator.Calculate(position.Location(), position.NextLocation());
        }

        /// <summary>
        /// Gets the meta attribute for route at the current position.
        /// </summary>
        public static string GetMetaAttribute(this RoutePosition position, string key)
        {
            var meta = position.Meta();
            if (meta == null ||
                meta.Attributes == null)
            {
                return string.Empty;
            }
            string value = string.Empty;
            if (!meta.Attributes.TryGetValue(key, out value))
            {
                return string.Empty;
            }
            return value;
        }

        /// <summary>
        /// Returns true if the meta attribute for the route at the current position contains the given attribute.
        /// </summary>
        public static bool ContainsMetaAttribute(this RoutePosition position, string key, string value)
        {
            var meta = position.Meta();
            if (meta == null ||
                meta.Attributes == null)
            {
                return false;
            }
            return meta.Attributes.Contains(key, value);
        }

        /// <summary>
        /// Gets the next route position.
        /// </summary>
        public static RoutePosition? Next(this RoutePosition position)
        {
            if(position.MoveNext())
            {
                return position;
            }
            return null;
        }

        /// <summary>
        /// Gets the previous route position.
        /// </summary>
        public static RoutePosition? Previous(this RoutePosition position)
        {
            if (position.MovePrevious())
            {
                return position;
            }
            return null;
        }

        /// <summary>
        /// Gets the next position until a given stop condition is met.
        /// </summary>
        public static RoutePosition? GetNextUntil(this RoutePosition position, Func<RoutePosition, bool> stopHere)
        {
            var next = position.Next();
            while (next != null)
            {
                if (stopHere(next.Value))
                {
                    return next;
                }
            }
            return null;
        }

        /// <summary>
        /// Gets the previous position until a given stop condition is met.
        /// </summary>
        public static RoutePosition? GetPreviousUntil(this RoutePosition position, Func<RoutePosition, bool> stopHere)
        {
            var next = position.Previous();
            while (next != null)
            {
                if (stopHere(next.Value))
                {
                    return next;
                }
            }
            return null;
        }
    }
}