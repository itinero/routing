using Itinero.Profiles.Lua.DataStructs;
using Itinero.Profiles.Lua.Execution.VM;

namespace Itinero.Profiles.Lua.Execution
{
	interface ILoop
	{
		void CompileBreak(ByteCode bc);
		bool IsBoundary();
	}


	internal class LoopTracker
	{
		public FastStack<ILoop> Loops = new FastStack<ILoop>(16384);
	}
}
