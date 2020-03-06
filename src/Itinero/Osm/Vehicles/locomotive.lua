-- See https://wiki.openstreetmap.org/wiki/Key:railway for infomation
-- train globals
name = "locomotive"
vehicle_types = { "vehicle", "motor_vehicle", "motorcar", "train", "freight" }
constraints =  { "maxweight", "maxwidth" }

minspeed = 10
maxspeed = 300

-- default speed profiles
speed_profile = {
	["rail"] = 100,	
  	["default"] = 60
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
	["use_sidepath"] = false,
	["siding"] = true,
	["spur"] = true,
	["cutting"] = true,
	["embankment"] = true,
	["crossover"] = true,
	["yard"] = true,
	["freight"] = true,
	["station"] = true,
	["roundhouse"] = true,
	["wash"] = true,
	["turntable"] = true,
	["railway_crossing"] = true
	--["passenger"] = false
}

--tags of rail which are not able to be traversed
unusable = {
	["disused"] = true,
	["razed"] = true,
	["dismantled"] = true,
	["demolished"] = true,
	["abandoned"] = true,
	["tram"] = true,
	["proposed"] = true,
}

-- whitelists for profile and meta
profile_whitelist = {
	"railway",
	"maxspeed",
	"route",
	"barrier",
	"service"
}
meta_whitelist = {
	"bridge",
	"tunnel",
	"embankment",
	"cutting",
	"electrified",	
	"embedded_rails",
	"frequency",
	"railway:track_ref",
	"tracks",
	"spur",
	"siding",
	"crossover",
	"name",
	"ref",
	"owner",
	"operator",
	"usage",
	"from",
	"to",
	"via",
	"voltage",
	"wikipedia"
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
			elseif access_values[attributes.access] != nil then
				result.attributes_to_keep[attributes.access] = true
				last_access = access
			end
		end
	end
	return last_access
end

-- turns a oneway tag value into a direction, any attributes can be given with the specified key to check
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
	 local railway = attributes.railway
	 
	 result.speed = 0
	 result.direction = 0
	 result.canstop = true
	 result.attributes_to_keep = {}

	  -- do not consider unusable track
	 if unusable[railway] then
		return
	 end

	 -- get default speed profiles
	 local railway_speed = speed_profile[railway]

	 --get service attribute
	 local service = attributes.service

	 --Check Speeed and Service
	 if railway_speed then
        result.speed = railway_speed
        result.direction = 0
		result.canstop = false

		--Store in result.attributes_to_keep
		result.attributes_to_keep.railway = railway
		result.attributes_to_keep.service = service
		result.attributes_to_keep.route = attributes.route
		result.attributes_to_keep.usage = attributes.usage

		--Check if can be stopped on
		if railway == "construction" or
		   railway == "disused" or
		   railway == "station" or
		   railway == "halt" or
		   railway == "crossing" or
		   railway == "buffer_stop" or
		   railway == "derail" or
		   railway == "switch" or
		   railway == "railway_crossing" or
		   railway == "turntable" or
		   railway == "roundhouse" or
		   railway == "traverser" or
		   railway == "wash" or
		   railway == "signal" then
		   result.canstop = true
		end

		--Check Service
		if service == "siding" or 
		   service == "spur" or
		   service == "yard" or
		   service == "crossover" then
		   result.canstop = true
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
	end
	local direction = is_oneway (attributes, "oneway")
	if direction != nil then
		result.direction = direction
	end

	--usage could be saved see if this is a railnetwork

end

-- multiplication factors per classification
classifications_factors = {
	["rail"] = 5
}

-- the classifications function for the classifications profile
function factor_and_speed_classifications (attributes, result)

	factor_and_speed (attributes, result)

	if result.speed == 0 then
		return
	end

	result.factor = 1.0 / (result.speed / 3.6)
	local classification_factor = classifications_factors[attributes.railway]
	if classification_factor != nil then
		result.factor = result.factor / classification_factor
	else
		result.factor = result.factor / 4
	end
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

-- gets a roundabout instruction
function get_roundabout (route_position, language_reference, instruction) 
	if route_position.attributes.junction == "roundabout" and
		(not route_position.is_last()) then
		local attributes = route_position.next().attributes
		if attributes.junction then
		else
			local exit = 1
			local count = 1
			local previous = route_position.previous()
			while previous and previous.attributes.junction == "roundabout" do
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

			if exit <= 1 then
				instruction.text = itinero.format(language_reference.get("Take the first exit at the next roundabout."))
			elseif exit == 2 then
				instruction.text = itinero.format(language_reference.get("Take the second exit at the next roundabout."))
			elseif exit == 3 then
				instruction.text = itinero.format(language_reference.get("Take the third exit at the next roundabout."))
			else
				instruction.text = itinero.format(language_reference.get("Take the {0}th exit at the next roundabout."), "" .. exit)
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
			name = next.attributes.name
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