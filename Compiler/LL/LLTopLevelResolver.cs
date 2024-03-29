﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
    public partial class LL
    {
        private List<AASMStructType> structs = new List<AASMStructType>();
        private List<ImportStatement> imports = new List<ImportStatement>();
        private List<FunctionStatement> functions = new List<FunctionStatement>();

        public List<AASMStructType> Structs
        {
            get { return this.structs; }
        }

        public List<FunctionStatement> Functions
        {
            get { return this.functions; }
        }

        public List<ImportStatement> Imports
        {
            get { return this.imports; }
        }

        private void LLTopLevelResolver()
        {
            CompoundStatement topLevel = this.ast.TopLevel;

            for (int i = 0; i < topLevel.Statements.Count; i++)
            {
                Statement statement = topLevel.Statements[i];

                if (statement is FunctionStatement)
                {
                    this.functions.Add(statement as FunctionStatement);
                }
                else if (statement is ImportStatement)
                {
                    this.imports.Add(statement as ImportStatement);
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

            for (int i = 0; i < this.imports.Count; i++)
            {
                if (this.imports[i].ImportedName.Value == callExpression.Name.Value)
                {
                    callExpression.LLImportTarget = this.imports[i];
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
        private ImportStatement llImportTarget;

        public FunctionStatement LLTarget
        {
            get { return this.llTarget; }
            set { this.llTarget = value; }
        }

        public ImportStatement LLImportTarget
        {
            get { return this.llImportTarget; }
            set { this.llImportTarget = value; }
        }
    }
}