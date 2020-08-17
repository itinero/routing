-- train
name = "train"
vehicle_types = { "vehicle" }

minspeed = 30
maxspeed = 200

-- default speed profiles
speed_profile = {
	["rail"] = 90
}

-- default access values
access_values = {

}

-- whitelists for profile and meta
profile_whitelist = {
	"railway"
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
	 local railway = attributes.railway
	 
	 result.speed = 0
	 result.direction = 0
	 result.canstop = false -- was true
	 result.attributes_to_keep = {}

	 -- get default speed profiles
	 local railway_speed = speed_profile[railway]
	 if railway_speed then
        result.speed = railway_speed
        result.direction = 0
		result.canstop = true
		result.attributes_to_keep.railway = railway
	 else
	    return
	 end

end