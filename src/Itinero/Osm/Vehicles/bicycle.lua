
name = "bicycle"
vehicle_types = { "vehicle", "bicycle" }

minspeed = 15
maxspeed = 15

speed_profile = {
	["primary"] = { speed = 15, access = true },
	["primary_link"] = { speed = 15, access = true },
	["secondary"] = { speed = 15, access = true },
	["secondary_link"] = { speed = 15, access = true },
	["tertiary"] = { speed = 15, access = true },
	["tertiary_link"] = { speed = 15, access = true },
	["unclassified"] = { speed = 15, access = true },
	["residential"] = { speed = 15, access = true },
	["service"] = { speed = 15, access = true },
	["services"] = { speed = 15, access = true },
	["road"] = { speed = 15, access = true },
	["track"] = { speed = 15, access = true },
	["cycleway"] = { speed = 15, access = true },
	["footway"] = { speed = 15, access = false },
	["pedestrian"] = { speed = 15, access = false },
	["path"] = { speed = 15, access = true },
	["living_street"] = { speed = 15, access = true },
	["ferry"] = { speed = 15, access = true },
	["movable"] = { speed = 15, access = true },
	["shuttle_train"] = { speed = 15, access = true },
  	["default"] = { speed = 15, access = true }
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
	"oneway",
	"bicycle",
	"vehicle",
	"access",
	"maxspeed",
	"maxweight",
	"maxwidth",
	"roundabout",
	"cycleway", 
	"cyclenetwork",
	"oneway:bicycle"
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
		metric = "distance"
	},
	{ 
		name = "balanced",
		function_name = "factor_and_speed_balanced",
		metric = "custom"
	},
	{ 
		name = "networks",
		function_name = "factor_and_speed_networks",
		metric = "custom"
	}
}

-- processes relation and addes the attributes_to_keep to the child ways for use in routing
function relation_tag_processor (attributes, result)
	if attributes.type == "route" and
	   attributes.route == "bicycle" then
		result.attributes_to_keep = {
			cyclenetwork = "yes"
		}
	end
end

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
		if oneway == "no" then
			return 0
		end
	end
	return nil
end

function factor_and_speed (attributes, result)
	 local highway = attributes.highway
	 
	 result.speed = 0
	 result.direction = 0
	 result.canstop = true
	 result.attributes_to_keep = {}

	 local highway_speed = speed_profile[highway]
	 if highway_speed then
        result.speed = highway_speed.speed
		result.access = highway_speed.access
        result.direction = 0
		result.canstop = true
		result.attributes_to_keep.highway = highway
	 else
		return
	 end

	 local access = can_access (attributes, result)
	 if access != nil then
		result.access = access
	 end

	 if result.access then
	 else
		result.speed = 0
		result.direction = 0
		result.canstop = true
		return
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
	direction = is_oneway (attributes, "oneway:bicycle")
	if direction != nil then
		result.direction = direction
		result.attributes_to_keep["oneway:bicycle"] = true
	end
end

highest_avoid_factor = 0.8
avoid_factor = 0.9
prefer_factor = 1.1
highest_prefer_factor = 1.2

-- multiplication factors per classification
bicycle_balanced_factors = {
	["primary"] = highest_avoid_factor,
	["primary_link"] = highest_avoid_factor,
	["secondary"] = highest_avoid_factor,
	["secondary_link"] = highest_avoid_factor,
	["tertiary"] = avoid_factor,
	["tertiary_link"] = avoid_factor,
	["residential"] = 1,
	["path"] = highest_prefer_factor,
	["cycleway"] = highest_prefer_factor,
	["footway"] = prefer_factor,
	["pedestrian"] = prefer_factor,
	["steps"] = prefer_factor
}

-- the factor function for the factor profile
function factor_and_speed_balanced (attributes, result)

	factor_and_speed (attributes, result)

	if result.speed == 0 then
		return
	end

	result.factor = 1.0 / (result.speed / 3.6)
	local balanced_factor = bicycle_balanced_factors[attributes.highway]
	if balanced_factor != nil then
		result.factor = result.factor / balanced_factor
	end

end

function factor_and_speed_networks (attributes, result)
	
	factor_and_speed_balanced (attributes, result)

	if result.speed == 0 then
		return
	end

	if attributes.cyclenetwork then
		result.factor = result.factor / 5
	end

end