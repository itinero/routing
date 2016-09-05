### RouterDb from Shapefile

This sample shows how to create a routerdb from a shapefile.

First create a vehicle definition:

```csharp
	/// <summary>
    /// A vehicle profile for NWB-based shapefiles.
    /// </summary>
    public class Car : Vehicle
    {
        /// <summary>
        /// Gets the unique name.
        /// </summary>
        public override string UniqueName
        {
            get
            {
                return "NWB.Car";
            }
        }

        /// <summary>
        /// Gets the vehicle types.
        /// </summary>
        public override List<string> VehicleTypes
        {
            get
            {
                return new List<string>(new[] { "motor_vehicle", "car" });
            }
        }

        /// <summary>
        /// Returns true if an attribute with the given key is relevant for the profile.
        /// </summary>
        public override bool IsRelevantForProfile(string key)
        {
            return key == "BST_CODE" || 
                key == "BAANSUBSRT" ||
                key == "RIJRICHTNG" ||
                key == "WEGBEHSRT" ||
                key == "HECTO_LTTR";
        }

        /// <summary>
        /// Returns the maximum speed.
        /// </summary>
        /// <returns></returns>
        public override float MaxSpeed()
        {
            return 130;
        }

        /// <summary>
        /// Returns the minimum speed.
        /// </summary>
        /// <returns></returns>
        public override float MinSpeed()
        {
            return 5;
        }

        /// <summary>
        /// Returns the probable speed.
        /// </summary>
        public override float ProbableSpeed(IAttributeCollection tags)
        {
            string highwayType;
            if (tags.TryGetValue("BAANSUBSRT", out highwayType))
            {
                switch (highwayType)
                {
                    case "BVD":
                        return 50;
                    case "AF":
                    case "OP":
                        return 70;
                    case "HR":
                        return 120;
                    default:
                        return 70;
                }
            }
            return 0;
        }

        /// <summary>
        /// Returns true if the edge is oneway forward, false if backward, null if bidirectional.
        /// </summary>
        protected override bool? IsOneWay(IAttributeCollection tags)
        {
            string oneway;
            if (tags.TryGetValue("RIJRICHTNG", out oneway))
            {
                if (oneway == "H")
                {
                    return true;
                }
                else if (oneway == "T")
                {
                    return false;
                }
            }
            return null;
        }
    }
```

And process the data:

```csharp
// create a new router db and load the shapefile.
var vehicle = new Car(); // load data for the car profile.
var routerDb = new RouterDb(EdgeDataSerializer.MAX_DISTANCE);
routerDb.LoadFromShape("/path/to/shape/", "wegvakken.shp", 
  "JTE_ID_BEG",  "JTE_ID_END", vehicle);

// write the router db to disk for later use.
routerDb.Serialize("nwb.routerdb");
```