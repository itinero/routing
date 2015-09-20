// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2015 Abelshausen Ben
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

using OsmSharp.Routing.Exceptions;
using OsmSharp.Routing.Network;
using System;

namespace OsmSharp.Routing
{
    /// <summary>
    /// Represents a result of some calculation and associated status information.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Result<T>
    {
        private readonly T _value;
        private readonly Func<string, Exception> _createException;

        internal Result(T result)
        {
            _value = result;
            this.ErrorMessage = string.Empty;
            this.Status = ResultStatus.OK;
        }

        internal Result(string errorMessage, Func<string, Exception> createException)
        {
            _value = default(T);
            _createException = createException;
            this.ErrorMessage = errorMessage;
            this.Status = ResultStatus.Error;
        }

        /// <summary>
        /// Gets the result.
        /// </summary>
        public T Value
        {
            get
            {
                if(this.Status != ResultStatus.OK)
                {
                    throw _createException(this.ErrorMessage);
                }
                return _value;
            }
        }

        /// <summary>
        /// Gets the status.
        /// </summary>
        public ResultStatus Status { get; private set; }

        /// <summary>
        /// Gets the error message.
        /// </summary>
        public string ErrorMessage { get; private set; }

        /// <summary>
        /// Creates a router point result error.
        /// </summary>
        /// <returns></returns>
        internal static Result<RouterPoint> CreateRouterPointError(string message)
        {
            return new Result<RouterPoint>(message, (m) =>
            {
                return new ResolveFailedException(m);
            });
        }

        /// <summary>
        /// Creates a route result error.
        /// </summary>
        /// <returns></returns>
        internal static Result<RouterPoint> CreateRouteError(string message)
        {
            return new Result<RouterPoint>(message, (m) =>
            {
                return new RouteNotFoundException(m);
            });
        }
    }

    /// <summary>
    /// Represents the status of a result.
    /// </summary>
    public enum ResultStatus
    {
        /// <summary>
        /// Everything is fine.
        /// </summary>
        OK,
        /// <summary>
        /// An error occurred.
        /// </summary>
        Error
    }
}