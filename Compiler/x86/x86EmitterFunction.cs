using System;
using System.Collections.Generic;

namespace Compiler
{
    public class x86FunctionEmitter
    {
        private AASM.AASM aasm;
        private List<x86.Opcode> opcodes = new List<x86.Opcode>();
        private Dictionary<AASM.Label, x86.Label> labelsDictionary = new Dictionary<AASM.Label, x86.Label>();
        private uint x86Size;
        private uint offsetInX86;

        public x86FunctionEmitter(AASM.AASM aasm)
        {
            this.aasm = aasm;
        }
        
        public uint X86Size
        {
            get { return this.x86Size; }
        }

        public uint OffsetInX86
        {
            get { return this.offsetInX86; }
            set { this.offsetInX86 = value; }
        }

        public void ResolveLabels()
        {
            for (int i = 0; i < this.aasm.Opcodes.Count; i++)
            {
                if (!(this.aasm.Opcodes[i] is AASM.Label))
                {
                    continue;
                }

                AASM.Label op = this.aasm.Opcodes[i] as AASM.Label;

                this.labelsDictionary[op] = new x86.Label();
            }
        }

        public void CreateOpcodes(Dictionary<string, uint> importAddresses)
        {
            this.opcodes.Add(new x86.PushEbp());
            this.opcodes.Add(new x86.MovEbpEsp());

            for (int i = 0; i < this.aasm.Opcodes.Count; i++)
            {
                AASM.Opcode op = this.aasm.Opcodes[i];

                if (op is AASM.Label)
                {
                    this.opcodes.Add(this.labelsDictionary[op as AASM.Label]);
                }
                else if (op is AASM.Call)
                {
                    this.opcodes.Add(new x86.Call(this, (op as AASM.Call).Function));
                }
                else if (op is AASM.CallImport)
                {
                    uint address = importAddresses[(op as AASM.CallImport).Import.ImportedName.Value];
                    this.opcodes.Add(new x86.CallAbsolute(address));
                }
                else if (op is AASM.AddSp)
                {
                    this.opcodes.Add(new x86.AddEsp(-(op as AASM.AddSp).Offset));
                }
                else if (op is AASM.GetLocal)
                {
                    this.opcodes.Add(new x86.MovEaxDerefEbp(-((op as AASM.GetLocal).Offset)));
                    this.opcodes.Add(new x86.PushEax());
                }
                else if (op is AASM.SetLocal)
                {
                    this.opcodes.Add(new x86.PopEax());
                    this.opcodes.Add(new x86.PushEax()); // We still want it on the stack.
                    this.opcodes.Add(new x86.MovDerefEbpEax(-((op as AASM.SetLocal).Offset)));
                }
                else if (op is AASM.GetLocalStruct)
                {
                    this.opcodes.Add(new x86.LeaEaxDerefEbp(-((op as AASM.GetLocalStruct).Offset)));
                    this.opcodes.Add(new x86.PushEax());
                }
                else if (op is AASM.GetArgument)
                {
                    this.opcodes.Add(new x86.MovEaxDerefEbp(2 * AASM.AASM.AddressSize + (op as AASM.GetArgument).Offset));
                    this.opcodes.Add(new x86.PushEax());
                }
                else if (op is AASM.SetArgument)
                {
                    this.opcodes.Add(new x86.PopEax());
                    this.opcodes.Add(new x86.PushEax()); // We still want it on the stack.
                    this.opcodes.Add(new x86.MovDerefEbpEax(2 * AASM.AASM.AddressSize + (op as AASM.SetArgument).Offset));
                }
                else if (op is AASM.Push)
                {
                    this.opcodes.Add(new x86.Push((op as AASM.Push).Value));
                }
                else if (op is AASM.PushRetVal)
                {
                    this.opcodes.Add(new x86.PushEax());
                }
                else if (op is AASM.Pop)
                {
                    this.opcodes.Add(new x86.PopEax());
                }
                else if (op is AASM.And)
                {
                    this.opcodes.Add(new x86.PopEcx());
                    this.opcodes.Add(new x86.PopEax());
                    this.opcodes.Add(new x86.AndEaxEcx());
                    this.opcodes.Add(new x86.PushEax());
                }
                else if (op is AASM.Or)
                {
                    this.opcodes.Add(new x86.PopEcx());
                    this.opcodes.Add(new x86.PopEax());
                    this.opcodes.Add(new x86.OrEaxEcx());
                    this.opcodes.Add(new x86.PushEax());
                }
                else if (op is AASM.Add)
                {
                    this.opcodes.Add(new x86.PopEcx());
                    this.opcodes.Add(new x86.PopEax());
                    this.opcodes.Add(new x86.AddEaxEcx());
                    this.opcodes.Add(new x86.PushEax());
                }
                else if (op is AASM.Sub)
                {
                    this.opcodes.Add(new x86.PopEcx());
                    this.opcodes.Add(new x86.PopEax());
                    this.opcodes.Add(new x86.SubEaxEcx());
                    this.opcodes.Add(new x86.PushEax());
                }
                else if (op is AASM.Mul)
                {
                    this.opcodes.Add(new x86.PopEcx());
                    this.opcodes.Add(new x86.PopEax());
                    this.opcodes.Add(new x86.MulEaxEcx());
                    this.opcodes.Add(new x86.PushEax());
                }
                else if (op is AASM.Eq)
                {
                    this.opcodes.Add(new x86.PopEcx());
                    this.opcodes.Add(new x86.PopEax());
                    this.opcodes.Add(new x86.PushEaxEqEcx());
                }
                else if (op is AASM.Neq)
                {
                    this.opcodes.Add(new x86.PopEcx());
                    this.opcodes.Add(new x86.PopEax());
                    this.opcodes.Add(new x86.PushEaxNeqEcx());
                }
                else if (op is AASM.Gt)
                {
                    this.opcodes.Add(new x86.PopEcx());
                    this.opcodes.Add(new x86.PopEax());
                    this.opcodes.Add(new x86.PushEaxGtEcx());
                }
                else if (op is AASM.Lt)
                {
                    this.opcodes.Add(new x86.PopEcx());
                    this.opcodes.Add(new x86.PopEax());
                    this.opcodes.Add(new x86.PushEaxLtEcx());
                }
                else if (op is AASM.Gte)
                {
                    this.opcodes.Add(new x86.PopEcx());
                    this.opcodes.Add(new x86.PopEax());
                    this.opcodes.Add(new x86.PushEaxGteEcx());
                }
                else if (op is AASM.Lte)
                {
                    this.opcodes.Add(new x86.PopEcx());
                    this.opcodes.Add(new x86.PopEax());
                    this.opcodes.Add(new x86.PushEaxLteEcx());
                }
                else if (op is AASM.Ret)
                {
                    this.opcodes.Add(new x86.PopEax()); // Get ret val.
                    this.opcodes.Add(new x86.MovEspEbp());
                    this.opcodes.Add(new x86.PopEbp());
                    this.opcodes.Add(new x86.Ret());
                }
                else if (op is AASM.Int3)
                {
                    this.opcodes.Add(new x86.Int3());
                }
                else if (op is AASM.Jmp)
                {
                    this.opcodes.Add(new x86.Jmp(this.labelsDictionary[(op as AASM.Jmp).Target]));
                }
                else if (op is AASM.JmpFalse)
                {
                    this.opcodes.Add(new x86.PopEax());
                    this.opcodes.Add(new x86.CmpEax0());
                    this.opcodes.Add(new x86.Je(this.labelsDictionary[(op as AASM.JmpFalse).Target]));
                }
                else if (op is AASM.MemberRead)
                {
                    AASM.MemberRead memberRead = op as AASM.MemberRead;
                    uint offset = memberRead.StructType.StructStatement.Members[memberRead.Member].LLAASMStructOffset;

                    this.opcodes.Add(new x86.PopEax());
                    this.opcodes.Add(new x86.MovEaxDerefEax(offset));
                    this.opcodes.Add(new x86.PushEax());
                }
                else if (op is AASM.MemberWrite)
                {
                    AASM.MemberWrite memberWrite = op as AASM.MemberWrite;
                    uint offset = memberWrite.StructType.StructStatement.Members[memberWrite.Member].LLAASMStructOffset;

                    this.opcodes.Add(new x86.PopEax());
                    this.opcodes.Add(new x86.PopEcx());
                    this.opcodes.Add(new x86.MovDerefEaxEcx(offset));
                    this.opcodes.Add(new x86.PushEax());
                }
                else
                {
                    throw new Exception("Don't know how to emit opcode " + op.ToString());
                }
            }
        }

