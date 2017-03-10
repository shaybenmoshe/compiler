using System;
using System.Collections.Generic;

namespace Compiler
{
    public class Token
    {
        public enum Types
        {
            Char,
            String,
            Punct,
            BinaryOp,
            Keyword,
            Name,
            Number,
        }

        private int position;

        public Token(int position)
        {
            this.position = position;
        }

        public virtual Types Type { get; }

        public int Position
        {
            get { return this.position; }
        }
    }

    public class PunctToken : Token
    {
        public enum Puncts
        {
            Semicolon,
            Colon,
            LBraces,
            RBraces,
            LParenthese,
            RParenthese,
            Comma,
        }

        private Puncts value;

        public PunctToken(int position, Puncts value) : base(position)
        {
            this.value = value;
        }

        public Puncts Value
        {
            get { return this.value; }
        }

        public override Token.Types Type
        {
            get { return Token.Types.Punct; }
        }

        public override string ToString()
        {
            switch (this.value)
            {
                case Puncts.Semicolon:
                    return ";\r\n";
                case Puncts.Colon:
                    return ":";
                case Puncts.LBraces:
                    return " {\r\n";
                case Puncts.RBraces:
                    return "}\r\n";
                case Puncts.LParenthese:
                    return "(";
                case Puncts.RParenthese:
                    return ")";
                case Puncts.Comma:
                    return ",";
            }

            throw new CompilerException("Invalid Punct type", this.Position);
        }
    }

    public class BinaryOpToken : Token
    {
        public enum Ops
        {
            Ass,
            Eq,
            Add,
            Mul,
            Gt,
            Gte,
            Lt,
            Lte,
        }

        private Ops value;

        public BinaryOpToken(int position, Ops value) : base(position)
        {
            this.value = value;
        }

        public Ops Value
        {
            get { return this.value; }
        }

        public override Token.Types Type
        {
            get { return Token.Types.BinaryOp; }
        }

        public override string ToString()
        {
            switch (this.value)
            {
                case Ops.Ass:
                    return " = ";
                case Ops.Eq:
                    return " == ";
                case Ops.Add:
                    return " + ";
                case Ops.Gt:
                    return " > ";
                case Ops.Gte:
                    return " >= ";
                case Ops.Lt:
                    return " < ";
                case Ops.Lte:
                    return " <= ";
            }

            throw new CompilerException("Invalid BinaryOp type", this.Position);
        }
    }

    public class CharToken : Token
    {
        private char value;

        public CharToken(int position, char value) : base(position)
        {
            this.value = value;
        }

        public override Token.Types Type
        {
            get { return Token.Types.Char; }
        }

        public override string ToString()
        {
            return "'" + this.value + "'";
        }
    }

    public class StringToken : Token
    {
        private string value;

        public StringToken(int position, string value) : base(position)
        {
            this.value = value;
        }

        public override Token.Types Type
        {
            get { return Token.Types.String; }
        }

        public override string ToString()
        {
            return "\"" + this.value + "\"";
        }
    }

    public class KeywordToken : Token
    {
        public enum Keywords
        {
            Var,
            If,
            Else,
            Function,
            Return,
            Int3,
        }

        private Keywords value;

        public KeywordToken(int position, Keywords value) : base(position)
        {
            this.value = value;
        }

        public Keywords Value
        {
            get { return this.value; }
        }

        public override Token.Types Type
        {
            get { return Token.Types.Keyword; }
        }

        public override string ToString()
        {
            switch (this.value)
            {
                case Keywords.Var:
                    return "var ";
                case Keywords.If:
                    return "if ";
                case Keywords.Else:
                    return "else ";
                case Keywords.Function:
                    return "function ";
                case Keywords.Return:
                    return "return ";
            }

            throw new CompilerException("Invalid BinaryOp type", this.Position);
        }
    }

    public class NameToken : Token
    {
        private string value;

        public NameToken(int position, string value) : base(position)
        {
            this.value = value;
        }

        public override Token.Types Type
        {
            get { return Token.Types.Name; }
        }

        public string Value
        {
            get { return value; }
        }

        public override string ToString()
        {
            return this.value;
        }
    }

    public class NumberToken : Token
    {
        private uint value;

        public NumberToken(int position, uint value) : base(position)
        {
            this.value = value;
        }

        public override Token.Types Type
        {
            get { return Token.Types.Number; }
        }

        public uint Value
        {
            get { return this.value; }
        }

        public override string ToString()
        {
            return this.value.ToString();
        }
    }
}