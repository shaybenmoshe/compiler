using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Compiler
{
    class Program
    {
        static void Main(string[] args)
        {
            string input = @"
function myfunc(x:uint32, y:uint32):uint32 {
    var z:uint32 = 3;
    return x + y + z;
}

function main():void {
    var x:uint32 = 100;
    var y:uint32 = 8 + myfunc(1, 2, otherCall()) + 2;
    var z:uint32 = x + y;
    var u:uint32 = 1 + ((2 + 3) + (4 + (5 + 6)) + 7 + 8) + 9;
    var s:string = ""waaa

    this is multiline
    "";

    if(x > y) {
        t = 3000;
    }
    else
        t = 100;

    y = 20;
}
";

            try
            {

                InputStream inputStream = new InputStream(input);

                Lexer lexer = new Lexer(inputStream);
                List<Token> tokens = lexer.LexAll();
                
                /*for (int i = 0; i < tokens.Count; i++)
                {
                    Console.Write(tokens[i]);
                }*/

                TokenStream tokenStream = new TokenStream(tokens, input.Length);

                Parser parser = new Parser(tokenStream);
                AST ast = parser.ParseAll();

                Console.WriteLine(ast.TopLevel.ToString());

                Console.WriteLine("Done");
            }
            catch (CompilerException e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("CompilerException at position " + e.Position + ".");
                Console.WriteLine(e.Message);
                Console.ResetColor();

                Console.WriteLine();
                
                Console.Write(input.Substring(0, e.Position));
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("<<<!ERROR!>>>");
                Console.ResetColor();
                Console.Write(input.Substring(e.Position));
            }

            Console.ReadKey();
        }
    }
}
