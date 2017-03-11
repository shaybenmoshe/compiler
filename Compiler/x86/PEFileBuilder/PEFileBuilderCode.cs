using System;
using System.Collections.Generic;

namespace Compiler
{
    public partial class PEFileBuilder
    {
        private uint codeRVA = 0x1000;
        
        private uint codeOffset;
        
        List<byte> code;
        private uint entryPointInCode;
        
        public void EmitCode(List<byte> code, uint entryPointInCode)
        {
            this.code = code;
            this.entryPointInCode = entryPointInCode;

            this.codeOffset = (uint)this.output.Count;
            this.output.AddRange(this.code);
            this.AlignToSectionAlignment();
            int afterCode = this.output.Count;

            uint codeSize = (uint)(afterCode - this.codeOffset);

            this.valuesFixers[ValuesFixerKeys.SizeOfImage].Value = this.codeRVA + codeSize;

            this.valuesFixers[ValuesFixerKeys.CodeVirtualSize].Value = (uint)this.code.Count;
            this.valuesFixers[ValuesFixerKeys.CodeVirtualOffset].Value = this.codeRVA;
            this.valuesFixers[ValuesFixerKeys.EntryPoint].Value = this.codeRVA + this.entryPointInCode;
            this.valuesFixers[ValuesFixerKeys.CodeDiskSize].Value = (uint)(afterCode - this.codeOffset);
            this.valuesFixers[ValuesFixerKeys.CodeRVA].Value = this.codeOffset;
        }
    }
}