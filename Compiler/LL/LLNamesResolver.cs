using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
    public partial class LL
    {
        public void LLNamesResolver()
        {
            for (int i = 0; i < this.functions.Count; i++)
            {
                this.functions[i].LLNamesResolver();
            }
        }
    }

    public partial class FunctionStatement : Statement
    {
        public void LLNamesResolver()
        {
            this.TraverseExpressions(this.LLRenewLocalOrArgument);
        }

        private void LLRenewLocalOrArgument(Expression expression)
        {
            if (!(expression is NameExpression))
            {
                return;
            }

            NameExpression immediateExpression = expression as NameExpression;
            
            NameDefStatement nameDef = null;

            if (nameDef == null)
            {
                nameDef = Utils.FindNameDefsList(this.llLocals, immediateExpression.Value);
            }
            if (nameDef == null)
            {
                nameDef = Utils.FindNameDefsList(this.arguments, immediateExpression.Value);
            }
            if (nameDef == null)
            {
                throw new CompilerException("Name `" + immediateExpression.Value.Value + "` is not a name of an argument or a local.",
                    immediateExpression.Value.Position);
            }

            immediateExpression.LLNameDef = nameDef;
        }
    }

    public partial class NameExpression : ImmediateExpression<NameToken>
    {
        private NameDefStatement llNameDef;

        public NameDefStatement LLNameDef
        {
            get { return this.llNameDef; }
            set { this.llNameDef = value; }
        }
    }
}