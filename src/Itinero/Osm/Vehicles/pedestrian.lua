
name = "pedestrian"
vehicle_types = { "foot" }

minspeed = 4
maxspeed = 5

speed_profile = {
	["primary"] = 4,
	["primary_link"] = 4,
	["secondary"] = 4,
	["secondary_link"] = 4,
	["tertiary"] = 4,
	["tertiary_link"] = 4,
	["unclassified"] = 4,
	["residential"] = 4,
	["service"] = 4,
	["services"] = 4,
	["road"] = 4,
	["track"] = 4,
	["cycleway"] = 4,
	["path"] = 4,
	["footway"] = 4,
	["pedestrian"] = 4,
	["living_street"] = 4,
	["ferry"] = 4,
	["movable"] = 4,
	["shuttle_train"] = 4,
  	["default"] = 4
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
	"highway",
	"foot",
	"access"
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
	 else
	    return
	 end

	 if can_access (attributes, result) == false then
	 	result.speed = 0
		result.direction = 0
		result.canstop = true
	    return
	 end
end