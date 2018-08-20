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

        /// <summary>
        /// Returns a description.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            if (this.IsError)
            {
                return $"Result<{nameof(T)}>: {this.ErrorMessage}";
            }
            if (this.Value == null)
            {
                return $"Result<{nameof(T)}>: null";
            }
            return $"Result<{nameof(T)}>: {this.Value.ToString()}";
        }
    }
}