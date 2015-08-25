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

using System.Collections.Generic;
using OsmSharp.Math.Geo;
using OsmSharp.Routing.Instructions;
using OsmSharp.Routing.Instructions.LanguageGeneration;
using OsmSharp.Routing.Interpreter;
using OsmSharp.Units.Distance;
using OsmSharp.Units.Time;

namespace OsmSharp.Routing.Navigation
{
    /// <summary>
    /// A tracker to track the progress along a route.
    /// </summary>
    public class RouteTracker
    {
        /// <summary>
        /// Holds the route to track.
        /// </summary>
        private readonly Route _route;

        /// <summary>
        /// Holds the instructions list.
        /// </summary>
        private readonly List<Instruction> _instructions;

        /// <summary>
        /// Creates a route tracker that tracks the given route and it's instructions.
        /// </summary>
        /// <param name="route"></param>
        /// <param name="interpreter"></param>
        public RouteTracker(Route route, IRoutingInterpreter interpreter)
        {
            _route = route;
            _instructions = InstructionGenerator.Generate(route, interpreter);
        }

        /// <summary>
        /// Creates a route tracker that tracks the given route and it's instructions.
        /// </summary>
        /// <param name="route"></param>
        /// <param name="interpreter"></param>
        /// <param name="languageGenerator"></param>
        public RouteTracker(Route route, IRoutingInterpreter interpreter, ILanguageGenerator languageGenerator)
        {
            _route = route;
            _instructions = InstructionGenerator.Generate(route, interpreter, languageGenerator);
        }

        /// <summary>
        /// Holds the current position along the given route. 
        /// </summary>
        private GeoCoordinate _currentRoutePosition;

        /// <summary>
        /// Holds the current position.
        /// </summary>
        private GeoCoordinate _currentPosition;

        /// <summary>
        /// Holds the index of the next instruction.
        /// </summary>
        private int _nextInstructionIdx = -1;

        /// <summary>
        /// Returns the position on the route closest to the current position.
        /// </summary>
        public GeoCoordinate PositionRoute
        {
            get
            {
                return _currentRoutePosition;
            }
        }

        /// <summary>
        /// Returns the current position.
        /// </summary>
        public GeoCoordinate Position
        {
            get
            {
                return _currentPosition;
            }
        }

        /// <summary>
        /// Returns the position on the route of the next instruction.
        /// </summary>
        public GeoCoordinate PositionNextInstruction
        {
            get
            {
                if (this.NextInstruction != null)
                { // the next instruction exists.
                    var entry = _route.Segments[this.NextInstruction.LastSegmentIdx];
                    if (entry != null)
                    { // entry found.
                        return new GeoCoordinate(entry.Latitude, entry.Longitude);
                    }
                }
                return null;
            }
        }

        /// <summary>
        /// Holds the estimated time from start loction.
        /// </summary>
        private Second _timeFromStart;

        /// <summary>
        /// Holds the ESTIMATED time between the start position and the current position.
        /// </summary>
        public Second TimeFromStart
        {
            get
            {
                return _timeFromStart;
            }
        }

        /// <summary>
        /// Returns true if this tracker is reporting time information.
        /// </summary>
        public bool HasTimes
        {
            get
            {
                return _route.HasTimes;
            }
        }

        /// <summary>
        /// Holds the distance from the start location.
        /// </summary>
        private Meter _distanceFromStart;

        /// <summary>
        /// Returns the distance between the start position and the current position.
        /// </summary>
        public Meter DistanceFromStart
        {
            get
            {
                return _distanceFromStart;
            }
        }

        /// <summary>
        /// Holds the ESTIMATED time to the end location.
        /// </summary>
        private Second _timeToEnd;

        /// <summary>
        /// Holds the ESTIMATED time to the end location.
        /// </summary>
        public Second TimeToEnd
        {
            get
            {
                return _timeToEnd;
            }
        }

        /// <summary>
        /// Holds the distance to the end location.
        /// </summary>
        private Meter _distanceToEnd;

        /// <summary>
        /// Returns the distance between the end position and the current position.
        /// </summary>
        public Meter DistanceToEnd
        {
            get
            {
                return _distanceToEnd;
            }
        }

        /// <summary>
        /// Returns the position after the given distance is travelled relative to the current position.
        /// </summary>
        /// <param name="distance"></param>
        /// <returns></returns>
        public GeoCoordinate PositionAfter(Meter distance)
        {
            return _route.PositionAfter(_distanceFromStart + distance);
        }

        /// <summary>
        /// Holds the distance to the next instruction.
        /// </summary>
        private Meter _distanceNextInstruction;

        /// <summary>
        /// Returns the distance between the current position and the route.
        /// </summary>
        public Meter DistanceNextInstruction
        {
            get
            {
                return _distanceNextInstruction;
            }
        }

        /// <summary>
        /// Returns the next instruction.
        /// </summary>
        public Instruction NextInstruction
        {
            get
            {
                if (_nextInstructionIdx > 0 && _nextInstructionIdx < _instructions.Count)
                {
                    return _instructions[_nextInstructionIdx];
                }
                return null;
            }
        }

        /// <summary>
        /// Returns the next instruction index.
        /// </summary>
        public int NextInstructionIdx
        {
            get
            {
                return _nextInstructionIdx;
            }
        }

        /// <summary>
        /// Returns the instruction list that 
        /// </summary>
        public List<Instruction> NextInstructionList
        {
            get
            {
                return _instructions;
            }
        }

        /// <summary>
        /// Updates the tracker with the given location.
        /// </summary>
        /// <param name="location">The measured location.</param>
        public void Track(GeoCoordinate location)
        {
            // set the current location.
            _currentPosition = location;

            // calculate the total distance.
            var previous = new GeoCoordinate(_route.Segments[0].Latitude, _route.Segments[0].Longitude); ;
            var totalDistance = 0.0;
            for (int idx = 1; idx < _route.Segments.Length; idx++)
            {
                GeoCoordinate next = new GeoCoordinate(_route.Segments[idx].Latitude, _route.Segments[idx].Longitude);
                totalDistance = totalDistance + previous.DistanceReal(next).Value;
                previous = next;
            }
            double totalTime = _route.TotalTime;

            // project onto the route.
            int entryIdx;
            _route.ProjectOn(_currentPosition, out _currentRoutePosition, out entryIdx, out _distanceFromStart, out _timeFromStart);
            _distanceToEnd = totalDistance - _distanceFromStart;
            _timeToEnd = totalTime - _timeFromStart.Value;

            // find the next instruction.
            _nextInstructionIdx = -1;
            for (int instructionIdx = 0; instructionIdx < _instructions.Count; instructionIdx++)
            {
                var instruction = _instructions[instructionIdx];
                if (instruction.LastSegmentIdx > entryIdx)
                { // stop here!
                    _nextInstructionIdx = instructionIdx;
                    break;
                }
            }
            if(_nextInstructionIdx < 0)
            { // no instruction was found after the entryIdx: assume last instruction.
                _nextInstructionIdx = _instructions.Count - 1;
            }

            // calculate the distance to the next instruction.
            previous = _currentRoutePosition;
            var distance = 0.0;
            for (int idx = entryIdx + 1; idx <= _instructions[_nextInstructionIdx].LastSegmentIdx && idx < _route.Segments.Length; idx++)
            {
                var next = (new GeoCoordinate(_route.Segments[idx].Latitude, _route.Segments[idx].Longitude));
                distance = distance + previous.DistanceReal(next).Value;
                previous = next;
            }
            _distanceNextInstruction = distance;
        }
    }
}