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
            this.LLTopLevelResolver();
            this.LLLocalsResolver();
            this.LLNamesResolver();
            
            this.LLStructsResolver();

            this.LLAASMStackOffsets();
            this.LLAASMEmit();
        }
    }
}