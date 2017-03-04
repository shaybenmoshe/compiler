using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
    namespace AASM
    {
        public class AASM
        {
            public const int AddressSize = 4;

            private List<Opcode> opcodes = new List<Opcode>();

            public List<Opcode> Opcodes
            {
                get { return opcodes; }
            }

            public void Add(Opcode opcode)
            {
                this.opcodes.Add(opcode);
            }

            public void Add(AASM aasm)
            {
                this.opcodes.AddRange(aasm.Opcodes);
            }
        }

        public class Opcode
        {
        }

        public class Call : Opcode
        {
            private FunctionStatement function;

            public Call(FunctionStatement function)
            {
                this.function = function;
            }
        }

        public class AddSp : Opcode
        {
            private int offset;

            public AddSp(int offset)
            {
                this.offset = offset;
            }
        }

        public class GetLocal : Opcode
        {
            private int offset;

            public GetLocal(int offset)
            {
                this.offset = offset;
            }
        }

        public class SetLocal : Opcode
        {
            private int offset;

            public SetLocal(int offset)
            {
                this.offset = offset;
            }
        }

        public class Push : Opcode
        {
            private uint value;

            public Push(uint value)
            {
                this.value = value;
            }
        }

        public class Pop : Opcode
        {
        }

        public class Add : Opcode
        {
        }

        public class Gt : Opcode
        {
        }

        public class Ret : Opcode
        {
        }

        public class BaseJmp : Opcode
        {
            private int offset;

            public BaseJmp(int offset)
            {
                this.offset = offset;
            }

            public int Offset
            {
                get { return this.offset; }
                set { this.offset = value; }
            }
        }

        public class Jmp : BaseJmp
        {
            public Jmp(int offset) : base(offset)
            {
            }
        }

        public class JmpFalse : BaseJmp
        {
            public JmpFalse(int offset) : base(offset)
            {
            }
        }
    }
}