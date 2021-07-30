using System.Collections.Generic;
using System.Linq;

namespace StatDataLibrary.GameObjects
{
    /// <summary>
    /// Represent a given treasure table, or loot table in the TreasureTable.txt file
    /// </summary>
    public class TreasureTable
    {
        /// <summary>
        /// Name of the unique treasure/loot table
        /// </summary>
        public string Name { get; protected set; }

        /// <summary>
        /// Compiled names of the items belonging to the treasure/loot table
        /// </summary>
        /// 
        public IEnumerable<string> Objects { get; protected set; }

        /// <summary>
        /// Generic quantity of every item in the treasure/loot table
        /// </summary>
        public int ItemQuantity { get; protected set; }

        protected TreasureTable(string name, IEnumerable<string> objects, int itemQuantity)
        {
            Name = name;
            Objects = objects;
            ItemQuantity = itemQuantity;
        }

        /// <summary>
        /// Return a new instance of a treasure table
        /// </summary>
        /// <param name="name">Name of the unique treasure/loot table</param>
        /// <param name="objects">List of items to be integrated in the treasure/loot table</param>
        /// <param name="itemQuantity">Generic quantity of every item in the treasure/loot table</param>
        /// <param name="modName">The name of your mod</param>
        /// <returns>A treasure/loot table populated with a given entry list</returns>
        public static TreasureTable Create(string name, IEnumerable<StatDataEntry> objects, int itemQuantity = 1, string modName = null)
        {
            modName ??= "MOD";
            var objectCategories = objects.Select(o => o.IsBase() ? $"I_{modName}_[PLACEHOLDER{o.Name}]" : $"I_MOD_{o.Name}");
            return new TreasureTable(name, objectCategories, itemQuantity);
        }
    }
}
