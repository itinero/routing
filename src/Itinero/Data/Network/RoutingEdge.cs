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

using Itinero.Data.Network.Edges;
using Itinero.Graphs.Geometric.Shapes;

namespace Itinero.Data.Network
{
    /// <summary>
    /// Represents an edge in a routing network.
    /// </summary>
    public class RoutingEdge
    {
        /// <summary>
        /// Creates a new edge.
        /// </summary>
        internal RoutingEdge(uint id, uint from, uint to, EdgeData data, bool edgeDataInverted,
            ShapeBase shape)
        {
            this.Id = id;
            this.To = to;
            this.From = from;
            this.Data = data;
            this.DataInverted = edgeDataInverted;
            this.Shape = shape;
        }

        /// <summary>
        /// Creates a new edge keeping the current state of the given enumerator.
        /// </summary>
        internal RoutingEdge(RoutingNetwork.EdgeEnumerator enumerator)
        {
            this.Id = enumerator.Id;
            this.To = enumerator.To;
            this.From = enumerator.From;
            this.Data = enumerator.Data;
            this.DataInverted = enumerator.DataInverted;
            this.Shape = enumerator.Shape;
        }

        /// <summary>
        /// Gets the id.
        /// </summary>
        public uint Id { get; private set; }

        /// <summary>
        /// Gets the from.
        /// </summary>
        public uint From { get; private set; }

        /// <summary>
        /// Gets the to.
        /// </summary>
        public uint To { get; private set; }

        /// <summary>
        /// Gets the data.
        /// </summary>
        public EdgeData Data { get; private set; }

        /// <summary>
        /// Gets the inverted-flag.
        /// </summary>
        public bool DataInverted { get; set; }

        /// <summary>
        /// Gets the shape.
        /// </summary>
        public ShapeBase Shape
        {
            get;
            private set;
        }

        /// <summary>
        /// Returns a string that represents the current object.
        /// </summary>
        /// <returns></returns>
        public override string ToString()
        {
            return string.Format("[{0}]--{1}->[{2}]", this.From, this.Id, this.To);
        }
    }
}