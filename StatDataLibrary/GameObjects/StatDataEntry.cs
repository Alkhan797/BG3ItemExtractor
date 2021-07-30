using System;
using System.Collections.Generic;

namespace StatDataLibrary.GameObjects
{
    /// <summary>
    /// Represent an entry in the game's stats txt files
    /// </summary>
    public class StatDataEntry
    {
        /// <summary>
        /// Name of the entry, as defined by the "new entry" property
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// Type of the entry (Armor, Weapon, etc), as defined by the "type" property
        /// </summary>
        public string Type { get; protected set; }

        /// <summary> Name of the file from which the entry was extracted </summary>
        /// <remarks> This is especially useful for statuses and spells as each files split them by categories </remarks>
        public string SourceFile { get; set; }

        /// <summary> The template on which the entry is based, as defined by the "using" property</summary>
        /// <remarks> This is used to establish inheritance among entries </remarks>
        public string UsingReference { get; protected set; }

        /// <summary> The data fields of the entry, as defined by the "data" properties</summary>
        /// <remarks> Contains every specific attribute of the entry </remarks>
        public Dictionary<string, string> DataFields { get; protected set; }

        protected StatDataEntry(
            string name,
            string type,
            string sourceFile,
            string usingReference,
            Dictionary<string, string> dataFields)
        {
            Name = name;
            Type = type;
            SourceFile = sourceFile;
            UsingReference = usingReference;
            DataFields = dataFields;
        }

        /// <summary>
        /// Creates a new instance of StatDataEntry
        /// </summary>
        /// <param name="name">The name of the entry</param>
        /// <param name="type">The type of the entry</param>
        /// <param name="usingReference">The parent of the entry</param>
        /// <param name="sourceFile">The file from which the entry was extracted</param>
        /// <param name="dataFields">The data fields of the entry</param>
        /// <returns></returns>
        public static StatDataEntry Create(string name, string type, string usingReference = null, string sourceFile = null, Dictionary<string, string> dataFields = null)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                throw new Exception("GameObject must have a name");
            }

            if (string.IsNullOrWhiteSpace(type))
            {
                throw new Exception("GameObject must have a type");
            }

            return new StatDataEntry(name, type, sourceFile, usingReference, dataFields ?? new Dictionary<string, string>());
        }

        /// <summary> Is the entry a base template ? </summary>
        /// <remarks>
        /// Base templates are used to define the generic categories of items.
        /// Generally they either match an item category or an equipment slot
        /// </remarks>
        /// <returns></returns>
        public bool IsBase()
        {
            return Name.StartsWith("_") && !IsReference();
        }

        /// <summary> Is the entry a reference template ? </summary>
        /// <remarks> Reference templates are (probably) mainly used for development and testing by the devs </remarks>
        /// <returns></returns>
        public bool IsReference()
        {
            return Name.Contains("_REF") || Name.ToLower().Contains("reference");
        }
    }
}
