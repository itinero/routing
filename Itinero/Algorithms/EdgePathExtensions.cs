// Itinero - OpenStreetMap (OSM) SDK
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

namespace Itinero.Algorithms
{
    /// <summary>
    /// Contains extension methods from the edge path.
    /// </summary>
    public static class EdgePathExtensions
    {

        /// <summary>
        /// Returns the equivalent path.
        /// </summary>
        public static Path ToPath(this EdgePath edgePath)
        { // TODO: improved this to a non-recursive version.
            if (edgePath.From == null)
            {
                return new Path(edgePath.Vertex);
            }
            return new Path(edgePath.Vertex, edgePath.Weight, edgePath.From.ToPath());
        }

        /// <summary>
        /// Returns the equivalent edge-path for the path.
        /// </summary>
        public static EdgePath ToEdgePath(this Path path)
        { // TODO: improved this to a non-recursive version.
            if (path.From == null)
            {
                return new EdgePath(path.Vertex);
            }
            return new EdgePath(path.Vertex, path.Weight, path.From.ToEdgePath());
        }
    }
}