using Itinero.Profiles.Lua.Debugging;
using Itinero.Profiles.Lua.Execution;
using Itinero.Profiles.Lua.Execution.VM;


namespace Itinero.Profiles.Lua.Tree.Statements
{
	class BreakStatement : Statement
	{
		SourceRef m_Ref;

		public BreakStatement(ScriptLoadingContext lcontext)
			: base(lcontext)
		{
			m_Ref = CheckTokenType(lcontext, TokenType.Break).GetSourceRef();
			lcontext.Source.Refs.Add(m_Ref);
		}



		public override void Compile(ByteCode bc)
		{
			using (bc.EnterSource(m_Ref))
			{
				if (bc.LoopTracker.Loops.Count == 0)
					throw new SyntaxErrorException(this.Script, m_Ref, "<break> at line {0} not inside a loop", m_Ref.FromLine);

				ILoop loop = bc.LoopTracker.Loops.Peek();

				if (loop.IsBoundary())
					throw new SyntaxErrorException(this.Script, m_Ref, "<break> at line {0} not inside a loop", m_Ref.FromLine);

				loop.CompileBreak(bc);
			}
		}
	}
}
