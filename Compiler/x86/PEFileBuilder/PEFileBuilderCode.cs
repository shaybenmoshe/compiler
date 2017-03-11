using System;
using System.Collections.Generic;

namespace Compiler
{
    public partial class PEFileBuilder
    {
        public void EmitCode(List<byte> code, uint entryPointInCode)
        {
            int codeRVA = 0x1000;

            int codeOffset = this.output.Count;
            this.output.AddRange(code);
            this.AlignToSectionAlignment();
            int afterCode = this.output.Count;

            uint codeSize = (uint)(afterCode - codeOffset);

            this.valuesFixers[ValuesFixerKeys.SizeOfImage].Value = (uint)(codeRVA + codeSize);

            this.valuesFixers[ValuesFixerKeys.CodeVirtualSize].Value = (uint)code.Count;
            this.valuesFixers[ValuesFixerKeys.CodeVirtualOffset].Value = (uint)codeRVA;
            this.valuesFixers[ValuesFixerKeys.EntryPoint].Value = (uint)(codeRVA + entryPointInCode);
            this.valuesFixers[ValuesFixerKeys.CodeDiskSize].Value = (uint)(afterCode - codeOffset);
            this.valuesFixers[ValuesFixerKeys.CodeRVA].Value = (uint)codeOffset;
        }
    }
}