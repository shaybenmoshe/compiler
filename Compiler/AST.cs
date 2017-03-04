using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
    public class AST
    {
    }

    public class Statement
    {
    }

    public class ScopeStatement : Statement
    {
        List<Statement> statements = new List<Statement>();

        public void Add(Statement statement)
        {
            this.statements.Add(statement);
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

    public class VarStatement : Statement
    {
        private NameToken name;
        private NameToken type;
        private Expression value;

        public VarStatement(NameToken name, NameToken type, Expression value)
        {
            this.name = name;
            this.type = type;
            this.value = value;
        }

        public override string ToString()
        {
            return "var " + this.name.ToString() + ":" + this.type.ToString() + " = " + this.value.ToString() + ";";
        }
    }

    public class IfElseStatement : Statement
    {
        private Expression cond;
        private Statement body;
        private Statement elseBody;

        public IfElseStatement(Expression cond, Statement body, Statement elseBody)
        {
            this.cond = cond;
            this.body = body;
            this.elseBody = elseBody;
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

    public class ExpressionStatement : Statement
    {
        private Expression expression;

        public ExpressionStatement(Expression expression)
        {
            this.expression = expression;
        }

        public override string ToString()
        {
            return this.expression.ToString() + ";";
        }
    }

    public class Expression
    {
    }

    public class ParentheseExpression : Expression
    {
        private Expression value;

        public ParentheseExpression(Expression value)
        {
            this.value = value;
        }

        public override string ToString()
        {
            return "(" + this.value.ToString() + ")";
        }
    }

    public class ImmediateExpression : Expression
    {
        private Token value;

        public ImmediateExpression(Token value)
        {
            this.value = value;
        }

        public override string ToString()
        {
            return this.value.ToString();
        }
    }

    public class CallExpression : Expression
    {
        private NameToken func;
        private List<Expression> parameters;

        public CallExpression(NameToken func, List<Expression> parameters)
        {
            this.func = func;
            this.parameters = parameters;
        }

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            sb.Append(this.func + "(");
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

    public class BinaryOpExpression : Expression
    {
        private BinaryOpToken binaryOpToken;
        private Expression operand1;
        private Expression operand2;

        public BinaryOpExpression(BinaryOpToken binaryOpToken, Expression operand1, Expression operand2)
        {
            this.binaryOpToken = binaryOpToken;
            this.operand1 = operand1;
            this.operand2 = operand2;
        }

        public override string ToString()
        {
            return this.operand1.ToString() + this.binaryOpToken.ToString() + this.operand2.ToString();
        }
    }
}