using System;
using System.Collections.Generic;
using System.Globalization;
using Itinero.Attributes;

namespace Itinero.Profiles
{
    internal static class IAttributeCollectionExtensions
    {
        public static bool TryParseTranslated(this IAttributeCollection attributes, string profileName, out FactorAndSpeed factorAndSpeed)
        {
            if (attributes.Contains("translated_profile", "yes"))
            {
                if (!attributes.TryGetValue(profileName, out var value) || string.IsNullOrWhiteSpace(value)) throw new Exception("Could not read factor and speed from translated profile.");

                var split = value.Split('|');
                if (split == null || split.Length != 3) throw new Exception("Could not read factor and speed from translated profile.");

                factorAndSpeed = new FactorAndSpeed()
                {
                    Direction = short.Parse(split[0], NumberStyles.Any),
                    Value = float.Parse(split[1], NumberStyles.Any, 
                        System.Globalization.CultureInfo.InvariantCulture),
                    SpeedFactor = float.Parse(split[2], NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture)
                };
                return true;
            }

            factorAndSpeed = default(FactorAndSpeed);
            return false;
        }

        public static bool IsTranslatedProfile(this IAttributeCollection attributes)
        {
            return attributes.Contains("translated_profile", "yes");
        }
    }
}