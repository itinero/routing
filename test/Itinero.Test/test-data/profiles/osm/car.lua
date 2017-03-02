
name = "car"
vehicle_types = { "vehicle", "motor_vehicle", "motorcar" }
constraints =  { "maxweight", "maxwidth" }

minspeed = 30
maxspeed = 200

speed_profile = {
	["motorway"] = 120,
	["motorway_link"] = 120,
	["trunk"] = 90,
	["trunk_link"] = 90,
	["primary"] = 90,
	["primary_link"] = 90,
	["secondary"] = 70,
	["secondary_link"] = 70,
	["tertiary"] = 70,
	["tertiary_link"] = 70,
	["unclassified"] = 50,
	["residential"] = 50,
	["service"] = 30,
	["services"] = 30,
	["road"] = 30,
	["track"] = 30,
	["living_street"] = 5,
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
		name = "shortest",
		function_name = "factor_and_speed",
		metric = "distance",
	}
}

function can_access (attributes, result)
	local last_access = true
	local access = access_values[attributes.access]
	if access != nil then
		result.attributes_to_keep.access = true
		last_access = access
	end
	for i=0, 10 do
		local access_key = attributes[vehicle_types[i]]
		if access_key then
			access = access_values[access_key]
			if access != nil then
				result.attributes_to_keep[access_key] = true
				last_access = access
			end
		end
	end
	return last_access
end

function factor_and_speed (attributes, result)
	 local highway = attributes.highway
	 
	 result.speed = 0
	 result.direction = 0
	 result.canstop = true
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

	 if can_access (attributes, result) == false then
	 	result.speed = 0
		result.direction = 0
		result.canstop = true
		result.attributes_to_keep = {}
	    return
	 end
	 
	if attributes.maxspeed then
		local speed = itinero.parsespeed (attributes.maxspeed)
		if speed then
			result.speed = speed * 0.75
		end
	end
end