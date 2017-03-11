using System;
using System.Collections.Generic;

namespace Compiler
{
    public partial class PEFileBuilder
    {
        public void EmitCode(List<byte> code, uint entryPointInCode)
        {
            this.AlignToSectionAlignment();

            int codeRVA = 0x2000;

            int codeOffset = this.output.Count;
            this.output.AddRange(code);
            this.AlignToSectionAlignment();
            int afterCode = this.output.Count;

            uint codeSize = (uint)(afterCode - codeOffset);

            this.valuesFixers[ValuesFixerKeys.SizeOfImage].Value = this.AlignTo((uint)(codeRVA + codeSize), pageSize);

            this.valuesFixers[ValuesFixerKeys.CodeVirtualSize].Value = (uint)code.Count;
            this.valuesFixers[ValuesFixerKeys.CodeVirtualOffset].Value = (uint)codeRVA;
            this.valuesFixers[ValuesFixerKeys.EntryPoint].Value = (uint)(codeRVA + entryPointInCode);
            this.valuesFixers[ValuesFixerKeys.CodeDiskSize].Value = codeSize;
            this.valuesFixers[ValuesFixerKeys.CodeDiskOffset].Value = (uint)codeOffset;
        }
    }
}