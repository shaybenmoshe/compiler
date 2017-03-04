using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace Compiler
{
    public class InputStream
    {
        private string input;
        private int position;

        public InputStream(string input)
        {
            this.input = input;
            this.position = 0;
        }

        public int Position
        {
            get { return this.position; }
        }

        public bool Ended()
        {
            return this.position >= this.input.Length;
        }

        public char Next()
        {
            return this.input[this.position++];
        }

        public bool NextIs(char c)
        {
            if (!this.PeekNextIs(c))
            {
                return false;
            }
            this.position++;
            return true;
        }

        public bool NextIs(string s)
        {
            if (!this.PeekNextIs(s))
            {
                return false;
            }
            this.position += s.Length;
            return true;
        }

        public char PeekNext()
        {
            return this.input[this.position];
        }

        public bool PeekNextIs(char c)
        {
            return !this.Ended()
                && this.PeekNext() == c;
        }

        public bool PeekNextIs(string s)
        {
            return this.position + s.Length <= this.input.Length
                   && this.input.Substring(this.position, s.Length).Equals(s);
        }

        public void EnsureNextIs(char c)
        {
            if (this.Next() != c)
            {
                throw new CompilerException("Expected " + c + " here.", this.position);
            }
        }
    }
}