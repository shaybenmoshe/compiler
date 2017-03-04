using System;
using System.Collections.Generic;
using System.Text;
using Compiler.AASM;

namespace Compiler
{
    public partial class LL
    {
        public void LLAASMEmit()
        {
            for (int i = 0; i < this.functions.Count; i++)
            {
                this.functions[i].LLAASM = this.functions[i].LLAASMEmit();
            }
        }
    }

    public partial class Statement : PositionThing
    {
        public virtual AASM.AASM LLAASMEmit()
        {
            // @todo
            throw new CompilerException("Not implemented!", this.Position);
        }
    }

    public partial class FunctionStatement : Statement
    {
        private AASM.AASM llAASM;

        public AASM.AASM LLAASM
        {
            get { return this.llAASM; }
            set { this.llAASM = value; }
        }

        public override AASM.AASM LLAASMEmit()
        {
            AASM.AASM aasm = new AASM.AASM();
            aasm.Add(new AASM.AddSp(this.llAASMSize));
            aasm.Add(this.body.LLAASMEmit());
            return aasm;
        }
    }

    public partial class CompoundStatement : Statement
    {
        public override AASM.AASM LLAASMEmit()
        {
            AASM.AASM aasm = new AASM.AASM();
            for (int i = 0; i < this.statements.Count; i++)
            {
                aasm.Add(this.statements[i].LLAASMEmit());
            }
            return aasm;
        }
    }

    public partial class VarStatement : Statement
    {
        public override AASM.AASM LLAASMEmit()
        {
            AASM.AASM aasm = new AASM.AASM();
            aasm.Add(this.value.LLAASMEmit());
            aasm.Add(new AASM.SetLocal(this.nameDef.LLAASMOffset));
            aasm.Add(new AASM.Pop());
            return aasm;
        }
    }

    public partial class ReturnStatement : Statement
    {
        public override AASM.AASM LLAASMEmit()
        {
            AASM.AASM aasm = new AASM.AASM();
            aasm.Add(this.value.LLAASMEmit());
            aasm.Add(new AASM.Ret());
            return aasm;
        }
    }

    public partial class IfElseStatement : Statement
    {
        public override AASM.AASM LLAASMEmit()
        {
            AASM.AASM aasm = new AASM.AASM();

            int beforeCondPos = aasm.Opcodes.Count;
            aasm.Add(this.cond.LLAASMEmit());

            AASM.JmpFalse condJmp = new JmpFalse(0);
            aasm.Add(condJmp);
            
            aasm.Add(this.body.LLAASMEmit());
            int condJmpTargetPos = aasm.Opcodes.Count;

            if (this.elseBody != null)
            {
                int beforeSkipElseBodyPos = aasm.Opcodes.Count;
                AASM.Jmp skipElseBodyJmp = new Jmp(0);
                aasm.Add(skipElseBodyJmp);
                condJmpTargetPos = aasm.Opcodes.Count;

                aasm.Add(this.elseBody.LLAASMEmit());
                int afterElseBodyPos = aasm.Opcodes.Count;

                skipElseBodyJmp.Offset = afterElseBodyPos - beforeSkipElseBodyPos;
            }

            condJmp.Offset = condJmpTargetPos - beforeCondPos;

            return aasm;
        }
    }

    public partial class ExpressionStatement : Statement
    {
        public override AASM.AASM LLAASMEmit()
        {
            AASM.AASM aasm = new AASM.AASM();
            aasm.Add(this.expression.LLAASMEmit());
            aasm.Add(new AASM.Pop());
            return aasm;
        }
    }




    public partial class Expression : PositionThing
    {
        public virtual AASM.AASM LLAASMEmit()
        {
            // @todo
            throw new CompilerException("Not implemented!", this.Position);
        }
    }

    public partial class ParentheseExpression : Expression
    {
        public override AASM.AASM LLAASMEmit()
        {
            return this.value.LLAASMEmit();
        }
    }

    public partial class ImmediateExpression : Expression
    {
        public override AASM.AASM LLAASMEmit()
        {
            if (this.value is NumberToken)
            {
                AASM.AASM aasm = new AASM.AASM();
                aasm.Add(new AASM.Push((this.value as NumberToken).Value));
                return aasm;
            }

            throw new CompilerException("Don't know how to AASM immediate which is not a number.", this.Position);
        }
    }

    public partial class NameExpression : ImmediateExpression
    {
        public override AASM.AASM LLAASMEmit()
        {
            AASM.AASM aasm = new AASM.AASM();
            aasm.Add(new AASM.GetLocal(this.llNameDef.LLAASMOffset));
            return aasm;
        }
    }

    public partial class CallExpression : Expression
    {
        public override AASM.AASM LLAASMEmit()
        {
            AASM.AASM aasm = new AASM.AASM();
            for (int i = this.parameters.Count - 1; i >= 0; i--)
            {
                aasm.Add(this.parameters[i].LLAASMEmit());
            }
            aasm.Add(new AASM.Call(this.llTarget));
            // @todo: clean stack
            return aasm;
        }
    }

    public partial class BinaryOpExpression : Expression
    {
        public override AASM.AASM LLAASMEmit()
        {
            AASM.AASM aasm = new AASM.AASM();

            // Assignment is completley different.
            if (this.binaryOpToken.Value == BinaryOpToken.Ops.Ass)
            {
                aasm.Add(this.operand2.LLAASMEmit());

                if (!(this.operand1 is NameExpression))
                {
                    throw new CompilerException("Left side of assignment must be a local or argument.", this.Position);
                }
                NameExpression left = this.operand1 as NameExpression;
                aasm.Add(new AASM.SetLocal(left.LLNameDef.LLAASMOffset));

                return aasm;
            }

            aasm.Add(this.operand1.LLAASMEmit());
            aasm.Add(this.operand2.LLAASMEmit());

            switch (this.binaryOpToken.Value)
            {
                case BinaryOpToken.Ops.Add:
                    aasm.Add(new AASM.Add());
                    break;
                case BinaryOpToken.Ops.Gt:
                    aasm.Add(new AASM.Gt());
                    break;
                default:
                    throw new CompilerException("Don't know how to AASM binary op " + this.binaryOpToken.Value + ".", this.Position);
            }

            return aasm;
        }
    }
}