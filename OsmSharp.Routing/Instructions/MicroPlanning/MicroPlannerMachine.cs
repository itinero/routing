// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2015 Abelshausen Ben
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
using OsmSharp.Math.Automata;
using OsmSharp.Math.StateMachines;
using OsmSharp.Math.Geo;
using OsmSharp.Collections.Tags;
using OsmSharp.Routing.Instructions.ArcAggregation.Output;

namespace OsmSharp.Routing.Instructions.MicroPlanning
{
    /// <summary>
    /// An abstract micro planner machine.
    /// </summary>
    public abstract class MicroPlannerMachine : FiniteStateMachine<MicroPlannerMessage>
    {
        /// <summary>
        /// The contains the microplanner to report back to.
        /// </summary>
        private MicroPlanner _planner;

        /// <summary>
        /// The priority.
        /// </summary>
        private int _priority;

        /// <summary>
        /// Creates a new event machine.
        /// </summary>
        /// <param name="planner"></param>
        /// <param name="priority"></param>
        protected MicroPlannerMachine(MicroPlanner planner, int priority)
        {
            _planner = planner;
            _priority = priority;

            this.IsSuccesfull = false;
        }

        /// <summary>
        /// Builds the initial state.
        /// </summary>
        /// <returns></returns>
        protected abstract override FiniteStateMachineState<MicroPlannerMessage> BuildStates();

        /// <summary>
        /// Returns the microplanner.
        /// </summary>
        public MicroPlanner Planner
        {
            get
            {
                return _planner;
            }
        }

        /// <summary>
        /// Returns the priority.
        /// </summary>
        public int Priority
        {
            get
            {
                return _priority;
            }
        }

        /// <summary>
        /// Holds the consumed messages.
        /// </summary>

        private IList<MicroPlannerMessage> _messages;

        /// <summary>
        /// Returns the message consumed.
        /// </summary>
        public IList<MicroPlannerMessage> FinalMessages
        {
            get
            {
                return _messages;
            }
        }

        /// <summary>
        /// Called when this machine is succesfull.
        /// </summary>
        public abstract void Succes();

        /// <summary>
        /// Gets the succesfull state.
        /// </summary>
        public bool IsSuccesfull { get; set; }

        /// <summary>
        /// Returns the boundingbox for the current message consumed in this machine.
        /// </summary>
        /// <returns></returns>
        protected GeoCoordinateBox GetBoxForCurrentMessages()
        {
            if (this.FinalMessages.Count == 0) { return null; }

            var route = this.FinalMessages[0].Route;

            // get first and last point.
            AggregatedPoint firstPoint = null;
            if (this.FinalMessages[0] is MicroPlannerMessagePoint)
            { // machine started on a point.
                firstPoint = (this.FinalMessages[0] as MicroPlannerMessagePoint).Point;
            }
            else
            { // get the previous point.
                firstPoint = (this.FinalMessages[0] as MicroPlannerMessageArc).Arc.Previous;
            }
            AggregatedPoint lastPoint = null;
            if (this.FinalMessages[this.FinalMessages.Count - 1] is MicroPlannerMessagePoint)
            { // machine ended on a point.
                lastPoint = (this.FinalMessages[this.FinalMessages.Count - 1] as MicroPlannerMessagePoint).Point;
            }
            else
            { // machine ended on an arc.
                lastPoint = (this.FinalMessages[this.FinalMessages.Count - 1] as MicroPlannerMessageArc).Arc.Next;
            }

            // build the boundingbox.
            var points = new List<GeoCoordinate>();
            for (int segmentIdx = firstPoint.SegmentIdx; segmentIdx <= lastPoint.SegmentIdx; segmentIdx++)
            {
                var segment = route.Segments[segmentIdx];
                points.Add(new GeoCoordinate(segment.Latitude, segment.Longitude));
            }
            return new GeoCoordinateBox(points.ToArray());
        }

        /// <summary>
        /// Returns a collection of tags that has not changed int the messages consumed in this machine.
        /// </summary>
        /// <returns></returns>
        protected TagsCollectionBase GetConstantTagsForCurrentMessages()
        {
            TagsCollectionBase constantTags = null;
            foreach(var message in this.FinalMessages)
            {
                if(message is MicroPlannerMessageArc)
                {
                    var tags = (message as MicroPlannerMessageArc).Arc.Tags;
                    if(tags == null)
                    { // one of the tags collection is empty, intersection is also empty.
                        return new TagsCollection();
                    }
                    else
                    { // calculate intersection.
                        if(constantTags == null)
                        { // first tags.
                            constantTags = tags;
                        }
                        else
                        { // calculate intersection.
                            constantTags.Intersect(tags);
                            if(constantTags.Count == 0)
                            { // no intersection.
                                return constantTags;
                            }
                        }
                    }
                }
            }
            return constantTags;
        }

        /// <summary>
        /// Called when a final state is reached.
        /// </summary>
        /// <param name="messages"></param>
        protected override void RaiseFinalStateEvent(IList<MicroPlannerMessage> messages)
        {
            _messages = new List<MicroPlannerMessage>(messages);

            this.Planner.ReportFinal(this, messages);
        }

        /// <summary>
        /// Called when a reset event occured.
        /// </summary>
        /// <param name="even"></param>
        /// <param name="state"></param>
        protected override void RaiseResetEvent(MicroPlannerMessage even, FiniteStateMachineState<MicroPlannerMessage> state)
        {
            this.Planner.ReportReset(this);
        }
    }
}
