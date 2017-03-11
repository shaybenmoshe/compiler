using System;
using System.Collections.Generic;

namespace Compiler
{
    public class DLLImports
    {
        private List<ImportStatement> imports = new List<ImportStatement>();
        private string name;

        public DLLImports(string name)
        {
            this.name = name;
        }
        
        public List<ImportStatement> Imports
        {
            get { return this.imports; }
        }

        public string Name
        {
            get { return this.name; }
        }
    }

    public partial class PEFileBuilder
    {
        private uint importsRVA = 0x1000;
        private uint importsOffset;

        private Dictionary<string, DLLImports> dllImportsDictionary = new Dictionary<string, DLLImports>();
        private Dictionary<string, uint> importAddresses = new Dictionary<string, uint>();

        public uint ImportCurrentRVA
        {
            get { return (uint)this.output.Count + this.importsRVA - this.importsOffset; }
        }

        public Dictionary<string, uint> ImportAddresses
        {
            get { return this.importAddresses; }
        }

        public void EmitImports(List<ImportStatement> imports)
        {
            this.AlignToSectionAlignment();

            this.importsOffset = (uint)this.output.Count;

            this.BuildImportsDictionary(imports);
            this.EmitDLLNames();
            this.EmitThunks();
            this.EmitImageImportDescriptors();

            uint virtualEnd = (uint)this.output.Count;

            this.AlignToSectionAlignment();

            uint diskEnd = (uint)this.output.Count;

            this.valuesFixers[ValuesFixerKeys.ImportsDiskOffset].Value = this.importsOffset;
            this.valuesFixers[ValuesFixerKeys.ImportsDiskSize].Value = (uint)diskEnd - this.importsOffset;

            this.valuesFixers[ValuesFixerKeys.ImportsVirtualOffset].Value = this.importsRVA;
            this.valuesFixers[ValuesFixerKeys.ImportsVirtualSize].Value = (uint)virtualEnd - this.importsOffset;
        }

        private void BuildImportsDictionary(List<ImportStatement> imports)
        {
            for (int i = 0; i < imports.Count; i++)
            {
                if (!this.dllImportsDictionary.ContainsKey(imports[i].DLL.Value))
                {
                    this.dllImportsDictionary[imports[i].DLL.Value] = new DLLImports(imports[i].DLL.Value);
                }
                this.dllImportsDictionary[imports[i].DLL.Value].Imports.Add(imports[i]);
            }
        }

        private void EmitDLLNames()
        {
            foreach (string dll in this.dllImportsDictionary.Keys)
            {
                this.valuesFixersStr["dll.Name." + dll] = new ValuesFixer();
                this.valuesFixersStr["dll.Name." + dll].Value = this.ImportCurrentRVA;
                Utils.Write(this.output, dll + ".dll");
            }
        }

        private void EmitThunks()
        {
            foreach (DLLImports dllImports in this.dllImportsDictionary.Values)
            {
                string dllName = dllImports.Name;

                List<uint> actualThunksOffsets = new List<uint>();

                // Actual thunks
                for (int i = 0; i < dllImports.Imports.Count; i++)
                {
                    actualThunksOffsets.Add(this.ImportCurrentRVA);
                    Utils.Write(this.output, 0, 2); // Hint
                    Utils.Write(this.output, dllImports.Imports[i].Function.Value); // Name
                }


                // First thunk
                this.valuesFixersStr["dll.FT." + dllName] = new ValuesFixer();
                this.valuesFixersStr["dll.FT." + dllName].Value = this.ImportCurrentRVA;

                for (int i = 0; i < dllImports.Imports.Count; i++)
                {
                    this.importAddresses[dllImports.Imports[i].ImportedName.Value] = this.ImportCurrentRVA + imageBase; // @todo: handle image base better
                    Utils.Write(this.output, actualThunksOffsets[i], 4);
                }
                Utils.Write(this.output, 0, 4); // Empty entry


                // Original first thunk
                this.valuesFixersStr["dll.OFT." + dllName] = new ValuesFixer();
                this.valuesFixersStr["dll.OFT." + dllName].Value = this.ImportCurrentRVA;

                for (int i = 0; i < dllImports.Imports.Count; i++)
                {
                    Utils.Write(this.output, actualThunksOffsets[i], 4);
                }
                Utils.Write(this.output, 0, 4); // Empty entry
            }
        }

        private void EmitImageImportDescriptors()
        {
            int imageImportDescriptorsOffset = this.output.Count;

            foreach (string dll in this.dllImportsDictionary.Keys)
            {
                this.valuesFixersStr["dll.OFT." + dll].Targets.Add(this.output.Count);
                Utils.Write(this.output, 0, 4); // OFT

                Utils.Write(this.output, 0, 4); // TimeDateStamp
                Utils.Write(this.output, 0, 4); // ForwarderChain
                
                this.valuesFixersStr["dll.Name." + dll].Targets.Add(this.output.Count);
                Utils.Write(this.output, 0, 4); // Name

                this.valuesFixersStr["dll.FT." + dll].Targets.Add(this.output.Count);
                Utils.Write(this.output, 0, 4); // FT
            }

            // Write zero entry.
            Utils.Write(this.output, 0, 4); // OFT
            Utils.Write(this.output, 0, 4); // TimeDateStamp
            Utils.Write(this.output, 0, 4); // ForwarderChain
            Utils.Write(this.output, 0, 4); // Name
            Utils.Write(this.output, 0, 4); // FT

            int imageImportDescriptorsEnd = this.output.Count;

            this.valuesFixers[ValuesFixerKeys.ImageImportDescriptorsVirtualOffset].Value =
                (uint)imageImportDescriptorsOffset + this.importsRVA - this.importsOffset;
            this.valuesFixers[ValuesFixerKeys.ImageImportDescriptorsVirtualSize].Value =
                (uint)(imageImportDescriptorsEnd - imageImportDescriptorsOffset);
        }
    }
}