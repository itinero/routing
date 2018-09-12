/*
 *  Licensed to SharpSoftware under one or more contributor
 *  license agreements. See the NOTICE file distributed with this work for 
 *  additional information regarding copyright ownership.
 * 
 *  SharpSoftware licenses this file to you under the Apache License, 
 *  Version 2.0 (the "License"); you may not use this file except in 
 *  compliance with the License. You may obtain a copy of the License at
 * 
 *       http://www.apache.org/licenses/LICENSE-2.0
 * 
 *  Unless required by applicable law or agreed to in writing, software
 *  distributed under the License is distributed on an "AS IS" BASIS,
 *  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *  See the License for the specific language governing permissions and
 *  limitations under the License.
 */

using Itinero.Profiles;
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
        public static void LoadFromShape(this RouterDb routerDb, string path, string searchPattern, params Vehicle[] vehicles)
        {
            routerDb.LoadFromShape(path, searchPattern, null, null, vehicles);
        }

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
        public static void LoadFromShape(this RouterDb routerDb, IEnumerable<ShapefileDataReader> shapefileReaders, params Vehicle[] vehicles)
        {
            routerDb.LoadFromShape(shapefileReaders, null, null, vehicles);
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

        /// <summary>
        /// Writes the routerdb to a shapefile.
        /// </summary>
        public static void WriteToShape(this RouterDb routerDb, string fileName, params Profile[] profiles)
        {
            var writer = new Writer.ShapeFileWriter(routerDb, profiles, fileName);
            writer.Run();
        }
    }
}