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
using OsmSharp.Collections.Tags;
using OsmSharp.Math.Geo.Meta;
using OsmSharp.Routing.Instructions.ArcAggregation.Output;
using OsmSharp.Routing.Instructions;
using OsmSharp.Routing.Instructions.LanguageGeneration;

namespace OsmSharp.Test.Unittests.Routing.Instructions
{
    /// <summary>
    /// Language test generator.
    /// </summary>
    public class LanguageTestGenerator : LanguageGenerator
    {
        /// <summary>
        /// Direction turn instruction.
        /// </summary>
        /// <param name="countBefore"></param>
        /// <param name="street"></param>
        /// <param name="direction"></param>
        /// <param name="pois"></param>
        /// <returns></returns>
        protected override string GenerateDirectTurn(int countBefore, TagsCollectionBase street, RelativeDirection direction, List<PointPoi> pois)
        {
            return string.Format("GenerateDirectTurn:{0}_{1}_{2}", countBefore, direction.Direction.ToString(), pois.Count);
        }

        /// <summary>
        /// Generates an indirect turn.
        /// </summary>
        /// <param name="countBefore"></param>
        /// <param name="street"></param>
        /// <param name="direction"></param>
        /// <param name="pois"></param>
        /// <returns></returns>
        protected override string GenerateIndirectTurn(int countBefore, TagsCollectionBase street, RelativeDirection direction, List<PointPoi> pois)
        {
            return string.Format("GenerateIndirectTurn:{0}_{1}_{2}", countBefore, direction.Direction.ToString(), pois.Count);
        }

        /// <summary>
        /// Generates POI instruction.
        /// </summary>
        /// <param name="direction"></param>
        /// <param name="pois"></param>
        /// <returns></returns>
        protected override string GeneratePOI(RelativeDirection direction, List<PointPoi> pois)
        {
            if (direction != null)
            {
                return string.Format("GeneratePoi:{0}_{1}", pois.Count, direction.Direction.ToString());
            }
            else
            {
                return string.Format("GeneratePoi:{0}", pois.Count);
            }
        }

        /// <summary>
        /// Generates a direct follow turn.
        /// </summary>
        /// <param name="countBefore"></param>
        /// <param name="street"></param>
        /// <param name="direction"></param>
        /// <param name="pois"></param>
        /// <returns></returns>
        protected override string GenerateDirectFollowTurn(int countBefore, TagsCollectionBase street, RelativeDirection direction, List<PointPoi> pois)
        {
            return string.Format("GenerateDirectFollowTurn:{0}_{1}_{2}", countBefore, direction.Direction.ToString(), pois.Count);
        }

        /// <summary>
        /// Generates an indirect follow turn.
        /// </summary>
        /// <param name="countBefore"></param>
        /// <param name="street"></param>
        /// <param name="direction"></param>
        /// <param name="list"></param>
        /// <returns></returns>
        protected override string GenerateIndirectFollowTurn(int countBefore, TagsCollectionBase street, RelativeDirection direction, List<PointPoi> list)
        {
            return string.Format("GenerateDirectFollowTurn:{0}_{1}_{2}",
                                             countBefore, direction.Direction.ToString(), list.Count);
        }

        /// <summary>
        /// Generates an immidiate turn.
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
            return string.Format("GenerateImmidiateTurn:{0}_{1}_{2}_{3}", countBefore, firstDirection, firstDirection.Direction.ToString(), secondDirection.Direction.ToString());
        }

        /// <summary>
        /// Generates a roundabout instruction.
        /// </summary>
        /// <param name="count"></param>
        /// <param name="street"></param>
        /// <returns></returns>
        protected override string GenerateRoundabout(int count, TagsCollectionBase street)
        {
            return string.Format("GenerateRoundabout:{0}", count);
        }

        /// <summary>
        /// Generates a simple turn instruction.
        /// </summary>
        /// <param name="direction"></param>
        /// <returns></returns>
        protected override string GenerateTurn(RelativeDirection direction)
        {
            return string.Format("GenerateSimpleTurn:{0}", direction.ToString());
        }
    }
}