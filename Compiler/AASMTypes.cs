using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
    public class AASMType
    {
        private int size;
        private string name;

        public AASMType(int size, string name)
        {
            this.size = size;
            this.name = name;
        }

        public int Size
        {
            get { return this.size; }
        }

        public static AASMType Resolve(string name)
        {
            return AASMUint32;
        }

        public static AASMType AASMUint32 = new AASMType(4, "uint32");
    }
}