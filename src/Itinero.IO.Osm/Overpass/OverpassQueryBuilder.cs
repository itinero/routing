using System.Collections.Generic;
using System.Text;
using Itinero.LocalGeo;

namespace Itinero.IO.Osm.Overpass
{
    /// <summary>
    /// Builds overpass queries.
    /// </summary>
    public static class OverpassQueryBuilder
    {
        /// <summary>
        /// Builds a query based on a bounding box.
        /// </summary>
        /// <param name="box"></param>
        /// <returns></returns>
        public static string BuildQueryForBoundingBox(Box box)
        {
            return BuildQueryForPolygon(box.ToPolygon().ExteriorRing);
        }

        /// <summary>
        /// Builds a query based on a polygon.
        /// </summary>
        /// <param name="box"></param>
        /// <returns></returns>
        public static string BuildQueryForPolygon(Polygon polygon)
        {
            return BuildQueryForPolygon(polygon.ExteriorRing);
        }

        /// <summary>
        /// Builds a query based on the polygon represented by the given coordinates.
        /// </summary>
        public static string BuildQueryForPolygon(params Coordinate[] coordinates)
        {
            return BuildQueryForPolygon((IEnumerable<Coordinate>)coordinates);
        }

        /// <summary>
        /// Builds a query based on the polygon represented by the given coordinates.
        /// </summary>
        public static string BuildQueryForPolygon(IEnumerable<Coordinate> coordinates)
        {
            // build polygon string.
            var polygon = new StringBuilder();
            Coordinate? first = null;
            var last = default(Coordinate);
            foreach (var coordinate in coordinates)
            {
                if (first == null)
                {
                    first = coordinate;
                }
                else
                {
                    polygon.Append(' ');
                }
                last = coordinate;

                polygon.Append(coordinate.Latitude.ToInvariantString());
                polygon.Append(' ');
                polygon.Append(coordinate.Longitude.ToInvariantString());
            }

            if (first.Value.Latitude != last.Latitude ||
                first.Value.Longitude != last.Longitude)
            { // make sure polygon is closed.
                polygon.Append(first.Value.Latitude.ToInvariantString());
                polygon.Append(' ');
                polygon.Append(first.Value.Longitude.ToInvariantString());
            }

            // add polygon to query.
            var query = "<osm-script><union><query type=\"way\"><has-kv k=\"highway\"/><polygon-query bounds=\"{0}\"/></query><query type=\"relation\"><has-kv k=\"type=restriction\"/><polygon-query bounds=\"{1}\"/></query></union><print mode=\"body\"/><recurse type=\"down\"/><print mode=\"skeleton\"/></osm-script>";
            query = string.Format(query, polygon.ToString(), polygon.ToString());

            return query;
        }
    }
}