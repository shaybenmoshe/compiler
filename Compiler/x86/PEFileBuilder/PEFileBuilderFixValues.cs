using System;
using System.Collections.Generic;
using System.Linq;

namespace Compiler
{
    public class ValuesFixer
    {
        private int size = 4;
        private uint value;
        private List<int> targets = new List<int>();

        public List<int> Targets
        {
            get { return this.targets; }
        }

        public int Size
        {
            get { return this.size; }
            set { this.size = value; }
        }

        public uint Value
        {
            get { return this.value; }
            set { this.value = value; }
        }
    }

    public partial class PEFileBuilder
    {
        public enum ValuesFixerKeys
        {
            CodeVirtualSize,
            CodeVirtualOffset,
            CodeDiskSize,
            CodeDiskOffset,
            EntryPoint,
            SizeOfImage,
            SizeOfHeaders,
            SizeOfOptionalHeaders,
            ImportsVirtualSize,
            ImportsVirtualOffset,
            ImportsDiskSize,
            ImportsDiskOffset,
            ImageImportDescriptorsVirtualSize,
            ImageImportDescriptorsVirtualOffset,
        }

        private Dictionary<ValuesFixerKeys, ValuesFixer> valuesFixers = new Dictionary<ValuesFixerKeys, ValuesFixer>();
        private Dictionary<string, ValuesFixer> valuesFixersStr = new Dictionary<string, ValuesFixer>();

        public void InitValuesFixers()
        {
            foreach (ValuesFixerKeys key in Enum.GetValues(typeof(ValuesFixerKeys)))
            {
                this.valuesFixers[key] = new ValuesFixer();
            }
        }

        public void FixValues()
        {
            foreach (ValuesFixerKeys key in Enum.GetValues(typeof(ValuesFixerKeys)))
            {
                ValuesFixer valuesFixer = this.valuesFixers[key];
                for (int i = 0; i < valuesFixer.Targets.Count; i++)
                {
                    Utils.Rewrite(this.output, valuesFixer.Value, valuesFixer.Size, valuesFixer.Targets[i]);
                }
            }

            foreach (ValuesFixer valuesFixer in this.valuesFixersStr.Values)
            {
                for (int i = 0; i < valuesFixer.Targets.Count; i++)
                {
                    Utils.Rewrite(this.output, valuesFixer.Value, valuesFixer.Size, valuesFixer.Targets[i]);
                }
            }
        }
    }
}