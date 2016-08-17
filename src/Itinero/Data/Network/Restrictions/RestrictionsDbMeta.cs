// Itinero - Routing for .NET
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

namespace Itinero.Data.Network.Restrictions
{
    /// <summary>
    /// Represents restriction db meta-data.
    /// </summary>
    public class RestrictionsDbMeta
    {
        /// <summary>
        /// Creates a new restriction db meta-object.
        /// </summary>
        internal RestrictionsDbMeta(string vehicle, RestrictionsDb db)
        {
            this.Vehicle = vehicle;
            this.RestrictionsDb = db;
        }

        /// <summary>
        /// Gets the vehicle.
        /// </summary>
        public string Vehicle { get; private set; }

        /// <summary>
        /// Gets the restrictions db.
        /// </summary>
        public RestrictionsDb RestrictionsDb { get; private set; }
    }
}