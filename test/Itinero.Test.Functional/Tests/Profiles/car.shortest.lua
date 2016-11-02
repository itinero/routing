
name = "car.shortest"
metric = "distance"
vehicle_types = { "motor_vehicle", "car" }

speed_profile = {
  ["motorway"] = 90,
  ["motorway_link"] = 45,
  ["trunk"] = 85,
  ["trunk_link"] = 40,
  ["primary"] = 65,
  ["primary_link"] = 30,
  ["secondary"] = 55,
  ["secondary_link"] = 25,
  ["tertiary"] = 40,
  ["tertiary_link"] = 20,
  ["unclassified"] = 25,
  ["residential"] = 25,
  ["living_street"] = 10,
  ["service"] = 15,
  ["track"] = 5,
  ["ferry"] = 5,
  ["movable"] = 5,
  ["shuttle_train"] = 10,
  ["default"] = 10
}

function factor_and_speed (attributes, result)
	 local highway = attributes.highway
	 
	 result.speed = 0
	 result.direction = 0
	 result.canstop = false

	 local highway_speed = speed_profile[highway]
	 if highway_speed then
        result.speed = highway_speed
        result.direction = 0
		result.canstop = true
	 end
end