using System;
using System.Collections.Generic;
using System.Text;

namespace Compiler
{
    public class Type
    {
    }

    public class Uint32Type : Type
    {
        public int Size
        {
            get { return 4; }
        }

        public string Name
        {
            get { return "uint32"; }
        }
    }
}