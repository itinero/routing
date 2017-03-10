/*
 *  Licensed to SharpSoftware under one or more contributor
 *  license agreements. See the NOTICE file distributed with this work for 
 *  additional information regarding copyright ownership.
 * 
 *  SharpSoftware licenses this file to you under the Apache License, 
 *  Version 2.0 (the "License"); you may not use this file except in 
 *  compliance with the License. You may obtain a copy of the License at
 * 
 *       http://www.apache.org/licenses/LICENSE-2.0
 * 
 *  Unless required by applicable law or agreed to in writing, software
 *  distributed under the License is distributed on an "AS IS" BASIS,
 *  WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
 *  See the License for the specific language governing permissions and
 *  limitations under the License.
 */

using System.Xml;
using System.Xml.Schema;
using System.Xml.Serialization;
using Itinero.IO.Xml;
using Itinero.LocalGeo;

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