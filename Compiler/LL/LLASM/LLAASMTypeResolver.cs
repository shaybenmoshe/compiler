using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
    public partial class LL
    {
        public void LLAASMTypeResolver()
        {
            for (int i = 0; i < this.functions.Count; i++)
            {
                this.functions[i].LLAASMTypeResolver();
            }
        }
    }

    public partial class FunctionStatement : Statement
    {
        private AASMType llAASMRetType;

        public void LLAASMTypeResolver()
        {
            this.llAASMRetType = AASMType.Resolve(this.retType.Value);

            for (int i = 0; i < this.llLocals.Count; i++)
            {
                this.llLocals[i].LLAASMType = AASMType.Resolve(this.llLocals[i].Type.Value);
            }
            for (int i = 0; i < this.arguments.Count; i++)
            {
                this.arguments[i].LLAASMType = AASMType.Resolve(this.arguments[i].Type.Value);
            }
        }
    }

    public partial class NameDefStatement : Statement
    {
        private AASMType llAASMType;

        public AASMType LLAASMType
        {
            get { return this.llAASMType; }
            set { this.llAASMType = value; }
        }
    }
}