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
            // Set default value.
            /*aasm.Add(this.value.LLAASMEmit());
            aasm.Add(new AASM.SetLocal(this.nameDef.LLAASMOffset));
            aasm.Add(new AASM.Pop());*/
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

    public partial class Int3Statement : Statement
    {
        public override AASM.AASM LLAASMEmit()
        {
            AASM.AASM aasm = new AASM.AASM();
            aasm.Add(new AASM.Int3());
            return aasm;
        }
    }

    public partial class IfElseStatement : Statement
    {
        public override AASM.AASM LLAASMEmit()
        {
            AASM.AASM aasm = new AASM.AASM();

            AASM.Label afterIfLabel = new Label();
            AASM.Label afterAllLabel = new Label();

            aasm.Add(this.cond.LLAASMEmit());
            aasm.Add(new JmpFalse(afterIfLabel));

            aasm.Add(this.body.LLAASMEmit());
            aasm.Add(afterIfLabel);

            if (this.elseBody != null)
            {
                aasm.Add(new Jmp(afterAllLabel));
                aasm.Add(this.elseBody.LLAASMEmit());
            }

            aasm.Add(afterAllLabel);

            return aasm;
        }
    }

    public partial class WhileStatement : Statement
    {
        public override AASM.AASM LLAASMEmit()
        {
            AASM.AASM aasm = new AASM.AASM();

            AASM.Label beforeCondLabel = new Label();
            AASM.Label afterWhileLabel = new Label();

            aasm.Add(beforeCondLabel);
            aasm.Add(this.cond.LLAASMEmit());
            aasm.Add(new JmpFalse(afterWhileLabel));

            aasm.Add(this.body.LLAASMEmit());

            aasm.Add(new Jmp(beforeCondLabel));

            aasm.Add(afterWhileLabel);

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

        public virtual AASM.AASM LLAASMEmitWrite()
        {
            // @todo
            throw new CompilerException("Write is invalid for this Expression!", this.Position);
        }
    }

    public partial class ParentheseExpression : Expression
    {
        public override AASM.AASM LLAASMEmit()
        {
            return this.value.LLAASMEmit();
        }

        public override AASM.AASM LLAASMEmitWrite()
        {
            return this.value.LLAASMEmitWrite();
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
            if (this.llNameDef.DefPosition == NameDefStatement.DefPositionTypes.Local)
            {
                /*if (this.LLAASMType is AASMStructType)
                {
                    aasm.Add(new AASM.GetLocalStruct(this.llNameDef.LLAASMOffset));
                }
                else
                {*/
                    aasm.Add(new AASM.GetLocal(this.llNameDef.LLAASMOffset));
                //}
            }
            else if (this.llNameDef.DefPosition == NameDefStatement.DefPositionTypes.Argument)
            {
                aasm.Add(new AASM.GetArgument(this.llNameDef.LLAASMOffset));
            }
            return aasm;
        }

        public override AASM.AASM LLAASMEmitWrite()
        {
            AASM.AASM aasm = new AASM.AASM();
            if (this.llNameDef.DefPosition == NameDefStatement.DefPositionTypes.Local)
            {
                /*if (this.LLAASMType is AASMStructType)
                {
                    throw new CompilerException("Can't set local struct", this.Position);
                }
                else
                {*/
                    aasm.Add(new AASM.SetLocal(this.llNameDef.LLAASMOffset));
                //}
            }
            else if (this.llNameDef.DefPosition == NameDefStatement.DefPositionTypes.Argument)
            {
                aasm.Add(new AASM.SetArgument(this.llNameDef.LLAASMOffset));
            }
            return aasm;
        }
    }

    public partial class SizeofExpression : Expression
    {
        public override AASM.AASM LLAASMEmit()
        {
            AASM.AASM aasm = new AASM.AASM();
            aasm.Add(new AASM.Push(this.LLAASMType.Size));
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
            // Call within program.
            if (this.llTarget != null)
            {
                aasm.Add(new AASM.Call(this.llTarget));
                aasm.Add(new AASM.AddSp(-this.parameters.Count*AASM.AASM.AddressSize));
            }
            // Call to import.
            else
            {
                aasm.Add(new AASM.CallImport(this.llImportTarget));
            }
            aasm.Add(new AASM.PushRetVal());
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
                aasm.Add(this.operand1.LLAASMEmitWrite());
                return aasm;
            }

            aasm.Add(this.operand1.LLAASMEmit());
            aasm.Add(this.operand2.LLAASMEmit());

            switch (this.binaryOpToken.Value)
            {
                case BinaryOpToken.Ops.Eq:
                    aasm.Add(new AASM.Eq());
                    break;
                case BinaryOpToken.Ops.Neq:
                    aasm.Add(new AASM.Neq());
                    break;
                case BinaryOpToken.Ops.And:
                    aasm.Add(new AASM.And());
                    break;
                case BinaryOpToken.Ops.Or:
                    aasm.Add(new AASM.Or());
                    break;
                case BinaryOpToken.Ops.Add:
                    aasm.Add(new AASM.Add());
                    break;
                case BinaryOpToken.Ops.Sub:
                    aasm.Add(new AASM.Sub());
                    break;
                case BinaryOpToken.Ops.Mul:
                    aasm.Add(new AASM.Mul());
                    break;
                case BinaryOpToken.Ops.Gt:
                    aasm.Add(new AASM.Gt());
                    break;
                case BinaryOpToken.Ops.Lt:
                    aasm.Add(new AASM.Lt());
                    break;
                case BinaryOpToken.Ops.Gte:
                    aasm.Add(new AASM.Gte());
                    break;
                case BinaryOpToken.Ops.Lte:
                    aasm.Add(new AASM.Lte());
                    break;
                default:
                    throw new CompilerException("Don't know how to AASM binary op " + this.binaryOpToken.Value + ".", this.Position);
            }

            return aasm;
        }
    }

    public partial class MemberAccessExpression : Expression
    {
        public override AASM.AASM LLAASMEmit()
        {
            AASM.AASM aasm = new AASM.AASM();

            aasm.Add(this.operand.LLAASMEmit());
            aasm.Add(new AASM.MemberRead(this.operand.LLAASMType as AASMStructType, this.memberIndex));

            return aasm;
        }

        public override AASM.AASM LLAASMEmitWrite()
        {
            AASM.AASM aasm = new AASM.AASM();

            aasm.Add(this.operand.LLAASMEmit());
            aasm.Add(new AASM.MemberWrite(this.operand.LLAASMType as AASMStructType, this.memberIndex));

            return aasm;
        }
    }
}