This is a small guide on how to use Itinero to calculate a distance/time/x matrix between a number of locations.

You need a few basic things:
- A profile, like `Vehicle.Car.Fastest()`. Fastest will given you seconds, shortest will give you distance in meters.
- A [[RouterDb]] loaded with the routing network and with a contracted version of the network for the profile.

To create the [[RouterDb]], load it using your data as described in the [[RouterDb]] section. To speed up the calculations make sure you add a contracted version of the routing graph to the [[RouterDb]] using the same profile you are going to use the calculate the matrix.

Once you have that create a [[Router]] and call _Calculate_ using an array of locations. This will calculate a square matrix between all given locations.

A codesample:

```csharp
// Loading a routerDb straight from an OSM file, 
// you can also load a routerDb from disk, see routerDb docs.
var routerDb = new RouterDb();
using (var stream = new FileInfo(@"/path/to/some/osmfile.osm.pbf").OpenRead())
{
   routerDb.LoadOsmData(stream, Vehicle.Car);
}
routerDb.AddContracted(Vehicle.Car.Fastest());

var router = new Router(routerDb);
var locations = new Coordinate[]
{
   location1,
   location2,
   ...
};
var resolvedLocations = router.Resolve(Vehicle.Car.Fastest(), locations);
var invalids = new HashSet<int>(); // will hold the locations that cannot be calculated.
var weights = router.CalculateWeight(Vehicle.Car.Fastest(),
   resolvedLocations, invalids);
```

### Error Handling

When calculating large matrices you'll notice that sometimes, due to errors in either the locations provided or the routing network itself, some of the locations cannot be found or routes cannot be calculated. Thus in any real life application we would need a more advanced approach that can handle these error gracefully. You can write all of this code yourself but we have created a convenient _[WeightMatrixAlgorithm](https://github.com/itinero/routing/blob/develop/src/Itinero/Algorithms/Matrices/WeightMatrixAlgorithm.cs)_ helper class you can use:

```csharp
// Loading a routerDb straight from an OSM file, 
// you can also load a routerDb from disk, see routerDb docs.
var routerDb = new RouterDb();
using (var stream = new FileInfo(@"/path/to/some/osmfile.osm.pbf").OpenRead())
{
   routerDb.LoadOsmData(stream, Vehicle.Car);
}
routerDb.AddContracted(Vehicle.Car.Fastest());

var router = new Router(routerDb);
var locations = new Coordinate[]
{
   location1,
   location2,
   ...
};
var matrixCalculator = new Itinero.Algorithms.Matrices.WeightMatrixAlgorithm(
 router, Vehicle.Car.Fastest(), locations);
matrixCalculator.Run();
```