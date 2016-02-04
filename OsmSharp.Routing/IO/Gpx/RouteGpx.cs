using OsmSharp.IO.Xml.Gpx;
using OsmSharp.IO.Xml.Gpx.v1_1;
using OsmSharp.IO.Xml.Sources;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OsmSharp.Routing.IO.Gpx
{   /// <summary>
    /// Converts an route into a gpx.
    /// </summary>
    internal static class RouteGpx
    {
        /// <summary>
        /// Saves the route to a gpx file.
        /// </summary>
        internal static void Save(Stream stream, Route route)
        {
            var source = new XmlStreamSource(stream);
            var output_document = new GpxDocument(source);
            var output_gpx = new gpxType();
            output_gpx.trk = new trkType[1];

            // initialize all objects.
            var segments = new List<wptType>();
            var track = new trkType();
            var poi_gpx = new List<wptType>();

            track.trkseg = new trksegType[1];
            
            trksegType track_segment = new trksegType();
            for (int idx = 0; idx < route.Segments.Count; idx++)
            {
                var entry = route.Segments[idx];
                
                wptType waypoint;
                if (entry.Points != null)
                { // loop over all points and create a waypoint for each.
                    for (int p_idx = 0; p_idx < entry.Points.Length; p_idx++)
                    {
                        var point = entry.Points[p_idx];

                        var nameTag = point.Tags == null ? null : 
                            point.Tags.FirstOrDefault(x => x.Value == "name");

                        waypoint = new wptType();
                        waypoint.lat = (decimal)point.Latitude;
                        waypoint.lon = (decimal)point.Longitude;
                        waypoint.name = nameTag == null ? string.Empty : nameTag.Value;
                        poi_gpx.Add(waypoint);
                    }
                }

                waypoint = new wptType();
                waypoint.lat = (decimal)entry.Latitude;
                waypoint.lon = (decimal)entry.Longitude;

                segments.Add(waypoint);
            }

            // put the segment in the track.
            track_segment.trkpt = segments.ToArray();
            track.trkseg[0] = track_segment;

            // set the track to the output.
            output_gpx.trk[0] = track;
            output_gpx.wpt = poi_gpx.ToArray();

            // save the ouput.
            output_document.Gpx = output_gpx;
            output_document.Save();
        }
    }
}
