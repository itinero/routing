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

using Itinero.Graphs.Geometric.Shapes;

namespace Itinero.Graphs.Geometric
{
    /// <summary>
    /// A geometric edge.
    /// </summary>
    public class GeometricEdge
    {
        /// <summary>
        /// Creates a new geometric edge.
        /// </summary>
        public GeometricEdge(uint id, uint from, uint to, uint[] data, bool edgeDataInverted,
            ShapeBase shape)
        {
            this.Id = id;
            this.From = from;
            this.To = to;
            this.Data = data;
            this.DataInverted = edgeDataInverted;
            this.Shape = shape;
        }

        /// <summary>
        /// Creates a new geometric edge.
        /// </summary>
        internal GeometricEdge(GeometricGraph.EdgeEnumerator enumerator)
        {
            this.Id = enumerator.Id;
            this.From = enumerator.From;
            this.To = enumerator.To;
            this.Data = enumerator.Data;
            this.DataInverted = enumerator.DataInverted;
            this.Shape = enumerator.Shape;
        }

        /// <summary>
        /// Gets the edge id.
        /// </summary>
        public uint Id
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the vertex at the beginning of this edge.
        /// </summary>
        public uint From
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the vertex at the end of this edge.
        /// </summary>
        public uint To
        {
            get;
            private set;
        }

        /// <summary>
        /// Returns true if the edge data is inverted relative to the direction of this edge.
        /// </summary>
        public bool DataInverted
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the edge data.
        /// </summary>
        public uint[] Data
        {
            get;
            private set;
        }

        /// <summary>
        /// Gets the shape.
        /// </summary>
        public ShapeBase Shape
        {
            get;
            private set;
        }
    }
}