using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
    public interface IAASMType
    {
        uint Size { get; }
    }

    public class AASMNativeType : IAASMType
    {
        private uint size;
        private string name;

        public AASMNativeType(uint size, string name)
        {
            this.size = size;
            this.name = name;
        }

        public uint Size
        {
            get { return this.size; }
        }

        public static AASMNativeType AASMUint32 = new AASMNativeType(4, "uint32");
    }

    public partial class AASMStructType : IAASMType
    {
        private uint size;
        private StructStatement structStatement;

        public AASMStructType(StructStatement structStatement)
        {
            this.structStatement = structStatement;
        }

        // @todo
        public uint Size
        {
            get { return this.size; }
        }

        public StructStatement StructStatement
        {
            get { return this.structStatement; }
        }
    }
}