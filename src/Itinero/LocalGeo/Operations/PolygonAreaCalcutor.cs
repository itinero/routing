using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("Itinero.Test")]

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
        internal static float SurfaceArea(this List<Coordinate> points)
        {
            var l = points.Count;
            var area = 0f;
            for (var i = 1; i < l+1; i++)
            {
                var p = points[i % l];
                var pi = points[(i + 1) % l];
                var pm = points[(i - 1)];
                area += p.Longitude * (pi.Latitude - pm.Latitude);
            }

            return Math.Abs(area / 2);
        }
    }
}