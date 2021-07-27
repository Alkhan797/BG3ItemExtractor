using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GetAllItemsBG3.Services;
using GetAllItemsBG3.Services.Dto;

namespace GetAllItemsBG3
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Enter unpacked BG3 data root directory");
            var gameDirectoryPath = Console.ReadLine()?.Trim();

            Console.WriteLine("Enter Mod Stats/Generated directory");
            var modDirectoryPath = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(gameDirectoryPath) || string.IsNullOrWhiteSpace(modDirectoryPath))
            {
                Console.Write("Empty Line not allowed");
                Console.Write($"{Environment.NewLine}Press any key to exit...");
                Console.ReadKey(true);
            }

            var typeSelectors = new List<EntryTypeSelector>
            {
                new("Armor"),
                new("Weapon"),
                new("Object", new[] {"_MagicScroll", "_Poison",
                    "_Grenade", "_Potion", "_Arrow", "_Kit",
                    "_Potion_Of_Resistance" }),
                //new("Passive"),
                //new("Status_BOOST")
            };

            var entriesByType = StatObjectService.ProcessStatsEntries(gameDirectoryPath);

            if (!Directory.Exists($"{modDirectoryPath}\\Data"))
            {
                Directory.CreateDirectory($"{modDirectoryPath}\\Data");
            }
            foreach (var (type, entries) in entriesByType)
            {
                FileService.WriteModStatFile($"{modDirectoryPath}\\Data\\{type}.txt", entries);
            }

            var stackSizes = new Dictionary<string, int>();
            foreach (var entryType in StatObjectService.LootTypes)
            {
                Console.WriteLine($"Enter Size of the {entryType} stacks");
                int.TryParse(Console.ReadLine()?.Trim(), out var quantity);
                stackSizes.Add(entryType, quantity > 0 ? quantity : 1);
            }

            StatObjectService.GenerateTreasureTable(modDirectoryPath, entriesByType, stackSizes);

            Console.Write($"{Environment.NewLine}Press any key to exit...");
            Console.ReadKey(true);
        }
    }
}
