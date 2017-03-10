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

            public FunctionStatement Function
            {
                get { return this.function; }
            }
        }

        public class AddSp : Opcode
        {
            private int offset;

            public AddSp(int offset)
            {
                this.offset = offset;
            }

            public int Offset
            {
                get { return this.offset; }
            }
        }

        public class VarAccess : Opcode
        {
            private int offset;

            public VarAccess(int offset)
            {
                this.offset = offset;
            }

            public int Offset
            {
                get { return this.offset; }
            }
        }

        public class GetLocal : VarAccess
        {
            public GetLocal(int offset) : base(offset)
            {
            }
        }

        public class SetLocal : VarAccess
        {
            public SetLocal(int offset) : base(offset)
            {
            }
        }

        public class GetLocalStruct : VarAccess
        {
            public GetLocalStruct(int offset) : base(offset)
            {
            }
        }
        
        public class GetArgument : VarAccess
        {
            public GetArgument(int offset) : base(offset)
            {
            }
        }

        public class SetArgument : VarAccess
        {
            public SetArgument(int offset) : base(offset)
            {
            }
        }

        public class Push : Opcode
        {
            private uint value;

            public Push(uint value)
            {
                this.value = value;
            }

            public uint Value
            {
                get { return this.value; }
            }
        }

        public class PushRetVal : Opcode
        {
        }

        public class Pop : Opcode
        {
        }

        public class Add : Opcode
        {
        }

        public class Sub : Opcode
        {
        }

        public class Mul : Opcode
        {
        }

        public class Gt : Opcode
        {
        }

        public class Ret : Opcode
        {
        }

        public class Int3 : Opcode
        {
        }

        public class BaseJmp : Opcode
        {
            private Label target;

            public BaseJmp(Label target)
            {
                this.target = target;
            }

            public Label Target
            {
                get { return this.target; }
                set { this.target = value; }
            }
        }

        public class Jmp : BaseJmp
        {
            public Jmp(Label target) : base(target)
            {
            }
        }

        public class JmpFalse : BaseJmp
        {
            public JmpFalse(Label target) : base(target)
            {
            }
        }

        public class Label : Opcode
        {
        }

        public class Deref : Opcode
        {
        }

        public class MemberBase : Opcode
        {
            private AASMStructType structType;
            private int member;

            public MemberBase(AASMStructType structType, int member)
            {
                this.structType = structType;
                this.member = member;
            }

            public AASMStructType StructType
            {
                get { return this.structType; }
            }

            public int Member
            {
                get { return this.member; }
            }
        }

        public class MemberRead : MemberBase
        {
            public MemberRead(AASMStructType structType, int member) : base(structType, member)
            {
            }
        }

        public class MemberWrite : MemberBase
        {
            public MemberWrite(AASMStructType structType, int member) : base(structType, member)
            {
            }
        }
    }
}