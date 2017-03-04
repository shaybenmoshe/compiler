﻿using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
    public class Lexer
    {
        private InputStream inputStream;
        private List<Token> tokens;

        public Lexer(InputStream inputStream)
        {
            this.inputStream = inputStream;
            tokens = new List<Token>(0);
        }

        public List<Token> LexAll()
        {
            this.SkipShit();
            while (!this.inputStream.Ended())
            {
                this.tokens.Add(this.LexNext());
                this.SkipShit();
            }

            return this.tokens;
        }

        private Token LexNext()
        {
            int startPos = this.inputStream.Position;

            // Char.
            if (this.inputStream.NextIs('\''))
            {
                char charVal = this.GetNextCharValue();
                this.inputStream.EnsureNextIs('\'');
                return new CharToken(startPos, charVal);
            }

            // String.
            if (this.inputStream.NextIs('"'))
            {
                StringBuilder sb = new StringBuilder();
                while (!this.inputStream.Ended())
                {
                    if (this.inputStream.PeekNextIs('"'))
                    {
                        break;
                    }
                    sb.Append(this.GetNextCharValue());
                }
                this.inputStream.EnsureNextIs('"');

                return new StringToken(startPos, sb.ToString());
            }

            // Punct.
            if (this.inputStream.NextIs(';')) { return new PunctToken(startPos, PunctToken.Puncts.Semicolon); }
            if (this.inputStream.NextIs(':')) { return new PunctToken(startPos, PunctToken.Puncts.Colon); }
            if (this.inputStream.NextIs('{')) { return new PunctToken(startPos, PunctToken.Puncts.LBraces); }
            if (this.inputStream.NextIs('}')) { return new PunctToken(startPos, PunctToken.Puncts.RBraces); }
            if (this.inputStream.NextIs('(')) { return new PunctToken(startPos, PunctToken.Puncts.LParenthese); }
            if (this.inputStream.NextIs(')')) { return new PunctToken(startPos, PunctToken.Puncts.RParenthese); }
            if (this.inputStream.NextIs(',')) { return new PunctToken(startPos, PunctToken.Puncts.Comma); }

            // Binary op.
            if (this.inputStream.NextIs('=')) { return new BinaryOpToken(startPos, BinaryOpToken.Ops.Ass); }
            if (this.inputStream.NextIs("==")) { return new BinaryOpToken(startPos, BinaryOpToken.Ops.Eq); }
            if (this.inputStream.NextIs('+')) { return new BinaryOpToken(startPos, BinaryOpToken.Ops.Add); }
            if (this.inputStream.NextIs('>')) { return new BinaryOpToken(startPos, BinaryOpToken.Ops.Gt); }
            if (this.inputStream.NextIs(">=")) { return new BinaryOpToken(startPos, BinaryOpToken.Ops.Gte); }
            if (this.inputStream.NextIs('<')) { return new BinaryOpToken(startPos, BinaryOpToken.Ops.Lt); }
            if (this.inputStream.NextIs("<=")) { return new BinaryOpToken(startPos, BinaryOpToken.Ops.Lte); }

            // Keyword.
            if (this.inputStream.NextIs("var")) { return new KeywordToken(startPos, KeywordToken.Keywords.Var); }
            if (this.inputStream.NextIs("if")) { return new KeywordToken(startPos, KeywordToken.Keywords.If); }
            if (this.inputStream.NextIs("else")) { return new KeywordToken(startPos, KeywordToken.Keywords.Else); }
            if (this.inputStream.NextIs("function")) { return new KeywordToken(startPos, KeywordToken.Keywords.Function); }
            if (this.inputStream.NextIs("return")) { return new KeywordToken(startPos, KeywordToken.Keywords.Return); }

            // Name.
            if (this.ValidNameFirstChar(this.inputStream.PeekNext()))
            {
                StringBuilder sb = new StringBuilder();
                do
                {
                    sb.Append(this.inputStream.Next());
                } while (!this.inputStream.Ended() && this.ValidNameChar(this.inputStream.PeekNext()));
                return new NameToken(startPos, sb.ToString());
            }

            // Number.
            if (this.ValidNumberChar(this.inputStream.PeekNext()))
            {
                uint val = 0;
                do
                {
                    val = val*10 + (uint) (this.inputStream.Next() - '0');
                } while (!this.inputStream.Ended() && this.ValidNumberChar(this.inputStream.PeekNext()));
                return new NumberToken(startPos, val);
            }

            throw new CompilerException("Invalid token found.", this.inputStream.Position);
        }

        private bool ValidNumberChar(char c)
        {
            return '0' <= c && c <= '9';
        }

        private bool ValidNameFirstChar(char c)
        {
            return ('a' <= c && c <= 'z') || ('A' <= c && c <= 'Z') || c == '_';
        }

        private bool ValidNameChar(char c)
        {
            return this.ValidNameFirstChar(c) || ('0' <= c && c <= '9');
        }

        private char GetNextCharValue()
        {
            // @todo: escaped
            return this.inputStream.Next();
        }

        private void SkipShit()
        {
            bool skippedAny;

            do
            {
                skippedAny = false;
                skippedAny |= this.SkipWhiteChars();
                skippedAny |= this.SkipOneLineComment();
                skippedAny |= this.SkipMultiLineComment();
            } while (skippedAny);
        }

        private bool SkipWhiteChars()
        {
            bool skippedAny = false;

            while (!this.inputStream.Ended())
            {
                char cur = this.inputStream.PeekNext();
                if (cur != ' ' && cur != '\t' && cur != '\n' && cur != '\r')
                {
                    break;
                }
                this.inputStream.Next();
                skippedAny = true;
            }

            return skippedAny;
        }

        private bool SkipOneLineComment()
        {
            if (!this.inputStream.PeekNextIs("//"))
            {
                return false;
            }

            // Skip everything until new line.
            while (!this.inputStream.Ended())
            {
                char cur = this.inputStream.PeekNext();
                if (cur == '\n' || cur == '\r')
                {
                    break;
                }
                this.inputStream.Next();
            }

            return true;
        }

        private bool SkipMultiLineComment()
        {
            if (!this.inputStream.PeekNextIs("/*"))
            {
                return false;
            }
            
            while (!this.inputStream.Ended())
            {
                if (this.inputStream.PeekNextIs("*/"))
                {
                    this.inputStream.Next();
                    this.inputStream.Next();
                    break;
                }
                this.inputStream.Next();
            }

            return true;
        }
    }
}