using Itinero.Algorithms.Weights;
using System;
using Itinero.Graphs.Directed;
using Itinero.Profiles;
using Itinero.Data.Contracted;

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

        public override float GetEdgeWeight(DynamicEdge edge, out bool? direction)
        {
            throw new NotImplementedException();
        }

        public override float GetEdgeWeight(DirectedDynamicGraph.EdgeEnumerator edge, out bool? direction)
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