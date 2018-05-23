using System.Collections.Generic;
using System.IO;
using Itinero.Algorithms.Default;
using Itinero.LocalGeo;
using NUnit.Framework;

namespace Itinero.Test.Algorithms.Default
{
    public class ConvextHullTests
    {
        public ConvexHull CreateTestSet0()
        {
            // een erg braaf test-setje
            var lons = new[] {0f, 1f, 1f, 0f, 0.6f};
            var lats = new[] {0f, 1f, 0f, 1f, 0.5f};

            return new ConvexHull(lats, lons);
        }

        public List<Coordinate> TestPoints()
        {
            var lons = new[] {0f, 1f, 1f, 0f, 0.6f};
            var lats = new[] {0f, 1f, 0f, 1f, 0.5f};
            var coors = new List<Coordinate>();
            for (var i = 0; i < lons.Length; i++)
            {
                coors.Add(new Coordinate(lats[i], lons[i]));
            }

            return coors;
        }

        [Test]
        public void TestSwap()
        {
            var arr = new[] {0, 1, 2, 3, 4};
            ConvexHull.Swap(arr, 1, 3);
            Assert.AreEqual(arr, new[] {0, 3, 2, 1, 4});


            arr = new[] {0, 1, 9, 8, 2, 3, 7, 6};
            ConvexHull.Merge(arr, 2, 4, 6);
            Assert.AreEqual(arr, new[] {0, 1, 2, 3, 9, 8, 7, 6});
        }


        [Test]
        public void TestSimple()
        {
            var cv0 = CreateTestSet0();
            cv0.CalculateMinMaxX(out var min, out var max);

            Assert.AreEqual(min, 0);
            Assert.AreEqual(max, 1);

            var cutoff = cv0.PartitionLeftRight(0, 1, 2, 4);
            var pts = cv0.Points;
            Log("" + cutoff + "\n");
            foreach (var f in pts)
            {
                Log(f + " ");
            }

            Log("\n");

            // In the right partition, search longest
            var furthestInd = cv0.LongestDistance(0, 1, cutoff, 5);
            Log("Furhtest: > " + cv0.Points[furthestInd] + "(index " + furthestInd + ")\n");

            Assert.AreEqual(furthestInd, 3);
            Assert.AreEqual(pts[furthestInd], 2);

            // Line = pt0 and pt1; start-index = 2 (ignore first two = pt0;pt1); end index = 3 (only consider
            cutoff = cv0.PartitionInTriangle(0, 1, 2, 2, 4);
            Log("" + cutoff + "\n");
            foreach (var f in pts)
            {
                Log(f + " ");
            }

            Log("\n");


            cutoff = cv0.FindHull(0, 1, 2, 5);
            Log("\nFindhull> " + cutoff + "\n");
            foreach (var f in pts)
            {
                Log(f + " ");
            }

            Log("\n");
            cv0 = CreateTestSet0();
            cutoff = cv0.Quickhull();

            Log("\n\n> " + cutoff + "\n");
            foreach (var f in cv0.Points)
            {
                Log(f + " ");
            }

            Log("\n");

            Assert.AreEqual(new[] {0, 3, 1, 2, 4}, cv0.Points);
            Assert.AreEqual(cutoff, 4);
            Log("Done");
        }


        [Test]
        public void TestUpdate()
        {
            var tp = TestPoints();
            var hull = ConvexHull.Quickhull(tp);

            for (int i = 0; i < 5; i++)
            {
                // Adding already known points shouldn't have any effect
                var ind = ConvexHull.UpdateHull(hull, tp[i]);
                Assert.AreEqual(-1, ind);
            }


            Log("\n");
            foreach (var pt in hull)
            {
                Log(pt.ToString()+"    ");
            }
            Log("\n");

            var index = ConvexHull.UpdateHull(hull, new Coordinate(0.5f, -0.5f));

            Log("\n");
            foreach (var pt in hull)
            {
                Log(pt.ToString()+"    ");
            }
            Log("\n");
            Assert.AreEqual(1, index);
            
            
            index = ConvexHull.UpdateHull(hull, new Coordinate(2f, 2f));

            Assert.AreEqual(3, index);
            Log("DONE");
            
        }


        private static void Log(string msg)
        {
            File.AppendAllText("/home/pietervdvn/Desktop/log.txt", msg);
        }

        private static void Clearlog()
        {
            File.WriteAllText("/home/pietervdvn/Desktop/log.txt", "");
        }
    }
}