using System.Collections.Generic;
using System.IO;
using Itinero.LocalGeo;
using NetTopologySuite.Features;
using NetTopologySuite.Geometries;
using Newtonsoft.Json;

namespace Itinero.Test.Functional.Staging
{
    public static class StagingHelper
    {
        public static Itinero.LocalGeo.Coordinate[] GetLocations(string geojsonFile)
        {
            var features = GetFeatureCollection(geojsonFile);

            var locations = new List<Itinero.LocalGeo.Coordinate>();
            foreach (var feature in features)
            {
                if (feature.Geometry is Point p)
                {
                    locations.Add(new Itinero.LocalGeo.Coordinate((float)p.Coordinate.Y, (float)p.Coordinate.X));
                }
            }

            return locations.ToArray();
        }
        
        /// <summary>
        /// Gets a feature collection for the given embedded resource.
        /// </summary>
        public static FeatureCollection GetFeatureCollection(string geojsonFile)
        {
            using (var stream = File.OpenRead(geojsonFile))
            using (var streamReader = new StreamReader(stream))
            {
                var json = streamReader.ReadToEnd();
                return json.ToFeatures();
            }
        }

        /// <summary>
        /// Converts this geojson into a feature collection.
        /// </summary>
        public static FeatureCollection ToFeatures(this string geoJson)
        {
            using (var stream = new JsonTextReader(new StringReader(geoJson)))
            {
                var geoJsonSerializer = NetTopologySuite.IO.GeoJsonSerializer.Create();
                var obj = geoJsonSerializer.Deserialize<FeatureCollection>(stream);
                return obj;
            }
        }
    }
}