        public void Optimize()
        {
            this.OptimizePushEaxPopEax();
        }

        public void OptimizePushEaxPopEax()
        {
            for (int i = 0; i < this.opcodes.Count - 1; i++)
            {
                if (!(this.opcodes[i] is x86.PushEax))
                {
                    continue;
                }
                if (!(this.opcodes[i + 1] is x86.PopEax))
                {
                    continue;
                }
                this.opcodes.RemoveRange(i, 2);
                i -= 2;
            }
        }

        public void ResolveOffsesInX86()
        {
            uint offset = 0;

            for (int i = 0; i < this.opcodes.Count; i++)
            {
                x86.Opcode op = this.opcodes[i];
                op.OffsetInX86 = offset;
                offset += op.X86Size;
            }

            this.x86Size = offset;
        }

        public void LinkFunctions(Dictionary<FunctionStatement, x86FunctionEmitter> functionsDictionary)
        {
            for (int i = 0; i < this.opcodes.Count; i++)
            {
                if (!(this.opcodes[i] is x86.Call))
                {
                    continue;
                }

                x86.Call op = this.opcodes[i] as x86.Call;
                op.TargetEmitterFunction = functionsDictionary[op.TargetFunction];
            }
        }

        public List<byte> Emit()
        {
            List<byte> x86 = new List<byte>();
            for (int i = 0; i < this.opcodes.Count; i++)
            {
                x86.AddRange(this.opcodes[i].Emit());
            }
            return x86;
        }
    }
}