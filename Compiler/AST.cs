﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
    public class AST
    {
        CompoundStatement topLevel;

        public CompoundStatement TopLevel
        {
            get { return this.topLevel; }
        }

        public AST(CompoundStatement topLevel)
        {
            this.topLevel = topLevel;
        }
    }

    public class PositionThing
    {
        private int position;

        public int Position
        {
            get { return this.position; }
        }

        public PositionThing(int position)
        {
            this.position = position;
        }
    }






    public partial class Statement : PositionThing
    {
        public Statement(int position) : base(position)
        {
        }

        public virtual void TraverseStatements(Action<Statement> cb)
        {
            cb(this);
        }

        public virtual void TraverseExpressions(Action<Expression> cb)
        {
        }
    }

    public partial class CompoundStatement : Statement
    {
        List<Statement> statements = new List<Statement>();

        public List<Statement> Statements
        {
            get { return this.statements; }
        }

        public CompoundStatement(int position) : base(position)
        {
        }

        public void Add(Statement statement)
        {
            this.statements.Add(statement);
        }

        public override void TraverseStatements(Action<Statement> cb)
        {
            base.TraverseStatements(cb);
            for (int i = 0; i < this.statements.Count; i++)
            {
                this.statements[i].TraverseStatements(cb);
            }
        }

        public override void TraverseExpressions(Action<Expression> cb)
        {
            base.TraverseExpressions(cb);
            for (int i = 0; i < this.statements.Count; i++)
            {
                this.statements[i].TraverseExpressions(cb);
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("{\r\n");
            for (int i = 0; i < this.statements.Count; i++)
            {
                string p = this.statements[i].ToString();
                p = "\t" + p.Replace("\n", "\n\t") + "\r\n";
                sb.Append(p);
            }
            sb.Append("}");

            return sb.ToString();
        }
    }

    public partial class NameDefStatement : Statement
    {
        private NameToken name;
        private NameToken type;

        public NameDefStatement(int position, NameToken name, NameToken type) : base(position)
        {
            this.name = name;
            this.type = type;
        }

        public NameToken Name
        {
            get { return name; }
        }

        public NameToken Type
        {
            get { return type; }
            set { type = value; }
        }

        public override string ToString()
        {
            return this.name.ToString() + ":" + this.type.ToString();
        }
    }

    public partial class VarStatement : Statement
    {
        private NameDefStatement nameDef;

        public NameDefStatement NameDef
        {
            get { return this.nameDef; }
        }

        public VarStatement(int position, NameDefStatement nameDef) : base(position)
        {
            this.nameDef = nameDef;
        }

        public override string ToString()
        {
            return "var " + this.nameDef.ToString() + ";";
        }
    }
    
    public partial class ReturnStatement : Statement
    {
        private Expression value;

        public ReturnStatement(int position, Expression value) : base(position)
        {
            this.value = value;
        }

        public override void TraverseExpressions(Action<Expression> cb)
        {
            base.TraverseExpressions(cb);
            this.value.TraverseExpressions(cb);
        }

        public override string ToString()
        {
            return "return " + this.value.ToString() + ";";
        }
    }

    public partial class Int3Statement : Statement
    {
        public Int3Statement(int position) : base(position)
        {
        }

        public override string ToString()
        {
            return "int3;";
        }
    }

    public partial class ImportStatement : Statement
    {
        private NameToken dll;
        private NameToken function;
        private NameToken importedName;

        public ImportStatement(int position, NameToken dll, NameToken function, NameToken importedName) : base(position)
        {
            this.dll = dll;
            this.function = function;
            this.importedName = importedName;
        }

        public NameToken ImportedName
        {
            get { return this.importedName; }
        }

        public NameToken DLL
        {
            get { return this.dll; }
        }

        public NameToken Function
        {
            get { return this.function; }
        }

        public override string ToString()
        {
            return "import " + dll + " " + function + " " + importedName + ";";
        }
    }

    public partial class IfElseStatement : Statement
    {
        private Expression cond;
        private Statement body;
        private Statement elseBody;

        public IfElseStatement(int position, Expression cond, Statement body, Statement elseBody) : base(position)
        {
            this.cond = cond;
            this.body = body;
            this.elseBody = elseBody;
        }

        public override void TraverseStatements(Action<Statement> cb)
        {
            base.TraverseStatements(cb);
            this.body.TraverseStatements(cb);
            if (elseBody != null)
            {
                this.elseBody.TraverseStatements(cb);
            }
        }

        public override void TraverseExpressions(Action<Expression> cb)
        {
            base.TraverseExpressions(cb);
            this.cond.TraverseExpressions(cb);
            this.body.TraverseExpressions(cb);
            if (elseBody != null)
            {
                this.elseBody.TraverseExpressions(cb);
            }
        }

        public override string ToString()
        {
            string s = "if (" + this.cond.ToString() + ") " + this.body.ToString();
            if (this.elseBody != null)
            {
                s += "\r\nelse " + this.elseBody.ToString();
            }
            return s;
        }
    }

    public partial class WhileStatement : Statement
    {
        private Expression cond;
        private Statement body;

        public WhileStatement(int position, Expression cond, Statement body) : base(position)
        {
            this.cond = cond;
            this.body = body;
        }

        public override void TraverseStatements(Action<Statement> cb)
        {
            base.TraverseStatements(cb);
            this.body.TraverseStatements(cb);
        }

        public override void TraverseExpressions(Action<Expression> cb)
        {
            base.TraverseExpressions(cb);
            this.cond.TraverseExpressions(cb);
            this.body.TraverseExpressions(cb);
        }

        public override string ToString()
        {
            string s = "while (" + this.cond.ToString() + ") " + this.body.ToString();
            return s;
        }
    }

    public partial class StructStatement : Statement
    {
        private NameToken name;
        private List<NameDefStatement> members;

        public NameToken Name
        {
            get { return this.name; }
        }

        public List<NameDefStatement> Members
        {
            get { return this.members; }
        }

        public StructStatement(int position, NameToken name, List<NameDefStatement> members) : base(position)
        {
            this.name = name;
            this.members = members;
        }

        public override void TraverseStatements(Action<Statement> cb)
        {
            base.TraverseStatements(cb);
            for (int i = 0; i < this.members.Count; i++)
            {
                this.members[i].TraverseStatements(cb);
            }
        }

        public override void TraverseExpressions(Action<Expression> cb)
        {
            base.TraverseExpressions(cb);
            for (int i = 0; i < this.members.Count; i++)
            {
                this.members[i].TraverseExpressions(cb);
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("struct " + this.name.ToString() + "{");
            for (int i = 0; i < this.members.Count; i++)
            {
                string p = this.members[i].ToString();
                if (i > 0)
                {
                    sb.Append(";");
                }
                sb.Append(p);
            }
            sb.Append("}");

            return sb.ToString();
        }
    }

    public partial class FunctionStatement : Statement
    {
        private NameToken name;
        private NameToken retType;
        private List<NameDefStatement> arguments;
        private Statement body;
        
        public NameToken Name
        {
            get { return this.name; }
        }

        public List<NameDefStatement> Arguments
        {
            get { return this.arguments; }
        }

        public FunctionStatement(int position, NameToken name, NameToken retType, List<NameDefStatement> arguments, Statement body) : base(position)
        {
            this.name = name;
            this.retType = retType;
            this.arguments = arguments;
            this.body = body;
        }

        public override void TraverseStatements(Action<Statement> cb)
        {
            base.TraverseStatements(cb);
            this.body.TraverseStatements(cb);
            for (int i = 0; i < this.arguments.Count; i++)
            {
                this.arguments[i].TraverseStatements(cb);
            }
        }

        public override void TraverseExpressions(Action<Expression> cb)
        {
            base.TraverseExpressions(cb);
            this.body.TraverseExpressions(cb);
            for (int i = 0; i < this.arguments.Count; i++)
            {
                this.arguments[i].TraverseExpressions(cb);
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append("function " + this.name.ToString() + "(");
            for (int i = 0; i < this.arguments.Count; i++)
            {
                string p = this.arguments[i].ToString();
                if (i > 0)
                {
                    sb.Append(",");
                }
                sb.Append(p);
            }
            sb.Append("): " + this.retType.ToString());
            sb.Append(this.body);

            return sb.ToString();
        }
    }

    public partial class ExpressionStatement : Statement
    {
        private Expression expression;

        public ExpressionStatement(int position, Expression expression) : base(position)
        {
            this.expression = expression;
        }

        public override void TraverseExpressions(Action<Expression> cb)
        {
            base.TraverseExpressions(cb);
            this.expression.TraverseExpressions(cb);
        }

        public override string ToString()
        {
            return this.expression.ToString() + ";";
        }
    }






    public partial class Expression : PositionThing
    {
        public Expression(int position) : base(position)
        {
        }

        public virtual void TraverseExpressions(Action<Expression> cb)
        {
            cb(this);
        }
    }

    public partial class ParentheseExpression : Expression
    {
        private Expression value;

        public ParentheseExpression(int position, Expression value) : base(position)
        {
            this.value = value;
        }

        public override void TraverseExpressions(Action<Expression> cb)
        {
            base.TraverseExpressions(cb);
            this.value.TraverseExpressions(cb);
        }

        public override string ToString()
        {
            return "(" + this.value.ToString() + ")";
        }
    }

    public partial class ImmediateExpression : Expression
    {
        private Token value;

        public ImmediateExpression(int position, Token value) : base(position)
        {
            this.value = value;
        }

        public Token Value
        {
            get { return this.value; }
        }

        public override string ToString()
        {
            return this.value.ToString();
        }
    }

    public partial class NameExpression : ImmediateExpression
    {
        public NameToken NameValue
        {
            get { return this.Value as NameToken; }
        }

        public NameExpression(int position, NameToken value) : base(position, value)
        {
        }
    }

    public partial class SizeofExpression : Expression
    {
        private NameToken typeName;

        public SizeofExpression(int position, NameToken typeName) : base(position)
        {
            this.typeName = typeName;
        }

        public NameToken TypeName
        {
            get { return this.typeName; }
        }

        public override string ToString()
        {
            return "sizeof " + this.typeName.ToString();
        }
    }

    public partial class CallExpression : Expression
    {
        private NameToken name;
        private List<Expression> parameters;

        public CallExpression(int position, NameToken name, List<Expression> parameters) : base(position)
        {
            this.name = name;
            this.parameters = parameters;
        }

        public NameToken Name
        {
            get { return this.name; }
        }

        public List<Expression> Parameters
        {
            get { return parameters; }
        }

        public override void TraverseExpressions(Action<Expression> cb)
        {
            base.TraverseExpressions(cb);
            for (int i = 0; i < this.parameters.Count; i++)
            {
                this.parameters[i].TraverseExpressions(cb);
            }
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(this.name + "(");
            for (int i = 0; i < this.parameters.Count; i++)
            {
                string p = this.parameters[i].ToString();
                if (i > 0)
                {
                    sb.Append(",");
                }
                sb.Append(p);
            }
            sb.Append(")");

            return sb.ToString();
        }
    }

    public partial class BinaryOpExpression : Expression
    {
        private BinaryOpToken binaryOpToken;
        private Expression operand1;
        private Expression operand2;

        public BinaryOpExpression(int position, BinaryOpToken binaryOpToken, Expression operand1, Expression operand2) : base(position)
        {
            this.binaryOpToken = binaryOpToken;
            this.operand1 = operand1;
            this.operand2 = operand2;
        }

        public override void TraverseExpressions(Action<Expression> cb)
        {
            base.TraverseExpressions(cb);
            this.operand1.TraverseExpressions(cb);
            this.operand2.TraverseExpressions(cb);
        }

        public override string ToString()
        {
            return this.operand1.ToString() + this.binaryOpToken.ToString() + this.operand2.ToString();
        }
    }

    public partial class MemberAccessExpression : Expression
    {
        private Expression operand;
        private NameToken member;

        public MemberAccessExpression(int position, Expression operand, NameToken member) : base(position)
        {
            this.operand = operand;
            this.member = member;
        }

        public override void TraverseExpressions(Action<Expression> cb)
        {
            this.operand.TraverseExpressions(cb);
            base.TraverseExpressions(cb);
        }

        public override string ToString()
        {
            return this.operand.ToString() + "." + this.member.ToString();
        }
    }
}