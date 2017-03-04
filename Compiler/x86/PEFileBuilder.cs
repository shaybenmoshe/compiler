using System;
using System.Collections.Generic;

namespace Compiler
{
    public class PEFileBuilder
    {
        private const uint sectionAlignmentSize = 0x200;

        private const uint codeRVA = 0x1000;

        private uint sizeOfHeaders;
        private uint codeOffset;

        private int offsetSizeOfOptionalHeaders;
        private int offsetSizeOfImage;
        private int offsetSizeOfHeaders;
        private int offsetCodeOffset;
        private int offsetCodeDiskSize;
        private int offsetCodeDiskSize2;

        List<byte> output = new List<byte>();
        List<byte> code;
        private uint entryPointInCode;

        public PEFileBuilder(List<byte> code, uint entryPointInCode)
        {
            this.code = code;
            this.entryPointInCode = entryPointInCode;
        }

        private void alignToSecionAlignment()
        {
            while ((this.output.Count & (sectionAlignmentSize - 1)) != 0)
            {
                this.output.Add(0);
            }
        }

        public List<byte> Emit()
        {
            this.EmitDosHeader();
            this.EmitPEHeader();
            int beforeOptionalHeaders = this.output.Count;
            this.EmitOptionalHeaders();
            this.EmitDataDirectories();
            int afterOptionalHeaders = this.output.Count;
            this.EmitSections();
            this.alignToSecionAlignment();

            this.sizeOfHeaders = (uint)this.output.Count;

            this.codeOffset = (uint)this.output.Count;
            this.output.AddRange(this.code);
            this.alignToSecionAlignment();
            int afterCode = this.output.Count;

            uint codeSize = (uint)(afterCode - this.codeOffset);

            uint sizeOfImage = codeRVA + codeSize;

            Utils.Rewrite(this.output, (uint)(afterOptionalHeaders - beforeOptionalHeaders), 2, this.offsetSizeOfOptionalHeaders);
            Utils.Rewrite(this.output, sizeOfImage, 4, this.offsetSizeOfImage);
            Utils.Rewrite(this.output, this.sizeOfHeaders, 4, this.offsetSizeOfHeaders);
            Utils.Rewrite(this.output, this.codeOffset, 4, this.offsetCodeOffset);
            Utils.Rewrite(this.output, codeSize, 4, this.offsetCodeDiskSize);
            Utils.Rewrite(this.output, codeSize, 4, this.offsetCodeDiskSize2);

            return this.output;
        }

        private void EmitDosHeader()
        {
            this.output.Add((byte)'M');
            this.output.Add((byte)'Z');

            for (int i = 2; i < 0x3C; i++)
            {
                this.output.Add(0);
            }

            uint PEOffset = (uint)this.output.Count + 4;
            Utils.Write(this.output, PEOffset, 4);
        }

        private void EmitPEHeader()
        {
            this.output.Add((byte)'P');
            this.output.Add((byte)'E');
            this.output.Add(0);
            this.output.Add(0);

            Utils.Write(this.output, 0x014c, 2); // Machine = x86
            Utils.Write(this.output, 1, 2); // @todo Number of sections
            Utils.Write(this.output, 0, 4); // Timestamp
            Utils.Write(this.output, 0, 4); // Symbols stuff
            Utils.Write(this.output, 0, 4); // Symbols stuff
            this.offsetSizeOfOptionalHeaders = this.output.Count;
            Utils.Write(this.output, 0, 2); // Size of optional headers

            uint characteristics = 0;
            characteristics |= 0x0002; // Exe
            characteristics |= 0x0020; // Can handle > 2gb
            Utils.Write(this.output, characteristics, 2); // Char
        }

