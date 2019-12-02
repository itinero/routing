using System.Threading;
using Itinero.Algorithms.Weights;
using Itinero.Data.Network;
using Itinero.Profiles;

namespace Itinero.Algorithms.Default.EdgeBased
{
    /// <summary>
    /// Calculates one-hop routes if they exist between the given source and target.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class OneHopRouter<T> : AlgorithmBase
        where T : struct
    {
        private readonly RouterDb _routerDb;
        private readonly IProfileInstance _profileInstance;
        private readonly WeightHandler<T> _weightHandler;
        private readonly RouterPoint _source;
        private readonly bool? _sourceForward;
        private readonly RouterPoint _target;
        private readonly bool? _targetForward;

        /// <summary>
        /// Creates a new algorithm.
        /// </summary>
        /// <param name="routerDb">The router db.</param>
        /// <param name="profileInstance">The profile instance.</param>
        /// <param name="weightHandler">The weight handler.</param>
        /// <param name="source">The source.</param>
        /// <param name="sourceForward">The source direction flag.</param>
        /// <param name="target">The target.</param>
        /// <param name="targetForward">The target direction flag.</param>
        public OneHopRouter(RouterDb routerDb, IProfileInstance profileInstance, WeightHandler<T> weightHandler,
            RouterPoint source, bool? sourceForward, RouterPoint target, bool? targetForward)
        {
            _routerDb = routerDb;
            _profileInstance = profileInstance;
            _weightHandler = weightHandler;
            _source = source;
            _sourceForward = sourceForward;
            _target = target;
            _targetForward = targetForward;
        }

        protected override void DoRun(CancellationToken cancellationToken)
        {
            if (_sourceForward.HasValue && _targetForward.HasValue &&
                _sourceForward != _targetForward)
            {
                // impossible route inside one edge.
                this.HasSucceeded = true;
                return;
            }

            if (_source.EdgeId == _target.EdgeId)
            {
                // check for a path on the same edge.
                var edgePath = _source.EdgePathTo(_routerDb, _weightHandler, _sourceForward, _target, _targetForward);
                if (edgePath != null)
                {
                    this.Result = edgePath;
                    this.HasSucceeded = true;
                    return;
                }
            }

            // when either source or target is a vertex,
            // it's possible there are more one-hop paths.
            // check them all here.
            var sourceVertexId = _source.VertexId(_routerDb);
            var targetVertexId = _target.VertexId(_routerDb);
            if (sourceVertexId != Constants.NO_VERTEX &&
                targetVertexId != Constants.NO_VERTEX)
            {
                // check if any of the neighbouring edges match and can be traversed by the profile.
                if (sourceVertexId == targetVertexId)
                {
                    this.Result = new EdgePath<T>(sourceVertexId);
                    this.HasSucceeded = true;
                    return;
                }

                var sourceEnumerator = _routerDb.Network.GetEdgeEnumerator(sourceVertexId);
                while (sourceEnumerator.MoveNext())
                {
                    var sourceEdgeId = sourceEnumerator.Id;
                    var targetEnumerator = _routerDb.Network.GetEdgeEnumerator(targetVertexId);
                    while (targetEnumerator.MoveNext())
                    {
                        var targetEdgeId = targetEnumerator.Id;
                        if (sourceEdgeId != targetEdgeId) continue;

                        // there is a match!
                        var newSourceRouterPoint = sourceEnumerator.CreateRouterPoint(_routerDb);
                        var newTargetRouterPoint = targetEnumerator.CreateRouterPoint(_routerDb);

                        this.Result = newSourceRouterPoint.EdgePathTo(_routerDb, _weightHandler, _sourceForward,
                            newTargetRouterPoint, _targetForward);
                        this.HasSucceeded = true;
                        return;
                    }
                }
            }
            else if (sourceVertexId != Constants.NO_VERTEX)
            {
                // check if any of the source edges are a match with the target edge.
                var sourceEnumerator = _routerDb.Network.GetEdgeEnumerator(sourceVertexId);
                while (sourceEnumerator.MoveNext())
                {
                    var sourceEdgeId = sourceEnumerator.Id;
                    var targetEdgeId = _target.EdgeId;
                    if (sourceEdgeId != targetEdgeId) continue;

                    // there is a match!
                    var newSourceRouterPoint = sourceEnumerator.CreateRouterPoint(_routerDb);

                    this.Result = newSourceRouterPoint.EdgePathTo(_routerDb, _weightHandler, _sourceForward, _target,
                        _targetForward);
                    this.HasSucceeded = true;
                    return;
                }
            }
            else if (targetVertexId != Constants.NO_VERTEX)
            {
                // check if any of the target edges are a match with the source edge.
                var targetEnumerator = _routerDb.Network.GetEdgeEnumerator(targetVertexId);
                while (targetEnumerator.MoveNext())
                {
                    var sourceEdgeId = _source.EdgeId;
                    var targetEdgeId = targetEnumerator.Id;
                    if (sourceEdgeId != targetEdgeId) continue;

                    // there is a match!
                    var newTargetRouterPoint = targetEnumerator.CreateRouterPoint(_routerDb);

                    this.Result = _source.EdgePathTo(_routerDb, _weightHandler, _sourceForward, newTargetRouterPoint,
                        _targetForward);
                    this.HasSucceeded = true;
                    return;
                }
            }

            this.HasSucceeded = true;
        }

        /// <summary>
        /// Gets the resulting path, if any.
        /// </summary>
        public EdgePath<T> Result
        {
            get;
            private set;
        }
    }
}