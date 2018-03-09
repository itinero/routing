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

using System.Reflection;
using NUnit.Framework;

namespace Itinero.Test
{
    /// <summary>
    /// Contains tests for the router db.
    /// </summary>
    [TestFixture]
    public class RouterDbDeserializationTests
    {
        /// <summary>
        /// Tests deserializing a db from 1.4.0 v8.
        /// </summary>
        [Test]
        public void Test_1_4_0_v8()
        {
            using (var stream = Assembly.GetExecutingAssembly().GetManifestResourceStream(
                    "Itinero.Test.test_data.routerdbs.db.1.4.0.v8.routerdb"))
            {
                var routerDb = RouterDb.Deserialize(stream);
            }
        }
    }
}