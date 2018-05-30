using System.Collections.Generic;

namespace Itinero.LocalGeo.Operations
{
    /// <summary>
    ///  Calculates the surface areas of polygons
    /// </summary>
    internal static class PolygonAreaCalcutor
    {
        /// <summary>
        /// Calculates the surface area of a closed, not-self-intersecting polygon
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        internal static float surfaceOf(this List<Coordinate> points)
        {
            var l = points.Count;
            var area = 0f;
            for (var i = 1; i < l+1; i++)
            {
                var p = points[i];
                var pi = points[(i + 1) % l];
                var pm = points[(i - 1)];
                area += p.Longitude * (pi.Latitude - pm.Latitude);
            }

            return area / 2;
        }
    }
}