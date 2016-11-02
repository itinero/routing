
name = "car.shortest"
vehicle_types = { "motor_vehicle", "car" }

function factor_and_speed (attributes, result)
	 local highway = attributes.highway
	 
	 result.speed = 0
	 result.direction = 0
	 result.canstop = false

	 if highway and "residential" == highway then
		result.direction = 0
		result.canstop = true
		result.speed = 80
	 end
end