using System;
using System.Collections.Generic;

namespace Compiler
{
    public partial class PEFileBuilder
    {
        private const uint sectionAlignmentSize = 0x200;
        
        List<byte> output = new List<byte>();

        public PEFileBuilder()
        {
            this.InitValuesFixers();
        }

        public List<byte> Output
        {
            get { return this.output; }
        }

        private void AlignToSectionAlignment()
        {
            while ((this.output.Count & (sectionAlignmentSize - 1)) != 0)
            {
                this.output.Add(0);
            }
        }
        
        public void FinalizePE()
        {
            this.FixValues();
        }
    }
}