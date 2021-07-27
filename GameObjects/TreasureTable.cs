using System.Collections.Generic;
using System.Linq;

namespace GetAllItemsBG3.GameObjects
{
    public class TreasureTable
    {
        public string Name { get; protected set; }
        public IEnumerable<string> Objects { get; protected set; }
        public int ItemQuantity { get; protected set; }

        protected TreasureTable(string name, IEnumerable<string> objects, int itemQuantity)
        {
            Name = name;
            Objects = objects;
            ItemQuantity = itemQuantity;
        }

        public static TreasureTable Create(string name, IEnumerable<StatDataEntry> objects, int itemQuantity = 1)
        {
            var objectCategories = objects.Select(o => $"I_MOD_{o.Name}");
            return new TreasureTable(name, objectCategories, itemQuantity);
        }
    }
}
