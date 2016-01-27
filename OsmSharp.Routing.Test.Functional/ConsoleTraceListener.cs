// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2016 Abelshausen Ben
// 
// This file is part of OsmSharp.
// 
// OsmSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// OsmSharp is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with OsmSharp. If not, see <http://www.gnu.org/licenses/>.

using OsmSharp.Logging;
using System;

namespace OsmSharp.Routing.Test.Functional
{
    /// <summary>
    /// A log trace listener that writes message to the console.
    /// </summary>
    public class ConsoleTraceListener : TraceListener
    {
        /// <summary>
        /// Creates a new console trace listener.
        /// </summary>
        public ConsoleTraceListener()
        {

        }

        /// <summary>
        /// Writes the given message to the console.
        /// </summary>
        /// <param name="message"></param>
        public override void Write(string message)
        {
            Console.Write(message);
        }

        /// <summary>
        /// Writes the given message to the console.
        /// </summary>
        /// <param name="message"></param>
        public override void WriteLine(string message)
        {
            Console.WriteLine(message);
        }
    }
}