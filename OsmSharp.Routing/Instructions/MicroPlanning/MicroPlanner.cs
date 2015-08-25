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
using OsmSharp.Routing.Instructions.ArcAggregation.Output;
using OsmSharp.Routing.Instructions.LanguageGeneration;
using OsmSharp.Routing.Instructions.MicroPlanning.Machines;
using OsmSharp.Routing.Interpreter;
using OsmSharp.Logging;

namespace OsmSharp.Routing.Instructions.MicroPlanning
{
    /// <summary>
    /// Plans aggregated messages into instructions.
    /// </summary>
    public class MicroPlanner
    {
        /// <summary>
        /// Holds the routing interpreter.
        /// </summary>
        private IRoutingInterpreter _interpreter;

        /// <summary>
        /// Creates a new planner.
        /// </summary>
        public MicroPlanner(ILanguageGenerator languageGenerator, IRoutingInterpreter interpreter)
        {
            _interpreter = interpreter;

            _machines = new List<MicroPlannerMachine>();
            this.InitializeMachines(_machines);
            this.InitializeMessagesStack();

            this.SentencePlanner = new SentencePlanner(languageGenerator);
        }

        /// <summary>
        /// Returns the routing interpreter.
        /// </summary>
        public IRoutingInterpreter Interpreter
        {
            get
            {
                return _interpreter;
            }
        }

        /// <summary>
        /// The scentence planner for this micro planner.
        /// </summary>
        public SentencePlanner SentencePlanner
        {
            get;
            private set;
        }

        /// <summary>
        /// Holds the current object from the aggregated stream.
        /// </summary>
        private Aggregated _current;

        /// <summary>
        /// Plans all the messages in the aggregated 
        /// </summary>
        /// <param name="route"></param>
        /// <param name="p"></param>
        public List<Instruction> Plan(Route route, AggregatedPoint p)
        {
            // set the current aggregated object.
            _current = p;

            // loop until the current object is null.
            while (_current != null)
            {
                while (_current != null)
                {
                    // plan the current message.
                    this.PlanNewMessage(route, _current);

                    // get the next object.
                    _current = _current.GetNext();
                }

                // show the latest success anyway.
                if (_latestFinal >= 0)
                { // do the latest succes.
                    this.Success(_latestMachine);

                    // get the next object.
                    if (_current != null)
                    {
                        _current = _current.GetNext();
                    }
                }
                else if (_messagesStack.Count > 0)
                { // no machine matches everything until the end of the route.
                    throw new MicroPlannerException("No machine could be found matching the current stack of messages!", _messagesStack);
                }
            }

            // return the instructions list accumulated in the scentence planner.
            return this.SentencePlanner.Instructions;
        }

        /// <summary>
        /// Creates and plans a new message.
        /// </summary>
        /// <param name="aggregated"></param>
        private void PlanNewMessage(Route route, Aggregated aggregated)
        {
            // create the message.
            MicroPlannerMessage message = null;
            if (aggregated is AggregatedPoint)
            {
                var point = new MicroPlannerMessagePoint(route);
                point.Point = aggregated as AggregatedPoint;

                message = point;
            }
            else if (aggregated is AggregatedArc)
            {
                var arc = new MicroPlannerMessageArc(route);
                arc.Arc = aggregated as AggregatedArc;

                message = arc;
            }

            // plan the message.
            this.Plan(message);
        }

        #region Machines

        /// <summary>
        /// Keeps a list of microplanners.
        /// </summary>
        private List<MicroPlannerMachine> _machines;

        /// <summary>
        /// Initializes the list of machines.
        /// </summary>
        protected virtual void InitializeMachines(List<MicroPlannerMachine> machines)
        {
            machines.Add(new TurnMachine(this));
            machines.Add(new PoiMachine(this));
            machines.Add(new PoiWithTurnMachine(this));
            machines.Add(new ImmidateTurnMachine(this));
            machines.Add(new RoundaboutMachine(this));
        }

        /// <summary>
        /// Returns the machines list.
        /// </summary>
        public List<MicroPlannerMachine> Machines
        {
            get
            {
                return _machines;
            }
        }

        #endregion

        #region Planning Queue

        /// <summary>
        /// Holds the current messages stack.
        /// </summary>
        private List<MicroPlannerMessage> _messagesStack;

        /// <summary>
        /// Holds the current list of invalid machines.
        /// </summary>
        private List<MicroPlannerMachine> _invalidMachines;

        /// <summary>
        /// Holds the current list of machines that reached a final machine.
        /// </summary>
        private List<MicroPlannerMachine> _validMachines;

        /// <summary>
        /// Holds the position of the latest final.
        /// </summary>
        private int _latestFinal;

