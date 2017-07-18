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
    public abstract class RelationTagProcessor : ITwoPassProcessor
    {
        private readonly Dictionary<long, LinkedRelation> _linkedRelations; // keeps an index of way->relation relations.
        private readonly Dictionary<long, LinkedRelation> _linkedMemberRelations; // keeps an index of relation->relation relations.
        private readonly Dictionary<long, TagsCollectionBase> _relationTags; // keeps an index or relationId->tags.

        /// <summary>
        /// Creates a new relation tag processor.
        /// </summary>
        public RelationTagProcessor(bool processMemberRelations = false)
        {
            _linkedRelations = new Dictionary<long, LinkedRelation>();
            if (processMemberRelations)
            {
                _linkedMemberRelations = new Dictionary<long, LinkedRelation>();
            }
            _relationTags = new Dictionary<long, TagsCollectionBase>();
        }

        /// <summary>
        /// Returns true if the given relation is relevant.
        /// </summary>
        public abstract bool IsRelevant(Relation relation);

        /// <summary>
        /// Adds relation tags to the given way.
        /// </summary>
        public abstract void AddTags(Way way, TagsCollectionBase attributes);

        /// <summary>
        /// Gets or sets the action executed after normalization on the normalized collection and the original collection.
        /// </summary>
        public virtual Action<TagsCollectionBase, TagsCollectionBase> OnAfterWayTagsNormalize
        {
            get
            {
                return null;
            }
        }

        private HashSet<long> _relationsAsMembers = new HashSet<long>();
        private HashSet<long> _relations = new HashSet<long>();

        /// <summary>
        /// Executes the first pass for relations.
        /// </summary>
        public bool FirstPass(Relation relation)
        {
            if (relation.Id.Value == 3147287)
            {
                System.Diagnostics.Debug.WriteLine(string.Empty);
            }
            if (_relationsAsMembers.Contains(relation.Id.Value) ||
                IsRelevant(relation))
            {
                if (_relations.Contains(relation.Id.Value))
                {
                    return false;
                }
                _relations.Add(relation.Id.Value);
                _relationsAsMembers.Remove(relation.Id.Value);

                var tags = relation.Tags;
                LinkedRelation linkedRelation;
                if (_linkedMemberRelations != null &&
                    _linkedMemberRelations.TryGetValue(relation.Id.Value, out linkedRelation))
                {
                    while(linkedRelation != null)
                    {
                        var parentTags = _relationTags[linkedRelation.RelationId];

                        foreach(var parentTag in parentTags)
                        {
                            tags.AddOrAppend(parentTag);
                        }

                        linkedRelation = linkedRelation.Next;
                    }
                }

                _relationTags[relation.Id.Value] = relation.Tags;
                
                if (relation.Members == null)
                {
                    return false;
                }

                bool oneMorePass = false;
                foreach(var member in relation.Members)
                {
                    if (member.Type == OsmGeoType.Way)
                    {
                        _linkedRelations.TryGetValue(member.Id, out linkedRelation);
                        _linkedRelations[member.Id] = new LinkedRelation()
                        {
                            Next = linkedRelation,
                            RelationId = relation.Id.Value
                        };
                    }
                    else if (member.Type == OsmGeoType.Relation)
                    {
                        if (_linkedMemberRelations != null)
                        { // only do this when requested.
                            oneMorePass = true;
                            _linkedMemberRelations.TryGetValue(member.Id, out linkedRelation);
                            _linkedMemberRelations[member.Id] = new LinkedRelation()
                            {
                                Next = linkedRelation,
                                RelationId = relation.Id.Value
                            };
                            _relations.Remove(member.Id);
                        }
                    }
                }
                return oneMorePass;
            }
            return false;
        }

        /// <summary>
        /// Executes the first pass for ways.
        /// </summary>
        public void FirstPass(Way way)
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
                    this.AddTags(way, tags);

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