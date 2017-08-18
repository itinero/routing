## How to setup/install?

### Quickly setup a routing server

Check the [routing-api](https://github.com/itinero/routing-api) project.

### Install Itinero in a .NET project

Itinero is available as a Nuget package:

    PM> Install-Package Itinero

If you are loading raw OSM-data also include:

    PM> Install-Package Itinero.IO.Osm

If you are loading data from shapefile include:

    PM> Install-Package Itinero.IO.Shape

And if you want to work with NTS:

    PM> Install-Package Itinero.Geo
	
## Example

A routing example to calculate one A->B route. First build a router db and a router, and then calculate a route:
```csharp
// using Itinero;
// using Itinero.IO.Osm;
// using Itinero.Osm.Vehicles;

// load some routing data and create a router.
var routerDb = new RouterDb();
var router = new Router(routerDb);
using (var stream = new FileInfo(@"/path/to/some/osmfile.osm.pbf").OpenRead())
{
    routerDb.LoadOsmData(stream, Vehicle.Car);
}

// calculate a route.
var route = router.Calculate(Vehicle.Car.Fastest(),
    51.26797020271655f, 4.801905155181885f, 51.26100849597512f, 4.780721068382263f);
var geoJson = route.ToGeoJson();
```

This is fine for a small area, city or country but when loading a large area check the [start building real-world application](https://github.com/itinero/routing/wiki#processing-data-into-a-network) instructions.

## API

The most important concepts are **[[RouterDb]]**, **[[Router]]**, **[[Profiles]]** and **RouterPoint**:

- **[[RouterDb]]**: Manages the data of one routing network. It holds all data in RAM or uses a memory mapping strategy to load data on demand. It holds all the network geometry, meta data and topology.
- **[[Router]]**: The main facade for all routing functionality available. It will decide the best algorithm to use based on a combination of what's requested and what data is available in the RouterDb.
- **[[Profiles]]**: Definitions of vehicle and their behaviour that can traverse the routing network.
- **RouterPoint**: A location on the routing network to use as a start or endpoint of a route. It's defined by an edge-id and an offset-value uniquely identifying it's location on the network.

## Start to build real-world applications

Most real-world routing applications will work in two steps:

1. Process raw data and write to disk.
1. Load the preprocessed data from disk and use it for routing.

Processing routing data from raw (OSM-)data can take a while and takes some processing power. Loading entire countries or continents is not an easy task. This is why Itinero has the ability to separate the processing and routing steps avoiding the need to start from raw data every time. 

#### Processing data into a network

Processing data means building a router db instance containing the routing network. The easiest way to create a router db is to just load in raw data:
```csharp
// using Itinero;
// using Itinero.IO.Osm;
// using Itinero.Osm.Vehicles;

var routerDb = new RouterDb();
using (var stream = new FileInfo(@"/path/to/some/osmfile.osm.pbf").OpenRead())
{
    routerDb.LoadOsmData(stream, Vehicle.Car);
}
```
You can then write the data inside any routerDb instance to disk using:
```csharp
using (var stream = new FileInfo(@"/path/to/some/osmfile.routing").Open(
   FileMode.Create, FileAccess.ReadWrite))
{
   routerDb.Serialize(stream);
}
```
This process is also built into Itinero's data processing tool. Just give it a raw OSM-data file and out comes a serialized router db:

#### Load the network

Loading a router db is just as easy as writing it to disk:
```csharp
using (var stream = new FileInfo(@"/path/to/some/osmfile.routing").OpenRead())
{
    var routerDb = RouterDb.Deserialize(stream);
}
```
At this point you can choose to hang on to the stream and let the router db load data on-the-fly when it needs to and give it a strategy or 'profile' it can use to do this efficiently:
```csharp
var stream = new FileInfo(@"/path/to/some/osmfile.routing").OpenRead();
var routerDb = RouterDb.Deserialize(stream, RouterDbProfile.NoCache);
```

#### Calculating routes

After a router db has been created the router class can be used to calculate routes, times or distances (depending on the vehicle profiles used). A simple example of creating a router and calculating a route:

```csharp
var router = new Router(routerDb);
var route = router.Calculate(Vehicle.Car.Fastest(),
    51.26797020271655f, 4.801905155181885f, 51.26100849597512f, 4.780721068382263f);
```

The router class is stateless so you can keep it around, there is no need to recreate it every time a route is calculated. In the above example a lot is happening behing the scenes: A decision is made what algorithm to use, the coordinates are used to search the best point on the network to start/end the route and the algorithm is executed.

The process of converting a lat/lon location into a location on the road network is called _resolving_ a location. The router will call a search algorithm that searches the network for the best location and returns this as a **RouterPoint**. 

Most real-world routing applications will also want control over this process in case it fails. A location can be outside of the loaded routing network or the closest road is just too far away too make sense. A real-world application will take the following steps to calculate a route:

1. Resolve the locations to route from/to.
1. Check the result from the resolve operation and respond accordingly.
1. Call the router again with the obtained RouterPoints.
1. Check the result from the routing call and respond accordingly.

A more advanced code sample to do the exact same thing as the previous example but with more control over the process:

```csharp
var profile = Vehicle.Car.Fastest();
var routerPoint1 = router.TryResolve(profile, 51.26797020271655f, 4.801905155181885f);
if(routerPoint1.IsError)
{
    // do something or retry.
}
var routerPoint2 = router.TryResolve(profile, 51.26100849597512f, 4.780721068382263f);
if (routerPoint2.IsError)
{
    // do something or retry.
}
var route = router.TryCalculate(Vehicle.Car.Fastest(),
    routerPoint1.Value, routerPoint2.Value);
if(route.IsError)
{
    // do something or retry.
}
```
