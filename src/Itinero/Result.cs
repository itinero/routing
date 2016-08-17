// Itinero - Routing for .NET
// Copyright (C) 2015 Abelshausen Ben
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

using Itinero.Exceptions;
using Itinero.Data.Network;
using System;

namespace Itinero
{
    /// <summary>
    /// Represents a result of some calculation and associated status information.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Result<T>
    {
        private readonly T _value;
        private readonly Func<string, Exception> _createException;

        /// <summary>
        /// Creates a new result.
        /// </summary>
        public Result(T result)
        {
            _value = result;
            this.ErrorMessage = string.Empty;
            this.IsError = false;
        }

        /// <summary>
        /// Creates a new result.
        /// </summary>
        public Result(string errorMessage)
            : this(errorMessage, (m) => new Exception(m))
        {

        }

        /// <summary>
        /// Creates a new result.
        /// </summary>
        public Result(string errorMessage, Func<string, Exception> createException)
        {
            _value = default(T);
            _createException = createException;
            this.ErrorMessage = errorMessage;
            this.IsError = true;
        }

        /// <summary>
        /// Gets the result.
        /// </summary>
        public T Value
        {
            get
            {
                if(this.IsError)
                {
                    throw _createException(this.ErrorMessage);
                }
                return _value;
            }
        }

        /// <summary>
        /// Gets the status.
        /// </summary>
        public bool IsError { get; private set; }

        /// <summary>
        /// Gets the error message.
        /// </summary>
        public string ErrorMessage { get; private set; }

        /// <summary>
        /// Converts this result, when an error to an result of another type.
        /// </summary>
        /// <returns></returns>
        public Result<TNew> ConvertError<TNew>()
        {
            if(!this.IsError)
            {
                throw new Exception("Cannot convert a result that represents more than an error.");
            }
            return new Result<TNew>(this.ErrorMessage, this._createException);
        }

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
}