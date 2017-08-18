## Concepts

To configure routing in Itinero there are three main concepts, Vehicles, Profiles and Profile instances:

- Vehicle: A **definition of a vehicle** that travels the network. This can be a _car_ but also a _pedestrian_ for example.
- Profile: A profile describes **the behaviour of a vehicle**. For a car this could be _shortest_ or _fastest_ taking the shortest or fastest route respectively. For bicycles this could be _recreational_ meaning this profile focuses on nice routes to get from A to B.

## Lua

A vehicle can be defined by a lua script. This vehicle definition gets embedded in a routerdb when created. 

```lua
name = "car"


-- whitelists for profile and meta
profile_whitelist = {
	"highway"
}
meta_whitelist = {
	"name"
}

-- profile definitions linking a function to a profile
profiles = {
	{
		name = "",
		function_name = "factor_and_speed",
		metric = "time"
	},
	{ 
		name = "shortest",
		function_name = "factor_and_speed",
		metric = "distance",
	}
}

-- the main function turning attributes into a factor_and_speed and a tag whitelist
function factor_and_speed (attributes, result)

	 result.speed = 0
	 result.direction = 0
	 result.canstop = true
	 result.attributes_to_keep = {}

	 -- get default speed profiles
	 local highway = attributes.highway
	 if highway == "motorway" or 
	    highway == "motorway_link" then
		result.speed = 100 -- speed in km/h
		result.direction = 0
		result.canstop = true
		result.attributes_to_keep.highway = highway
	end
end
```

The main _factor_and_speed_ is function is the most important active part of the vehicle definition but these variables/functions need to be defined in any vehicle definition:

- _name_: The name of the vehicle, should be unique for all loaded profiles.
- _profile_whitelist_: A list of all attributes that will be added to an edge as part of the profile.
- _meta_whitelist_: A list of all attributes will be added to an edge as meta data.
- _profiles_: Defines the profiles for this vehicle, by default at minimum _fastest_ (with no name) and _shortest_ have to be defined.
- _factor_and_speed_: The core of the vehicle definition, converts edge attributes into a factor and speed.

#### Profiles

A profile describes **the behaviour of a vehicle** and is defined by a _metric_, a function to calculate speed/factor and a name:

- _metric_: This can be _time_, _distance_ or completely _custom_.
- _function_name_: The name of the function to calculate factor and/or speed.
- _name_: The name of the profile (an empty string automatically means 'fastest', the default).

#### Factor and speed functions

A factor and speed function converts a set of attributes into a speed and/or factor. Usually the default _factor_and_speed_ will define a speed for each possible set of profile attributes. Based on this Itinero can define a _fastest_ and a _shortest_ profile with metrics being _time_ and _distance_ respectively. A _custom_ profile uses a _factor_ to define weights of edges. For shortest this factor is constant, usually equal to 1.

To summarize there are three options when using a _factor_and_speed_ function:

- With metric _distance_: Itinero will use the function to get the speed and set factor to a constant, usually 1.
- With metric _time_: Itinero will use the function to get the speed and set factor to 1/speed.
- With metric _custom_: Itinero will use the function to get the speed and the factor.

## Default profiles

There are some [vehicle definitions](https://github.com/itinero/routing/blob/develop/src/Itinero/Osm/) included in Itinero by default assuming OSM data is used. A few examples here:

- The [car](https://github.com/itinero/routing/blob/develop/src/Itinero/Osm/Vehicles/car.lua) vehicle: Defines a default _factor_and_speed_ function but also a custom function to define a car profile called **classifications** to prefer higher classified roads even more than the regular profile.  
- The [bicycle](https://github.com/itinero/routing/blob/develop/src/Itinero/Osm/Vehicles/bicycle.lua) vehicle: Also definas a default _factor_and_speed_ function but in addition it defines two more:
  - **balanced**: Aggressively chooses bicycle-only parts of the network.
  - **networks**: Aggressifely chooses bicycle routing networks.

## Build a routerdb with embedded vehicles

By default all vehicles that are used to build a routerdb are embedded into it. When you have your own custom lua profile you can build a routerdb as follows:

```csharp
var vehicle = DynamicVehicle.LoadFromStream(File.OpenRead("path/to/custom.lua"));
var routerDb = new RouterDb();
// load the raw osm data.
using (var stream = new FileInfo(@"/path/to/some/osmfile.osm.pbf").OpenRead())
{
    routerDb.LoadOsmData(stream, vehicle);
}
// write the routerdb to disk.
using (var stream = new FileInfo(@"/path/to/some/osmfile.routerdb").OpenWrite())
{
    routerDb.Serialize(stream);
}
```

Now the vehicles are embedded in the routerdb, you can get them by their name, in this case assumed to be _custom_:

```csharp
var routerDb = RouterDb.Deserialize(File.OpenRead(@"/path/to/some/osmfile.routerdb"));
var vehicle = routerDb.GetSupportedVehicle("custom");
var router = new Router(routerDb);

var route = router.Calculate(vehicle.Fastest(), ...);
```

