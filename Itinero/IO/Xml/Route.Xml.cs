// Itinero - OpenStreetMap (OSM) SDK
// Copyright (C) 2016 Abelshausen Ben
// 
// This file is part of Itinero.
// 
// Itinero is free software: you can redistribute it and/or modify
// it under the terms of the GNU General Public License as published by
// the Free Software Foundation, either version 2 of the License, or
// (at your option) any later version.
// 
// Itinero is distributed in the hope that it will be useful,
// but WITHOUT ANY WARRANTY; without even the implied warranty of
// MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE. See the
// GNU General Public License for more details.
// 
// You should have received a copy of the GNU General Public License
// along with Itinero. If not, see <http://www.gnu.org/licenses/>.

using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Itinero.IO.Xml;
using Itinero.Geo;
using System.Collections.Generic;
using System;

namespace Itinero
{
    /// <summary>
    /// Represents a route.
    /// </summary>
    [XmlRoot("route")]
    public partial class Route : IXmlSerializable
    {
        XmlSchema IXmlSerializable.GetSchema()
        {
            return null;
        }

        void IXmlSerializable.ReadXml(XmlReader reader)
        {

        }

        void IXmlSerializable.WriteXml(XmlWriter writer)
        {
            if (this.Shape != null)
            {
                writer.WriteStartElement("shape");

                for (var i = 0; i < this.Shape.Length; i++)
                {
                    writer.WriteStartElement("c");
                    writer.WriteAttribute("lat", this.Shape[i].Latitude);
                    writer.WriteAttribute("lon", this.Shape[i].Longitude);
                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
            }

            if (this.ShapeMeta != null)
            {
                writer.WriteStartElement("metas");

                for (var i = 0; i < this.ShapeMeta.Length; i++)
                {
                    var meta = this.ShapeMeta[i];

                    writer.WriteStartElement("meta");
                    writer.WriteAttributeString("shape", meta.Shape.ToInvariantString());

                    if (meta.Attributes != null)
                    {
                        foreach(var attribute in meta.Attributes)
                        {
                            writer.WriteStartElement("property");
                            writer.WriteAttributeString("k", attribute.Key);
                            writer.WriteAttributeString("v", attribute.Value);
                            writer.WriteEndElement();
                        }
                    }

                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
            }

            if (this.Branches != null)
            {
                writer.WriteStartElement("branches");

                for (var i = 0; i < this.Branches.Length; i++)
                {
                    var branch = this.Branches[i];

                    writer.WriteStartElement("branch");
                    writer.WriteAttributeString("shape", branch.Shape.ToInvariantString());

                    if (branch.Attributes != null)
                    {
                        foreach (var attribute in branch.Attributes)
                        {
                            writer.WriteStartElement("property");
                            writer.WriteAttributeString("k", attribute.Key);
                            writer.WriteAttributeString("v", attribute.Value);
                            writer.WriteEndElement();
                        }
                    }

                    writer.WriteEndElement();
                }

                writer.WriteEndElement();
            }

            if (this.Stops != null)
            {
                writer.WriteStartElement("stops");
                for (var i = 0; i < this.Stops.Length; i++)
                {
                    var stop = this.Stops[i];

                    writer.WriteStartElement("stop");

                    writer.WriteAttributeString("shape", stop.Shape.ToInvariantString());
                    writer.WriteAttribute("lat", stop.Coordinate.Latitude);
                    writer.WriteAttribute("lon", stop.Coordinate.Longitude);

                    if (stop.Attributes != null)
                    {
                        foreach (var attribute in stop.Attributes)
                        {
                            writer.WriteStartElement("property");
                            writer.WriteAttributeString("k", attribute.Key);
                            writer.WriteAttributeString("v", attribute.Value);
                            writer.WriteEndElement();
                        }
                    }

                    writer.WriteEndElement();
                }
                writer.WriteEndElement();
            }
        }
    }
}