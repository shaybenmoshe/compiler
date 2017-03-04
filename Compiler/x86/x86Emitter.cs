using System;
using System.Collections.Generic;

namespace Compiler
{
    public class x86Emitter
    {
        private AASM.AASM aasm;
        private List<byte> x86 = new List<byte>();

        public x86Emitter(AASM.AASM aasm)
        {
            this.aasm = aasm;
        }

        public List<Byte> Emit()
        {
            return this.x86;
        }

        private void EmitCall(AASM.Call op)
        {
            
        }
    }
}