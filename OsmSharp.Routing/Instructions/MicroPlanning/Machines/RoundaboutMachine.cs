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

using OsmSharp.Math.Automata;
using OsmSharp.Math.Geo;
using OsmSharp.Math.StateMachines;
using OsmSharp.Routing.Instructions.ArcAggregation.Output;
using System.Collections.Generic;

namespace OsmSharp.Routing.Instructions.MicroPlanning.Machines
{
    /// <summary>
    /// A roundabout machine.
    /// </summary>
    public class RoundaboutMachine : MicroPlannerMachine
    {
        /// <summary>
        /// Creates a new roundabout machine.
        /// </summary>
        /// <param name="planner">The planner.</param>
        public RoundaboutMachine(MicroPlanner planner)
            : base(planner, 200)
        {

        }

        /// <summary>
        /// Builds the initial states.
        /// </summary>
        /// <returns></returns>
        protected override FiniteStateMachineState<MicroPlannerMessage> BuildStates()
        {
            // generate states.
            List<FiniteStateMachineState<MicroPlannerMessage>> states = FiniteStateMachineState<MicroPlannerMessage>.Generate(3);

            // state 2 is final.
            states[2].Final = true;

            // 0
            FiniteStateMachineTransition<MicroPlannerMessage>.Generate(states, 0, 0, typeof(MicroPlannerMessageArc));
            FiniteStateMachineTransition<MicroPlannerMessage>.Generate(states, 0, 1, typeof(MicroPlannerMessagePoint),
                new FiniteStateMachineTransitionCondition<MicroPlannerMessage>.FiniteStateMachineTransitionConditionDelegate(TestRoundaboutEntry));

            // 1
            FiniteStateMachineTransition<MicroPlannerMessage>.Generate(states, 1, 1, typeof(MicroPlannerMessagePoint),
                new FiniteStateMachineTransitionCondition<MicroPlannerMessage>.FiniteStateMachineTransitionConditionDelegate(TestNonRoundaboutExit));
            FiniteStateMachineTransition<MicroPlannerMessage>.Generate(states, 1, 1, typeof(MicroPlannerMessageArc),
                new FiniteStateMachineTransitionCondition<MicroPlannerMessage>.FiniteStateMachineTransitionConditionDelegate(TestRoundaboutArc));

            // 2
            FiniteStateMachineTransition<MicroPlannerMessage>.Generate(states, 1, 2, typeof(MicroPlannerMessagePoint),
                new FiniteStateMachineTransitionCondition<MicroPlannerMessage>.FiniteStateMachineTransitionConditionDelegate(TestRoundaboutExit));

            // return the start automata with intial state.
            return states[0];
        }

        /// <summary>
        /// Tests if the given turn is a turn onto a roundabout.
        /// </summary>
        /// <param name="machine"></param>
        /// <param name="test"></param>
        /// <returns></returns>
        private static bool TestRoundaboutEntry(FiniteStateMachine<MicroPlannerMessage> machine, object test)
        {
            if (test is MicroPlannerMessagePoint)
            {
                MicroPlannerMessagePoint point = (test as MicroPlannerMessagePoint);
                if (point.Point.Next != null)
                {
                    if ((machine as MicroPlannerMachine).Planner.Interpreter.EdgeInterpreter.IsRoundabout(
                        point.Point.Next.Tags))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Returns true if the given object is an arc of a roundabout.
        /// </summary>
        /// <param name="machine"></param>
        /// <param name="test"></param>
        /// <returns></returns>
        private static bool TestRoundaboutArc(FiniteStateMachine<MicroPlannerMessage> machine, object test)
        {
            if (test is MicroPlannerMessageArc)
            {
                MicroPlannerMessageArc arc = (test as MicroPlannerMessageArc);
                if ((machine as MicroPlannerMachine).Planner.Interpreter.EdgeInterpreter.IsRoundabout(arc.Arc.Tags))
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        /// Tests if the given turn is a turn out of a roundabout.
        /// </summary>
        /// <param name="test"></param>
        /// <param name="machine"></param>
        /// <returns></returns>
        public static bool TestNonRoundaboutExit(FiniteStateMachine<MicroPlannerMessage> machine, object test)
        {
            return !RoundaboutMachine.TestRoundaboutExit(machine, test);
        }

        /// <summary>
        /// Tests if the given turn is a turn out of a roundabout.
        /// </summary>
        /// <param name="machine"></param>
        /// <param name="test"></param>
        /// <returns></returns>
        private static bool TestRoundaboutExit(FiniteStateMachine<MicroPlannerMessage> machine, object test)
        {
            if (test is MicroPlannerMessagePoint)
            {
                var point = (test as MicroPlannerMessagePoint);
                if (point.Point.Next != null)
                {
                    if (!(machine as MicroPlannerMachine).Planner.Interpreter.EdgeInterpreter.IsRoundabout(point.Point.Next.Tags))
                    {
                        return true;
                    }
                }
            }
            return false;
        }

        /// <summary>
        /// Called when this machine is succesfull.
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

            // let the scentence planner generate the correct information.
            var metaData = new Dictionary<string, object>();
            metaData["count"] = count + 1;
            metaData["street"] = latestPoint.Next.Tags;
            metaData["pois"] = latestPoint.Points;
            metaData["type"] = "roundabout";
            this.Planner.SentencePlanner.GenerateInstruction(metaData, firstPoint.SegmentIdx, latestPoint.SegmentIdx, box, latestPoint.Points);
        }

        /// <summary>
        /// Returns true if the objects are equal.
        /// </summary>
        /// <param name="obj"></param>
        /// <returns></returns>
        public override bool Equals(object obj)
        {
            if (obj is RoundaboutMachine)
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
