// Itinero - Routing for .NET
// Copyright (C) 2015 Abelshausen Ben
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

using Itinero.Attributes;
using System;

namespace Itinero.Profiles.Lua.ItineroLib
{
    /// <summary>
    /// Class implementing itinero Lua functions 
    /// </summary>
    [MoonSharpModule(Namespace = "itinero")]
    public class ItineroModule
    {
        [MoonSharpModuleMethod]
        public static DynValue parseweight(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            DynValue weightstring = args.AsType(0, "parseweight", DataType.String, false);

            float weight;
            if (!IAttributeCollectionExtension.TryParseWeight(weightstring.String, out weight))
            {
                return DynValue.Nil;
            }
            return DynValue.NewNumber(weight);
        }

        [MoonSharpModuleMethod]
        public static DynValue parsewidth(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            DynValue weightstring = args.AsType(0, "parsewidth", DataType.String, false);

            float weight;
            if (!IAttributeCollectionExtension.TryParseLength(weightstring.String, out weight))
            {
                return DynValue.Nil;
            }
            return DynValue.NewNumber(weight);
        }

        [MoonSharpModuleMethod]
        public static DynValue parsespeed(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            DynValue weightstring = args.AsType(0, "parsespeed", DataType.String, false);

            float weight;
            if (!IAttributeCollectionExtension.TryParseSpeed(weightstring.String, out weight))
            {
                return DynValue.Nil;
            }
            return DynValue.NewNumber(weight);
        }

        [MoonSharpModuleMethod]
        public static DynValue log(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            DynValue text = args.AsType(0, "log", DataType.String, false);

            Itinero.Logging.Logger.Log("Lua", Logging.TraceEventType.Information, text.String);
            return DynValue.NewBoolean(true);
        }

        [MoonSharpModuleMethod]
        public static DynValue format(ScriptExecutionContext executionContext, CallbackArguments args)
        {
            var text = args.AsType(0, "format", DataType.String, false);
            var formatArgs = new string[args.Count - 1];
            for(var i = 1; i < args.Count;i++)
            {
                var formatArg = args.AsType(i, "format", DataType.String, false);
                formatArgs[i - 1] = formatArg.String;
            }

            return DynValue.NewString(string.Format(text.String, formatArgs));
        }
    }
}