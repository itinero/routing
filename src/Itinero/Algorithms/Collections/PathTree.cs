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

namespace Itinero.Algorithms.Collections
{
    /// <summary>
    /// Represents a tree of paths by linking their segments together.
    /// </summary>
    public class PathTree
    {
        /// <summary>
        /// Creates a new path tree.
        /// </summary>
        public PathTree()
        {
            _data = new uint[1024];
        }
        
        private uint[] _data;
        private uint _pointer;
        
        /// <summary>
        /// Adds a new segment.
        /// </summary>
        /// <returns></returns>
        public uint Add(OriginalEdge edge, uint data0)
        {
            var id = _pointer;
            if (_data.Length <= _pointer + 3)
            {
                System.Array.Resize(ref _data, _data.Length * 2);
            }
            _data[_pointer + 0] = edge.Vertex1;
            _data[_pointer + 1] = edge.Vertex2;
            _data[_pointer + 2] = data0;
            _pointer += 3;
            return id;
        }

        /// <summary>
        /// Adds a new segment.
        /// </summary>
        /// <returns></returns>
        public uint Add(OriginalEdge edge, uint data0, uint data1)
        {
            var id = _pointer;
            if (_data.Length <= _pointer + 4)
            {
                System.Array.Resize(ref _data, _data.Length * 2);
            }
            _data[_pointer + 0] = edge.Vertex1;
            _data[_pointer + 1] = edge.Vertex2;
            _data[_pointer + 2] = data0;
            _data[_pointer + 3] = data1;
            _pointer += 4;
            return id;
        }

        /// <summary>
        /// Adds a new segment.
        /// </summary>
        /// <returns></returns>
        public uint Add(OriginalEdge edge, uint data0, uint data1, uint data2)
        {
            var id = _pointer;
            if (_data.Length <= _pointer + 5)
            {
                System.Array.Resize(ref _data, _data.Length * 2);
            }
            _data[_pointer + 0] = edge.Vertex1;
            _data[_pointer + 1] = edge.Vertex2;
            _data[_pointer + 2] = data0;
            _data[_pointer + 3] = data1;
            _data[_pointer + 4] = data2;
            _pointer += 5;
            return id;
        }

        /// <summary>
        /// Gets the data at the given pointer.
        /// </summary>
        public OriginalEdge Get(uint pointer, out uint data0)
        {
            data0 = _data[pointer + 2];
            return new OriginalEdge(_data[pointer + 0],
                _data[pointer + 1]);
        }

        /// <summary>
        /// Gets the data at the given pointer.
        /// </summary>
        public OriginalEdge Get(uint pointer, out uint data0, out uint data1)
        {
            data0 = _data[pointer + 2];
            data1 = _data[pointer + 3];
            return new OriginalEdge(_data[pointer + 0],
                _data[pointer + 1]);
        }

        /// <summary>
        /// Gets the data at the given pointer.
        /// </summary>
        public OriginalEdge Get(uint pointer, out uint data0, out uint data1, out uint data2)
        {
            data0 = _data[pointer + 2];
            data1 = _data[pointer + 3];
            data2 = _data[pointer + 4];
            return new OriginalEdge(_data[pointer + 0],
                _data[pointer + 1]);
        }

        /// <summary>
        /// Clears all data.
        /// </summary>
        public void Clear()
        {
            _pointer = 0;
        }
    }
}
