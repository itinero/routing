using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Globalization;

namespace OsmSharp.Routing.Interpreter.Roads
{
    /// <summary>
    /// Contains common tags extensions for usage in EdgeInterpreters.
    /// </summary>
    public static class TagsExtensions
    {
        /// <summary>
        /// Returns a numeric value from a tags collection.
        /// </summary>
        /// <param name="tags"></param>
        /// <param name="key"></param>
        /// <returns></returns>
        /// <remarks>Uses the CultureInfo.InvariantCulture property to parse the data.</remarks>
        public static double? GetNumericValue(this IDictionary<string, string> tags,
            string key)
        {
            if (tags != null)
            {
                string value;
                if (tags.TryGetValue(key, out value))
                { // the value is there.
                    double value_numeric;
                    if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out value_numeric))
                    {
                        return value_numeric;
                    }
                }
            }
            return null;
        }
    }
}
