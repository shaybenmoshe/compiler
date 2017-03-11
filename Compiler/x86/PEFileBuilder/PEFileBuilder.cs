using System;
using System.Collections.Generic;

namespace Compiler
{
    public partial class PEFileBuilder
    {
        private const uint pageSize = 0x1000;
        private const uint sectionAlignmentSize = 0x200;
        private const uint imageBase = 0x00400000;

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

        private uint AlignTo(uint value, uint to)
        {
            return (value + (to - 1)) & ~(to - 1);
        }

        public void FinalizePE()
        {
            this.FixValues();
        }
    }
}