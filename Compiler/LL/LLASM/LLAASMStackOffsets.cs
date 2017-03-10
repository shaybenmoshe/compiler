using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
    public partial class LL
    {
        public void LLAASMStackOffsets()
        {
            for (int i = 0; i < this.functions.Count; i++)
            {
                this.functions[i].LLAASMStackOffsets();
            }
        }
    }

    public partial class FunctionStatement : Statement
    {
        private int llAASMSize;

        public void LLAASMStackOffsets()
        {
            this.llAASMSize = this.llLocals.Count * AASM.AASM.AddressSize;

            int localOffset = AASM.AASM.AddressSize;
            for (int i = 0; i < this.llLocals.Count; i++)
            {
                this.llLocals[i].LLAASMOffset = localOffset;
                localOffset += this.llLocals[i].LLAASMType.Size;
            }

            int argumentOffset = - 2 * AASM.AASM.AddressSize;
            for (int i = 0; i < this.arguments.Count; i++)
            {
                this.arguments[i].LLAASMOffset = argumentOffset;
                argumentOffset -= this.arguments[i].LLAASMType.Size;
            }
        }
    }

    public partial class NameDefStatement : Statement
    {
        private int llAASMOffset;

        public int LLAASMOffset
        {
            get { return this.llAASMOffset; }
            set { this.llAASMOffset = value; }
        }
    }
}