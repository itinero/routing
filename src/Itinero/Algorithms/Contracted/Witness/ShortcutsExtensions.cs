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

using System.Collections.Generic;
using Itinero.Algorithms.Weights;

namespace Itinero.Algorithms.Contracted.Witness
{
    /// <summary>
    /// Contains extension methods related to shortcuts.
    /// </summary>
    public static class ShortcutsExtensions
    {
        /// <summary>
        /// Removes witnessed shortcuts.
        /// </summary>
        /// <param name="shortcuts"></param>
        /// <param name="witnesses"></param>
        /// <returns></returns>
        public static bool RemoveWitnessed<T>(this Shortcuts<T> shortcuts, WeightHandler<T> weightHandler, uint vertex, Dictionary<uint, Dictionary<OriginalEdge, T>> witnesses)
            where T : struct
        {
            if (witnesses.TryGetValue(vertex, out Dictionary<OriginalEdge, T> vertexWitnesses))
            {
                return shortcuts.RemoveWitnessed(weightHandler, vertexWitnesses);
            }
            return false;
        }

        /// <summary>
        /// Removes witnessed shortcuts.
        /// </summary>
        /// <param name="shortcuts"></param>
        /// <param name="witnesses"></param>
        /// <returns></returns>
        public static bool RemoveWitnessed<T>(this Shortcuts<T> shortcuts, WeightHandler<T> weightHandler, Dictionary<OriginalEdge, T> witnesses)
            where T : struct
        {
            bool witnessed = false;
            foreach (var witness in witnesses)
            {
                var edge = witness.Key;
                if (shortcuts.TryGetValue(edge, out Shortcut<T> shortcut))
                {
                    if (weightHandler.IsSmallerThan(witness.Value, shortcut.Forward))
                    {
                        shortcut.Forward = witness.Value;
                        shortcuts.AddOrUpdate(edge, shortcut, weightHandler);
                        witnessed = true;
                    }
                }
                // TODO: this should be 'else', in theory this extra check isn't needed.
                edge = edge.Reverse();
                if (shortcuts.TryGetValue(edge, out shortcut))
                {
                    if (weightHandler.IsSmallerThan(witness.Value, shortcut.Backward))
                    {
                        shortcut.Backward = witness.Value;
                        shortcuts.AddOrUpdate(edge, shortcut, weightHandler);
                        witnessed = true;
                    }
                }
            }
            return witnessed;
        }
    }
}