using System.Collections.Generic;
using System.Text.RegularExpressions;
using OsmSharp.Collections.Tags;
using OsmSharp.Routing.Interpreter.Roads;
using OsmSharp.Routing.Vehicles;

namespace OsmSharp.Routing.Osm.Interpreter.Edge
{
    /// <summary>
    ///     Default edge interpreter.
    /// </summary>
    public class EdgeInterpreter : IEdgeInterpreter
    {
        /// <summary>
        ///     Returns true if the edge with the given tags is only accessible locally.
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        public bool IsOnlyLocalAccessible(TagsCollectionBase tags)
        {
            string tag;
            if (tags.TryGetValue("highway", out tag))
            {
                if (tag == "service")
                {
                    return true;
                }
            }
            if (tags.TryGetValue("access", out tag))
            {
                if (tag == "private" || tag == "official")
                {
                    return true;
                }
            }
            return false;
        }

        /// <summary>
        ///     Returns true if the edge with the given tags is routable.
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        public bool IsRoutable(TagsCollectionBase tags)
        {
            if (tags != null && tags.Count > 0)
            {
                return tags.ContainsKey("highway");
            }
            return false;
        }

        /// <summary>
        ///     Returns the name of a given way.
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        public string GetName(TagsCollectionBase tags)
        {
            var name = string.Empty;
            if (tags.ContainsKey("name"))
            {
                name = tags["name"];
            }
            return name;
        }

        /// <summary>
        ///     Returns all the names in all languages and alternatives.
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        public Dictionary<string, string> GetNamesInAllLanguages(TagsCollectionBase tags)
        {
            var names = new Dictionary<string, string>();
            //if (tags != null)
            //{
            //    foreach (var pair in tags)
            //    {
            //        var m = Regex.Match(pair.Key, "name:[a-zA-Z]");
            //        if (m.Success)
            //        {
            //            //throw new NotImplementedException();
            //        }
            //    }
            //}
            return names;
        }

        /// <summary>
        ///     Returns true if the edge with the given properties represents a roundabout.
        /// </summary>
        /// <param name="tags"></param>
        /// <returns></returns>
        public bool IsRoundabout(TagsCollectionBase tags)
        {
            string junction;
            return (tags != null && tags.TryGetValue("junction", out junction) && junction == "roundabout");
        }

        /// <summary>
        /// Returns true if the edge with given tags can be traversed by the given vehicle.
        /// </summary>
        /// <param name="tags"></param>
        /// <param name="vehicle"></param>
        /// <returns></returns>
        public bool CanBeTraversedBy(TagsCollectionBase tags, Vehicle vehicle)
        {
            return vehicle.CanTraverse(tags);
        }
    }
}