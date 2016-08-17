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

using System;
using System.Collections.Generic;
using System.IO;

namespace Itinero.IO.Json
{
    /// <summary>
    /// A json-writer.
    /// </summary>
    public class JsonWriter
    {
        private readonly TextWriter _writer;
        private readonly Stack<Status> _statusStack;
        
        /// <summary>
        /// Creates a new json writer.
        /// </summary>
        public JsonWriter(TextWriter writer)
        {
            _writer = writer;
            _statusStack = new Stack<Status>();
        }

        /// <summary>
        /// Writes the object open char.
        /// </summary>
        public void WriteOpen()
        {
            if (_statusStack.Count > 0)
            {
                var status = _statusStack.Peek();
                
                if (status == Status.ArrayValueWritten)
                {
                    _writer.Write(',');
                }
            }

            _statusStack.Push(Status.ObjectOpened);
            _writer.Write('{');
        }

        /// <summary>
        /// Writes the object close char.
        /// </summary>
        public void WriteClose()
        {
            Status status;
            if (_statusStack.Count == 0)
            {
                throw new Exception("Cannot close object at this point.");
            }
            if (_statusStack.Count > 0)
            {
                status = _statusStack.Peek();

                if(status == Status.PropertyNameWritten)
                {
                    throw new Exception("Cannot close object right after writing a property name.");
                }
            }
            _writer.Write('}');
            while (_statusStack.Peek() != Status.ObjectOpened)
            {
                _statusStack.Pop();
            }
            _statusStack.Pop();

            if (_statusStack.Count > 0)
            {
                status = _statusStack.Peek();
                if (status == Status.PropertyNameWritten)
                { // the object was a property value.
                    _statusStack.Push(Status.PropertyValueWritten);
                }
                if (status == Status.ArrayOpenWritten ||
                    status == Status.ArrayValueWritten)
                { // the array was an array value.
                    _statusStack.Push(Status.ArrayValueWritten);
                }
            }
        }

        /// <summary>
        /// Writes a property name.
        /// </summary>
        public void WritePropertyName(string name, bool escape = false)
        {
            Status status;
            if (_statusStack.Count == 0)
            {
                throw new Exception("Cannot write property name at this point.");
            }
            else
            {
                status = _statusStack.Peek();
                if (status != Status.PropertyValueWritten &&
                    status != Status.ObjectOpened)
                {
                    throw new Exception("Cannot write property name at this point.");
                }

                if (status == Status.PropertyValueWritten)
                { // write comma before starting new property.
                    _writer.Write(',');
                }
            }

            _writer.Write('"');
            if (escape)
            {
                name = JsonTools.Escape(name);
            }
            _writer.Write(name);
            _writer.Write('"');
            _writer.Write(':');
            _statusStack.Push(Status.PropertyNameWritten);
        }
        
        /// <summary>
        /// Writes a property value.
        /// </summary>
        public void WritePropertyValue(string value, bool useQuotes = false, bool escape = false)
        {
            Status status;
            if (_statusStack.Count == 0)
            {
                throw new Exception("Cannot write property value at this point.");
            }
            else
            {
                status = _statusStack.Peek();
                if (status != Status.PropertyNameWritten)
                {
                    throw new Exception("Cannot write property value at this point.");
                }
            }

            if (useQuotes) _writer.Write('"');
            if (escape)
            {
                value = JsonTools.Escape(value);
            }
            _writer.Write(value);
            if (useQuotes) _writer.Write('"');
            _statusStack.Push(Status.PropertyValueWritten);
        }

        /// <summary>
        /// Writes a property and it's value.
        /// </summary>
        public void WriteProperty(string name, string value, bool useQuotes = false, bool escape = false)
        {
            this.WritePropertyName(name, escape);
            this.WritePropertyValue(value, useQuotes, escape);
        }

        /// <summary>
        /// Writes the array open char.
        /// </summary>
        public void WriteArrayOpen()
        {
            Status status;
            if (_statusStack.Count == 0)
            {
                throw new Exception("Cannot open array at this point.");
            }
            else
            {
                status = _statusStack.Peek();
                if (status != Status.PropertyNameWritten &&
                    status != Status.ArrayOpenWritten &&
                    status != Status.ArrayValueWritten)
                {
                    throw new Exception("Cannot open array at this point.");
                }

                if (status == Status.ArrayValueWritten)
                {
                    _writer.Write(',');
                }
            }

            _writer.Write('[');
            _statusStack.Push(Status.ArrayOpenWritten);
        }

        /// <summary>
        /// Writes the array close char.
        /// </summary>
        public void WriteArrayClose()
        {
            Status status;
            if (_statusStack.Count == 0)
            {
                throw new Exception("Cannot open array at this point.");
            }
            else
            {
                status = _statusStack.Peek();
                if (status != Status.ArrayOpenWritten &&
                    status != Status.ArrayValueWritten)
                {
                    throw new Exception("Cannot open array at this point.");
                }
            }

            _writer.Write(']');

            status = _statusStack.Peek();
            while(status != Status.ArrayOpenWritten)
            {
                _statusStack.Pop();
                status = _statusStack.Peek();
            }
            _statusStack.Pop();


            if (_statusStack.Count > 0)
            {
                status = _statusStack.Peek();
                if (status == Status.PropertyNameWritten)
                { // the array was a property value.
                    _statusStack.Push(Status.PropertyValueWritten);
                }
                if (status == Status.ArrayOpenWritten ||
                    status == Status.ArrayValueWritten)
                { // the array was an array value.
                    _statusStack.Push(Status.ArrayValueWritten);
                }
            }
        }

        /// <summary>
        /// Writes an array value.
        /// </summary>
        public void WriteArrayValue(string value)
        {
            Status status;
            if (_statusStack.Count == 0)
            {
                throw new Exception("Cannot open array at this point.");
            }
            else
            {
                status = _statusStack.Peek();
                if (status != Status.ArrayOpenWritten &&
                    status != Status.ArrayValueWritten)
                {
                    throw new Exception("Cannot open array at this point.");
                }

                if (status == Status.ArrayValueWritten)
                {
                    _writer.Write(",");
                }
            }

            _writer.Write(value);
            _statusStack.Push(Status.ArrayValueWritten);
        }

        private enum Status
        {
            ObjectOpened,
            PropertyNameWritten,
            PropertyValueWritten,
            ArrayOpenWritten,
            ArrayValueWritten
        }
    }
}