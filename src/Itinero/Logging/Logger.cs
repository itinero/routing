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

using System.Collections.Generic;

namespace Itinero.Logging
{
    /// <summary>
    /// A logger.
    /// </summary>
    public class Logger
    {
        private readonly string _name;

        /// <summary>
        /// Creates a new logger.
        /// </summary>
        public Logger(string name)
        {
            _name = name;
        }

        /// <summary>
        /// Creates a new logger.
        /// </summary>
        internal static Logger Create(string name)
        {
            return new Logger(name);
        }

        /// <summary>
        /// Logs a message.
        /// </summary>
        public void Log(TraceEventType type, string message, params object[] args)
        {
            if (Logger.LogAction == null)
            {
                Logger.LogAction = (o, level, localmessage, parameters) =>
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("[{0}] {1} - {2}", o, level, localmessage));
                };
            }

            Logger.LogAction(_name, type.ToInvariantString().ToLower(), string.Format(message, args), null);
        }

        /// <summary>
        /// Logs a message.
        /// </summary>
        public static void Log(string name, TraceEventType type, string message, params object[] args)
        {
            if (Logger.LogAction == null)
            {
                Logger.LogAction = (o, level, localmessage, parameters) =>
                {
                    System.Diagnostics.Debug.WriteLine(string.Format("[{0}] {1} - {2}", o, level, localmessage));
                };
            }
            Logger.LogAction(name, type.ToInvariantString().ToLower(), string.Format(message, args), null);
        }

        /// <summary>
        /// Defines the log action function.
        /// </summary>
        /// <param name="origin">The origin of the message, a class or module name.</param>
        /// <param name="level">The level of the message, 'critical', 'error', 'warning', 'verbose' or 'information'.</param>
        /// <param name="message">The message content.</param>
        /// <param name="parameters">Any parameters that may be useful.</param>
        public delegate void LogActionFunction(string origin, string level, string message,
            Dictionary<string, object> parameters);

        /// <summary>
        /// Gets or sets the action to actually log a message.
        /// </summary>
        public static LogActionFunction LogAction
        {
            get;
            set;
        }
    }
}