// Itinero - Routing for .NET
// Copyright (C) 2016 Abelshausen Ben
// 
// This file is part of Itinero.
// 
// Itinero is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// Itinero is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Itinero. If not, see <http://www.gnu.org/licenses/>.

using OsmSharp;

namespace Itinero.IO.Osm.Streams
{
    /// <summary>
    /// Abstract representation of two-pass based osm-data processor.
    /// </summary>
    public interface ITwoPassProcessor
    {
        /// <summary>
        /// Processes the first pass of this node.
        /// </summary>
        void FirstPass(Node node);

        /// <summary>
        /// Processes the first pass of this way.
        /// </summary>
        void FirstPass(Way way);

        /// <summary>
        /// Processes the first pass of this relation.
        /// </summary>
        void FirstPass(Relation relation);

        /// <summary>
        /// Processes a node in the second pass.
        /// </summary>
        void SecondPass(Node node);

        /// <summary>
        /// Processes a way in the second pass.
        /// </summary>
        void SecondPass(Way way);

        /// <summary>
        /// Processes a relation in a second pass.
        /// </summary>
        void SecondPass(Relation relation);
    }
}