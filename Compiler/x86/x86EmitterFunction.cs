using System;
using System.Collections.Generic;

namespace Compiler
{
    public class x86FunctionEmitter
    {
        private int stack;
        private AASM.AASM aasm;
        private List<byte> x86 = new List<byte>();

        public x86FunctionEmitter(AASM.AASM aasm)
        {
            this.aasm = aasm;
            this.stack = 0;
        }

        public List<byte> X86
        {
            get { return this.x86; }
        }

        private void EmitPopEax()
        {
            this.stack -= 4;
            this.x86.Add(0x58);
        }

        private void EmitPopEcx()
        {
            this.stack -= 4;
            this.x86.Add(0x59);
        }

        private void EmitPushEax()
        {
            this.stack += 4;
            this.x86.Add(0x50);
        }

        public void Emit()
        {
            int offset;

            // @todo add push ebp; mov ebp, esp

            for (int i = 0; i < this.aasm.Opcodes.Count; i++)
            {
                AASM.Opcode op = this.aasm.Opcodes[i];

                if (op is AASM.Call)
                {
                    this.x86.Add(0xe8);
                    Utils.Write(this.x86, 0, 4); // @todo fix target
                    // @todo this.stack after clean stack
                }
                else if (op is AASM.AddSp)
                {
                    offset = (op as AASM.AddSp).Offset;
                    Utils.Write(this.x86, 0xc481, 2); // add esp, ?
                    Utils.Write(this.x86, (uint)offset, 4);
                    this.stack += offset;
                }
                else if (op is AASM.GetLocal)
                {
                    offset = (op as AASM.GetLocal).Offset;
                    Utils.Write(this.x86, 0x458b, 2); // mov eax, dword ptr [ebp+?]
                    this.x86.Add((byte)offset);
                    this.EmitPushEax();
                }
                else if (op is AASM.SetLocal)
                {
                    offset = (op as AASM.SetLocal).Offset;
                    this.EmitPopEax();
                    this.EmitPushEax(); // We still want it on the stack.
                    Utils.Write(this.x86, 0x4589, 2); // mov dword ptr [ebp+?], eax
                    this.x86.Add((byte)offset);
                }
                else if (op is AASM.Push)
                {
                    this.stack += 4;
                    Utils.Write(this.x86, 0x68, 1); // push ?
                    Utils.Write(this.x86, (op as AASM.Push).Value, 4);
                }
                else if (op is AASM.Pop)
                {
                    this.EmitPopEax();
                }
                else if (op is AASM.Add)
                {
                    this.EmitPopEax();
                    this.EmitPopEcx();
                    Utils.Write(this.x86, 0xc801, 2); // add eax, ecx
                    this.EmitPushEax();
                }
                else if (op is AASM.Gt)
                {
                    this.EmitPopEax();
                    this.EmitPopEcx();
                    Utils.Write(this.x86, 0xc839, 2); // cmp eax, ecx
                    Utils.Write(this.x86, 0x0477, 2); // ja+4
                    Utils.Write(this.x86, 0x006a, 2); // push 0
                    Utils.Write(this.x86, 0x02eb, 2); // jmp+2
                    Utils.Write(this.x86, 0x016a, 2); // push 1
                    this.stack += 4;
                }
                else if (op is AASM.Ret)
                {
                    Utils.Write(this.x86, 0xc481, 2); // add esp, ?
                    Utils.Write(this.x86, (uint)this.stack, 4);
                    this.x86.Add(0xc3); // ret
                }
                else if (op is AASM.Jmp)
                {
                    this.x86.Add(0xe9); // jmp
                    Utils.Write(this.x86, 0, 4); // @todo fix offset
                }
                else if (op is AASM.JmpFalse)
                {
                    this.EmitPopEax();
                    this.x86.Add(0xa9); // test eax, ?
                    Utils.Write(this.x86, 0, 4);
                    Utils.Write(this.x86, 0x840f, 4); // je ?
                    Utils.Write(this.x86, 0, 4); // @todo fix offset
                }
                else
                {
                    throw new Exception("Don't know how to emit opcode " + op.ToString());
                }
            }
        }
    }
}