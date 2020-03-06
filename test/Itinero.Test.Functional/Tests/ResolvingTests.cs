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
using Itinero.LocalGeo;
using System;
using System.Collections.Generic;
using System.Linq;
using Itinero.Algorithms.Search;
using Itinero.Algorithms.Search.Cache;

namespace Itinero.Test.Functional.Tests
{
    /// <summary>
    /// Contains resolving tests.
    /// </summary>
    public static class ResolvingTests
    {
        /// <summary>
        /// Runs resolving tests on the given routerDb.
        /// </summary>
        public static void Run(RouterDb routerDb)
        {
            var router = new Router(routerDb);

            // resolving with an invalid coordinate should fail gracefully and result with IsError == true
            Assert.IsTrue(router.TryResolve(Itinero.Osm.Vehicles.Vehicle.Car.Fastest(), 51.660199503280325f, 5291824042797089f).IsError);

            //resolving with a valid coordinate should result with IsError == false
            Assert.IsTrue(router.TryResolve(Itinero.Osm.Vehicles.Vehicle.Locomotive.Fastest(), 49.968567f, 5.935257f).IsError == false);

            //Test each supported profile
            foreach(var profile in router.Db.GetSupportedProfiles())
            {
                GetTestRandomResolves(router, profile, 1000).TestPerf("Random resolves");
                GetTestRandomResolvesParallel(router, profile, 1000).TestPerf("Random resolves in parallel");
                GetTestRandomResolvesCached(router, profile, 1000, 100).TestPerf("Random resolves");
                GetTestRandomResolvesParallelCached(router, profile, 1000, 100).TestPerf("Random resolves in parallel");
                GetTestRandomResolvesConnectedParallel(router, profile, 1000).TestPerf("Random connected resolve in parallel");
                GetMultipleResolve(router, profile).TestPerf("Multiple resolve test.");
            }
        }

        public static Action GetMultipleResolve(Router router, Profiles.Profile profile)
        {
            float Offset(Coordinate centerLocation, Itinero.Data.Network.RoutingNetwork routingNetwork)
            {
                var offsetLocation = centerLocation.OffsetWithDistances(routingNetwork.MaxEdgeDistance / 2);
                var latitudeOffset = System.Math.Abs(centerLocation.Latitude - offsetLocation.Latitude);
                var longitudeOffset = System.Math.Abs(centerLocation.Longitude - offsetLocation.Longitude);

                return System.Math.Max(latitudeOffset, longitudeOffset);
            }
            
            var isAcceptable = router.GetIsAcceptable(router.Db.GetSupportedProfile("bicycle"));
            var multiResolve = new ResolveMultipleAlgorithm(router.Db.Network.GeometricGraph, 49.60471690708019f,
                6.116970777511597f, Offset(new Coordinate(49.60471690708019f,
                    6.116970777511597f), router.Db.Network),
                200, isAcceptable, true);
            return () => multiResolve.Run();
        }

        /// <summary>
        /// Tests a number of resolves.
        /// </summary>
        public static Func<string> GetTestRandomResolves(Router router, Profiles.Profile profile, int count)
        {
            var random = new System.Random();
            var vertices = new List<Coordinate>();
            
            // make sure all locations are near to accesible roads.
            while (vertices.Count < count)
            {
                var v1 = (uint)random.Next((int)router.Db.Network.VertexCount);
                var f1 = router.Db.Network.GetVertex(v1);
                
                var factor = router.GetDefaultGetFactor(profile);
                var edgeEnumerator = router.Db.Network.GetEdgeEnumerator();
                edgeEnumerator.MoveTo(v1);
                while (edgeEnumerator.MoveNext())
                {
                    var f = factor(edgeEnumerator.Current.Data.Profile);
                    if (f.Value != 0)
                    {
                        vertices.Add(router.Db.Network.GetVertex(v1));
                        break;
                    }
                }
            }

            // make sure the router is fresh.
            router = new Router(router.Db);
            
            // resolve all.
            return () =>
            {
                var errors = 0;
                for(var i = 0; i < vertices.Count; i++)
                {
                    var routerPoint = router.TryResolve(profile, vertices[i]);
                    if (routerPoint.IsError)
                    {
                        errors++;
                    }
                }
                return string.Format("{0}/{1} resolves failed.", errors, vertices.Count);
            };
        }

        /// <summary>
        /// Tests a number of resolves.
        /// </summary>
        public static Func<string> GetTestRandomResolvesCached(Router router, Profiles.Profile profile, int count, int unique)
        {
            var random = new System.Random();
            var pool = new List<Coordinate>();
            
            // make sure all locations are near to accessible roads.
            while (pool.Count < unique)
            {
                var v1 = (uint)random.Next((int)router.Db.Network.VertexCount);
                var f1 = router.Db.Network.GetVertex(v1);
                
                var factor = router.GetDefaultGetFactor(profile);
                var edgeEnumerator = router.Db.Network.GetEdgeEnumerator();
                edgeEnumerator.MoveTo(v1);
                while (edgeEnumerator.MoveNext())
                {
                    var f = factor(edgeEnumerator.Current.Data.Profile);
                    if (f.Value != 0)
                    {
                        pool.Add(router.Db.Network.GetVertex(v1));
                        break;
                    }
                }
            }
           
            var vertices = new List<Coordinate>();
            while (vertices.Count < count)
            {
                var p = random.Next(unique);    
                vertices.Add(pool[p]);
            }

            // make sure the router is fresh.
            router = new Router(router.Db);
            
            // resolve all.
            return () =>
            {
                router.ResolverCache = new ResolverCache();
                
                var errors = 0;
                for(var i = 0; i < vertices.Count; i++)
                {
                    var routerPoint = router.TryResolve(profile, vertices[i]);
                    if (routerPoint.IsError)
                    {
                        errors++;
                    }
                }

                router.ResolverCache = null;
                return string.Format("{0}/{1} cached resolves failed.", errors, count);
            };
        }

