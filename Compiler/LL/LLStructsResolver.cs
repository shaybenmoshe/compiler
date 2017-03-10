﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
    public partial class LL
    {
        public void LLStructsResolver()
        {
            for (int i = 0; i < this.structs.Count; i++)
            {
                this.structs[i].LLResolve(this);
            }
        }
    }

    public partial class AASMStructType : IAASMType
    {
        public void LLResolve(LL ll)
        {
            uint offset = 0;

            for (int i = 0; i < this.structStatement.Members.Count; i++)
            {
                this.structStatement.Members[i].LLAASMStructOffset = offset;
                offset += this.structStatement.Members[i].LLAASMType.Size;
            }

            this.size = offset;
        }
    }

    public partial class NameDefStatement : Statement
    {
        private uint llAASMStructOffset;

        public uint LLAASMStructOffset
        {
            get { return this.llAASMStructOffset; }
            set { this.llAASMStructOffset = value; }
        }
    }
}