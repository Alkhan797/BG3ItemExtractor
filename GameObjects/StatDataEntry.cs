using System;
using System.Collections.Generic;

namespace GetAllItemsBG3.GameObjects
{
    public class StatDataEntry
    {
        public string Name { get; protected set; }
        public string Type { get; protected set; }
        public string SourceFile { get; set; }
        public string UsingReference { get; protected set; }
        public Dictionary<string, string> Data { get; protected set; }

        protected StatDataEntry(
            string name,
            string type,
            string sourceFile,
            string usingReference,
            Dictionary<string, string> data)
        {
            Name = name;
            Type = type;
            SourceFile = sourceFile;
            UsingReference = usingReference;
            Data = data;
        }

        public static StatDataEntry Create(string name, string type, string usingReference = null, string sourceFile = null, Dictionary<string, string> data = null)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new Exception("GameObject must have a name");
            }

            if (string.IsNullOrWhiteSpace(type))
            {
                throw new Exception("GameObject must have a type");
            }

            return new StatDataEntry(name, type, sourceFile, usingReference, data ?? new Dictionary<string, string>());
        }

        public bool IsTemplate()
        {
            return Name.StartsWith("_");
        }

        public bool IsReference()
        {
            return Name.Contains("_REF");
        }
    }
}