        private void EmitOptionalHeaders()
        {
            Utils.Write(this.output, 0x010b, 2); // Magic = PE32

            this.output.Add(0); // Major linker version
            this.output.Add(0); // Minor linker version
            this.offsetCodeDiskSize2 = this.output.Count;
            Utils.Write(this.output, 0, 4); // Size of code
            Utils.Write(this.output, 0, 4); // Size of initialized data
            Utils.Write(this.output, 0, 4); // Size of uninitialized data
            Utils.Write(this.output, this.entryPointInCode + codeRVA, 4); // Entry point
            Utils.Write(this.output, codeRVA, 4); // Offset of code
            Utils.Write(this.output, 0, 4); // Offset of data
            Utils.Write(this.output, 0x00400000, 4); // Image base
            Utils.Write(this.output, 0x1000, 4); // Section alignment
            Utils.Write(this.output, sectionAlignmentSize, 4); // File alignment
            Utils.Write(this.output, 5, 2); // Major os
            Utils.Write(this.output, 1, 2); // Minor os
            Utils.Write(this.output, 0, 2); // Major image
            Utils.Write(this.output, 0, 2); // Minor image
            Utils.Write(this.output, 5, 2); // Major subsystem
            Utils.Write(this.output, 1, 2); // Minor subsystem
            Utils.Write(this.output, 0, 4); // Reserved (Win32 version value)
            this.offsetSizeOfImage = this.output.Count;
            Utils.Write(this.output, 0, 4);
            this.offsetSizeOfHeaders = this.output.Count;
            Utils.Write(this.output, 0, 4);
            Utils.Write(this.output, 0, 4); // Checksum
            Utils.Write(this.output, 3, 2); // Subsystem = console

            uint DLLCharacteristics = 0;
            //DLLCharacteristics |= 0x0040; // Relocate
            DLLCharacteristics |= 0x0100; // DEP
            DLLCharacteristics |= 0x0400; // No SEH
            Utils.Write(this.output, DLLCharacteristics, 2); // DLLCharacteristics

            Utils.Write(this.output, 0x00100000, 4); // Stack reserver
            Utils.Write(this.output, 0x00001000, 4); // Stack commit
            Utils.Write(this.output, 0, 4); // Heap reserve
            Utils.Write(this.output, 0, 4); // Heap commit
            Utils.Write(this.output, 0, 4); // Loader flags
            Utils.Write(this.output, 0x10, 4); // Number of rva and sizes
        }

        private void EmitDataDirectories()
        {
            // RVA, Size
            Utils.Write(this.output, 0, 4); // Export
            Utils.Write(this.output, 0, 4);
            Utils.Write(this.output, 0, 4); // Import
            Utils.Write(this.output, 0, 4);
            Utils.Write(this.output, 0, 4); // Resource
            Utils.Write(this.output, 0, 4);
            Utils.Write(this.output, 0, 4); // Exception
            Utils.Write(this.output, 0, 4);
            Utils.Write(this.output, 0, 4); // Security
            Utils.Write(this.output, 0, 4);
            Utils.Write(this.output, 0, 4); // Relocation
            Utils.Write(this.output, 0, 4);
            Utils.Write(this.output, 0, 4); // Debug
            Utils.Write(this.output, 0, 4);
            Utils.Write(this.output, 0, 4); // Arch
            Utils.Write(this.output, 0, 4);
            Utils.Write(this.output, 0, 4); // Global prt
            Utils.Write(this.output, 0, 4);
            Utils.Write(this.output, 0, 4); // TLS
            Utils.Write(this.output, 0, 4);
            Utils.Write(this.output, 0, 4); // Conf
            Utils.Write(this.output, 0, 4);
            Utils.Write(this.output, 0, 4); // Bound
            Utils.Write(this.output, 0, 4);
            Utils.Write(this.output, 0, 4); // IAT
            Utils.Write(this.output, 0, 4);
            Utils.Write(this.output, 0, 4); // Delay import
            Utils.Write(this.output, 0, 4);
            Utils.Write(this.output, 0, 4); // .NET
            Utils.Write(this.output, 0, 4);
            Utils.Write(this.output, 0, 4); // Reserved
            Utils.Write(this.output, 0, 4);
        }

        private void EmitSections()
        {
            this.output.Add((byte)'.');
            this.output.Add((byte)'t');
            this.output.Add((byte)'e');
            this.output.Add((byte)'x');
            this.output.Add((byte)'t');
            this.output.Add(0);
            this.output.Add(0);
            this.output.Add(0);

            Utils.Write(this.output, (uint)this.code.Count, 4); // Virtual size
            Utils.Write(this.output, codeRVA, 4); // RVA
            this.offsetCodeDiskSize = this.output.Count;
            Utils.Write(this.output, 0, 4); // Size of raw data
            this.offsetCodeOffset = this.output.Count;
            Utils.Write(this.output, 0, 4); // Offset of code in file
            Utils.Write(this.output, 0, 4); // Relocations
            Utils.Write(this.output, 0, 4); // Line numbers
            Utils.Write(this.output, 0, 2); // Num of relocations
            Utils.Write(this.output, 0, 2); // Num of line numbers

            uint flags = 0;
            flags |= 0x00000020; // Contains code.
            flags |= 0x20000000; // Exec.
            flags |= 0x40000000; // Read.
            Utils.Write(this.output, flags, 4); // Flags
        }
    }
}