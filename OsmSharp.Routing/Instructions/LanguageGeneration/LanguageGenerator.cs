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

using OsmSharp.Collections.Tags;
using OsmSharp.Math.Geo.Meta;
using OsmSharp.Routing.Instructions.ArcAggregation.Output;
using System.Collections.Generic;

namespace OsmSharp.Routing.Instructions.LanguageGeneration
{
    /// <summary>
    /// Baseclass containing a basic implementation to start a language generator.
    /// </summary>
    public abstract class LanguageGenerator : ILanguageGenerator
    {
        /// <summary>
        /// Generates a text-instruction based on the given data.
        /// </summary>
        /// <param name="instructionData"></param>
        /// <param name="text"></param>
        /// <returns>Returns true if an instruction was generated, false otherwise.</returns>
        public virtual bool Generate(Dictionary<string, object> instructionData, out string text)
        {
            object typeObject;
            if(instructionData.TryGetValue("type", out typeObject))
            { // there is a type field.
                var type = (typeObject as string);
                int countBefore, count;
                TagsCollectionBase street, firstStreet, secondStreet;
                RelativeDirection direction;
                RelativeDirection firstDirection, secondDirection;
                List<PointPoi> pois;

                switch(type)
                {
                    case "direct_turn":
                        if(instructionData.TryGetValue<int>("count_before", out countBefore) &&
                            instructionData.TryGetValue<TagsCollectionBase>("street", out street) &&
                            instructionData.TryGetValue<RelativeDirection>("direction", out direction) &&
                            instructionData.TryGetValue<List<PointPoi>>("pois", out pois))
                        {
                            text = this.GenerateDirectTurn(countBefore, street, direction, pois);
                            return true;
                        }
                        break;
                    case "indirect_turn":
                        if (instructionData.TryGetValue<int>("count_before", out countBefore) &&
                            instructionData.TryGetValue<TagsCollectionBase>("street", out street) &&
                            instructionData.TryGetValue<RelativeDirection>("direction", out direction) &&
                            instructionData.TryGetValue<List<PointPoi>>("pois", out pois))
                        {
                            text = this.GenerateIndirectTurn(countBefore, street, direction, pois);
                            return true;
                        }
                        break;
                    case "poi":
                        if(instructionData.TryGetValue<RelativeDirection>("direction", out direction) &&
                            instructionData.TryGetValue<List<PointPoi>>("pois", out pois))
                        {
                            text = this.GeneratePOI(direction, pois);
                            return true;
                        }
                        break;
                    case "direct_follow_turn":
                        if (instructionData.TryGetValue<int>("count_before", out countBefore) &&
                            instructionData.TryGetValue<TagsCollectionBase>("street", out street) &&
                            instructionData.TryGetValue<RelativeDirection>("direction", out direction) &&
                            instructionData.TryGetValue<List<PointPoi>>("pois", out pois))
                        {
                            text = this.GenerateDirectFollowTurn(countBefore, street, direction, pois);
                            return true;
                        }
                        break;
                    case "indirection_follow_turn":
                        if (instructionData.TryGetValue<int>("count_before", out countBefore) &&
                            instructionData.TryGetValue<TagsCollectionBase>("street", out street) &&
                            instructionData.TryGetValue<RelativeDirection>("direction", out direction) &&
                            instructionData.TryGetValue<List<PointPoi>>("pois", out pois))
                        {
                            text = this.GenerateIndirectFollowTurn(countBefore, street, direction, pois);
                            return true;
                        }
                        break;
                    case "immidiate_turn":
                        if (instructionData.TryGetValue<int>("count_before", out countBefore) &&
                            instructionData.TryGetValue<TagsCollectionBase>("first_street", out firstStreet) &&
                            instructionData.TryGetValue<RelativeDirection>("first_direction", out firstDirection) &&
                            instructionData.TryGetValue<TagsCollectionBase>("second_street", out secondStreet) &&
                            instructionData.TryGetValue<RelativeDirection>("second_direction", out secondDirection) &&
                            instructionData.TryGetValue<List<PointPoi>>("pois", out pois))
                        {
                            text = this.GenerateImmidiateTurn(countBefore, firstStreet, firstDirection, secondStreet, secondDirection, pois);
                            return true;
                        }
                        break;
                    case "roundabout":
                        if (instructionData.TryGetValue<int>("count", out count) &&
                            instructionData.TryGetValue<TagsCollectionBase>("street", out street))
                        {
                            text = this.GenerateRoundabout(count, street);
                            return true;
                        }
                        break;
                    case "turn":
                        if(instructionData.TryGetValue<RelativeDirection>("direction", out direction))
                        {
                            text = this.GenerateTurn(direction);
                            return true;
                        }
                        break;
                }
            }
            text = null;
            return false;
        }

