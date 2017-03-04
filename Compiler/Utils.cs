using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace Compiler
{
    public static class Utils
    {
        public static NameDefStatement FindNameDefsList(List<NameDefStatement> list, NameToken value)
        {
            for (int i = 0; i < list.Count; i++)
            {
                if (list[i].Name.Value == value.Value)
                {
                    return list[i];
                }
            }
            return null;
        }
    }
}