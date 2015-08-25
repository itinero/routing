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

using OsmSharp.Collections.Tags;
using OsmSharp.Math.Geo;
using OsmSharp.Math.Geo.Meta;
using OsmSharp.Routing.Instructions.ArcAggregation.Output;
using System.Collections.Generic;

namespace OsmSharp.Routing.Instructions.LanguageGeneration
{
    /// <summary>
    /// Scentence planner for routing instructions.
    /// </summary>
    public class SentencePlanner
    {
        /// <summary>
        /// Holds the list of planned instructions.
        /// </summary>
        private List<Instruction> _instructions;

        /// <summary>
        /// Holds the language-specific generator.
        /// </summary>
        private ILanguageGenerator _generator;

        /// <summary>
        /// Creates a new scentence planner.
        /// </summary>
        /// <param name="generator"></param>
        public SentencePlanner(ILanguageGenerator generator)
        {
            _generator = generator;
            _instructions = new List<Instruction>();
        }

        /// <summary>
        /// Not sure this should be here?
        /// </summary>
        /// <returns></returns>
        public List<Instruction> Instructions
        {
            get
            {
                return _instructions;
            }
        }

        /// <summary>
        /// Generates an instruction from the given meta data and given pois.
        /// </summary>
        /// <param name="metaData"></param>
        /// <param name="firstSegmentIdx"></param>
        /// <param name="lastSegmentIdx"></param>
        /// <param name="box"></param>
        /// <param name="pois"></param>
        public void GenerateInstruction(Dictionary<string, object> metaData, int firstSegmentIdx, int lastSegmentIdx, GeoCoordinateBox box, List<PointPoi> pois)
        {
            string text;
            if (_generator.Generate(metaData, out text))
            { // add the instruction to the instructions list.
                _instructions.Add(new Instruction(metaData, firstSegmentIdx, lastSegmentIdx, box, text, pois));
            }
        }
    }
}
