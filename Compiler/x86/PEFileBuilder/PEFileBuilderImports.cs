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
    }

    public partial class PEFileBuilder
    {
        private uint importsRVA = 0x1000;
        
        private Dictionary<string, DLLImports> dllImportsDictionary = new Dictionary<string, DLLImports>();

        public void EmitImports(List<ImportStatement> imports)
        {
            this.BuildImportsDictionary(imports);
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

        private void EmitDLLsName()
        {
        }
    }
}