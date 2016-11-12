-- moped globals
name = "moped"
vehicle_types = { "vehicle", "motor_vehicle", "moped" }

minspeed = 40
maxspeed = 40

-- default speed profiles
speed_profile = {
	["primary"] = 40,
	["primary_link"] = 40,
	["secondary"] = 40,
	["secondary_link"] = 40,
	["tertiary"] = 40,
	["tertiary_link"] = 40,
	["unclassified"] = 40,
	["residential"] = 40,
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

-- default access values
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

-- whitelists for profile and meta
profile_whitelist = {

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

-- interprets access tags
function can_access (attributes, result)
	local last_access = nil
	local access = access_values[attributes.access]
	if access != nil then
		result.attributes_to_keep.access = true
		last_access = access
	end
	for i=0, 10 do
		local access_key_key = vehicle_types[i]
		local access_key = attributes[access_key_key]
		if access_key then
			access = access_values[access_key]
			if access != nil then
				result.attributes_to_keep[access_key_key] = true
				last_access = access
			end
		end
	end
	return last_access
end

-- turns a oneway tag value into a direction
function is_oneway (attributes, name)
	local oneway = attributes[name]
	if oneway != nil then
		if oneway == "yes" or 
		   oneway == "true" or 
		   oneway == "1" then
			return 1
		end
		if oneway == "-1" then
			return 2
		end
	end
	return nil
end

-- the main function turning attributes into a factor_and_speed and a tag whitelist
function factor_and_speed (attributes, result)
	 local highway = attributes.highway
	 
	 result.speed = 0
	 result.direction = 0
	 result.canstop = true
	 result.attributes_to_keep = {}

	 -- get default speed profiles
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

	 -- interpret access tags
	 if can_access (attributes, result) == false then
	 	result.speed = 0
		result.direction = 0
		result.canstop = true
	    return
	 end
	 
	 -- get maxspeed if any.
	 if attributes.maxspeed then
		local speed = itinero.parsespeed (attributes.maxspeed)
		if speed then
			if speed > 40 then
				speed = 40
			end
			result.speed = speed * 0.75
			result.attributes_to_keep.maxspeed = true
		end
	end

	-- get directional information
	local junction = attributes.junction
	if junction == "roundabout" then
		result.direction = 1
		result.attributes_to_keep.junction = true
	end
	local direction = is_oneway (attributes, "oneway")
	if direction != nil then
		result.direction = direction
		result.attributes_to_keep.oneway = true
	end
end