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

namespace OsmSharp.Routing.Instructions.LanguageGeneration.Defaults
{
    /// <summary>
    /// A simple instruction generator, translating instructions into the dutch language.
    /// </summary>
    public class GermanLanguageGenerator : LanguageGenerator
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
                        return "rechts";
                    case RelativeDirectionEnum.Left:
                    case RelativeDirectionEnum.SharpLeft:
                    case RelativeDirectionEnum.SlightlyLeft:
                        return "links";
                    case RelativeDirectionEnum.TurnBack:
                        return "zurÃ¼ck";
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
                return string.Format("Nimm die erste Abzweigung {0}, auf die {1}", TurnDirection(direction), this.GetName("de", street));
            }
            else
            {
                return string.Format("Nimm die {0}te Abzweigung {1}, auf die {2}.", countBefore, TurnDirection(direction), this.GetName("de", street));
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
            return string.Format("Fahre {1}.", countBefore, TurnDirection(direction), this.GetName("de", street));
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
            return string.Format("Fahre {1}.", countBefore, TurnDirection(direction), this.GetName("de", street));
        }

        /// <summary>
        /// Generates an instruction for an indirect turn.
        /// </summary>
        /// <param name="countBefore"></param>
        /// <param name="street"></param>
        /// <param name="direction"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        protected override string GenerateIndirectFollowTurn(int countBefore, TagsCollectionBase street, RelativeDirection direction, List<PointPoi> list)
        {
            countBefore++;
            if (countBefore == 1)
            {
                return string.Format("Fahre {1} und bleibe auf der {0}.", this.GetName("de", street), TurnDirection(direction));
            }
            else
            {
                return string.Format("Nimm {1}te StraÃŸe {2} und bleib auf der {0}.", this.GetName("de", street), countBefore, TurnDirection(direction));
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
                return string.Format("Nimm die erste Abzweigung {0}, auf die {1}, und fahre sofort {2} auf die {3}.",
                    TurnDirection(firstDirection),
                    this.GetName("de", firstStreet),
                    TurnDirection(secondDirection),
                    this.GetName("de", secondStreet));
            }
            else
            {
                return string.Format("Nimm die {4}te Abzweigung {0}, auf die {1}, und fahre sofort {2} auf die {3}.",
                    TurnDirection(firstDirection),
                    this.GetName("de", firstStreet),
                    TurnDirection(secondDirection),
                    this.GetName("de", secondStreet),
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
            return string.Format("Nimm die {0}te Ausfahrt auf dem folgenden Kreisverkehr", count, this.GetName("de", street));
        }

        /// <summary>
        /// Generates an instruction for a simple turn.
        /// </summary>
        /// <param name="count"></param>
        /// <param name="direction"></param>
        /// <returns></returns>
        protected override string GenerateTurn(RelativeDirection direction)
        {
            return string.Format("Fahre {0}", this.TurnDirection(direction));
        }
    }
}