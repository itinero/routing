-- car globals
name = "nwb.car"

-- global profile parameters.
-- defines columns in the shapefile.
parameters = {
	source_vertex = "JTE_ID_BEG",
	target_vertex = "JTE_ID_END"
}

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