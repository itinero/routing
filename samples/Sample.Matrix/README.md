### Matix Calculation sample

This sample shows how to calculate a distance/time matrix with itinero.

```csharp
// METHOD1: quick and easy for high-quality data already on the road network.
// calculate drive time in seconds between all given locations.
var resolved = router.Resolve(Vehicle.Car.Fastest(), locations.ToArray());
var invalidPoints = new HashSet<int>();
var matrix = router.CalculateWeight(Vehicle.Car.Fastest(), resolved, invalidPoints);

// METHOD2: most datasets contain large numbers of unconfirmed locations that may be too far from the road network or contain errors.
//          this method can handle coordinates sets that contains errors.

// let's add a location that's in the middle of nowhere.
var invalidLocation = new Coordinate(51.275689280878694f, 4.7779369354248040f);
locations.Insert(3, invalidLocation);

// for advanced applications there is a helper class
var matrixCalculator = new Itinero.Algorithms.WeightMatrixAlgorithm(router, Vehicle.Car.Fastest(), locations.ToArray());
matrixCalculator.Run();

// there is some usefull output data here now.
var weights = matrixCalculator.Weights; // the weights, in this case seconds travel time.
var errors = matrixCalculator.Errors; // some locations could be unreachable, this contains details about those locations.
resolved = matrixCalculator.RouterPoints.ToArray(); // the resolved routerpoints, you can use these later without the need to resolve again.

// when there are failed points, the weight matrix is smaller, use these functions to map locations from the original array to succeeded points.
var newIndex = matrixCalculator.IndexOf(4); // returns the index of the original location in the weight matrix.
var oldIndex = matrixCalculator.LocationIndexOf(5); // returns the index of the weight matrix point in the original locations array.
```