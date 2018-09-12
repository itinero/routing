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

using Itinero.Algorithms;
using NetTopologySuite.IO;
using System.Collections.Generic;
using System.Threading;
using NetTopologySuite.Geometries;
using Itinero.Profiles;

namespace Itinero.IO.Shape.Writer
{
    /// <summary>
    /// A writer that writes shapefile(s) and builds a routing network.
    /// </summary>
    public class ShapeFileWriter : AlgorithmBase
    {
        private readonly RouterDb _routerDb;
        private readonly IEnumerable<Profile> _profiles;
        private readonly string _fileName;

        /// <summary>
        /// Creates a new reader.
        /// </summary>
        public ShapeFileWriter(RouterDb routerDb, IEnumerable<Profile> profiles, string fileName)
        {
            _routerDb = routerDb;
            _profiles = profiles;
            _fileName = fileName;
        }

        /// <summary>
        /// Executes the actual algorithm.
        /// </summary>
        protected override void DoRun(CancellationToken cancellationToken)
        {            
            // assumed here all arguments are as they should be.
            var features = new FeaturesList(_routerDb, _profiles);

            var header = ShapefileDataWriter.GetHeader(features[0], features.Count);
            var shapeWriter = new ShapefileDataWriter(_fileName, new GeometryFactory())
            {
                Header = header
            };
            shapeWriter.Write(features);

            this.HasSucceeded = true;
        }
    }
}