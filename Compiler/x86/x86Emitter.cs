using System;
using System.Collections.Generic;

namespace Compiler
{
    public class x86Emitter
    {
        private List<FunctionStatement> functions;
        private List<x86FunctionEmitter> functionEmitters;
        private Dictionary<FunctionStatement, x86FunctionEmitter> functionsDictionary = new Dictionary<FunctionStatement, x86FunctionEmitter>();
        private List<byte> x86 = new List<byte>();
        private uint entryPoint;

        public x86Emitter(List<FunctionStatement> functions)
        {
            this.functions = functions;
        }

        public List<byte> X86
        {
            get { return this.x86; }
        }

        public uint EntryPoint
        {
            get { return this.entryPoint; }
        }

        public void Emit()
        {
            // Compute each function.
            this.functionEmitters = new List<x86FunctionEmitter>();
            for (int i = 0; i < this.functions.Count; i++)
            {
                x86FunctionEmitter functionEmitter = new x86FunctionEmitter(this.functions[i].LLAASM);
                this.functionsDictionary[this.functions[i]] = functionEmitter;
                this.functionEmitters.Add(functionEmitter);

                functionEmitter.ResolveLabels();
                functionEmitter.CreateOpcodes();
                functionEmitter.ResolveOffsesInX86();
            }

            // Link functions.
            for (int i = 0; i < this.functionEmitters.Count; i++)
            {
                this.functionEmitters[i].LinkFunctions(this.functionsDictionary);
            }

            // Compute all offsets.
            uint offset = 0;
            for (int i = 0; i < this.functionEmitters.Count; i++)
            {
                x86FunctionEmitter functionEmitter = this.functionEmitters[i];
                functionEmitter.OffsetInX86 = offset;
                offset += functionEmitter.X86Size;

                if (this.functions[i].Name.Value == "main")
                {
                    this.entryPoint = functionEmitter.OffsetInX86;
                }
            }

            // Emit all.
            for (int i = 0; i < this.functions.Count; i++)
            {
                this.x86.AddRange(this.functionEmitters[i].Emit());
            }
        }
    }
}