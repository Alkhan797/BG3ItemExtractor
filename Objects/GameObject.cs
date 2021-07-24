using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GetAllItemsBG3.Objects
{
    public class GameObject
    {
        public string EntryName { get; protected set; }
        public string Type { get; protected set; }
        public string UsingReference { get; protected set; }
        public Dictionary<string, string> Data { get; protected set; }

        protected GameObject(
            string name,
            string type,
            string usingReference,
            Dictionary<string, string> data)
        {
            EntryName = name;
            Type = type;
            UsingReference = usingReference;
            Data = data;
        }

        public static GameObject Create(string name, string type, string usingReference = null, Dictionary<string, string> data = null)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new Exception("GameObject must have a name");
            }

            if (string.IsNullOrWhiteSpace(type))
            {
                throw new Exception("GameObject must have a type");
            }

            return new GameObject(name, type, usingReference, data ?? new Dictionary<string, string>());
        }
    }
}
