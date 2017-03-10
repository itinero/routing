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

namespace Itinero.Algorithms.Networks.Analytics.Heatmaps
{
    /// <summary>
    /// Represents the result of a heatmap call.
    /// </summary>
    public class HeatmapResult
    {
        /// <summary>
        /// Gets or sets the max.
        /// </summary>
        public float Max { get; set; }

        /// <summary>
        /// Gets or sets the metric.
        /// </summary>
        public string MaxMetric { get; set; }

        /// <summary>
        /// Gets or sets the data.
        /// </summary>
        public HeatmapSample[] Data { get; set; }
    }

    /// <summary>
    /// Represents one heatmap sample.
    /// </summary>
    public class HeatmapSample
    {
        /// <summary>
        /// Gets or sets the latitude.
        /// </summary>
        public float Latitude { get; set; }

        /// <summary>
        /// Gets or sets the longitude.
        /// </summary>
        public float Longitude { get; set; }

        /// <summary>
        /// Gets or sets the value.
        /// </summary>
        public float Value { get; set; }
    }
}