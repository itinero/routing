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
using OsmSharp.Math.Geo.Meta;
using OsmSharp.Routing.Instructions.ArcAggregation.Output;
using System.Collections.Generic;

namespace OsmSharp.Routing.Instructions.LanguageGeneration.Defaults
{
    /// <summary>
    /// A simple instruction generator, translating instructions into the english language.
    /// </summary>
    public class EnglishLanguageGenerator : LanguageGenerator
    {
        /// <summary>
        /// Generates a word for the the given direction.
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        private string TurnDirection(RelativeDirection direction)
        {
            if (direction != null)
            {
                switch (direction.Direction)
                {
                    case RelativeDirectionEnum.Right:
                    case RelativeDirectionEnum.SharpRight:
                    case RelativeDirectionEnum.SlightlyRight:
                        return "right";
                    case RelativeDirectionEnum.Left:
                    case RelativeDirectionEnum.SharpLeft:
                    case RelativeDirectionEnum.SlightlyLeft:
                        return "left";
                    case RelativeDirectionEnum.TurnBack:
                        return "back";
                }
            }
            return string.Empty;
        }

        /// <summary>
        /// Generates an instruction for a direct turn.
        /// </summary>
        /// <param name="countBefore"></param>
        /// <param name="street"></param>
        /// <param name="direction"></param>
        /// <param name="pois"></param>
        /// <returns></returns>
        protected override string GenerateDirectTurn(int countBefore, TagsCollectionBase street, RelativeDirection direction, List<PointPoi> pois)
        {
            countBefore++;
            if (countBefore == 1)
            {
                return string.Format("Take the first turn {0}, on {1}.", TurnDirection(direction), this.GetName("en", street));
            }
            else
            {
                return string.Format("Take the {0}th turn {1}, on {2}.", countBefore, TurnDirection(direction), this.GetName("en", street));
            }
        }

        /// <summary>
        /// Generates an instruction for a turn followed by another turn.
        /// </summary>
        /// <param name="countBefore"></param>
        /// <param name="street"></param>
        /// <param name="direction"></param>
        /// <param name="pois"></param>
        /// <returns></returns>
        protected override string GenerateDirectFollowTurn(int countBefore, TagsCollectionBase street, RelativeDirection direction, List<PointPoi> pois)
        {
            return string.Format("Take the {0}d turn {1}, on {2}.", countBefore, TurnDirection(direction), this.GetName("en",street));
        }

        /// <summary>
        /// Generates an instruction for an indirect turn.
        /// </summary>
        /// <param name="countBefore"></param>
        /// <param name="street"></param>
        /// <param name="direction"></param>
        /// <param name="pois"></param>
        /// <returns></returns>
        protected override string GenerateIndirectTurn(int countBefore, TagsCollectionBase street, RelativeDirection direction, List<PointPoi> pois)
        {
            return string.Format("Take the {0}d turn {1}, on {2}.", countBefore, TurnDirection(direction), this.GetName("en", street));
        }

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
        protected override string GenerateIndirectFollowTurn(int countBefore, TagsCollectionBase street, RelativeDirection direction, List<PointPoi> list)
        {
            countBefore++;
            if (countBefore == 1)
            {
                return string.Format("Turn {1} to stay on {0}.", this.GetName("en", street), TurnDirection(direction));
            }
            else
            {
                return string.Format("Take the {1}d street {2} to stay on {0}.", this.GetName("en", street), countBefore, TurnDirection(direction));
            }
        }

        /// <summary>
        /// Generates an instruction for a POI.
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="pois"></param>
        /// <returns></returns>
        protected override string GeneratePOI(RelativeDirection direction, List<PointPoi> pois)
        {
            if (direction == null)
            {
                return string.Format("Poi");
            }
            else
            {
                return string.Format("Poi:{0}", direction.Direction);
            }
        }

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
        protected override string GenerateImmidiateTurn(int countBefore, TagsCollectionBase firstStreet, RelativeDirection firstDirection, TagsCollectionBase secondStreet, RelativeDirection secondDirection, List<PointPoi> pois)
        {
            countBefore++;
            if (countBefore == 1)
            {
                return string.Format("Take the first turn {0}, on the {1}, and turn immidiately {2} on the {3}.",
                    TurnDirection(firstDirection),
                    this.GetName("en", firstStreet),
                    TurnDirection(secondDirection),
                    this.GetName("en", secondStreet));
            }
            else
            {
                return string.Format("Take the {4}d turn {0}, on the {1}, and turn immidiately {2} on the {3}.",
                    TurnDirection(firstDirection),
                    this.GetName("en", firstStreet),
                    TurnDirection(secondDirection),
                    this.GetName("en", secondStreet),
                    countBefore);
            }
        }

        /// <summary>
        /// Generates an instruction for a roundabout.
        /// </summary>
        /// <param name="count"></param>
        /// <param name="street"></param>
        /// <returns></returns>
        protected override string GenerateRoundabout(int count, TagsCollectionBase street)
        {
            return string.Format("Take the {0}d at the next roundabout on the {1}.", count, this.GetName("en", street));
        }

        /// <summary>
        /// Generates an instruction for a simple turn.
        /// </summary>
        /// <param name="count"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        protected override string GenerateTurn(RelativeDirection direction)
        {
            return string.Format("Turn {0}", this.TurnDirection(direction));
        }
    }
}
