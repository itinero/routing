#if DOTNET_CORE

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace Itinero.Profiles.Lua.Compatibility.Frameworks
{
	class FrameworkCurrent : FrameworkCoreBase
	{
		public override Type GetInterface(Type type, string name)
		{
			return type.GetInterfaces().FirstOrDefault(x => x.Name == name);
		}

		public override TypeInfo GetTypeInfoFromType(Type t)
		{
			return t.GetTypeInfo();
		}

		public override bool IsDbNull(object o)
		{
			return o != null && o.GetType().FullName.StartsWith("System.DBNull");
		}

		public override bool StringContainsChar(string str, char chr)
		{
			return str.Contains(chr);
		}
	}
}
#endif
