using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Compiler
{
    public class TokenStream
    {
        private List<Token> input;
        private int position;
        private int endPosition;

        public TokenStream(List<Token> input, int endPosition)
        {
            this.input = input;
            this.position = 0;
            this.endPosition = endPosition;
        }

        public int Position
        {
            get { return this.position; }
        }

        public int TokenPosition
        {
            get { return this.input[this.position].Position; }
        }

        public bool Ended()
        {
            return this.position >= this.input.Count;
        }

        public bool Ended(int offset)
        {
            return this.position + offset >= this.input.Count;
        }

        public Token Next()
        {
            return this.input[this.position++];
        }

        public NameToken NextName()
        {
            if (this.Ended())
            {
                throw new CompilerException("Expected name but ended.", this.endPosition);
            }
            if (!this.PeekNextIsType(Token.Types.Name))
            {
                throw new CompilerException("Expected name.", this.TokenPosition);
            }
            return this.Next() as NameToken;
        }

        public Token PeekNext()
        {
            return this.input[this.position];
        }

        public Token PeekNext(int offset)
        {
            return this.input[this.position + offset];
        }

        public bool PeekNextIsType(Token.Types type)
        {
            return !this.Ended() && this.PeekNext().Type == type;
        }

        public bool PeekNextIsPunct(PunctToken.Puncts value)
        {
            return this.PeekNextIsType(Token.Types.Punct)
                   && (this.PeekNext() as PunctToken).Value == value;
        }

        public PunctToken EnsureNextIsPunct(PunctToken.Puncts value)
        {
            if (this.Ended())
            {
                throw new CompilerException("Expected punct " + value + " but ended.", this.endPosition);
            }
            if (!this.PeekNextIsPunct(value))
            {
                throw new CompilerException("Expected punct " + value + ".", this.TokenPosition);
            }
            return this.Next() as PunctToken;
        }

        public bool PeekNextIsKeyword(KeywordToken.Keywords value)
        {
            return this.PeekNextIsType(Token.Types.Keyword)
                   && (this.PeekNext() as KeywordToken).Value == value;
        }

        public KeywordToken EnsureNextIsKeyword(KeywordToken.Keywords value)
        {
            if (this.Ended())
            {
                throw new CompilerException("Expected keyword " + value + " but ended.", this.endPosition);
            }
            if (!this.PeekNextIsKeyword(value))
            {
                throw new CompilerException("Expected keyword " + value + ".", this.TokenPosition);
            }
            return this.Next() as KeywordToken;
        }

        public bool PeekNextIsOp(BinaryOpToken.Ops value)
        {
            return this.PeekNextIsType(Token.Types.BinaryOp)
                   && (this.PeekNext() as BinaryOpToken).Value == value;
        }

        public BinaryOpToken EnsureNextIsOp(BinaryOpToken.Ops value)
        {
            if (this.Ended())
            {
                throw new CompilerException("Expected binary op " + value + " but ended.", this.endPosition);
            }
            if (!this.PeekNextIsOp(value))
            {
                throw new CompilerException("Expected binary op " + value + ".", this.TokenPosition);
            }
            return this.Next() as BinaryOpToken;
        }
    }
}