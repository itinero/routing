The router db contains all data related to a routing network meaning it's geometry, topology and meta-data. On top of that it can also contain optimized versions of the network dedicated to specific routing profiles.

Loading raw data, OSM-data or a routing network from shapefiles, takes a while and is very inefficiënt. The router db can be use to prevent the need to reload the entire network from it's raw data. You can also filter the source data leaving only the data relevant for the routing profiles you need.

#### Load from raw data

The simplest way of creating a router db is to load raw data into it, in this case from an OSM-PBF file:

```csharp
// using Itinero;
// using Itinero.IO.Osm;
// using Itinero.Osm.Vehicles;

var routerDb = new RouterDb();
// load the raw osm data.
using (var stream = new FileInfo(@"/path/to/some/osmfile.osm.pbf").OpenRead())
{
    routerDb.LoadOsmData(stream, Vehicle.Car);
}
// write the routerdb to disk.
using (var stream = new FileInfo(@"/path/to/some/osmfile.routerdb").OpenWrite())
{
    routerDb.Serialize(stream);
}
```

To load a network that's provided in a shapefile, check the [[Shapefiles]] sample.

#### Contraction

To add an optimized version of the network you can call the _AddContracted_ extension method. This will add a contracted version of the network to the routing db and the router will automatically detect and use it when appropriate:
```csharp
routerDb.AddContracted(Vehicle.Car.Fastest());
```
The contraction can take a while depending on the size of your network and the profile used. You can contract a network at the same time as you're executing routing requests. Speed up of the routing calls will be immediate once contraction is finished.

#### Loading a serialized database.

The default way to load a previously serialized router db:

```csharp
var routerDb = RouterDb.Deserialize(stream);
```
Most of the time your network will fit into RAM, for example a router db for the entire planet takes about 10GB of RAM. When using a mobile device or other low-resource environment you can tell the router db to not load everything just yet and load it on-demand. In this example, no cache means loading will be done only on-demand and no data will be kept in RAM except when using it:

```csharp
var routerDb = RouterDb.Deserialize(stream, RouterDbProfile.NoCache);
```

#### Extracting an area

It's also possible to extract an area from a routerdb, reducing it only to the area you need after it has been built.

```csharp
var smallerDb = routerDb.ExtractArea(minLatitude, minLongitude, 
     maxLatitude, maxLongitude);
```

You can also extract an area using any other function selecting relevant vertices:

```csharp
var smallerDb = routerDb.ExtractArea(v => 
     /*... return true if this vertex looks good ... */);
```
