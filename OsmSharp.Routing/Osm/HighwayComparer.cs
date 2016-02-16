// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2016 Abelshausen Ben
// 
// This file is part of OsmSharp.
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
// along with OsmSharp. If not, see <http://www.gnu.org/licenses/>.

using OsmSharp.Routing.Attributes;
using OsmSharp.Routing.Osm.Vehicles;

namespace OsmSharp.Routing.Osm
{
    /// <summary>
    /// Compares OSM highways.
    /// </summary>
    public static class HighwayComparer
    {
        /// <summary>
        /// Compares two highway tag collections and check if they mean the same but when they have a oneway tags it's opposite.
        /// </summary>
        /// <returns></returns>
        public static bool CompareOpposite(AttributesIndex tags, uint tags1, uint tags2)
        {
            var tagsCollection1 = tags.Get(tags1);
            var tagsCollection2 = tags.Get(tags2);

            var oneway1 = Vehicle.Car.IsOneWay(tagsCollection1);
            var oneway2 = Vehicle.Car.IsOneWay(tagsCollection2);
            if(oneway1 != null && oneway2 != null &&
                oneway1.Value == oneway2.Value)
            { // both have values but not opposite ones.
                return false;
            }
            foreach (var tag1 in tagsCollection1)
            {
                if(tag1.Key != "oneway")
                {
                    if(!tagsCollection2.Contains(tag1.Key, tag1.Value))
                    {
                        return false;
                    }
                }
            }
            foreach (var tag2 in tagsCollection2)
            {
                if (tag2.Key != "oneway")
                {
                    if (!tagsCollection1.Contains(tag2.Key, tag2.Value))
                    {
                        return false;
                    }
                }
            }
            return true;
        }

        /// <summary>
        /// Compares two highway tag collections and check if they mean the same.
        /// </summary>
        /// <returns></returns>
        public static bool Compare(AttributesIndex tags, uint tags1, uint tags2)
        {
            var tagsCollection1 = tags.Get(tags1);
            var tagsCollection2 = tags.Get(tags2);

            var oneway1 = Vehicle.Car.IsOneWay(tagsCollection1);
            var oneway2 = Vehicle.Car.IsOneWay(tagsCollection2);
            if (oneway1 != null && oneway2 != null &&
                oneway1.Value != oneway2.Value)
            { // both have values but opposite ones.
                return false;
            }
            foreach (var tag1 in tagsCollection1)
            {
                if (tag1.Key != "oneway")
                {
                    if (!tagsCollection2.Contains(tag1.Key, tag1.Value))
                    {
                        return false;
                    }
                }
            }
            foreach (var tag2 in tagsCollection2)
            {
                if (tag2.Key != "oneway")
                {
                    if (!tagsCollection1.Contains(tag2.Key, tag2.Value))
                    {
                        return false;
                    }
                }
            }
            return true;
        }
    }
}