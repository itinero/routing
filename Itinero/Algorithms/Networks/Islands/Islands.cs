using System;
using System.Collections;
using System.Collections.Generic;
using Itinero.Algorithms.Collections;

namespace Itinero
{
	/// <summary>
	/// Represents islands in a network.
	/// </summary>
	public class Islands
	{
		private readonly VertexIndex _singletons; // vertices that are islands of one.
		private readonly List<VertexIndex> _islands; // all the other islands.

		public Islands()
		{
			
		}
	}

	/// <summary>
	/// Represents one island in a network.
	/// </summary>
	public class Island : IEnumerable<uint>
	{
		private readonly uint _singleton = Constants.NO_VERTEX;
		private readonly VertexIndex _vertices;

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Itinero.Island"/> class.
		/// </summary>
		/// <param name="vertex">Vertex.</param>
		public Island(uint vertex)
		{
			_singleton = vertex;
			_vertices = null;
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="T:Itinero.Island"/> class.
		/// </summary>
		/// <param name="vertices">Vertices.</param>
		public Island(VertexIndex vertices)
		{
			_singleton = Constants.NO_VERTEX;
			_vertices = vertices;
		}

		/// <summary>
		/// Returns true if the given vertex is in this island.
		/// </summary>
		/// <returns>The vertex.</returns>
		/// <param name="vertex">Vertex.</param>
		public bool HasVertex(uint vertex)
		{
			if (_vertices != null)
			{
				return _vertices.Contains(vertex);
			}
			return _singleton == vertex;
		}

		public IEnumerator<uint> GetEnumerator()
		{
			if (_vertices != null)
			{
				return _vertices.GetEnumerator();
			}
			return new SingletonEnumerator(_singleton);
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			return this.GetEnumerator();
		}

		private struct SingletonEnumerator : IEnumerator<uint>
		{
			private readonly uint _singleton;

			public SingletonEnumerator(uint singleton)
			{
				_singleton = singleton;
				_current = false;
			}

			private bool _current;

			public uint Current
			{
				get
				{
					if (_current)
					{
						return _singleton;
					}
					throw new InvalidOperationException();
				}
			}

			object IEnumerator.Current
			{
				get
				{
					return this.Current;
				}
			}

			public void Dispose()
			{
				
			}

			public bool MoveNext()
			{
				if (!_current)
				{
					_current = true;
					return true;
				}
				return false;
			}

			public void Reset()
			{
				_current = false;
			}
		}
	}
}