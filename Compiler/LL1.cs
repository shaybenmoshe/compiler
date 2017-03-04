using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
    public class LL1
    {
        private AST ast;
        private List<FunctionStatement> functions = new List<FunctionStatement>();

        public LL1(AST ast)
        {
            this.ast = ast;
        }

        public void Emit()
        {
            CompoundStatement topLevel = this.ast.TopLevel;

            for (var i = 0; i < topLevel.Statements.Count; i++)
            {
                Statement statement = topLevel.Statements[i];
                if (!(statement is FunctionStatement))
                {
                    throw new CompilerException("Toplevel must contain only function definitions.", statement.Position);
                }

                FunctionStatement function = statement as FunctionStatement;
                function.LL1Prepare();
                this.functions.Add(function);
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
                    callExpression.LL1Target = this.functions[i];
                    return;
                }
            }

            throw new CompilerException("Name `" + callExpression.Name.Value + "` is not a name of a function.",
                callExpression.Name.Position);
        }
    }

    public partial class CallExpression : Expression
    {
        private FunctionStatement ll1Target;

        public FunctionStatement LL1Target
        {
            get { return this.ll1Target; }
            set { this.ll1Target = value; }
        }
    }

    public partial class FunctionStatement : Statement
    {
        private List<NameDefStatement> ll1Locals = new List<NameDefStatement>();

        public void LL1Prepare()
        {
            this.TraverseStatements(this.LL1AddLocalVar);
            this.TraverseExpressions(this.LL1RenewLocalOrArgument);
        }

        private void LL1AddLocalVar(Statement statement)
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

            if (Utils.FindNameDefsList(this.ll1Locals, varStatement.NameDef.Name) != null)
            {
                throw new CompilerException("Name `" + varStatement.NameDef.Name.Value + "` is already a local.", varStatement.Position);
            }

            this.ll1Locals.Add(varStatement.NameDef);
        }

        private void LL1RenewLocalOrArgument(Expression expression)
        {
            if (!(expression is NameExpression))
            {
                return;
            }

            NameExpression immediateExpression = expression as NameExpression;
            
            NameDefStatement nameDef = null;

            if (nameDef == null)
            {
                nameDef = Utils.FindNameDefsList(this.ll1Locals, immediateExpression.Value);
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

            immediateExpression.LL1NameDef = nameDef;
        }
    }

    public partial class NameExpression : ImmediateExpression<NameToken>
    {
        public NameDefStatement ll1NameDef;

        public NameDefStatement LL1NameDef
        {
            get { return this.ll1NameDef; }
            set { this.ll1NameDef = value; }
        }
    }
}