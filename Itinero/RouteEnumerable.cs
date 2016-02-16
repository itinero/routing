// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2013 Abelshausen Ben
// 
// This file is part of OsmSharp.
// 
// OsmSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// OsmSharp is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with OsmSharp. If not, see <http://www.gnu.org/licenses/>.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using OsmSharp.Math.Geo;
using OsmSharp.Units.Distance;

namespace OsmSharp.Routing
{
    /// <summary>
    /// An enumerator for an OsmSharpRoute.
    /// </summary>
    internal class RouteEnumerable : IEnumerable<GeoCoordinate>
    {
        /// <summary>
        /// Holds the router being enumerated.
        /// </summary>
        private Route _route;

        /// <summary>
        /// Holds the interval.
        /// </summary>
        private double _intervalMeter;

        /// <summary>
        /// Creates a new OsmSharpRoute enumerable.
        /// </summary>
        /// <param name="route"></param>
        internal RouteEnumerable(Route route)
        {
            _route = route;
            _intervalMeter = 10;
        }

        /// <summary>
        /// Creates a new OsmSharpRoute enumerable.
        /// </summary>
        /// <param name="route"></param>
        /// <param name="interval"></param>
        internal RouteEnumerable(Route route, Meter interval)
        {
            _route = route;
            _intervalMeter = interval.Value;
        }

        /// <summary>
        /// Returns an enumerator.
        /// </summary>
        /// <returns></returns>
        public IEnumerator<GeoCoordinate> GetEnumerator()
        {
            return new OsmSharpRouteEnumerator(_route, _intervalMeter);
        }

        /// <summary>
        /// Returns an enumerator.
        /// </summary>
        /// <returns></returns>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }

    /// <summary>
    /// An enumerator for an OsmSharpRoute.
    /// </summary>
    internal class OsmSharpRouteEnumerator : IEnumerator<GeoCoordinate>
    {
        /// <summary>
        /// Holds the router being enumerator.
        /// </summary>
        private Route _route;

        /// <summary>
        /// Holds the interval between enumerations.
        /// </summary>
        private double _intervalMeter;

        /// <summary>
        /// Creates a new OsmSharpRoute enumerator.
        /// </summary>
        /// <param name="route"></param>
        /// <param name="intervalMeter"></param>
        public OsmSharpRouteEnumerator(Route route, double intervalMeter)
        {
            _route = route;
            _currentMeter = 0;
            _intervalMeter = intervalMeter;
        }

        /// <summary>
        /// Holds the current meter.
        /// </summary>
        private double _currentMeter;

        /// <summary>
        /// Holds the current position.
        /// </summary>
        private GeoCoordinate _current;

        /// <summary>
        /// Returns the current position.
        /// </summary>
        public GeoCoordinate Current
        {
            get { return _current; }
        }

        /// <summary>
        /// Disposes all resource associated with this enumerator.
        /// </summary>
        public void Dispose()
        {
            _route = null;
        }

        /// <summary>
        /// Returns the current position.
        /// </summary>
        object System.Collections.IEnumerator.Current
        {
            get { return this.Current; }
        }

        /// <summary>
        /// Returns true if the move to the next position was succesfull.
        /// </summary>
        /// <returns></returns>
        public bool MoveNext()
        {
            _currentMeter += _intervalMeter;
            _current = _route.PositionAfter((Meter)(_currentMeter));
            return _current != null;
        }

        /// <summary>
        /// Resets this enumerator.
        /// </summary>
        public void Reset()
        {
            _currentMeter = 0;
        }
    }
}