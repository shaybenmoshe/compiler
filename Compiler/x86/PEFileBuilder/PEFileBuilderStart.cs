using System;
using System.Collections.Generic;

namespace Compiler
{
    public partial class PEFileBuilder
    {
        public void EmitStart()
        {
            this.EmitDosHeader();
            this.EmitPEHeader();
            int beforeOptionalHeaders = this.output.Count;
            this.EmitOptionalHeaders();
            this.EmitDataDirectories();
            int afterOptionalHeaders = this.output.Count;
            this.EmitSections();
            
            this.valuesFixers[ValuesFixerKeys.SizeOfOptionalHeaders].Value = (uint)(afterOptionalHeaders - beforeOptionalHeaders);
            this.valuesFixers[ValuesFixerKeys.SizeOfOptionalHeaders].Size = 2;
            this.valuesFixers[ValuesFixerKeys.SizeOfHeaders].Value = (uint)this.output.Count;
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
            Utils.Write(this.output, 2, 2); // @todo Number of sections
            Utils.Write(this.output, 0, 4); // Timestamp
            Utils.Write(this.output, 0, 4); // Symbols stuff
            Utils.Write(this.output, 0, 4); // Symbols stuff
            this.valuesFixers[ValuesFixerKeys.SizeOfOptionalHeaders].Targets.Add(this.output.Count);
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
            this.valuesFixers[ValuesFixerKeys.CodeDiskSize].Targets.Add(this.output.Count);
            Utils.Write(this.output, 0, 4); // Size of code
            this.valuesFixers[ValuesFixerKeys.ImportsDiskOffset].Targets.Add(this.output.Count);
            Utils.Write(this.output, 0, 4); // Size of initialized data
            Utils.Write(this.output, 0, 4); // Size of uninitialized data
            this.valuesFixers[ValuesFixerKeys.EntryPoint].Targets.Add(this.output.Count);
            Utils.Write(this.output, 0, 4); // Entry point
            this.valuesFixers[ValuesFixerKeys.CodeVirtualOffset].Targets.Add(this.output.Count);
            Utils.Write(this.output, 0, 4); // Offset of code
            Utils.Write(this.output, 0, 4); // Offset of data
            Utils.Write(this.output, imageBase, 4); // Image base
            Utils.Write(this.output, pageSize, 4); // Section alignment
            Utils.Write(this.output, sectionAlignmentSize, 4); // File alignment
            Utils.Write(this.output, 5, 2); // Major os
            Utils.Write(this.output, 1, 2); // Minor os
            Utils.Write(this.output, 0, 2); // Major image
            Utils.Write(this.output, 0, 2); // Minor image
            Utils.Write(this.output, 5, 2); // Major subsystem
            Utils.Write(this.output, 1, 2); // Minor subsystem
            Utils.Write(this.output, 0, 4); // Reserved (Win32 version value)
            this.valuesFixers[ValuesFixerKeys.SizeOfImage].Targets.Add(this.output.Count);
            Utils.Write(this.output, 0, 4);
            this.valuesFixers[ValuesFixerKeys.SizeOfHeaders].Targets.Add(this.output.Count);
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
            this.valuesFixers[ValuesFixerKeys.ImageImportDescriptorsVirtualOffset].Targets.Add(this.output.Count);
            Utils.Write(this.output, 0, 4); // Import
            this.valuesFixers[ValuesFixerKeys.ImageImportDescriptorsVirtualSize].Targets.Add(this.output.Count);
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
            this.valuesFixers[ValuesFixerKeys.ImportsVirtualOffset].Targets.Add(this.output.Count);
            Utils.Write(this.output, 0, 4); // IAT
            this.valuesFixers[ValuesFixerKeys.ImportsVirtualSize].Targets.Add(this.output.Count);
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
            uint flags;


            // Imports
            Utils.Write(this.output, ".rdata", 8);

            this.valuesFixers[ValuesFixerKeys.ImportsVirtualSize].Targets.Add(this.output.Count);
            Utils.Write(this.output, 0, 4); // Virtual size
            this.valuesFixers[ValuesFixerKeys.ImportsVirtualOffset].Targets.Add(this.output.Count);
            Utils.Write(this.output, 0, 4); // RVA
            this.valuesFixers[ValuesFixerKeys.ImportsDiskSize].Targets.Add(this.output.Count);
            Utils.Write(this.output, 0, 4); // Size of raw data
            this.valuesFixers[ValuesFixerKeys.ImportsDiskOffset].Targets.Add(this.output.Count);
            Utils.Write(this.output, 0, 4); // Offset of imports in file
            Utils.Write(this.output, 0, 4); // Relocations
            Utils.Write(this.output, 0, 4); // Line numbers
            Utils.Write(this.output, 0, 2); // Num of relocations
            Utils.Write(this.output, 0, 2); // Num of line numbers

            flags = 0;
            flags |= 0x00000040; // Initialized data.
            flags |= 0x40000000; // Read.
            Utils.Write(this.output, flags, 4); // Flags


            // Code
            Utils.Write(this.output, ".text", 8);

            this.valuesFixers[ValuesFixerKeys.CodeVirtualSize].Targets.Add(this.output.Count);
            Utils.Write(this.output, 0, 4); // Virtual size
            this.valuesFixers[ValuesFixerKeys.CodeVirtualOffset].Targets.Add(this.output.Count);
            Utils.Write(this.output, 0, 4); // RVA
            this.valuesFixers[ValuesFixerKeys.CodeDiskSize].Targets.Add(this.output.Count);
            Utils.Write(this.output, 0, 4); // Size of raw data
            this.valuesFixers[ValuesFixerKeys.CodeDiskOffset].Targets.Add(this.output.Count);
            Utils.Write(this.output, 0, 4); // Offset of code in file
            Utils.Write(this.output, 0, 4); // Relocations
            Utils.Write(this.output, 0, 4); // Line numbers
            Utils.Write(this.output, 0, 2); // Num of relocations
            Utils.Write(this.output, 0, 2); // Num of line numbers

            flags = 0;
            flags |= 0x00000020; // Contains code.
            flags |= 0x20000000; // Exec.
            flags |= 0x40000000; // Read.
            Utils.Write(this.output, flags, 4); // Flags
        }
    }
}