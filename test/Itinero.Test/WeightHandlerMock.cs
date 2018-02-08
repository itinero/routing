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

using Itinero.Algorithms.Weights;
using System;
using Itinero.Graphs.Directed;
using Itinero.Profiles;
using Itinero.Data.Contracted;
using Itinero.Algorithms.Collections;

namespace Itinero.Test
{
    class DefaultWeightHandlerMock : WeightHandler<float>
    {
        public override float Infinite
        {
            get
            {
                return float.MaxValue;
            }
        }

        public override float Zero
        {
            get
            {
                return 0;
            }
        }

        public override float Add(float weight1, float weight2)
        {
            return weight1 + weight1;
        }

        public override float Add(float weight, ushort edgeProfile, float distance, out Factor factor)
        {
            factor = new Factor()
            {
                Direction = 0,
                Value = 1
            };
            return weight + distance;
        }

        public override WeightAndDir<float> GetEdgeWeight(MetaEdge edge)
        {
            throw new NotImplementedException();
        }

        public override void AddEdge(DirectedDynamicGraph graph, uint vertex1, uint vertex2, bool? direction, float weight)
        {
            throw new NotImplementedException();
        }

        public override void AddEdge(DirectedMetaGraph graph, uint vertex1, uint vertex2, uint contractedId, bool? direction, float weight)
        {
            throw new NotImplementedException();
        }

        public override void AddOrUpdateEdge(DirectedMetaGraph graph, uint vertex1, uint vertex2, uint contractedId, bool? direction, float weight)
        {
            throw new NotImplementedException();
        }

        public override uint AddPathTree(PathTree tree, uint vertex, float weight, uint previous)
        {
            throw new NotImplementedException();
        }

        public override void GetPathTree(PathTree tree, uint pointer, out uint vertex, out float weight, out uint previous)
        {
            throw new NotImplementedException();
        }

        public override void AddOrUpdateEdge(DirectedDynamicGraph graph, uint vertex1, uint vertex2, uint contractedId, bool? direction, float weight, uint[] s1, uint[] s2)
        {
            throw new NotImplementedException();
        }

        public override float Calculate(ushort edgeProfile, float distance, out Factor factor)
        {
            factor = new Factor()
            {
                Direction = 0,
                Value = 1
            };
            return distance;
        }

        public override WeightAndDir<float> CalculateWeightAndDir(ushort edgeProfile, float distance, out bool accessible)
        {
            throw new NotImplementedException();
        }

        public override WeightAndDir<float> CalculateWeightAndDir(ushort edgeProfile, float distance)
        {
            throw new NotImplementedException();
        }

        public override float GetEdgeWeight(DynamicEdge edge, out bool? direction)
        {
            throw new NotImplementedException();
        }

        public override float GetEdgeWeight(DirectedDynamicGraph.EdgeEnumerator edge, out bool? direction)
        {
            throw new NotImplementedException();
        }

        public override WeightAndDir<float> GetEdgeWeight(DirectedMetaGraph.EdgeEnumerator edge)
        {
            throw new NotImplementedException();
        }

        public override float GetEdgeWeight(MetaEdge edge, out bool? direction)
        {
            throw new NotImplementedException();
        }

        public override float GetEdgeWeight(DirectedMetaGraph.EdgeEnumerator edge, out bool? direction)
        {
            throw new NotImplementedException();
        }

        public override float GetMetric(float weight)
        {
            return weight;
        }

        public override float Subtract(float weight1, float weight2)
        {
            return weight1 - weight2;
        }

        public override bool CanUse(ContractedDb db)
        {
            return true;
        }

        public override int DynamicSize
        {
            get
            {
                return 1;
            }
        }

        public override int MetaSize
        {
            get
            {
                return 1;
            }
        }
        public override bool IsSmallerThanAny(float weight, float max)
        {
            return weight < max;
        }
    }
}