using NetTopologySuite.Features;
using System.IO;

namespace Itinero.Test.Functional
{
    public static class Extensions
    {
        public static string ToGeoJson(this FeatureCollection featureCollection)
        {
            var jsonSerializer = new NetTopologySuite.IO.GeoJsonSerializer();
            var jsonStream = new StringWriter();
            jsonSerializer.Serialize(jsonStream, featureCollection);
            var json = jsonStream.ToInvariantString();
            return json;
        }
    }
}
