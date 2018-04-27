using System;
using System.Collections.Generic;
using Itinero.Algorithms.Collections;
using Itinero.Algorithms.PriorityQueues;
using Itinero.Algorithms.Weights;
using Itinero.Data.Contracted.Edges;
using Itinero.Graphs.Directed;

namespace Itinero.Algorithms.Contracted.Dual.Witness
{
    public class NeighbourWitnessCalculator
    {
        protected readonly DirectedGraph _graph;
        protected readonly int _hopLimit;
        protected readonly int _maxSettles;

        public NeighbourWitnessCalculator(DirectedGraph graph, int hopLimit = 4, int maxSettles = int.MaxValue)
        {
            _graph = graph;
            _hopLimit = hopLimit;
            _maxSettles = maxSettles;
        }

        protected PathTree pathTree = new PathTree();
        protected BinaryHeap<uint> pointerHeap = new BinaryHeap<uint>();
        
        public void Run(uint vertex, HashSet<uint> dirty, Action<uint, uint, Shortcut<float>> witnessCallback)
        {
            var forwardSettled = new Dictionary<uint, float>();
            var backwardSettled = new Dictionary<uint, float>();

            pathTree.Clear();
            pointerHeap.Clear();

            var p = pathTree.AddSettledVertex(vertex, new WeightAndDir<float>()
            {
                Direction = new Dir(true, true),
                Weight = 0
            }, 0);
            pointerHeap.Push(p, 0);

            // dequeue vertices until stopping conditions are reached.
            var witness = new Shortcut<float>();
            var cVertex = Constants.NO_VERTEX;
            WeightAndDir<float> cWeight;
            var cHops = uint.MaxValue;
            var queued2Hops = -1;
            var enumerator = _graph.GetEdgeEnumerator();
            while (pointerHeap.Count > 0)
            {
                var cPointer = pointerHeap.Pop();
                pathTree.GetSettledVertex(cPointer, out cVertex, out cWeight, out cHops);

                if (cHops == 2)
                { // check if the search can stop or not.
                    queued2Hops--;

                    witness.Forward = float.MaxValue;
                    witness.Backward = float.MaxValue;
                    if (cWeight.Direction.F && 
                        forwardSettled.TryGetValue(cVertex, out float best))
                    { // this is a 2-hop and vertex was settled before, we have a witness!
                        witness.Forward = best;
                    }
                    if (cWeight.Direction.B && 
                        backwardSettled.TryGetValue(cVertex, out best))
                    { // this is a 2-hop and vertex was settled before, we have a witness!
                        witness.Backward = best;
                    }
                    if (witness.Backward != float.MaxValue ||
                        witness.Forward != float.MaxValue)
                    { // report witness here.
                        if (vertex != cVertex)
                        { // TODO: check this, how can they ever be the same?
                            witnessCallback(vertex, cVertex, witness);
                        }
                    }

                    if (queued2Hops == 0)
                    { // all 2-hops have been considered, stop the search.
                        return;
                    }
                }

                if (forwardSettled.ContainsKey(cVertex) ||
                    forwardSettled.Count > _maxSettles)
                {
                    cWeight.Direction = new Dir(false, cWeight.Direction.B);
                }
                if (backwardSettled.ContainsKey(cVertex) ||
                    backwardSettled.Count > _maxSettles)
                {
                    cWeight.Direction = new Dir(cWeight.Direction.F, false);
                }
                
                var isRelevant = false;
                if (cWeight.Direction.F)
                {
                    forwardSettled.Add(cVertex, cWeight.Weight);
                    isRelevant = true;
                }
                if (cWeight.Direction.B)
                {
                    backwardSettled.Add(cVertex, cWeight.Weight);
                    isRelevant = true;
                }

                if (!isRelevant)
                { // not forward, not backwards.
                    continue;
                }

                cHops++;
                if (cHops >= _hopLimit)
                { // over hop limit.
                    continue;
                }

                if (forwardSettled.Count > _maxSettles &&
                    backwardSettled.Count > _maxSettles)
                { // over settled count.
                    break;
                }

                enumerator.MoveTo(cVertex);
                while (enumerator.MoveNext())
                {
                    var nVertex = enumerator.Neighbour;

                    if (dirty != null && cHops == 1)
                    { // check if the neighbour is dirty.
                        if (!dirty.Contains(nVertex))
                        { // only do dirty vertices if they are defined.
                            continue;
                        }
                    }

                    Dir nDir;
                    float nWeight;
                    ContractedEdgeDataSerializer.Deserialize(enumerator.Data0,
                        out nDir, out nWeight);

                    nDir._val = (byte)(cWeight.Direction._val & nDir._val);

                    // if (nDir.F)
                    // {
                    //     nDir._val = (byte)(nDir._val & 2);
                    // }
                    // if (nDir.B)
                    // {
                    //     nDir._val = (byte)(nDir._val & 1);
                    // }
                    if (nDir._val == 0)
                    {
                        continue;
                    }

                    nWeight = nWeight + cWeight.Weight;

                    var nPoiner = pathTree.AddSettledVertex(nVertex, nWeight, nDir, cHops);
                    if (cHops == 2)
                    { // increase 2 hops counter.
                        if (queued2Hops == -1)
                        { // go from -1 -> 1
                            queued2Hops = 0;
                        }
                        queued2Hops++;
                    }
                    pointerHeap.Push(nPoiner, nWeight);
                }
            }
        }
    }
}