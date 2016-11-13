//// Itinero - Routing for .NET
//// Copyright (C) 2016 Abelshausen Ben
//// 
//// This file is part of Itinero.
//// 
//// OsmSharp is free software: you can redistribute it and/or modify
//// it under the terms of the GNU General Public License as published by
//// the Free Software Foundation, either version 2 of the License, or
//// (at your option) any later version.
//// 
//// OsmSharp is distributed in the hope that it will be useful,
//// but WITHOUT ANY WARRANTY; without even the implied warranty of
//// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
//// GNU General Public License for more details.
//// 
//// You should have received a copy of the GNU General Public License
//// along with Itinero. If not, see <http://www.gnu.org/licenses/>.

//using OsmSharp;
//using OsmSharp.Tags;
//using System;

//namespace Itinero.IO.Osm.Relations
//{
//    /// <summary>
//    /// A cycle network processor. Adds an extra tag to way-members of a cycle network.
//    /// </summary>
//    public class CycleNetworkProcessor : RelationTagProcessor
//    {
//        /// <summary>
//        /// Creates a new cycle network processor.
//        /// </summary>
//        public CycleNetworkProcessor()
//            : base(IsRelevant, AddTags)
//        {

//        }

//        static Func<Relation, bool> IsRelevant = (r) =>
//        {
//            return r.Tags != null && 
//                (r.Tags.Contains("type", "route") &&
//                 r.Tags.Contains("route", "bicycle"));
//        };

//        static Action<Way, TagsCollectionBase> AddTags = (w, t) =>
//        {
//            if (w.Tags == null)
//            {
//                return;
//            }
//            w.Tags.AddOrReplace("cyclenetwork", "yes");
//        };
        
//        /// <summary>
//        /// Gets or sets the action executed after normalization on the normalized collection and the original collection.
//        /// </summary>
//        public override Action<TagsCollectionBase, TagsCollectionBase> OnAfterWayTagsNormalize
//        {
//            get
//            {
//                return (after, before) =>
//                {
//                    if (after == null || before == null)
//                    {
//                        return;
//                    }
//                    if (before.Contains("cyclenetwork", "yes"))
//                    {
//                        after.AddOrReplace("cyclenetwork", "yes");
//                    }
//                };
//            }
//        }
//    }
//}
