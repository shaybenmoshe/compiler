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
            int offset = 0;
            for (int i = 0; i < this.llLocals.Count; i++)
            {
                this.llLocals[i].DefPosition = NameDefStatement.DefPositionTypes.Local;
                // We add the offset before writing it.
                // This is special to x86, as the stack is the other way around.
                // I think this is correct :(.
                //offset += (int)this.llLocals[i].LLAASMType.Size;
                offset += (int)AASM.AASM.AddressSize;
                this.llLocals[i].LLAASMOffset = offset;
            }

            this.llAASMSize = offset;

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