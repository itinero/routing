/*
 *  Licensed to SharpSoftware under one or more contributor
 *  license agreements. See the NOTICE file distributed with this work for 
 *  additional information regarding copyright ownership.
 * 
 *  SharpSoftware licenses this file to you under the Apache License, 
 *  Version 2.0 (the "License"); you may not use this file except in 
 *  compliance with the License. You may obtain a copy of the License at
 * 
 *       http://www.apache.org/licenses/LICENSE-2.0
 * 
 *  Unless required by applicable law or agreed to in writing, software
 *  distributed under the License is distributed on an "AS IS" BASIS,
 *  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *  See the License for the specific language governing permissions and
 *  limitations under the License.
 */

using Itinero.Profiles;
using System;
using System.Threading;
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
            routerDb.AddShortcuts(shortcutSpecs, CancellationToken.None);
        }

        /// <summary>
        /// Adds multiple shortcut db's at the same time.
        /// </summary>
        public static void AddShortcuts(this RouterDb routerDb, ShortcutSpecs[] shortcutSpecs, CancellationToken cancellationToken)
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
                routerPointEmbedder.Run(cancellationToken);
            }

            for(var i = 0; i < shortcutSpecs.Length; i++)
            {
                var specs = shortcutSpecs[i];
                Itinero.Logging.Logger.Log("RouterDbExtensions", Logging.TraceEventType.Information,
                    "Building shortcuts for {0}...", specs.Name);

                var shortcutBuilder = new Itinero.Algorithms.Shortcuts.ShortcutBuilder(routerDb, specs.Profile, specs.Name, specs.Locations,
                    specs.LocationsMeta, specs.TransferTime, specs.MinTravelTime, specs.MaxShortcutDuration);
                shortcutBuilder.Run(cancellationToken);

                routerDb.AddShortcuts(specs.Name, shortcutBuilder.ShortcutsDb);
            }
        }
    }
}