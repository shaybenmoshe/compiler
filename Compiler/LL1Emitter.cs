using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
    public class LL1Emitter
    {
        private AST ast;
        private List<LL1Function> functions = new List<LL1Function>();

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

                LL1Function function = new LL1Function(statement as FunctionStatement);
                function.Prepare();
                this.functions.Add(function);
            }
        }
    }

    public class LL1Function
    {
        private FunctionStatement functionStatement;
        private List<NameDefStatement> locals = new List<NameDefStatement>();

        public LL1Function(FunctionStatement functionStatement)
        {
            this.functionStatement = functionStatement;
        }

        public void Prepare()
        {
            this.FindAllVars();
        }

        private void FindAllVars()
        {
            this.functionStatement.TraverseStatements(this.VarsTraversal);
        }

        private void VarsTraversal(Statement statement)
        {
            if (!(statement is VarStatement))
            {
                return;
            }

            VarStatement varStatement = statement as VarStatement;
            this.locals.Add(varStatement.NameDef);
        }
    }
}