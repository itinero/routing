// Itinero - OpenStreetMap (OSM) SDK
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
using System.Collections;
using System.Collections.Generic;

namespace Itinero.Algorithms.Collections
{
	/// <summary>
	/// Represents a vertex index.
	/// </summary>
	public class VertexIndex : IEnumerable<uint>
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="T:Itinero.VertexIndex"/> class.
		/// </summary>
		public VertexIndex()
		{
			
		}

		private HashSet<uint> _set;
		private SparseLongIndex _sparseIndex;

		/// <summary>
		/// Add the specified vertex.
		/// </summary>
		/// <param name="vertex">Vertex.</param>
		public void Add(uint vertex)
		{
			if (_set != null && _set.Count > 65536)
			{
				_sparseIndex = new SparseLongIndex();
				foreach (var v in _set)
				{
					_sparseIndex.Add(v);
				}
				_set = null;
			}

			if (_sparseIndex != null)
			{
				_sparseIndex.Add(vertex);
				return;
			}
			_set.Add(vertex);
		}

		/// <summary>
		/// Remove the specified vertex.
		/// </summary>
		/// <param name="vertex">Vertex.</param>
		public void Remove(uint vertex)
		{
			if (_set != null)
			{
				_set.Remove(vertex);
			}
			else
			{
				_sparseIndex.Remove(vertex);
			}
		}

		/// <summary>
		/// Contains the specified vertex.
		/// </summary>
		/// <param name="vertex">Vertex.</param>
		public bool Contains(uint vertex)
		{
			if (_set != null)
			{
				return _set.Contains(vertex);
			}
			else
			{
				return _sparseIndex.Contains(vertex);
			}
		}

		/// <summary>
		/// Clear this instance.
		/// </summary>
		public void Clear()
		{
			if (_set != null)
			{
				_set.Clear();
			}
			else
			{
				_sparseIndex.Clear();
			}
			_set = new HashSet<uint>();
			_sparseIndex = null;
		}

		public IEnumerator<uint> GetEnumerator()
		{
			throw new NotImplementedException();
		}

		IEnumerator IEnumerable.GetEnumerator()
		{
			throw new NotImplementedException();
		}

		/// <summary>
		/// Gets or sets the count.
		/// </summary>
		/// <value>The count.</value>
		public long Count
		{
			get;
			set;
		}
	}
}

