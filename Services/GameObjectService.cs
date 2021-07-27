using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GetAllItemsBG3.Objects;

namespace GetAllItemsBG3.Services
{
    public class GameObjectService
    {
        private const string SharedStatsPath = "\\Shared\\Public\\Shared\\Stats\\Generated";
        private const string GustavStatsPath = "\\Gustav\\Public\\Gustav\\Stats\\Generated";

        private static IEnumerable<StatDataEntry> ProcessObjectFiles(string rootPath, string fileName)
        {
            var sharedObjects = FileService.GetObjectFileAsObjects($"{rootPath}{SharedStatsPath}{fileName}").ToList();
            var gustavObjects = FileService.GetObjectFileAsObjects($"{rootPath}{GustavStatsPath}{fileName}");

            sharedObjects.AddRange(gustavObjects);

            // Remove template and reference objects
            sharedObjects = sharedObjects.Where(a => 
                !a.Name.StartsWith("_") && !a.Name.Contains("_REF"))
                .ToList();

            return sharedObjects;
        }

        public static IDictionary<string, List<StatDataEntry>> ProcessGameFiles(string rootPath, string outputPath)
        {
            var usableObjectTypes = new[]
            {
                "_MagicScroll", "_Poison",
                "_Grenade", "_Potion",
                "_Arrow", "_Kit",
                "_Potion_Of_Resistance"
            };

            var armors = ProcessObjectFiles(rootPath, "\\Data\\Armor.txt").ToList();
            var weapons = ProcessObjectFiles(rootPath, "\\Data\\Weapon.txt").ToList();

            var items = ProcessObjectFiles(rootPath, "\\Data\\Object.txt")
                .Where(o => usableObjectTypes.Contains(o.UsingReference)).ToList();

            FileService.WriteModObjectFile($"{outputPath}\\Armor.txt", armors);
            FileService.WriteModObjectFile($"{outputPath}\\Weapon.txt", weapons);
            FileService.WriteModObjectFile($"{outputPath}\\Object.txt", items);

            var objectDictionary = new Dictionary<string, List<StatDataEntry>>
            {
                {"ALL_ARMORS", armors},
                {"ALL_WEAPONS", weapons},
                {"ALL_ITEMS", items}
            };

            return objectDictionary;
        }

        public static void GenerateTreasureTable(string outputPath, IDictionary<string, List<StatDataEntry>> objects, IDictionary<string, int> quantities)
        {
            if (!objects.Keys.All(quantities.ContainsKey))
            {
                throw new Exception("GameObject list and quantities list do not have the same amount of members");
            }

            var tables = new List<TreasureTable>();
            foreach (var (name, list) in objects)
            {
                tables.Add(TreasureTable.Create(name, list, quantities[name]));
            }

            FileService.WriteTreasureTable(outputPath, tables);
        }
    }
}
