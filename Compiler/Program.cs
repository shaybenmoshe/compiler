﻿using System;
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
            /*string input = @"
function myfunc(x:uint32, y:uint32):uint32 {
    var z:uint32 = 3;
    return x + y + z;
}

function main():uint32 {
    var x:uint32 = 100;
    var y:uint32 = 8 + myfunc(1, 2) + 2;
    int3;
    var z:uint32 = x + y;
    var u:uint32 = 1 + ((2 + 3) + (4 + (5 + 6)) + 7 + 8) + 9;

    if(x > y) {
        int3;
        var lolz:uint32 = 1;
    }
    else
        u = 100;

    y = 20;

    int3;
    return u;
}
";*/
            /*string input = @"
function main():uint32 {
    var x:uint32 = 7;
    var y:uint32 = fib(x);
    //int3;

    return 0;
}

function fib(x:uint32):uint32 {
    if (x > 1) {
        return fib(x + 4294967295) + fib(x + 4294967294);
    }
    return x;
}
";*/
            /*string input = @"
function main():uint32 {
    var x:uint32 = 7;
    var y:uint32 = fact(x);
    int3;

    return 0;
}

function fact(x:uint32):uint32 {
    if (x > 0) {
        return x * fact(x - 1);
    }
    return 1;
}
";*/
            /*string input = @"
struct FibPair {
    x:uint32,
    y:uint32,
}

function main():uint32 {
    var a:FibPair;
    //int3;
    (a.x) = 0;
    (a.y) = 1;
    fibPairStep(a);
    fibPairStep(a);
    fibPairStep(a);
    fibPairStep(a);
    fibPairStep(a);

    return 0;
}

function fibPairStep(a:FibPair):uint32 {
    var t:uint32;
    t = (a.x) + (a.y);
    (a.x) = (a.y);
    (a.y) = t;
    return 0;
}
";*/
            /*string input = @"
struct A {
    x:uint32,
    y:uint32,
}

struct B {
    a1:A,
    z:uint32,
    a2:A,
}

function main():uint32 {
    var l1:A;
    var l2:A;
    var b:B;
    int3;

    (l1.x) = 4294967295;
    (l1.y) = 4008636142;
    (l2.x) = 3722304989;
    (l2.y) = 3435973836;

    int3;
    (b.a1) = l1;
    (b.a2) = l2;
    int3;
    ((b.a1).x) = 1;
    ((b.a1).y) = 2;
    (b.z) = 3;
    ((b.a2).x) = 4;
    ((b.a2).y) = 5;
    int3;
    ((b.a1).x) = 1;
    ((b.a1).y) = 2;
    (b.z) = 3;
    ((b.a2).x) = 4;
    ((b.a2).y) = 5;
    int3;

    return 0;
}
";*/


            string input = "";
            input += System.IO.File.ReadAllText(@"C:\my-folders\projects\c-sharp\Compiler\sources\Heap");
            input += System.IO.File.ReadAllText(@"C:\my-folders\projects\c-sharp\Compiler\sources\CoalesceHeap");
            input += System.IO.File.ReadAllText(@"C:\my-folders\projects\c-sharp\Compiler\sources\FixedHeap");
            input += @"
function main():uint32 {
    var heap:Heap;
    heap = HeapCreate();

    var p1:Ptr;
    var p2:Ptr;
    var p3:Ptr;
    var p4:Ptr;
    var p5:Ptr;
    var p6:Ptr;
    var p7:Ptr;
    var p8:Ptr;
    p1 = HeapAlloc(heap, 0x300); // 1
    int3;
    p2 = HeapAlloc(heap, 0x300); // 2
    int3;
    p3 = HeapAlloc(heap, 0x300); // 3
    int3;
    HeapFree(p2);
    p4 = HeapAlloc(heap, 0x300); // 2
    int3;
    p5 = HeapAlloc(heap, 0x300); // 1'
    int3;
    HeapFree(p1); // 1
    HeapFree(p3); // 3
    HeapFree(p4); // 2
    p6 = HeapAlloc(heap, 0x300); // 2'
    int3;
    p7 = HeapAlloc(heap, 0x300); // 3'
    int3;
    p8 = HeapAlloc(heap, 0x300); // 2
    int3;

    var p:Ptr;
    /*p = HeapAlloc(heap, 0x10);
    int3;
    p = HeapAlloc(heap, 0x20);
    int3;
    p = HeapAlloc(heap, 0x80);
    int3;
    p = HeapAlloc(heap, 0x120);
    int3;
    p = HeapAlloc(heap, 0x200);
    int3;
    p = HeapAlloc(heap, 0x400);
    int3;
    p = HeapAlloc(heap, 0x800);
    int3;*/

    return 0;
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

                //Console.WriteLine(ast.TopLevel.ToString());

                LL ll = new LL(ast);
                ll.Emit();

                PEFileBuilder peFileBuilder = new PEFileBuilder();
                peFileBuilder.EmitStart();
                peFileBuilder.EmitImports(ll.Imports);

                x86Emitter emitter = new x86Emitter(ll.Functions);
                emitter.Emit(peFileBuilder.ImportAddresses);

                peFileBuilder.EmitCode(emitter.X86, emitter.EntryPoint);
                peFileBuilder.FinalizePE();
                
                List<byte> peFile = peFileBuilder.Output;
                System.IO.File.WriteAllBytes("test.exe", peFile.ToArray());


                /*List<byte> code = new List<byte>();
                code.Add(0xcc);
                code.Add(0xcc);
                code.Add(0xcc);
                code.Add(0x90);
                code.Add(0xcc);
                code.Add(0xcc);
                code.Add(0xcc);
                PEFileBuilder peFileBuilder = new PEFileBuilder(code, 4);
                List<byte> peFile = peFileBuilder.Emit();
                System.IO.File.WriteAllBytes("test.exe", peFile.ToArray());*/

                Console.WriteLine("Done");
            }
            catch (CompilerException e)
            {
                Console.ForegroundColor = ConsoleColor.Red;
                Console.WriteLine("CompilerException at position " + e.Position + ".");
                Console.WriteLine(e.Message);
                Console.ResetColor();

                Console.WriteLine();

                int startPos = Math.Max(e.Position - 500, 0);
                Console.Write(input.Substring(startPos, e.Position - startPos));
                Console.ForegroundColor = ConsoleColor.Red;
                Console.Write("<<<!ERROR!>>>");
                Console.ResetColor();
                Console.Write(input.Substring(e.Position, Math.Min(input.Length - e.Position, 500)));

                Console.ReadKey();
            }
        }
    }
}
