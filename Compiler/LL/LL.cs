using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
    public partial class LL
    {
        private AST ast;

        public LL(AST ast)
        {
            this.ast = ast;
        }

        public void Emit()
        {
            this.FunctionResolver();
            this.LLLocalsResolver();
            this.LLNamesResolver();

            this.LLAASMTypeResolver();
            this.LLAASMStackOffsets();
            this.LLAASMEmit();
        }
    }
}