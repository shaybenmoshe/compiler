using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
    public class LL1Emitter
    {
        private AST ast;
        private List<FunctionStatement> functions = new List<FunctionStatement>();

        public LL1Emitter(AST ast)
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
            FunctionStatement target = null;

            for (int i = 0; i < this.functions.Count; i++)
            {
                if (this.functions[i].Name.Value == callExpression.Name.Value)
                {
                    callExpression.Target = this.functions[i];
                    return;
                }
            }

            throw new CompilerException("Name `" + callExpression.Name.Value + "` is not a name of a function.",
                callExpression.Name.Position);
        }
    }

    public partial class ImmediateExpression : Expression
    {
        private NameDefStatement nameDef;
        
        public NameDefStatement NameDef
        {
            get { return this.nameDef; }
            set { this.nameDef = value; }
        }
    }

    public partial class CallExpression : Expression
    {
        private FunctionStatement target;

        public FunctionStatement Target
        {
            get { return this.target; }
            set { this.target = value; }
        }
    }

    public partial class FunctionStatement : Statement
    {
        private List<NameDefStatement> locals = new List<NameDefStatement>();

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

            if (Utils.FindNameDefsList(this.locals, varStatement.NameDef.Name) != null)
            {
                throw new CompilerException("Name `" + varStatement.NameDef.Name.Value + "` is already a local.", varStatement.Position);
            }

            this.locals.Add(varStatement.NameDef);
        }

        private void LL1RenewLocalOrArgument(Expression expression)
        {
            if (!(expression is ImmediateExpression))
            {
                return;
            }

            ImmediateExpression immediateExpression = expression as ImmediateExpression;

            if (!(immediateExpression.Value is NameToken))
            {
                return;
            }

            NameToken nameToken = immediateExpression.Value as NameToken;

            NameDefStatement nameDef = null;

            if (nameDef == null)
            {
                nameDef = Utils.FindNameDefsList(this.locals, nameToken);
            }
            if (nameDef == null)
            {
                nameDef = Utils.FindNameDefsList(this.arguments, nameToken);
            }
            if (nameDef == null)
            {
                throw new CompilerException("Name `" + nameToken.Value + "` is not a name of an argument or a local.",
                    nameToken.Position);
            }

            immediateExpression.NameDef = nameDef;
        }
    }
}