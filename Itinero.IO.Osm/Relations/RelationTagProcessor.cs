// Itinero - OpenStreetMap (OSM) SDK
// Copyright (C) 2016 Abelshausen Ben
// 
// This file is part of Itinero.
// 
// OsmSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// OsmSharp is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Itinero. If not, see <http://www.gnu.org/licenses/>.

using System;
using Itinero.IO.Osm.Streams;
using OsmSharp;
using OsmSharp.Tags;
using System.Collections.Generic;

namespace Itinero.IO.Osm.Relations
{
    /// <summary>
    /// A relation-tag processor that allows adding relation-tags to their member ways.
    /// </summary>
    public class RelationTagProcessor : ITwoPassProcessor
    {
        private readonly Action<Way, TagsCollectionBase> _addTags;
        private readonly Func<Relation, bool> _isRelevant;
        private readonly Dictionary<long, LinkedRelation> _linkedRelations; // keeps an index of way->relation relations.
        private readonly Dictionary<long, TagsCollectionBase> _relationTags; // keeps an index or relationId->tags.

        /// <summary>
        /// Creates a new relation tag processor.
        /// </summary>
        public RelationTagProcessor(Func<Relation, bool> isRelevant, Action<Way, TagsCollectionBase> addTags)
        {
            _addTags = addTags;
            _isRelevant = isRelevant;

            _linkedRelations = new Dictionary<long, LinkedRelation>();
            _relationTags = new Dictionary<long, TagsCollectionBase>();
        }

        /// <summary>
        /// Executes the first pass for relations.
        /// </summary>
        public void FirstPass(Relation relation)
        {
            if (_isRelevant(relation))
            {
                _relationTags[relation.Id.Value] = relation.Tags;

                if (relation.Members == null)
                {
                    return;
                }

                foreach(var member in relation.Members)
                {
                    if (member.Type == OsmGeoType.Way)
                    {
                        LinkedRelation linkedRelation = null;
                        _linkedRelations.TryGetValue(member.Id, out linkedRelation);
                        _linkedRelations[member.Id] = new LinkedRelation()
                        {
                            Next = linkedRelation,
                            RelationId = relation.Id.Value
                        };
                    }
                }
            }
        }

        /// <summary>
        /// Executes the first pass for ways.
        /// </summary>
        public void FirstPass(Way way)
        {

        }

        /// <summary>
        /// Executes the first pass for nodes.
        /// </summary>
        public void FirstPass(Node node)
        {

        }

        /// <summary>
        /// Executes the second pass for relations.
        /// </summary>
        public void SecondPass(Relation relation)
        {

        }

        /// <summary>
        /// Executes the second pass for ways.
        /// </summary>
        public void SecondPass(Way way)
        {
            LinkedRelation linkedRelation;
            if (_linkedRelations.TryGetValue(way.Id.Value, out linkedRelation))
            {
                while(linkedRelation != null)
                {
                    var tags = _relationTags[linkedRelation.RelationId];
                    _addTags(way, tags);

                    linkedRelation = linkedRelation.Next;
                }
            }
        }

        /// <summary>
        /// Executes the second pass for nodes.
        /// </summary>
        public void SecondPass(Node node)
        {

        }

        private class LinkedRelation
        {
            public long RelationId { get; set; }
            public LinkedRelation Next { get; set; }
        }
    }
}