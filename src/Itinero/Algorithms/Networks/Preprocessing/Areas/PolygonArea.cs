using System.Linq;
using Itinero.LocalGeo;

namespace Itinero.Algorithms.Networks.Preprocessing.Areas
{
    /// <summary>
    /// An area implemenation based on a simple polygon.
    /// </summary>
    public class PolygonArea : IArea
    {
        private readonly Polygon _polygon;
        private readonly Box _box;

        /// <summary>
        /// Creates a new polygon area.
        /// </summary>
        /// <param name="polygon">The polygon</param>
        public PolygonArea(Polygon polygon)
        {
            _polygon = polygon;

            _polygon.BoundingBox(out float north, out float east, out float south, out float west);
            _box = new Box(north, west, south, east);
        }

        /// <summary>
        /// Returns the location(s) the given line intersects with the area's boundary. Returns null if there is no intersection.
        /// </summary>
        public Coordinate[] Intersect(float latitude1, float longitude1, float latitude2, float longitude2)
        {
            var box = new Box(latitude1, longitude1, latitude2, longitude2);
            if (!box.Overlaps(_box))
            {
                return null;
            }

            // intersect with polyon.
            return _polygon.Intersect(latitude1, latitude2, longitude1, longitude2).ToArray();
        }

        /// <summary>
        /// Returns true if the given coordinate is inside the area.
        /// </summary>
        public bool Overlaps(float latitude, float longitude)
        {
            return _polygon.PointIn(new Coordinate(latitude, longitude));
        }
    }
}