        /// <summary>
        /// Tests a number of resolves.
        /// </summary>
        public static Func<string> GetTestRandomResolvesParallel(Router router, Profiles.Profile profile, int count)
        {
            var random = new System.Random();
            var vertices = new List<Coordinate>();

            // make sure all locations are near to accesible roads.
            while (vertices.Count < count)
            {
                var v1 = (uint)random.Next((int)router.Db.Network.VertexCount);
                var f1 = router.Db.Network.GetVertex(v1);

                var factor = router.GetDefaultGetFactor(profile);
                var edgeEnumerator = router.Db.Network.GetEdgeEnumerator();
                edgeEnumerator.MoveTo(v1);
                while (edgeEnumerator.MoveNext())
                {
                    var f = factor(edgeEnumerator.Current.Data.Profile);
                    if (f.Value != 0)
                    {
                        vertices.Add(router.Db.Network.GetVertex(v1));
                        break;
                    }
                }
            }

            // make sure the router is fresh.
            router = new Router(router.Db);
            
            // resolve all.
            return () =>
            {
                var errors = 0;

                System.Threading.Tasks.Parallel.ForEach(Enumerable.Range(0, count), (x) =>
                {
                    var routerPoint = router.TryResolve(profile, vertices[x]);
                    if (routerPoint.IsError)
                    {
                        errors++;
                    }
                });
                return string.Format("{0}/{1} resolves failed.", errors, vertices.Count);
            };
        }

        /// <summary>
        /// Tests a number of resolves.
        /// </summary>
        public static Func<string> GetTestRandomResolvesParallelCached(Router router, Profiles.Profile profile, int count, int unique)
        {
            var random = new System.Random();
            var pool = new List<Coordinate>();
            
            // make sure all locations are near to accessible roads.
            while (pool.Count < unique)
            {
                var v1 = (uint)random.Next((int)router.Db.Network.VertexCount);
                var f1 = router.Db.Network.GetVertex(v1);
                
                var factor = router.GetDefaultGetFactor(profile);
                var edgeEnumerator = router.Db.Network.GetEdgeEnumerator();
                edgeEnumerator.MoveTo(v1);
                while (edgeEnumerator.MoveNext())
                {
                    var f = factor(edgeEnumerator.Current.Data.Profile);
                    if (f.Value != 0)
                    {
                        pool.Add(router.Db.Network.GetVertex(v1));
                        break;
                    }
                }
            }
           
            var vertices = new List<Coordinate>();
            while (vertices.Count < count)
            {
                var p = random.Next(unique);    
                vertices.Add(pool[p]);
            }

            // make sure the router is fresh.
            router = new Router(router.Db);

            // resolve all.
            return () =>
            {
                router.ResolverCache = new ResolverCache();
                
                var errors = 0;

                System.Threading.Tasks.Parallel.ForEach(Enumerable.Range(0, count), (x) =>
                {
                    var routerPoint = router.TryResolve(profile, vertices[x]);
                    if (routerPoint.IsError)
                    {
                        errors++;
                    }
                });
                router.ResolverCache = null;
                
                return string.Format("{0}/{1} cached resolves failed.", errors, vertices.Count);
            };
        }

        /// <summary>
        /// Tests a number of resolve connected.
        /// </summary>
        public static Func<string> GetTestRandomResolvesConnectedParallel(Router router, Profiles.Profile profile, int count, int testIterations = 5)
        {
            var random = new System.Random();
            var vertices = new List<Coordinate>();

            // make sure all locations are near to accessible roads.
            while (vertices.Count < count)
            {
                var v1 = (uint)random.Next((int)router.Db.Network.VertexCount);
                var f1 = router.Db.Network.GetVertex(v1);

                var factor = router.GetDefaultGetFactor(profile);
                var edgeEnumerator = router.Db.Network.GetEdgeEnumerator();
                edgeEnumerator.MoveTo(v1);
                while (edgeEnumerator.MoveNext())
                {
                    var f = factor(edgeEnumerator.Current.Data.Profile);
                    if (f.Value != 0)
                    {
                        vertices.Add(router.Db.Network.GetVertex(v1));
                        break;
                    }
                }
            }

            return () =>
            {
                // resolve all.
                var errors = 0;
                
                for (var i = 0; i < testIterations; i++)
                {
                    // make sure the router is fresh.
                    router = new Router(router.Db);

                    System.Threading.Tasks.Parallel.ForEach(Enumerable.Range(0, count), (x) =>
                    {
                        var routerPoint = router.TryResolveConnected(profile, vertices[x], 1000, 250);
                        if (routerPoint.IsError)
                        {
                            errors++;
                        }
                    });
                }
                return $"{errors}/{vertices.Count * testIterations} resolves failed in {testIterations} iterations.";
            };
        }
    }
}