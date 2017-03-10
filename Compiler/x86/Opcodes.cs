using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
    namespace x86
    {
        public class Opcode
        {
            private uint offsetInX86;

            public uint OffsetInX86
            {
                get { return this.offsetInX86; }
                set { this.offsetInX86 = value; }
            }

            public virtual uint X86Size
            {
                get { return (uint)this.Emit().Count; }
            }

            public virtual List<byte> Emit()
            {
                return null;
            }

            public virtual int GetStackChange()
            {
                return 0;
            }
        }

        public class Label : Opcode
        {
            public override List<byte> Emit()
            {
                return new List<byte>();
            }
        }

        public class PopEax : Opcode
        {
            public override List<byte> Emit()
            {
                List<byte> x86 = new List<byte>();
                x86.Add(0x58);
                return x86;
            }

            public override int GetStackChange()
            {
                return -4;
            }
        }

        public class PopEcx : Opcode
        {
            public override List<byte> Emit()
            {
                List<byte> x86 = new List<byte>();
                x86.Add(0x59);
                return x86;
            }

            public override int GetStackChange()
            {
                return -4;
            }
        }

        public class PopEbp : Opcode
        {
            public override List<byte> Emit()
            {
                List<byte> x86 = new List<byte>();
                x86.Add(0x5d);
                return x86;
            }

            public override int GetStackChange()
            {
                return -4;
            }
        }

        public class PushEax : Opcode
        {
            public override List<byte> Emit()
            {
                List<byte> x86 = new List<byte>();
                x86.Add(0x50);
                return x86;
            }

            public override int GetStackChange()
            {
                return 4;
            }
        }

        public class PushEbp : Opcode
        {
            public override List<byte> Emit()
            {
                List<byte> x86 = new List<byte>();
                x86.Add(0x55);
                return x86;
            }

            public override int GetStackChange()
            {
                return 4;
            }
        }

        public class Call : Opcode
        {
            private FunctionStatement targetFunction;
            private x86FunctionEmitter ownerEmitterFunction;
            private x86FunctionEmitter targetEmitterFunction;

            public Call(x86FunctionEmitter ownerEmitterFunction, FunctionStatement targetFunction)
            {
                this.ownerEmitterFunction = ownerEmitterFunction;
                this.targetFunction = targetFunction;
            }

            public FunctionStatement TargetFunction
            {
                get { return this.targetFunction; }
            }

            public x86FunctionEmitter TargetEmitterFunction
            {
                set { this.targetEmitterFunction = value; }
            }

            public override List<byte> Emit()
            {
                uint offset = this.targetEmitterFunction.OffsetInX86
                    - (this.ownerEmitterFunction.OffsetInX86 + this.OffsetInX86 + this.X86Size);

                List<byte> x86 = new List<byte>();
                x86.Add(0xe8);
                Utils.Write(x86, offset, 4);
                return x86;
            }

            public override uint X86Size
            {
                get { return 5; }
            }
        }

        public class AddEsp : Opcode
        {
            private int offset;

            public AddEsp(int offset)
            {
                this.offset = offset;
            }

            public int Offset
            {
                get { return this.offset; }
            }

            public override List<byte> Emit()
            {
                List<byte> x86 = new List<byte>();
                Utils.Write(x86, 0xc481, 2); // add esp, ?
                Utils.Write(x86, (uint)this.offset, 4);
                return x86;
            }

            public override int GetStackChange()
            {
                return this.offset;
            }
        }

        public class MovEaxDerefEbp : Opcode
        {
            private int offset;

            public MovEaxDerefEbp(int offset)
            {
                this.offset = offset;
            }

            public int Offset
            {
                get { return this.offset; }
            }

            public override List<byte> Emit()
            {
                List<byte> x86 = new List<byte>();
                Utils.Write(x86, 0x458b, 2); // mov eax, dword ptr [ebp+?]
                x86.Add((byte)this.offset);
                return x86;
            }
        }

        public class SetDerefEbpEax : Opcode
        {
            private int offset;

            public SetDerefEbpEax(int offset)
            {
                this.offset = offset;
            }

            public int Offset
            {
                get { return this.offset; }
            }

            public override List<byte> Emit()
            {
                List<byte> x86 = new List<byte>();
                Utils.Write(x86, 0x4589, 2); // mov dword ptr [ebp+?], eax
                x86.Add((byte)this.offset);
                return x86;
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

            public override List<byte> Emit()
            {
                List<byte> x86 = new List<byte>();
                Utils.Write(x86, 0x68, 1); // push ?
                Utils.Write(x86, this.value, 4);
                return x86;
            }

            public override int GetStackChange()
            {
                return 4;
            }
        }

        public class AddEaxEcx : Opcode
        {
            public override List<byte> Emit()
            {
                List<byte> x86 = new List<byte>();
                Utils.Write(x86, 0xc801, 2); // add eax, ecx
                return x86;
            }
        }

        public class MovEbpEsp : Opcode
        {
            public override List<byte> Emit()
            {
                List<byte> x86 = new List<byte>();
                Utils.Write(x86, 0xec8b, 2); // mob ebp, esp
                return x86;
            }
        }

        public class MovEspEbp : Opcode
        {
            public override List<byte> Emit()
            {
                List<byte> x86 = new List<byte>();
                Utils.Write(x86, 0xe58b, 2); // mob esp, ebp
                return x86;
            }
        }

        public class CmpEax0 : Opcode
        {
            public override List<byte> Emit()
            {
                List<byte> x86 = new List<byte>();
                x86.Add(0x3d); // cmp eax, ?
                Utils.Write(x86, 0, 4);
                return x86;
            }
        }

        public class PushEaxGtEcx : Opcode
        {
            public override List<byte> Emit()
            {
                List<byte> x86 = new List<byte>();
                Utils.Write(x86, 0xc839, 2); // cmp eax, ecx
                Utils.Write(x86, 0x0477, 2); // ja+4
                Utils.Write(x86, 0x006a, 2); // push 0
                Utils.Write(x86, 0x02eb, 2); // jmp+2
                Utils.Write(x86, 0x016a, 2); // push 1
                return x86;
            }

            public override int GetStackChange()
            {
                return 4;
            }
        }

        public class Ret : Opcode
        {
            public override List<byte> Emit()
            {
                List<byte> x86 = new List<byte>();
                x86.Add(0xc3); // ret
                return x86;
            }
        }

        public class Jmp : Opcode
        {
            private Label label;

            public Jmp(Label label)
            {
                this.label = label;
            }

            public override List<byte> Emit()
            {
                uint offset = this.label.OffsetInX86 - (this.OffsetInX86 + this.X86Size);

                List<byte> x86 = new List<byte>();
                x86.Add(0xe9); // jmp
                Utils.Write(x86, offset, 4);
                return x86;
            }

            public override uint X86Size
            {
                get { return 5; }
            }
        }

        public class Je : Opcode
        {
            private Label label;

            public Je(Label label)
            {
                this.label = label;
            }

            public override List<byte> Emit()
            {
                uint offset = this.label.OffsetInX86 - (this.OffsetInX86 + this.X86Size);

                List<byte> x86 = new List<byte>();
                Utils.Write(x86, 0x840f, 2); // je ?
                Utils.Write(x86, offset, 4);
                return x86;
            }

            public override uint X86Size
            {
                get { return 6; }
            }
        }

        public class Jne : Opcode
        {
            private Label label;

            public Jne(Label label)
            {
                this.label = label;
            }

            public override List<byte> Emit()
            {
                uint offset = this.label.OffsetInX86 - (this.OffsetInX86 + this.X86Size);

                List<byte> x86 = new List<byte>();
                Utils.Write(x86, 0x850f, 2); // jne ?
                Utils.Write(x86, offset, 4);
                return x86;
            }

            public override uint X86Size
            {
                get { return 6; }
            }
        }

        public class Int3 : Opcode
        {
            public override List<byte> Emit()
            {
                List<byte> x86 = new List<byte>();
                x86.Add(0xcc); // int3
                return x86;
            }
        }
    }
}