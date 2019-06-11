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
        /// Calculates the surface area of a closed, not-self-intersecting polygon.
        /// Will return a negative result if the polygon is counterclockwise
        /// </summary>
        /// <param name="points"></param>
        /// <returns></returns>
        public static float SignedSurfaceArea(this List<Coordinate> points)
        {
            var l = points.Count;
            var area = 0f;
            for (var i = 1; i < l+1; i++)
            {
                var cur = points[i % l];
                var nxt = points[(i + 1) % l];
                var prev = points[(i - 1)];
                area += cur.Longitude * (prev.Latitude - nxt.Latitude);
            }

            return area / 2;
        }

        public static float SurfaceArea(this List<Coordinate> points)
        {
            return Math.Abs(points.SignedSurfaceArea());
        }
        
    }
}