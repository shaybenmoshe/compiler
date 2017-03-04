using System;
using System.Collections.Generic;

namespace Compiler
{
    public class x86Emitter
    {
        private List<FunctionStatement> functions;
        private List<x86FunctionEmitter> functionEmitters;
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
            this.functionEmitters = new List<x86FunctionEmitter>();
            for (int i = 0; i < this.functions.Count; i++)
            {
                x86FunctionEmitter functionEmitter = new x86FunctionEmitter(this.functions[i].LLAASM);
                functionEmitter.Emit();
                this.functionEmitters.Add(functionEmitter); 
            }

            // @todo: link functions

            for (int i = 0; i < this.functions.Count; i++)
            {
                if (this.functions[i].Name.Value == "main")
                {
                    this.entryPoint = (uint)this.x86.Count;
                }

                x86FunctionEmitter functionEmitter = this.functionEmitters[i];
                this.x86.AddRange(functionEmitter.X86);
            }
        }
    }
}