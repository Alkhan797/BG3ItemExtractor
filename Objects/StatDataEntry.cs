using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetAllItemsBG3.Objects
{
    public class StatDataEntry
    {
        public string Name { get; protected set; }
        public string Type { get; protected set; }
        public string UsingReference { get; protected set; }
        public Dictionary<string, string> Data { get; protected set; }

        protected StatDataEntry(
            string name,
            string type,
            string usingReference,
            Dictionary<string, string> data)
        {
            Name = name;
            Type = type;
            UsingReference = usingReference;
            Data = data;
        }

        public static StatDataEntry Create(string name, string type, string usingReference = null, Dictionary<string, string> data = null)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new Exception("GameObject must have a name");
            }

            if (string.IsNullOrWhiteSpace(type))
            {
                throw new Exception("GameObject must have a type");
            }

            return new StatDataEntry(name, type, usingReference, data ?? new Dictionary<string, string>());
        }
    }
}
