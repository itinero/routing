using System;

namespace Itinero.Profiles.Lua.Diagnostics.PerformanceCounters
{
	internal interface IPerformanceStopwatch
	{
		IDisposable Start();
		PerformanceResult GetResult();
	}
}
