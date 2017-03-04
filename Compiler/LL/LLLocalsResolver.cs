using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
    public partial class LL
    {
        public void LLLocalsResolver()
        {
            for (int i = 0; i < this.functions.Count; i++)
            {
                this.functions[i].LLLocalsResolver();
            }
        }
    }

    public partial class FunctionStatement : Statement
    {
        private List<NameDefStatement> llLocals = new List<NameDefStatement>();

        public void LLLocalsResolver()
        {
            this.TraverseStatements(this.LLAddLocalVar);
        }

        private void LLAddLocalVar(Statement statement)
        {
            if (!(statement is VarStatement))
            {
                return;
            }

            VarStatement varStatement = statement as VarStatement;

            if (Utils.FindNameDefsList(this.arguments, varStatement.NameDef.Name) != null)
            {
                throw new CompilerException("Name `" + varStatement.NameDef.Name.Value + "` is already an argument.", varStatement.Position);
            }

            if (Utils.FindNameDefsList(this.llLocals, varStatement.NameDef.Name) != null)
            {
                throw new CompilerException("Name `" + varStatement.NameDef.Name.Value + "` is already a local.", varStatement.Position);
            }

            this.llLocals.Add(varStatement.NameDef);
        }
    }
}