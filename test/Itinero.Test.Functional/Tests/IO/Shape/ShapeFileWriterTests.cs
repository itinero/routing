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

using System;
using Itinero.Profiles;
using Itinero.IO.Shape;

namespace Itinero.Test.Functional.Tests.IO.Shape
{
    /// <summary>
    /// Contains functional tests writing shapefile output.
    /// </summary>
    public static class ShapeFileWriterTests
    {
        /// <summary>
        /// Runs resolving tests on the given routerDb.
        /// </summary>
        public static void Run(RouterDb routerDb)
        {
            var profiles = new Profile[] {
                routerDb.GetSupportedProfile("car"),
                routerDb.GetSupportedProfile("bicycle"),
                routerDb.GetSupportedProfile("pedestrian")
            };

            GetTestWriteShapeFile(routerDb, profiles).TestPerf("Writing shapefile.");
        }

        /// <summary>
        /// Tests writing a shapefile.
        /// </summary>
        public static Action GetTestWriteShapeFile(RouterDb routerDb, params Profile[] profiles)
        {
            return () =>
            {
                routerDb.WriteToShape("shapefile", profiles);
            };
        }
    }
}