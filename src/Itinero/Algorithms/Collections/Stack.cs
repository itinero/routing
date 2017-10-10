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

using Reminiscence.Arrays;
using System;

namespace Itinero.Algorithms.Collections
{
    /// <summary>
    /// A stack implementation based on a memory array.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class Stack<T>
    {
        private readonly ArrayBase<T> _data;
        
        /// <summary>
        /// Creates a new stack.
        /// </summary>
        public Stack()
        {
            _data = new MemoryArray<T>(1024);
        }

        private int _pointer = -1;

        /// <summary>
        /// Pushes a new element.
        /// </summary>
        public void Push(T element)
        {
            if (_pointer >= _data.Length - 1)
            {
                _data.Resize(_data.Length + 1024);
            }

            _pointer++;
            _data[_pointer] = element;
        }

        /// <summary>
        /// Returns the element at the top.
        /// </summary>
        /// <returns></returns>
        public T Peek()
        {
            if (_pointer == -1)
            {
                throw new InvalidOperationException("Cannot peek into an empty stack.");
            }
            return _data[_pointer];
        }

        /// <summary>
        /// Pops an element from the stack.
        /// </summary>
        /// <returns></returns>
        public T Pop()
        {
            if (_pointer == -1)
            {
                throw new InvalidOperationException("Cannot pop from an empty stack.");
            }
            var e = _data[_pointer];
            _pointer--;
            return e;
        }

        /// <summary>
        /// Gets the number of elements on this stack.
        /// </summary>
        public int Count
        {
            get
            {
                return _pointer + 1;
            }
        }
    }
}
