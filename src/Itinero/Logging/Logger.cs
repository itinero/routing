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