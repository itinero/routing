
name = "car"
vehicle_types = { "vehicle", "motor_vehicle", "motorcar" }
minspeed = 30
maxspeed = 200

speed_profile = {
	["motorway"] = 120,
	["motorway_link"] = 70,
	["trunk"] = 90,
	["trunk_link"] = 70,
	["primary"] = 90,
	["primary_link"] = 70,
	["secondary"] = 70,
	["secondary_link"] = 50,
	["tertiary"] = 70,
	["tertiary_link"] = 50,
	["unclassified"] = 50,
	["residential"] = 50,
	["service"] = 30,
	["services"] = 30,
	["living_street"] = 5,
	["track"] = 5,
	["ferry"] = 5,
	["movable"] = 5,
	["shuttle_train"] = 10,
  	["default"] = 10
}

access_values = {
	["private"] = false,
	["yes"] = true,
	["no"] = false,
	["permissive"] = true,
	["destination"] = true,
	["customers"] = false,
	["designated"] = true,
	["public"] = true,
	["delivery"] = true,
	["use_sidepath"] = false
}

profile_whitelist = {

}

meta_whitelist = {
	"name"
}

profiles = {
	{
		name = "",
		function_name = "factor_and_speed",
		metric = "time"
	},
	{ 
		shortest = "shortest",
		function_name = "factor_and_speed",
		metric = "distance",
	}
}

function has_access (attributes, access_types)
	
end

function factor_and_speed (attributes, result)
	 local highway = attributes.highway
	 
	 result.speed = 0
	 result.direction = 0
	 result.canstop = false
	 result.attributes_to_keep = {}

	 local highway_speed = speed_profile[highway]
	 if highway_speed then
        result.speed = highway_speed
        result.direction = 0
		result.canstop = true
		result.attributes_to_keep.highway = highway
		if highway == "motorway" or 
		   highway == "motorway_link" then
		   result.canstop = false
		end
	 else
	    return
	 end

	 local access = access (attributes, vehicle_types)
end