        /// <summary>
        /// Holds the machine that finaled latest.
        /// </summary>
        private MicroPlannerMachine _latestMachine;

        /// <summary>
        /// Initializes the messages stack.
        /// </summary>
        private void InitializeMessagesStack()
        {
            this.ResetMessagesStack(true);
        }

        /// <summary>
        /// Resets the messages stack.
        /// </summary>
        private void ResetMessagesStack(bool reset_errors)
        {
            _invalidMachines = new List<MicroPlannerMachine>();
            _validMachines = new List<MicroPlannerMachine>();
            _messagesStack = new List<MicroPlannerMessage>();
            _latestFinal = -1;
            _latestMachine = null;
        }

        /// <summary>
        /// Boolean holding planning succes flag.
        /// </summary>
        private bool _succes = false;

        /// <summary>
        /// Boolean holding planning error flag.
        /// </summary>
        private bool _error = false;

        /// <summary>
        /// Plan the given message.
        /// </summary>
        /// <param name="message"></param>
        private void Plan(MicroPlannerMessage message)
        {
            _succes = false;
            _error = false;
            // add the message to the stack.
            _messagesStack.Add(message);

            // put the message through the machine.
            foreach (var machine in _machines)
            {
                if (!_invalidMachines.Contains(machine)
                    && !_validMachines.Contains(machine))
                { // only use machines that are still valid!
                    machine.Consume(message);

                    if (_succes)
                    {
                        break;
                    }

                    if (_error)
                    {
                        break;
                    }
                }
            }
            _succes = false;
            _error = false;
        }

        /// <summary>
        /// The given machine was successfull.
        /// </summary>
        /// <param name="machine"></param>
        internal void Success(MicroPlannerMachine machine)
        {
            // reset the current point/arc.
            if (_messagesStack.Count > _latestFinal + 1)
            {
                MicroPlannerMessage message = _messagesStack[_latestFinal];
                if (message is MicroPlannerMessageArc)
                {
                    _current = (message as MicroPlannerMessageArc).Arc;
                }
                if (message is MicroPlannerMessagePoint)
                {
                    _current = (message as MicroPlannerMessagePoint).Point;
                }
            }

            // reset the mesages stack.
            this.ResetMessagesStack(true);

            // tell the machine again it was successfull.
            machine.Succes();
            machine.IsSuccesfull = true;

            // re-initialize the machines.
            _machines.Clear();
            this.InitializeMachines(_machines);

            _succes = true;
        }

        /// <summary>
        /// Checks the machine for success.
        /// </summary>
        internal void CheckMachine(MicroPlannerMachine machine)
        {
            // check the other machines and their priorities.
            int priority = machine.Priority;
            foreach (var other_machine in _machines)
            {
                if (!_invalidMachines.Contains(other_machine))
                {
                    if (other_machine.Priority > priority)
                    { // not sure this machine's final state is actually the final state.
                        return;
                    }
                }
            }

            // no other machines exist with higher priority.
            this.Success(machine);
        }

        /// <summary>
        /// Reports a final state to this microplanner when some machine reaches it.
        /// </summary>
        /// <param name="machine"></param>
        /// <param name="messages"></param>
        internal void ReportFinal(MicroPlannerMachine machine, IList<MicroPlannerMessage> messages)
        {
            if (_latestFinal == _messagesStack.Count - 1)
            { // check if a machine with the same match length has higher priority.
                if (_latestMachine.Priority >= machine.Priority)
                { // the current machine has the same match length and has higher or the same priority.
                    return;
                }
            }

            // update the latest final value.
            _latestFinal = _messagesStack.Count - 1;
            _latestMachine = machine;

            // add the machine to the valid machines.
            _validMachines.Add(machine);

            // check and see if all other machines with higher priority are invalid.
            this.CheckMachine(machine);
        }

        /// <summary>
        /// Reports when a machine resets (meaning it reached an invalid state).
        /// </summary>
        /// <param name="machine"></param>
        internal void ReportReset(MicroPlannerMachine machine)
        {
            // the machine cannot be used anymore until a reset occurs.
            _invalidMachines.Add(machine);

            // check if the latest machine is now successfull.
            if (_latestMachine != null)
            {
                this.CheckMachine(_latestMachine);
            }

            // check to see if not all machine are invalid! 
            if (_invalidMachines.Count == _machines.Count)
            {
                if (_latestMachine == null)
                { // all machine went in error!
                    throw new MicroPlannerException("No machine could be found matching the current stack of messages!", _messagesStack);
                }
                else
                { // start all over with the current stack of messages.
                    this.Success(machine);
                }
            }
        }

        #endregion
    }
}