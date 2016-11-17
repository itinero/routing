-- bus globals
name = "bus"
vehicle_types = { "vehicle", "motor_vehicle", "motorcar" }
constraints =  { "maxweight", "maxwidth" }

minspeed = 30
maxspeed = 200

-- default speed profiles
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
	"highway",
	"oneway",
	"motorcar",
	"motor_vehicle",
	"vehicle",
	"access",
	"maxspeed",
	"maxweight",
	"maxwidth",
	"junction"
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
	},
	{
		name = "classifications",
		function_name = "factor_and_speed_classifications",
		metric = "custom"
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
			result.speed = speed * 0.75
			result.attributes_to_keep.maxspeed = true
		end
	end

	-- get maxweight and maxwidth constraints if any
	local maxweight = 0
	local maxwidth = 0
	if attributes.maxweight then
		maxweight = itinero.parseweight (attributes.maxweight)
	end
	if attributes.maxwidth then
		maxwidth = itinero.parseweight (attributes.maxwidth)
	end
	if maxwidth != 0 or maxweight != 0 then
		result.constraints = { maxweight, maxwidth }
		result.attributes_to_keep.maxweight = true
		result.attributes_to_keep.maxwidth = true
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

-- multiplication factors per classification
classifications_factors = {
	["motorway"] = 10,
	["motorway_link"] = 10,
	["trunk"] = 9,
	["trunk_link"] = 9,
	["primary"] = 8,
	["primary_link"] = 8,
	["secondary"] = 7,
	["secondary_link"] = 7,
	["tertiary"] = 6,
	["tertiary_link"] = 6,
	["unclassified"] = 5,
	["residential"] = 5
}

-- the classifications function for the classifications profile
function factor_and_speed_classifications (attributes, result)

	factor_and_speed (attributes, result)

	if result.speed == 0 then
		return
	end

	result.factor = 1.0 / (result.speed / 3.6)
	local classification_factor = classifications_factors[attributes.highway]
	if classification_factor != nil then
		result.factor = result.factor / classification_factor
	else
		result.factor = result.factor / 4
	end
end