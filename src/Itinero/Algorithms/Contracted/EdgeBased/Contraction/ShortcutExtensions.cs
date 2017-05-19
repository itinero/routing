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

using Itinero.Algorithms.Weights;

namespace Itinero.Algorithms.Contracted.EdgeBased.Contraction
{
    /// <summary>
    /// Contains extension methods related to the shortcuts data structures.
    /// </summary>
    public static class ShortcutExtensions
    {
        /// <summary>
        /// Removes witnessed shortcuts.
        /// </summary>
        public static void RemoveWitnessed<T>(this Shortcuts<T> shortcuts, uint vertex, DykstraWitnessCalculator<T> witnessCalculator)
            where T : struct
        {
            witnessCalculator.Calculate(vertex, shortcuts);
        }

        /// <summary>
        /// Adds a source only if it's not there yet.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void AddSourceIfNeeded<T>(this Shortcuts<T>.Accessor shortcuts, OriginalEdge source)
            where T : struct
        {
            // check for existing source.
            if (!shortcuts.HasSource ||
                !shortcuts.Source.Equals(source))
            { // current is not the same source, search for one.
                shortcuts.Reset();
                while (shortcuts.MoveNextSource())
                {
                    if (shortcuts.Source.Equals(source))
                    {
                        break;
                    }
                }
                if (!shortcuts.HasSource)
                {
                    shortcuts.AddSource(source);
                }
            }
        }

        /// <summary>
        /// Returns true if the given edge is a source and moves the accessor to that source.
        /// </summary>
        public static bool MoveToSource<T>(this Shortcuts<T>.Accessor shortcuts, OriginalEdge edge)
            where T : struct
        {
            // check for existing source.
            if (!shortcuts.HasSource ||
                !shortcuts.Source.Equals(edge))
            { // current is not the same source, search for one.
                shortcuts.Reset();
                while (shortcuts.MoveNextSource())
                {
                    if (shortcuts.Source.Equals(edge))
                    {
                        break;
                    }
                }
                return shortcuts.HasSource;
            }
            return true;
        }
        
        /// <summary>
        /// Returns true if the given edge is a target under the current source and moves the accessor tho that target.
        /// </summary>
        public static bool MoveToTarget<T>(this Shortcuts<T>.Accessor shortcuts, OriginalEdge edge)
            where T : struct
        {
            // check for existing source.
            if (!shortcuts.HasTarget ||
                !shortcuts.Target.Equals(edge))
            { // current is not the same source, search for one.
                shortcuts.ResetTarget();
                while (shortcuts.MoveNextTarget())
                {
                    if (shortcuts.Target.Edge.Equals(edge))
                    {
                        break;
                    }
                }
                return shortcuts.HasTarget;
            }
            return true;
        }

        /// <summary>
        /// Adds the given shortcut or updates the one in place.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        public static void AddOrUpdate<T>(this Shortcuts<T>.Accessor shortcuts, OriginalEdge source, Shortcut<T> shortcut, WeightHandler<T> weightHandler)
            where T : struct
        {
            // find shortcut source->target.
            if (shortcuts.MoveToSource(source) &&
                shortcuts.MoveToTarget(shortcut.Edge))
            { // shortcut was found, update it.
                var existing = shortcuts.Target;
                existing.Update(weightHandler, shortcut.Forward, shortcut.Backward);
                shortcuts.Target = existing;
            }
            else if(shortcuts.MoveToSource(shortcut.Edge.Reverse()) &&
                    shortcuts.MoveToTarget(source.Reverse()))
            { // reverse shortcut was found, update it.
                var existing = shortcuts.Target;
                existing.Update(weightHandler, shortcut.Backward, shortcut.Forward);
                shortcuts.Target = existing;
            }
            else
            {
                if (shortcuts.MoveToSource(source))
                {
                    shortcuts.Add(shortcut);
                }
                else if(shortcuts.MoveToSource(shortcut.Edge.Reverse()))
                {
                    shortcuts.Add(new Shortcut<T>()
                    {
                        Backward = shortcut.Forward,
                        Edge = source.Reverse(),
                        Forward = shortcut.Backward
                    });
                }
                else
                {
                    shortcuts.AddSource(source);
                    shortcuts.Add(shortcut);
                }
            }
        }
    }
}