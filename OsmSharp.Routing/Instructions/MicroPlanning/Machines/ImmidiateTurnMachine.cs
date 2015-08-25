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
using OsmSharp.Collections.Tags;
using OsmSharp.Math.Automata;
using OsmSharp.Math.Geo;
using OsmSharp.Math.Geo.Meta;
using OsmSharp.Math.StateMachines;
using OsmSharp.Routing.Instructions.ArcAggregation.Output;

namespace OsmSharp.Routing.Instructions.MicroPlanning.Machines
{
    /// <summary>
    /// Machine to detect significant turns.
    /// </summary>
    public class ImmidateTurnMachine : MicroPlannerMachine
    {
        /// <summary>
        /// Creates a new immidiate turn machine.
        /// </summary>
        /// <param name="planner">The planner.</param>
        public ImmidateTurnMachine(MicroPlanner planner)
            : base(planner, 101)
        {

        }

        /// <summary>
        /// Builds the initial states.
        /// </summary>
        /// <returns></returns>
        protected override FiniteStateMachineState<MicroPlannerMessage> BuildStates()
        {
            // generate states.
            var states = FiniteStateMachineState<MicroPlannerMessage>.Generate(5);

            // state 3 is final.
            states[4].Final = true;

            // 0
            FiniteStateMachineTransition<MicroPlannerMessage>.Generate(states, 0, 1, typeof(MicroPlannerMessageArc));

            // 1
            FiniteStateMachineTransition<MicroPlannerMessage>.Generate(states, 1, 0, typeof(MicroPlannerMessagePoint),
                new FiniteStateMachineTransitionCondition<MicroPlannerMessage>.FiniteStateMachineTransitionConditionDelegate(TestNonSignificantTurn));
            FiniteStateMachineTransition<MicroPlannerMessage>.Generate(states, 1, 2, typeof(MicroPlannerMessagePoint),
                new FiniteStateMachineTransitionCondition<MicroPlannerMessage>.FiniteStateMachineTransitionConditionDelegate(TestSignificantTurn));
            // 2
            FiniteStateMachineTransition<MicroPlannerMessage>.Generate(states, 2, 3, typeof(MicroPlannerMessageArc),
                new FiniteStateMachineTransitionCondition<MicroPlannerMessage>.FiniteStateMachineTransitionConditionDelegate(TestVeryShortArc));

            // 3
            FiniteStateMachineTransition<MicroPlannerMessage>.Generate(states, 3, 4, typeof(MicroPlannerMessagePoint),
                new FiniteStateMachineTransitionCondition<MicroPlannerMessage>.FiniteStateMachineTransitionConditionDelegate(TestSignificantTurn));

            // return the start automata with intial state.
            return states[0];
        }

        /// <summary>
        /// Returns true if the given test object is a very short arc!
        /// </summary>
        /// <param name="test"></param>
        /// <param name="machine"></param>
        /// <returns></returns>
        private static bool TestVeryShortArc(FiniteStateMachine<MicroPlannerMessage> machine, object test)
        {
            if (test is MicroPlannerMessageArc)
            {
                MicroPlannerMessageArc arc = (test as MicroPlannerMessageArc);
                return arc.Arc.Distance.Value < 20;
            }
            return false;
        }

        /// <summary>
        /// Tests if the given turn is significant.
        /// </summary>
        /// <param name="machine"></param>
        /// <param name="test"></param>
        /// <returns></returns>
        private static bool TestNonSignificantTurn(FiniteStateMachine<MicroPlannerMessage> machine, object test)
        {
            if (!ImmidateTurnMachine.TestSignificantTurn(machine, test))
            { // it is no signficant turn.
                return true;
            }
            return false;
        }

        /// <summary>
        /// Tests if the given turn is significant.
        /// </summary>
        /// <param name="machine"></param>
        /// <param name="test"></param>
        /// <returns></returns>
        private static bool TestSignificantTurn(FiniteStateMachine<MicroPlannerMessage> machine, object test)
        {
            if (test is MicroPlannerMessagePoint)
            {
                MicroPlannerMessagePoint point = (test as MicroPlannerMessagePoint);
                if (point.Point.Angle != null)
                {
                    if (point.Point.ArcsNotTaken == null || point.Point.ArcsNotTaken.Count == 0)
                    {
                        return false;
                    }
                    switch (point.Point.Angle.Direction)
                    {
                        case OsmSharp.Math.Geo.Meta.RelativeDirectionEnum.StraightOn:
                            return false;
                    }
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Called after this machine reached the final state.
        /// </summary>
        public override void Succes()
        {
            // get first point.
            AggregatedPoint firstPoint = null;
            if (this.FinalMessages[0] is MicroPlannerMessagePoint)
            { // machine started on a point.
                firstPoint = (this.FinalMessages[0] as MicroPlannerMessagePoint).Point;
            }
            else
            { // get the previous point.
                firstPoint = (this.FinalMessages[0] as MicroPlannerMessageArc).Arc.Previous;
            }

            // get the last arc and the last point.
            var latestArc = (this.FinalMessages[this.FinalMessages.Count - 2] as MicroPlannerMessageArc).Arc;
            var latestPoint = (this.FinalMessages[this.FinalMessages.Count - 1] as MicroPlannerMessagePoint).Point;
            var secondLatestArc = (this.FinalMessages[this.FinalMessages.Count - 4] as MicroPlannerMessageArc).Arc;
            var secondLatestPoint = (this.FinalMessages[this.FinalMessages.Count - 3] as MicroPlannerMessagePoint).Point;

            // count the number of streets in the same turning direction as the turn
            // that was found.
            int count = 0;
            if (MicroPlannerHelper.IsLeft(latestPoint.Angle.Direction, this.Planner.Interpreter))
            {
                count = MicroPlannerHelper.GetLeft(this.FinalMessages, this.Planner.Interpreter);
            }
            else if (MicroPlannerHelper.IsRight(latestPoint.Angle.Direction, this.Planner.Interpreter))
            {
                count = MicroPlannerHelper.GetRight(this.FinalMessages, this.Planner.Interpreter);
            }

            // construct the box indicating the location of the resulting find by this machine.
            var point1 = latestPoint.Location;
            var box = new GeoCoordinateBox(
                new GeoCoordinate(point1.Latitude - 0.001f, point1.Longitude - 0.001f),
                new GeoCoordinate(point1.Latitude + 0.001f, point1.Longitude + 0.001f));

            // get all the names/direction/counts.
            var nextName = latestPoint.Next.Tags;
            var betweenName = latestArc.Tags;
            var beforeName = secondLatestArc.Tags;

            int firstCount = count;

            RelativeDirection firstTurn = secondLatestPoint.Angle;
            RelativeDirection secondTurn = latestPoint.Angle;
            
            // let the scentence planner generate the correct information.
            var metaData = new Dictionary<string, object>();
            metaData["first_street"] = beforeName;
            metaData["first_direction"] = firstTurn;
            metaData["second_street"] = betweenName;
            metaData["second_direction"] = secondTurn;
            metaData["count_before"] = firstCount;
            metaData["pois"] = latestPoint.Points;
            metaData["type"] = "immidiate_turn";
            this.Planner.SentencePlanner.GenerateInstruction(metaData, firstPoint.SegmentIdx, latestPoint.SegmentIdx, box, latestPoint.Points);
        }

        /// <summary>
        /// Returns true if the objects are equal.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is ImmidateTurnMachine)
            { // if the machine can be used more than once 
                // this comparision will have to be updated.
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns the hashcode.
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {// if the machine can be used more than once 
            // this hashcode will have to be updated.
            return this.GetType().GetHashCode();
        }
    }
}
