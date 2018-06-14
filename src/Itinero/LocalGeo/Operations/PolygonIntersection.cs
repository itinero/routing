using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using System.Security.Cryptography.X509Certificates;

namespace Itinero.LocalGeo.Operations
{
    public static class PolygonIntersection
    {
        /// <summary>
        /// Produces an intersection of the given two polygons.
        /// Polygons should be closed and saved in a clockwise fashion.
        ///
        /// For now, interior rings are note supported as well
        /// </summary>
        /// <param name="a"></param>
        /// <param name="b"></param>
        /// <returns></returns>
        internal static List<Polygon> Intersect(this Polygon a, Polygon b)
        {
            // yes, this is not a state of the art algorithm. 
            if (!a.IsClockwise() || !b.IsClockwise())
            {
                throw new InvalidOperationException(
                    "Polygons should be constructed in a clockwise fasion for intersections");
            }

            if (a.InteriorRings.Count != 0 || b.InteriorRings.Count != 0)
            {
                throw new InvalidOperationException("Polygons with holes are not supported for intersections");
            }


            var result = new List<Polygon>();


            // quick bbox test

            a.BoundingBox(out var na, out var ea, out var sa, out var wa);
            b.BoundingBox(out var nb, out var eb, out var sb, out var wb);

            if (na < sb || nb < sa || ea < wb || eb < wa)
            {
                // bounding boxes do not overlap - no overlap is possible at all
                return result;
            }


            // How are the intersecting polygons calculated?
            // Calculate the intersecting points, and the indices of the lines in each polygon
            // Take an interecting point, and start walking from it, following the line of polygon a
            // (This assumes polygons are saved clockwise)
            // When anohter intersection point is met, we swap to following polygon b
            // (we follow the clockwise direction again)

            // We continue this until we reach the starting point again, building a new polygon while we go
            // Plot twist: the newly built polygon should be clockwise as well. If not, it is a hole between the polygons


            var intersections = IntersectionsBetween(a.ExteriorRing, b.ExteriorRing);


            var aIntersections = new Dictionary<int, SortedList<int, Coordinate>>();
            var bIntersections = new Dictionary<int, SortedList<int, Coordinate>>();


            foreach (var intersection in intersections)
            {
                var i = intersection.Item1;
                var j = intersection.Item2;
                var coor = intersection.Item3;
                if (!aIntersections.ContainsKey(i))
                {
                    aIntersections[i] = new SortedList<int, Coordinate>();
                }

                if (!bIntersections.ContainsKey(j))
                {
                    bIntersections[j] = new SortedList<int, Coordinate>();
                }

                aIntersections[i][j] = coor;
                bIntersections[j][i] = coor;
            }

            foreach (var intersection in intersections)
            {
                var p = WalkIntersection(intersection, aIntersections, bIntersections, a, b);
                if (!p.IsClockwise()) continue; // The polygon is a hole between a & b
                
                // The polygon might also be the result of walking along the outer edges of both a & b
                // This is the case if one of the points is not in one of the polygons

                var allValid = true;
                foreach (var coordinate in p)
                {
                    var inBoth = a.PointIn(coordinate) && b.PointIn(coordinate);
                    if (!inBoth)
                    {
                        allValid = false;
                        break;
                    }
                }

                if (!allValid)
                {
                    // Not each border point is part of each polygon
                    // This is an outer line polygon
                    break;
                }
                
                result.Add(new Polygon() {ExteriorRing = p});
            }


            return result;
        }


        private static List<Coordinate> WalkIntersection(Tuple<int, int, Coordinate> Intersection,
            Dictionary<int, SortedList<int, Coordinate>> aIntersections,
            Dictionary<int, SortedList<int, Coordinate>> bIntersections,
            Polygon a, Polygon b)
        {
            var newPolygon = new List<Coordinate>();

            var curPolyIsA = true;


            var startI = Intersection.Item1;
            var startJ = Intersection.Item2;

            var i = startI;
            var j = startJ;
            do
            {
                if (curPolyIsA)
                {
                    var result = FollowAlong(newPolygon, a, i, j, aIntersections);

                    i = result.Item1;
                    j = result.Item2;

                    curPolyIsA = false;
                }
                else
                {
                    var result = FollowAlong(newPolygon, b, j, i, bIntersections);
                    j = result.Item1; // Beware: i and j are swapped here
                    i = result.Item2;

                    curPolyIsA = true;
                }
            } while (startI != i && startJ != j);


            return newPolygon;
        }

        /// <summary>
        /// Follows a polygon until another intersection is met
        /// </summary>
        /// <param name="a"></param>
        /// <param name="startIndex">The startindex of the line (element of poly A) which causes this intersection</param>
        /// <param name="intersection"></param>
        private static Tuple<int, int> FollowAlong(List<Coordinate> route, Polygon a, int i, int j,
            Dictionary<int, SortedList<int, Coordinate>> aIntersections)
        {
            // We start at given intersection point. This will be part of the route
            route.Add(aIntersections[i][j]);

            var neighbour = aIntersections[i].NeighbourOf(j);
            if (neighbour != j)
            {
                // This intersection is followed by another intersection
                // Return which line cause this intersection
                return Tuple.Create(i, j);
            }


            while (true)
            {
                // No intersection for the rest of this segment
                // The next point is the element following on the startIndex
                i++;

                var coor = a.ExteriorRing[i];
                route.Add(coor);


                // Does this new line i intersect?

                if (!aIntersections.ContainsKey(i)) continue;
                // no intersections. Just continue the loop until we get to one


                // yes, there are intersections
                // How do we figure out what intersection is the next we'll see?
                // As we come from a fresh start, we can only get to a 'fresh' intersection
                // Meaning that we can only get to the collision with the highest or lowest collision index
                // Funilly, as both polygons are both clockwise, the highest collision number can aways be taken ...
                // ... except if they both pass each other (e.g. two rectangulars)
                // So we simply take the one that is closest by

                var intersections = aIntersections[i];
                var j0 = intersections.Keys[0];
                var j1 = intersections.Keys[intersections.Count - 1];

                j = j0;

                var d0 = intersections[j0].DistanceInDegrees(coor);
                var d1 = intersections[j1].DistanceInDegrees(coor);

                if (d1 < d0)
                {
                    j = j1;
                }

                return Tuple.Create(i, j);
            }
        }


        internal static TKey NeighbourOf<TKey, TValue>(this SortedList<TKey, TValue> list, TKey k)
        {
            return list.Keys[(list.IndexOfKey(k) + 1) % list.Count];
        }


        /// <summary>
        /// Compares al lines from ring0 and ring2, gives a list of intersection points and segments back.
        /// Assumes closed rings
        /// </summary>
        /// <param name="ring0"></param>
        /// <param name="ring1"></param>
        /// <returns>A list of intersections, plus the lines which caused the intersection. Elements from ring0 will always be Item0, whereas elements from ring1 will always be Item1</returns>
        internal static List<Tuple<int, int, Coordinate>> IntersectionsBetween(List<Coordinate> ring0,
            List<Coordinate> ring1)
        {
            var intersects = new List<Tuple<int, int, Coordinate>>();
            for (var i = 0; i < ring0.Count - 1; i++)
            {
                var l0 = new Line(ring0[i], ring0[i + 1]);
                for (var j = 0; j < ring1.Count - 1; j++)
                {
                    var l1 = new Line(ring1[j], ring1[j + 1]);
                    var coor = l0.Intersect(l1);
                    if (coor == null) continue;

                    intersects.Add(Tuple.Create(i, j, (Coordinate) coor));
                }
            }

            return intersects;
        }
    }
}