using System;

namespace Compiler
{
    public class CompilerException : Exception
    {
        public int Position;

        public CompilerException(string message, int position) : base(message)
        {
            this.Position = position;
        }
    }
}