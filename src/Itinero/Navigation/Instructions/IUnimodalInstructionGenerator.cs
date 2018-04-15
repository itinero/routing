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

using Itinero.Navigation.Language;
using System.Collections.Generic;
using System.Threading;

namespace Itinero.Navigation.Instructions
{
    /// <summary>
    /// Abstract representation of a unimodal instruction generator.
    /// </summary>
    public interface IUnimodalInstructionGenerator
    {
        /// <summary>
        /// Generates instructions for the given route using the given language reference.
        /// </summary>
        IList<Instruction> Generate(Route route, ILanguageReference languageReference);

        /// <summary>
        /// Generates instructions for the given route using the given language reference.
        /// </summary>
        IList<Instruction> Generate(Route route, ILanguageReference languageReference, CancellationToken cancellationToken);
    }
}