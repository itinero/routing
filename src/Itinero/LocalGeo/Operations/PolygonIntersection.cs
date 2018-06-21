using System;
using System.Collections.Generic;
using System.Runtime.InteropServices.ComTypes;
using Itinero.Algorithms.Contracted.EdgeBased.Witness;

namespace Itinero.LocalGeo.Operations
{
    public static class PolygonIntersection
    {
        ///  <summary>
        ///  Produces an intersection of the given two polygons.
        ///  Polygons should be closed and saved in a clockwise fashion.
        /// 
        ///  For now, interior rings are note supported as well
        ///  </summary>
        ///  <param name="a">The first polygon</param>
        /// <param name="b">The second polygon</param>
        /// <param name="union">The out parameter, which will contain the union ring afterwards. Might be null</param>
        /// <param name="differences">Set to true if you want the differences between the polygons instead of the intersecion</param>
        internal static List<Polygon> Intersect(this Polygon a, Polygon b, out Polygon union, bool differences = false)
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

            if (differences)
            {
                // Walking is suddenly inversed
                a.ExteriorRing.Reverse();
            }

            var result = new List<Polygon>();
            union = null;

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

            PrepareIntersectionMatrix(intersections, out var aIntersections, out var bIntersections);


            // Bounding box of polygons a | b
            var n = Math.Max(na, nb);
            var e = Math.Max(ea, eb);
            var s = Math.Min(sa, sb);
            var w = Math.Min(wa, wb);

            // We keep track of the northern-most point of each polygon
            // This is a trick to keep out duplicate polygons
            var excludedPoints = new HashSet<Coordinate>();

            foreach (var intersection in intersections)
            {
                var p = WalkIntersection(intersection, aIntersections, bIntersections, a, b);
                if (!p.IsClockwise()) continue; // The polygon is a hole between a & b

                // One polygon will also be the result of walking along the outer edges of both a & b
                // In other words, the union of the polygons
                // We just take the bbox of the polygon. If this polygon encompasses both other polygons, we're pretty sure its the union
                p.BoundingBox(out var pn, out var pe, out var ps, out var pw);
                var poly = new Polygon() {ExteriorRing = p};
                if (pn == n && pe == e && ps == s && pw == w)
                {
                    // This is the union polygon. 
                    union = poly;
                }
                else
                {
                    var northernMost = poly.NorthernMost();
                    if (excludedPoints.Contains(northernMost))
                    {
                        // This polygon has already been discovered previously
                        continue;
                    }
                    excludedPoints.Add(northernMost);
                    result.Add(poly);
                }
            }

           return result;
        }

        // TODO Make internal
        public static void PrepareIntersectionMatrix(List<Tuple<int, int, Coordinate>> intersections,
            out Dictionary<int, SortedList<int, Coordinate>> aIntersections,
            out Dictionary<int, SortedList<int, Coordinate>> bIntersections)
        {
            aIntersections = new Dictionary<int, SortedList<int, Coordinate>>();
            bIntersections = new Dictionary<int, SortedList<int, Coordinate>>();


            // Preprocessing of the intersections: build an easily accessible data structure, bit of a sparse matrix
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
        }


        public static List<Coordinate> WalkIntersection(Tuple<int, int, Coordinate> intersection,
            Dictionary<int, SortedList<int, Coordinate>> aIntersections,
            Dictionary<int, SortedList<int, Coordinate>> bIntersections,
            Polygon a, Polygon b)
        {
            var newPolygon = new List<Coordinate>();

            var curPolyIsA = true;


            var startI = intersection.Item1;
            var startJ = intersection.Item2;

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
            } while (!(startI == i && startJ == j));

            if (!newPolygon[0].Equals(newPolygon[newPolygon.Count - 1]))
            {
                newPolygon.Add(newPolygon[0]);
            }
            
            return newPolygon;
        }

        /// <summary>
        /// Follows a polygon until another intersection is met
        /// </summary>
        /// <param name="i">The segment causing the intersection in the passed polygon (in the passed matrix)</param>
        /// <param name="j">The segment causing the intersection in the other polygon</param>
        public static Tuple<int, int> FollowAlong(List<Coordinate> route, Polygon a, int i, int j,
            Dictionary<int, SortedList<int, Coordinate>> aIntersections)
        {
            if (i == 0 && j == 1)
            {
                i = i;
            }

            // We start at given intersection point. This will be part of the route
            route.Add(aIntersections[i][j]);
            
            
            // Then, we check if the intersection point we landed on is the only intersection
            if (aIntersections[i].Keys.Count != 1)
            {
                // Multiple intersections on the segment we have to walk
                // We'd better calculate the direction in which we have to go
                // We can't really make assumptions about clockwise/counterclockwise
                // So: we take the point to which we are walking from the polygon
                // And have a look to next/previous neighbour and their distances
                var walkingTo = (i + 1) % a.ExteriorRing.Count;
                var walkingToCoor = a.ExteriorRing[walkingTo];


                var inters = aIntersections[i];
                // We get both neighbours...
                var n1 = inters.NeighbourOf(j);
                var n2 = inters.PrevNeighbourOf(j);

                var d1 = walkingToCoor.DistanceInDegrees(inters[n1]);
                var d2 = walkingToCoor.DistanceInDegrees(inters[n2]);
                
                var n = n1;
                var d = d1;
                // ...and we get the neighbour closest to the edge point
                if (d2 < d1)
                {
                    n = n2;
                    d = d2;
                }
                
                
                // We have found the closest neighbour
                // One special edge case remains: what if the current intersection point (i,j) is closer to the point we are walking to then the closest neighbour?
                // For this, we check that the new intersection point is closer to our distance
                if (d < walkingToCoor.DistanceInDegrees(inters[j]))
                {
                    return Tuple.Create (i, n);
                }
                // If not, we just continue with walking along our polygon... (The loop below, out of the if)
            }
            


            while (true)
            {
                // No intersection for the rest of this segment
                // The next point is the element following on the startIndex
                i = (i + 1) % (a.ExteriorRing.Count - 1); // -1 to avoid the point closing the element

                var coor = a.ExteriorRing[i];
                route.Add(coor);


                // Does this new line i intersect?

                // if no intersections. Just continue the loop until we get to one
                if (!aIntersections.ContainsKey(i)) continue;


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

        internal static TKey PrevNeighbourOf<TKey, TValue>(this SortedList<TKey, TValue> list, TKey k)
        {
            var key = list.IndexOfKey(k) - 1;
            if (key < 0)
            {
                key = list.Keys.Count - 1;
            }

            return list.Keys[key];
        }


        /// <summary>
        /// Compares al lines from ring0 and ring2, gives a list of intersection points and segments back.
        /// Assumes closed rings
        /// </summary>
        /// <param name="ring0"></param>
        /// <param name="ring1"></param>
        /// <returns>A list of intersections, plus the lines which caused the intersection. Elements from ring0 will always be Item0, whereas elements from ring1 will always be Item1</returns>
        public static List<Tuple<int, int, Coordinate>> IntersectionsBetween(List<Coordinate> ring0,
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