        /// <summary>
        /// Returns the name for the given language key from the given tags collection.
        /// </summary>
        /// <param name="languageKey"></param>
        /// <param name="tags"></param>
        /// <returns></returns>
        protected virtual string GetName(string languageKey, TagsCollectionBase tags)
        {
            languageKey = languageKey.ToLower();

            string name = string.Empty;
            foreach (Tag tag in tags)
            {
                if (tag.Key != null && tag.Key.ToLower() == string.Format("name:{0}", languageKey))
                {
                    return tag.Value;
                }
                if (tag.Key != null && tag.Key.ToLower() == "name")
                {
                    name = tag.Value;
                }
            }
            return name;
        }

        #region ILanguageGenerator Members

        /// <summary>
        /// Generates an instruction for a direct turn.
        /// </summary>
        /// <param name="countBefore"></param>
        /// <param name="street"></param>
        /// <param name="direction"></param>
        /// <param name="pois"></param>
        /// <returns></returns>
        protected abstract string GenerateDirectTurn(int countBefore, TagsCollectionBase street, RelativeDirection direction, List<PointPoi> pois);

        /// <summary>
        /// Generates an instruction for a turn followed by another turn.
        /// </summary>
        /// <param name="countBefore"></param>
        /// <param name="street"></param>
        /// <param name="direction"></param>
        /// <param name="pois"></param>
        /// <returns></returns>
        protected abstract string GenerateDirectFollowTurn(int countBefore, TagsCollectionBase street, RelativeDirection direction, List<PointPoi> pois);

        /// <summary>
        /// Generates an instruction for an indirect turn.
        /// </summary>
        /// <param name="countBefore"></param>
        /// <param name="street"></param>
        /// <param name="direction"></param>
        /// <param name="pois"></param>
        /// <returns></returns>
        protected abstract string GenerateIndirectTurn(int countBefore, TagsCollectionBase street, RelativeDirection direction, List<PointPoi> pois);

        /// <summary>
        /// Generates an instruction for an indirect turn.
        /// </summary>
        /// <param name="instruction"></param>
        /// <param name="streetCountTurn"></param>
        /// <param name="streetCountBeforeTurn"></param>
        /// <param name="street_to"></param>
        /// <param name="direction"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        protected abstract string GenerateIndirectFollowTurn(int countBefore, TagsCollectionBase street, RelativeDirection direction, List<PointPoi> list);

        /// <summary>
        /// Generates an instruction for a POI.
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="pois"></param>
        /// <returns></returns>
        protected abstract string GeneratePOI(RelativeDirection direction, List<PointPoi> pois);

        /// <summary>
        /// Generates an instruction for an immidiate turn.
        /// </summary>
        /// <param name="countBefore"></param>
        /// <param name="firstStreet"></param>
        /// <param name="firstDirection"></param>
        /// <param name="secondStreet"></param>
        /// <param name="secondDirection"></param>
        /// <param name="pois"></param>
        /// <returns></returns>
        protected abstract string GenerateImmidiateTurn(int countBefore, TagsCollectionBase firstStreet, RelativeDirection firstDirection, TagsCollectionBase secondStreet, RelativeDirection secondDirection, List<PointPoi> pois);

        /// <summary>
        /// Generates an instruction for a roundabout.
        /// </summary>
        /// <param name="count"></param>
        /// <param name="street"></param>
        /// <returns></returns>
        protected abstract string GenerateRoundabout(int count, TagsCollectionBase street);

        /// <summary>
        /// Generates an instruction for a simple turn.
        /// </summary>
        /// <param name="count"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        protected abstract string GenerateTurn(RelativeDirection direction);

        #endregion
    }
}