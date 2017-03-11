using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
    public partial class LL
    {
        public void LLNamesResolver()
        {
            for (int i = 0; i < this.structs.Count; i++)
            {
                this.structs[i].LLNamesResolver(this);
            }
            for (int i = 0; i < this.functions.Count; i++)
            {
                this.functions[i].LLNamesResolver(this);
            }
        }

        public IAASMType LLResolveType(string name)
        {
            if (name == "uint32")
            {
                return AASMNativeType.AASMUint32;
            }

            for (int i = 0; i < this.structs.Count; i++)
            {
                if (this.structs[i].StructStatement.Name.Value == name)
                {
                    return this.structs[i];
                }
            }

            throw new Exception("Invalid type " + name);
        }
    }

    public partial class AASMStructType : IAASMType
    {
        public void LLNamesResolver(LL ll)
        {
            for (int i = 0; i < this.structStatement.Members.Count; i++)
            {
                this.structStatement.Members[i].LLAASMType = ll.LLResolveType(this.structStatement.Members[i].Type.Value);
            }
        }
    }

    public partial class FunctionStatement : Statement
    {
        private LL ll;

        private IAASMType llAASMRetType;

        public IAASMType LLAASMRetType
        {
            get { return this.llAASMRetType; }
        }

        public void LLNamesResolver(LL ll)
        {
            this.ll = ll;

            this.llAASMRetType = this.ll.LLResolveType(this.retType.Value);

            for (int i = 0; i < this.llLocals.Count; i++)
            {
                this.llLocals[i].LLAASMType = this.ll.LLResolveType(this.llLocals[i].Type.Value);
            }
            for (int i = 0; i < this.arguments.Count; i++)
            {
                this.arguments[i].LLAASMType = this.ll.LLResolveType(this.arguments[i].Type.Value);
            }

            this.TraverseExpressions(this.LLRenewName);
        }

        private void LLRenewName(Expression expression)
        {
            if (expression is NameExpression)
            {
                NameExpression nameExpression = expression as NameExpression;

                NameDefStatement nameDef = null;

                if (nameDef == null)
                {
                    nameDef = Utils.FindNameDefsList(this.llLocals, nameExpression.NameValue);
                }
                if (nameDef == null)
                {
                    nameDef = Utils.FindNameDefsList(this.arguments, nameExpression.NameValue);
                }
                if (nameDef == null)
                {
                    throw new CompilerException(
                        "Name `" + nameExpression.NameValue.Value + "` is not a name of an argument or a local.",
                        nameExpression.Value.Position);
                }

                nameExpression.LLNameDef = nameDef;
            }
            else if (expression is MemberAccessExpression)
            {
                (expression as MemberAccessExpression).LLResolveMemberIndex();
            }
            else if (expression is SizeofExpression)
            {
                SizeofExpression sizeofExpression = expression as SizeofExpression;
                sizeofExpression.LLAASMType = this.ll.LLResolveType(sizeofExpression.TypeName.Value);
            }
        }
    }

    public partial class NameDefStatement : Statement
    {
        private IAASMType llAASMType;

        public IAASMType LLAASMType
        {
            get { return this.llAASMType; }
            set { this.llAASMType = value; }
        }
    }

    public partial class Expression : PositionThing
    {
        public virtual IAASMType LLAASMType { get; }
    }

    public partial class ParentheseExpression : Expression
    {
        public override IAASMType LLAASMType
        {
            get { return this.value.LLAASMType; }
        }
    }

    public partial class ImmediateExpression : Expression
    {
        public override IAASMType LLAASMType
        {
            get
            {
                if (this.value is NumberToken)
                {
                    return AASMNativeType.AASMUint32;
                }

                throw new CompilerException("Don't know how to AASM immediate which is not a number.", this.Position);
            }
        }
    }

    public partial class NameExpression : ImmediateExpression
    {
        public override IAASMType LLAASMType
        {
            get { return this.llNameDef.LLAASMType; }
        }

        private NameDefStatement llNameDef;

        public NameDefStatement LLNameDef
        {
            get { return this.llNameDef; }
            set { this.llNameDef = value; }
        }
    }

    public partial class SizeofExpression : Expression
    {
        private IAASMType llAASMType;

        public IAASMType LLAASMType
        {
            get { return this.llAASMType; }
            set { this.llAASMType = value; }
        }
    }

    public partial class CallExpression : Expression
    {
        public override IAASMType LLAASMType
        {
            get { return this.LLTarget.LLAASMRetType; }
        }
    }

    public partial class BinaryOpExpression : Expression
    {
        public override IAASMType LLAASMType
        {
            get { return this.operand1.LLAASMType; }
        }
    }

    public partial class MemberAccessExpression : Expression
    {
        public override IAASMType LLAASMType
        {
            get { return this.MemberNameDef.LLAASMType; }
        }

        private int memberIndex;

        public int MemberIndex
        {
            get { return this.memberIndex; }
        }

        public NameDefStatement MemberNameDef
        {
            get
            {
                AASMStructType structType = this.operand.LLAASMType as AASMStructType;
                return structType.StructStatement.Members[this.memberIndex];
            }
        }

        public void LLResolveMemberIndex()
        {
            if (!(this.operand.LLAASMType is AASMStructType))
            {
                throw new CompilerException("Left of . must be a type struct", this.Position);
            }

            AASMStructType structType = this.operand.LLAASMType as AASMStructType;
            for (int i = 0; i < structType.StructStatement.Members.Count; i++)
            {
                NameDefStatement curr = structType.StructStatement.Members[i];
                if (curr.Name.Value == this.member.Value)
                {
                    this.memberIndex = i;
                    return;
                }
            }

            throw new Exception("Could not find member " + this.member.Value + " of " + structType.StructStatement.Name);
        }
    }
}