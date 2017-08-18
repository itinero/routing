By default Itinero works with OSM data but it's perfectly capable of handling other data. Think closed alternatives like TomTom MultiNet or HERE but there are also several open datasets out there most of them managed by local governments. Most of these datasets are available as shapefile and can be loaded using ```Itinero.IO.Shape```.

## RouterDb from Shapefile

This is an example of loading data from a shapefile into a router db. We are using an open dataset from the Dutch government called 'NWB', the 'national road registry'. We can't include samples for MultiNet or HERE because we're not sure we are allowed to do this.

You can check a full working sample [here](https://github.com/itinero/routing/tree/develop/samples/Sample.Shape).

#### Vehicle definition

First of all make sure to convert your shapefile to use WGS84. After that, the most important part is creating a vehicle definition that describes the attributes in the shapefile in such a way that Itinero knows how to handle the data. An example vehicle profile:

```lua
-- car globals
name = "nwb.car"

-- whitelists for profile and meta
profile_whitelist = {
    "BST_CODE", 
	"BAANSUBSRT", 
	"RIJRICHTNG", 
	"WEGBEHSRT", 
	"HECTO_LTTR"
}
meta_whitelist = {
	"STT_NAAM"
}

-- default speed profiles
speed_profiles = {
	["BVD"] = { speed = 50, oneway = nil },
	["AF"] = { speed = 70, oneway = nil },
	["OP"] = { speed = 70, oneway = nil },
	["HR"] = { speed = 120, oneway = nil },
	["MRB"] = { speed = 30, oneway = true},
	["NRB"] = { speed = 30, oneway = true}
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
	local BST_CODE = attributes.BST_CODE -- code of road type.
	local speed_profile = speed_profiles[BST_CODE]
	local speed = 70
	local direction = 0 -- bidirectional default
	result.attributes_to_keep.BST_CODE = true -- keep this code.
	if speed_profile then
		speed = speed_profile.speed
		if speed_profile.oneway then
			direction = 1 -- this type of edge is oneway forward by default
		end
	end

	local RIJRICHTNG = attributes.RIJRICHTNG -- oneway code.
	if RIJRICHTNG then
		result.attributes_to_keep.RIJRICHTNG = true -- keep the oneway stuff
		if RIJRICHTNG == "H" then
			direction = 1
		elseif RIJRICHTNG == "T" then
			direction = 2
		end
	end
	 
	result.speed = speed -- speed in km/h
    result.direction = direction
    result.canstop = true
end

-- instruction generators
instruction_generators = {
	{
		applies_to = "", -- applies to all profiles when empty
		generators = {
			{
				name = "start",
				function_name = "get_start"
			},
			{ 
				name = "stop",
				function_name = "get_stop"
			},
			{
				name = "roundabout",
				function_name = "get_roundabout"
			},
			{
				name = "turn",
				function_name = "get_turn"
			}
		}
	}
}

-- gets the first instruction
function get_start (route_position, language_reference, instruction)
	if route_position.is_first() then
		local direction = route_position.direction()
		instruction.text = itinero.format(language_reference.get("Start {0}."), language_reference.get(direction));
		instruction.shape = route_position.shape
		return 1
	end
	return 0
end

-- gets the last instruction
function get_stop (route_position, language_reference, instruction) 
	if route_position.is_last() then
		instruction.text = language_reference.get("Arrived at destination.");
		instruction.shape = route_position.shape
		return 1
	end
	return 0
end

function contains (attributes, key, value)
	if attributes then
		return localvalue == attributes[key];
	end	
end

-- gets a roundabout instruction
function get_roundabout (route_position, language_reference, instruction) 
	if (route_position.attributes.BST_CODE == "NRB" or 
		route_position.attributes.BST_CODE == "MRB") and
		(not route_position.is_last()) then
		local attributes = route_position.next().attributes
		if attributes.junction then
		else
			local exit = 1
			local count = 1
			local previous = route_position.previous()
			while previous and (previous.attributes.BST_CODE == "NRB" or 
					previous.attributes.BST_CODE == "MRB") do
				local branches = previous.branches
				if branches then
					branches = branches.get_traversable()
					if branches.count > 0 then
						exit = exit + 1
					end
				end
				count = count + 1
				previous = previous.previous()
			end

			instruction.text = itinero.format(language_reference.get("Take the {0}th exit at the next roundabout."), "" .. exit)
			if exit == 1 then
				instruction.text = itinero.format(language_reference.get("Take the first exit at the next roundabout."))
			elseif exit == 2 then
				instruction.text = itinero.format(language_reference.get("Take the second exit at the next roundabout."))
			elseif exit == 3 then
				instruction.text = itinero.format(language_reference.get("Take the third exit at the next roundabout."))
			end
			instruction.type = "roundabout"
			instruction.shape = route_position.shape
			return count
		end
	end
	return 0
end

-- gets a turn
function get_turn (route_position, language_reference, instruction) 
	local relative_direction = route_position.relative_direction().direction

	local turn_relevant = false
	local branches = route_position.branches
	if branches then
		branches = branches.get_traversable()
		if relative_direction == "straighton" and
			branches.count >= 2 then
			turn_relevant = true -- straight on at cross road
		end
		if  relative_direction != "straighton" and 
			branches.count > 0 then
			turn_relevant = true -- an actual normal turn
		end
	end

	if turn_relevant then
		local next = route_position.next()
		local name = nil
		if next then
			name = next.attributes.STT_NAAM
		end
		if name then
			instruction.text = itinero.format(language_reference.get("Go {0} on {1}."), 
				language_reference.get(relative_direction), name)
			instruction.shape = route_position.shape
		else
			instruction.text = itinero.format(language_reference.get("Go {0}."), 
				language_reference.get(relative_direction))
			instruction.shape = route_position.shape
		end

		return 1
	end
	return 0
end
```

You will need to create the same kind of profile for your own dataset, telling Itinero what to do and how to load the data.

#### Loading the data

Once this is done you can use the profile to create routerdb as follows:

```csharp
// create a new router db and load the shapefile.
var vehicle = new Car(); // load data for the car profile.
var routerDb = new RouterDb(EdgeDataSerializer.MAX_DISTANCE);
routerDb.LoadFromShape("/path/to/shape/", "wegvakken.shp", 
  "JTE_ID_BEG",  "JTE_ID_END", vehicle);

// write the router db to disk for later use.
routerDb.Serialize("nwb.routerdb");
```

## RouterDb to Shapefile

If for whatever reason you want to go in the opposite direction there is also an extension method in ```Itinero.IO.Shape``` to write a routerdb as a shapefile. To write a shapefile with support for car, bicycle and pedestrian just do:

```csharp
var profiles = new Profile[] {
  routerDb.GetSupportedProfile("car"),
  routerDb.GetSupportedProfile("bicycle"),
  routerDb.GetSupportedProfile("pedestrian")
};
routerDb.WriteToShape("shapefile", profiles);
```
