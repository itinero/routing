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

using OsmSharp.Routing.Instructions.ArcAggregation;
using OsmSharp.Routing.Instructions.ArcAggregation.Output;
using OsmSharp.Routing.Instructions.LanguageGeneration;
using OsmSharp.Routing.Instructions.MicroPlanning;
using OsmSharp.Routing.Interpreter;
using System;
using System.Collections.Generic;

namespace OsmSharp.Routing.Instructions
{
    /// <summary>
    /// Instruction generator.
    /// </summary>
    public static class InstructionGenerator
    {
        /// <summary>
        /// Generates instructions.
        /// </summary>
        /// <param name="route"></param>
        /// <param name="interpreter"></param>
        /// <returns></returns>
        public static List<Instruction> Generate(Route route, IRoutingInterpreter interpreter)
        {
            return InstructionGenerator.Generate(route, interpreter,
                new OsmSharp.Routing.Instructions.LanguageGeneration.Defaults.EnglishLanguageGenerator());
        }

        /// <summary>
        /// Generates instructions.
        /// </summary>
        /// <param name="route"></param>
        /// <param name="interpreter"></param>
        /// <param name="languageGenerator"></param>
        /// <returns></returns>
        public static List<Instruction> Generate(Route route, IRoutingInterpreter interpreter, ILanguageGenerator languageGenerator)
        {
            if (route == null) { throw new ArgumentNullException("route"); }
            if (route.Vehicle == null) { throw new InvalidOperationException("Vehicle not set on route: Cannot generate instruction for a route without a vehicle!"); }
            if (interpreter == null) { throw new ArgumentNullException("interpreter"); }
            if (languageGenerator == null) { throw new ArgumentNullException("languageGenerator"); }

            var aggregator = new ArcAggregator(interpreter);
            var point = aggregator.Aggregate(route);

			return InstructionGenerator.Generate(route, point, interpreter, languageGenerator);
        }

        /// <summary>
        /// Generates instructions.
        /// </summary>
        /// <param name="route"></param>
        /// <param name="point"></param>
        /// <param name="interpreter"></param>
        /// <returns></returns>
        public static List<Instruction> Generate(Route route, AggregatedPoint point, IRoutingInterpreter interpreter)
        {
			return InstructionGenerator.Generate(route, point, interpreter,
                new OsmSharp.Routing.Instructions.LanguageGeneration.Defaults.EnglishLanguageGenerator());
        }

        /// <summary>
        /// Generates instructions.
        /// </summary>
        /// <param name="route"></param>
        /// <param name="point"></param>
        /// <param name="interpreter"></param>
        /// <param name="languageGenerator"></param>
        /// <returns></returns>
        public static List<Instruction> Generate(Route route, AggregatedPoint point, IRoutingInterpreter interpreter, ILanguageGenerator languageGenerator)
        {
            if (point == null) { throw new ArgumentNullException("route"); }
            if (interpreter == null) { throw new ArgumentNullException("interpreter"); }
            if (languageGenerator == null) { throw new ArgumentNullException("languageGenerator"); }

            return InstructionGenerator.Generate(new MicroPlanner(languageGenerator, interpreter), route, point);
        }

        /// <summary>
        /// Generates instructions.
        /// </summary>
        /// <param name="planner"></param>
        /// <param name="route"></param>
        /// <returns></returns>
        public static List<Instruction> Generate(MicroPlanner planner, Route route)
        {
            if (route == null) { throw new ArgumentNullException("route"); }

            var aggregator = new ArcAggregator(planner.Interpreter);
            var point = aggregator.Aggregate(route);

            if(point == null)
            { // returns an empty list because of an empty route.
                return new List<Instruction>();
            }
            return InstructionGenerator.Generate(planner, route, point);
        }

        /// <summary>
        /// Generates instructions.
        /// </summary>
        /// <param name="planner"></param>
        /// <param name="route"></param>
        /// <param name="point"></param>
        /// <returns></returns>
        public static List<Instruction> Generate(MicroPlanner planner, Route route, AggregatedPoint point)
        {
            if (point == null) { throw new ArgumentNullException("route"); }
            if (planner == null) { throw new ArgumentNullException("planner"); }

            return planner.Plan(route, point);
        }

        /// <summary>
        /// Creates a new microplanner.
        /// </summary>
        /// <param name="languageGenerator"></param>
        /// <param name="interpreter"></param>
        /// <returns></returns>
        public static MicroPlanner CreatePlanner(ILanguageGenerator languageGenerator, IRoutingInterpreter interpreter)
        {
            return new MicroPlanner(languageGenerator, interpreter);
        }
    }
}