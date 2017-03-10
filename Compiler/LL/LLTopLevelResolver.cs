using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
    public partial class LL
    {
        private List<AASMStructType> structs = new List<AASMStructType>();
        private List<FunctionStatement> functions = new List<FunctionStatement>();

        public List<AASMStructType> Structs
        {
            get { return this.structs; }
        }

        public List<FunctionStatement> Functions
        {
            get { return this.functions; }
        }

        private void LLTopLevelResolver()
        {
            CompoundStatement topLevel = this.ast.TopLevel;

            for (var i = 0; i < topLevel.Statements.Count; i++)
            {
                Statement statement = topLevel.Statements[i];

                if (statement is FunctionStatement)
                {
                    this.functions.Add(statement as FunctionStatement);
                }
                else if (statement is StructStatement)
                {
                    this.structs.Add(new AASMStructType(statement as StructStatement));
                }
                else
                {
                    throw new CompilerException("Toplevel must contain only function and struct definitions.", statement.Position);
                }
            }

            topLevel.TraverseExpressions(this.RenewCalls);
        }

        private void RenewCalls(Expression expression)
        {
            if (!(expression is CallExpression))
            {
                return;
            }

            CallExpression callExpression = expression as CallExpression;

            for (int i = 0; i < this.functions.Count; i++)
            {
                if (this.functions[i].Name.Value == callExpression.Name.Value)
                {
                    callExpression.LLTarget = this.functions[i];
                    return;
                }
            }

            throw new CompilerException("Name `" + callExpression.Name.Value + "` is not a name of a function.",
                callExpression.Name.Position);
        }
    }

    public partial class CallExpression : Expression
    {
        private FunctionStatement llTarget;

        public FunctionStatement LLTarget
        {
            get { return this.llTarget; }
            set { this.llTarget = value; }
        }
    }
}