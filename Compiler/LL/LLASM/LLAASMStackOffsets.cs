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

            int offset = 0;
            for (int i = 0; i < this.llLocals.Count; i++)
            {
                this.llLocals[i].DefPosition = NameDefStatement.DefPositionTypes.Local;
                this.llLocals[i].LLAASMOffset = offset;
                offset += (int)this.llLocals[i].LLAASMType.Size;
            }

            offset = 0;
            for (int i = 0; i < this.arguments.Count; i++)
            {
                this.arguments[i].DefPosition = NameDefStatement.DefPositionTypes.Argument;
                this.arguments[i].LLAASMOffset = offset;
                offset += (int)this.arguments[i].LLAASMType.Size;
            }
        }
    }

    public partial class NameDefStatement : Statement
    {
        public enum DefPositionTypes
        {
            Local,
            Argument,
        }

        private DefPositionTypes defPosition;
        private int llAASMOffset;

        public int LLAASMOffset
        {
            get { return this.llAASMOffset; }
            set { this.llAASMOffset = value; }
        }

        public DefPositionTypes DefPosition
        {
            get { return this.defPosition; }
            set { this.defPosition = value; }
        }
    }
}