// OsmSharp - OpenStreetMap (OSM) SDK
// Copyright (C) 2015 Abelshausen Ben
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

using OsmSharp.Collections.Tags;
using OsmSharp.Collections.Tags.Serializer;
using OsmSharp.IO;
using OsmSharp.Routing.Vehicles;
using ProtoBuf.Meta;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OsmSharp.Routing.Graph.Serialization
{
    /// <summary>
    /// Abstract representation of a routing data source serializer.
    /// </summary>
    /// <remarks>Versioning is implemented in the file format to guarantee backward compatibility.</remarks>
    public abstract class RoutingDataSourceSerializerBase<TEdgeData>
        where TEdgeData : struct, IGraphEdgeData
    {
        #region Versioning

        /// <summary>
        /// Returns the version number.
        /// </summary>
        public abstract string VersionString { get; }

        /// <summary>
        /// Builds a uniform version header.
        /// </summary>
        /// <returns></returns>
        private byte[] BuildVersionHeader()
        {
            return System.Text.UTF8Encoding.UTF8.GetBytes(this.VersionString);
        }

        /// <summary>
        /// Writes the version header.
        /// </summary>
        private void WriteVersionHeader(Stream stream)
        {
            // seek to the beginning of the stream.
            stream.Seek(0, SeekOrigin.Begin);

            // write the header bytes.
            var header = this.BuildVersionHeader();
            stream.Write(header, 0, header.Length);
        }

        /// <summary>
        /// Reads and validates the header.
        /// </summary>
        /// <param name="stream"></param>
        private bool ReadAndValidateHeader(Stream stream)
        {
            // get the original version header.
            var header = this.BuildVersionHeader();

            try
            {
                // get the version string.
                var presentHeader = new byte[header.Length];
                stream.Seek(0, SeekOrigin.Begin);
                stream.Read(presentHeader, 0, header.Length);

                for (int idx = 0; idx < header.Length; idx++)
                {
                    if (header[idx] != presentHeader[idx])
                    {
                        return false;
                    }
                }
                return true;
            }
            catch (Exception)
            {

            }
            return false;
        }

        #endregion

        #region Metadata

        /// <summary>
        /// Reads the meta-data from the stream starting at the given position.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        private TagsCollectionBase ReadMeta(Stream stream)
        {
            return (new TagsCollectionSerializer()).DeserializeWithSize(stream);
        }

        /// <summary>
        /// Writes the meta-data to the stream starting at the given position.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="metaTags"></param>
        private void WriteMeta(Stream stream, TagsCollectionBase metaTags)
        {
            (new TagsCollectionSerializer()).SerializeWithSize(metaTags, stream);
        }

        #endregion

        #region Vehicle Profiles

        /// <summary>
        /// Builds vehicles runtime type model.
        /// </summary>
        /// <returns></returns>
        private RuntimeTypeModel BuildVehiclesRuntimeTypeModel()
        {
            var typeModel = TypeModel.Create();
            typeModel.Add(typeof(List<string>), true);
            return typeModel;
        }

        /// <summary>
        /// Reads the vehicle profiles from the stream starting at the given position.
        /// </summary>
        /// <param name="stream"></param>
        /// <returns></returns>
        private IEnumerable<string> ReadVehicleProfiles(Stream stream)
        {
            List<string> vehicles = null;
            var intBytes = new byte[4];
            stream.Read(intBytes, 0, 4);
            var metaLength = BitConverter.ToInt32(intBytes, 0);
            if (metaLength > 0)
            {
                // read meta byte array.
                var tagsBytes = new byte[metaLength];
                stream.Read(tagsBytes, 0, metaLength);
                vehicles = this.BuildVehiclesRuntimeTypeModel().Deserialize(new MemoryStream(tagsBytes), null, typeof(List<string>)) as List<string>;
            }
            else
            { // no metadata here!
                vehicles = new List<string>();
            }
            return vehicles;
        }

        /// <summary>
        /// Writes the meta-data to the stream starting at the given position.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="vehicleProfiles"></param>
        private void WriteVehicleProfiles(Stream stream, IEnumerable<Vehicle> vehicleProfiles)
        {
            var vehicles = new List<string>(
                vehicleProfiles.Select(x => x.UniqueName));
            this.WriteVehicleProfiles(stream, vehicles);
        }

        /// <summary>
        /// Writes the meta-data to the stream starting at the given position.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="vehicleProfiles"></param>
        private void WriteVehicleProfiles(Stream stream, IEnumerable<string> vehicleProfiles)
        {
            var vehiclesStream = new MemoryStream();
            this.BuildVehiclesRuntimeTypeModel().Serialize(vehiclesStream, new List<string>(vehicleProfiles));

            var tagsBytes = vehiclesStream.ToArray();
            stream.Write(BitConverter.GetBytes(tagsBytes.Length), 0, 4);
            stream.Write(tagsBytes, 0, tagsBytes.Length);
        }

        #endregion

        /// <summary>
        /// Returns true if this serializer can deserialize the data in the given stream.
        /// </summary>
        /// <param name="stream"></param>
        public virtual bool CanDeSerialize(Stream stream)
        {
            var canRead = this.ReadAndValidateHeader(stream);
            stream.Seek(0, SeekOrigin.Begin);
            return canRead;
        }

        /// <summary>
        /// Serializes the given graph.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="graph"></param>
        public void Serialize(Stream stream, RouterDataSource<TEdgeData> graph)
        {
            this.Serialize(stream, graph, new TagsCollection());
        }

        /// <summary>
        /// Serializes the given graph.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="graph"></param>
        /// <param name="metaTags"></param>
        public void Serialize(Stream stream, RouterDataSource<TEdgeData> graph, TagsCollectionBase metaTags)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");
            if (graph == null)
                throw new ArgumentNullException("graph");

            // write the header.
            this.WriteVersionHeader(stream);

            // write the vehicles.
            this.WriteVehicleProfiles(stream, graph.GetSupportedProfiles());

            // write the meta-data.
            this.WriteMeta(stream, metaTags);

            // wrap the stream.
            var routingSerializerStream = new LimitedStream(stream);

            // do the version-specific serialization.
            this.DoSerialize(routingSerializerStream, graph);
        }

        /// <summary>
        /// Serializes the given graph and tags index to the given stream.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="graph"></param>
        protected abstract void DoSerialize(LimitedStream stream, RouterDataSource<TEdgeData> graph);

        /// <summary>
        /// Deserializes the given stream into a routable graph.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="lazy"></param>
        /// <returns></returns>
        public RouterDataSource<TEdgeData> Deserialize(Stream stream, bool lazy = true)
        {
            TagsCollectionBase notNeeded;
            return this.Deserialize(stream, out notNeeded, lazy);
        }

        /// <summary>
        /// Deserializes the given stream into a routable graph.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="lazy"></param>
        /// <param name="metaTags"></param>
        /// <returns></returns>
        public RouterDataSource<TEdgeData> Deserialize(Stream stream, out TagsCollectionBase metaTags, bool lazy = true)
        {
            if (stream == null)
                throw new ArgumentNullException("stream");

            // make sure the stream seeks to the beginning.
            stream.Seek(0, SeekOrigin.Begin);

            if (this.CanDeSerialize(stream))
            {
                // read/verify the current version header.
                this.ReadAndValidateHeader(stream);

                // read vehicle profiles.
                var vehicles = this.ReadVehicleProfiles(stream);

                // deserialize meta data.
                metaTags = this.ReadMeta(stream);

                // wrap the stream.
                var routingSerializerStream = new LimitedStream(stream);

                // do the actual version-specific deserialization.
                return this.DoDeserialize(routingSerializerStream, lazy, vehicles);
            }
            throw new ArgumentOutOfRangeException("stream", "Cannot deserialize the given stream, version unsupported or content unrecognized!");
        }

        /// <summary>
        /// Deserializes the given stream into a routable graph.
        /// </summary>
        /// <param name="stream"></param>
        /// <param name="lazy"></param>
        /// <param name="vehicles"></param>
        /// <returns></returns>
        protected abstract RouterDataSource<TEdgeData> DoDeserialize(LimitedStream stream, bool lazy, IEnumerable<string> vehicles);
    }
}