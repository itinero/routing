// The MIT License (MIT)

// Copyright (c) 2016 Ben Abelshausen

// Permission is hereby granted, free of charge, to any person obtaining a copy
// of this software and associated documentation files (the "Software"), to deal
// in the Software without restriction, including without limitation the rights
// to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
// copies of the Software, and to permit persons to whom the Software is
// furnished to do so, subject to the following conditions:

// The above copyright notice and this permission notice shall be included in
// all copies or substantial portions of the Software.

// THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
// IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
// FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
// AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
// LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
// OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
// THE SOFTWARE.

using Itinero;
using Itinero.Algorithms.Matrices;
using Itinero.LocalGeo;
using Itinero.Osm.Vehicles;
using System.Collections.Generic;
using System.IO;

namespace Sample.Matrix
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Download.ToFile("http://files.itinero.tech/data/itinero/routerdbs/planet/europe/belgium.c.cf.routerdb", "belgium.c.cf.routerdb").Wait();

            var routerDb = RouterDb.Deserialize(File.OpenRead("belgium.c.cf.routerdb"));
            var router = new Router(routerDb);

            var locations = new List<Coordinate>(new Coordinate[]
            {
                new Coordinate(51.270453873703080f, 4.8008108139038080f),
                new Coordinate(51.264197451065370f, 4.8017120361328125f),
                new Coordinate(51.267446600889850f, 4.7830009460449220f),
                new Coordinate(51.260733228426076f, 4.7796106338500980f),
                new Coordinate(51.256489871317920f, 4.7884941101074220f),
                new Coordinate(51.270964016530680f, 4.7894811630249020f)
            });

            // METHOD1: quick and easy for high-quality data already on the road network.
            // calculate drive time in seconds between all given locations.
            var resolved = router.Resolve(Vehicle.Car.Fastest(), locations.ToArray(), 150);
            var invalidPoints = new HashSet<int>();
            var matrix = router.CalculateWeight(Vehicle.Car.Fastest(), resolved, invalidPoints);

            // METHOD2: most datasets contain large numbers of unconfirmed locations that may be too far from the road network or contain errors.
            //          this method can handle coordinates sets that contains errors.

            // let's add a location that's in the middle of nowhere.
            var invalidLocation = new Coordinate(51.275689280878694f, 4.7779369354248040f);
            locations.Insert(3, invalidLocation);

            // for advanced applications there is a helper class
            var matrixCalculator = new WeightMatrixAlgorithm(router, Vehicle.Car.Fastest(), locations.ToArray());
            matrixCalculator.Run();

            // there is some usefull output data here now.
            var weights = matrixCalculator.Weights; // the weights, in this case seconds travel time.
            var errors = matrixCalculator.Errors; // some locations could be unreachable, this contains details about those locations.
            resolved = matrixCalculator.RouterPoints.ToArray(); // the resolved routerpoints, you can use these later without the need to resolve again.

            // when there are failed points, the weight matrix is smaller, use these functions to map locations from the original array to succeeded points.
            var newIndex = matrixCalculator.MassResolver.ResolvedIndexOf(4); // returns the index of the original location in the weight matrix.
            var oldIndex = matrixCalculator.MassResolver.LocationIndexOf(5); // returns the index of the weight matrix point in the original locations array.
        }
    }
}