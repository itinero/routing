using Itinero.Attributes;
using Itinero.Profiles;

namespace Itinero.Osm.Vehicles.Ski
{
    /// <inheritdoc/>
    public class Downhill : Itinero.Profiles.Vehicle
    {
        /// <summary>
        /// Creates a new instance of the <see cref="Downhill"/> class. 
        /// </summary>
        public Downhill()
        {
            base.MetaWhiteList.Add("name");

            base.ProfileWhiteList.Add("piste:type");
            base.ProfileWhiteList.Add("aerialway");

            Register(new Profile("shortest", ProfileMetric.DistanceInMeters, VehicleTypes, null, this));
            Register(new Profile(string.Empty, ProfileMetric.TimeInSeconds, VehicleTypes, null, this));
        }

        /// <inheritdoc/>
        public override string Name => "downhill";

        /// <inheritdoc/>
        public override string[] VehicleTypes { get; } = [ "ski", "foot" ];

        /// <inheritdoc/>
        public override FactorAndSpeed FactorAndSpeed(IAttributeCollection attributes, Whitelist whitelist)
        {
            short direction = 0;
            float speedFactor = 0f, value = 0f;

            if (attributes is null) return Itinero.Profiles.FactorAndSpeed.NoFactor;

            if (attributes.TryGetValue("piste:type", out var piste_type) && piste_type == "downhill") {
                direction = 1; // always follow piste direction
                if (attributes.TryGetValue("piste:difficulty", out var piste_difficulty)) {
                    value = speedFactor = piste_difficulty switch
                    {
                        "easy" => 1 / (30f / 3.6f),
                        "intermediate" => 1 / (50f / 3.6f),
                        "advanced" => 1 / (30f / 3.6f),
                        _ => 1 / (20f / 3.6f),
                    };
                } else {
                    value = speedFactor = 1 / (20f / 3.6f);
                }
            } else if (attributes.TryGetValue("aerialway", out var aerialway)) {
                direction = 1; // forward the edge
                value = speedFactor = 1 / (10f / 3.6f);
            } else {
                return Itinero.Profiles.FactorAndSpeed.NoFactor;
            }

            return new FactorAndSpeed {
                SpeedFactor = speedFactor,
                Value = value,
                Direction = direction
            };
        }
    }
}