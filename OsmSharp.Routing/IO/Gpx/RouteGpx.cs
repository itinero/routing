// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2013 Abelshausen Ben
// 
// This file is part of OsmSharp.
// 
// OsmSharp is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// OsmSharp is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with OsmSharp. If not, see <http://www.gnu.org/licenses/>.

using System.Collections.Generic;
using System.IO;
using OsmSharp.IO.Xml.Gpx;
using OsmSharp.IO.Xml.Gpx.v1_1;
using OsmSharp.IO.Xml.Sources;

namespace OsmSharp.Routing.Gpx
{
    /// <summary>
    /// Converts an OsmSharpRoute into a gpx.
    /// </summary>
    internal class RouteGpx
    {
        /// <summary>
        /// Saves the route to a gpx file.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="route"></param>
        internal static void Save(Stream stream, Route route)
        {
            XmlStreamSource source = new XmlStreamSource(stream);
            GpxDocument output_document = new GpxDocument(source);
            gpxType output_gpx = new gpxType();
            output_gpx.trk = new trkType[1];

            // initialize all objects.
            List<wptType> segments = new List<wptType>();
            trkType track = new trkType();
            List<wptType> poi_gpx = new List<wptType>();

            track.trkseg = new trksegType[1];

            // ============= CONSTRUCT TRACK SEGMENT ==============
            trksegType track_segment = new trksegType();

            // loop over all points.
            for (int idx = 0; idx < route.Segments.Length; idx++)
            {
                // get the current entry.
                RouteSegment entry = route.Segments[idx];

                // ================== INITIALIZE A NEW SEGMENT IF NEEDED! ========
                wptType waypoint;
                if (entry.Points != null)
                { // loop over all points and create a waypoint for each.
                    for (int p_idx = 0; p_idx < entry.Points.Length; p_idx++)
                    {
                        RoutePoint point = entry.Points[p_idx];

                        waypoint = new wptType();
                        waypoint.lat = (decimal)point.Latitude;
                        waypoint.lon = (decimal)point.Longitude;
                        waypoint.name = point.Name;
                        poi_gpx.Add(waypoint);
                    }
                }

                // insert poi's.
                double longitde = entry.Longitude;
                double latitude = entry.Latitude;

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