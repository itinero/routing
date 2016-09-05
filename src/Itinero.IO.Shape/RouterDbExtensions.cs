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

using Itinero.IO.Shape.Vehicles;
using NetTopologySuite;
using NetTopologySuite.IO;
using System.Collections.Generic;
using System.IO;

namespace Itinero.IO.Shape
{
    /// <summary>
    /// Holds extensions to the router db.
    /// </summary>
    public static class RouterDbExtensions
    {
        /// <summary>
        /// Loads routing data from the given shapefiles for the given vehicles.
        /// </summary>
        public static void LoadFromShape(this RouterDb routerDb, string path, string searchPattern, string sourceVertexColumn, string targetVertexColumn,
            params Vehicle[] vehicles)
        {
            // build a list of nw-files.
            var directoryInfo = new DirectoryInfo(path);
            var networkFiles = directoryInfo.EnumerateFiles(searchPattern, SearchOption.AllDirectories);

            // create all readers.
            var readers = new List<ShapefileDataReader>();
            var geometryFactory = NtsGeometryServices.Instance.CreateGeometryFactory();
            foreach (var networkFile in networkFiles)
            {
                readers.Add(new ShapefileDataReader(networkFile.FullName, geometryFactory));
            }

            routerDb.LoadFromShape(readers, sourceVertexColumn, targetVertexColumn, vehicles);
        }

        /// <summary>
        /// Loads routing data from the given shapefiles for the given vehicles.
        /// </summary>
        public static void LoadFromShape(this RouterDb routerDb, IEnumerable<ShapefileDataReader> shapefileReaders, string sourceVertexColumn, string targetVertexColumn, 
            params Vehicle[] vehicles)
        {
            var reader = new IO.Shape.Reader.ShapefileReader(routerDb, new List<ShapefileDataReader>(shapefileReaders), vehicles, sourceVertexColumn, targetVertexColumn);
            reader.Run();
        }
    }
}