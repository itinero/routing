// Itinero - OpenStreetMap (OSM) SDK
// Copyright (C) 2016 Abelshausen Ben
// 
// This file is part of Itinero.
// 
// Itinero is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// Itinero is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Itinero. If not, see <http://www.gnu.org/licenses/>.

using Itinero.Profiles;
using System;
using System.Collections.Generic;

namespace Itinero.Algorithms.Shortcuts
{
    /// <summary>
    /// Contains extensions for the router db related to shortcut db's.
    /// </summary>
    public static class RouterDbExtensions
    {
        /// <summary>
        /// Adds multiple shortcut db's at the same time.
        /// </summary>
        public static void AddShortcuts(this RouterDb routerDb, params ShortcutSpecs[] shortcutSpecs)
        {
            // check all specs.
            if (shortcutSpecs == null) { throw new ArgumentNullException(); }
            for(var i = 0; i < shortcutSpecs.Length; i++)
            {
                if (shortcutSpecs[i] == null ||
                    shortcutSpecs[i].Locations == null)
                {
                    throw new ArgumentException(string.Format("Shortcut specs at index {0} not set or locations not set.", 
                        i));
                }
                if (shortcutSpecs[i].LocationsMeta != null &&
                    shortcutSpecs[i].LocationsMeta.Length != shortcutSpecs[i].Locations.Length)
                {
                    throw new ArgumentException(string.Format("Shortcut specs at index {0} has a different dimensions for locations and meta-data.",
                        i));
                }
            }
            
            for(var i = 0; i < shortcutSpecs.Length; i++)
            {
                var specs = shortcutSpecs[i];

                var profiles = new List<Profile>();
                profiles.Add(specs.Profile);
                profiles.AddRange(specs.TransitionProfiles);

                var routerPointEmbedder = new Itinero.Algorithms.Networks.RouterPointEmbedder(routerDb, profiles.ToArray(), specs.Locations);
                routerPointEmbedder.Run();
            }

            for(var i = 0; i < shortcutSpecs.Length; i++)
            {
                var specs = shortcutSpecs[i];
                Itinero.Logging.Logger.Log("RouterDbExtensions", Logging.TraceEventType.Information,
                    "Building shortcuts for {0}...", specs.Name);

                var shortcutBuilder = new Itinero.Algorithms.Shortcuts.ShortcutBuilder(routerDb, specs.Profile, specs.Name, specs.Locations,
                    specs.LocationsMeta, specs.TransferTime, specs.MinTravelTime, specs.MaxShortcutDuration);
                shortcutBuilder.Run();

                routerDb.AddShortcuts(specs.Name, shortcutBuilder.ShortcutsDb);
            }
        }
    }
}
