using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GetAllItemsBG3.GameObjects;
using GetAllItemsBG3.Services;
using GetAllItemsBG3.Services.Dto;

namespace GetAllItemsBG3
{
    internal class Program
    {
        private static List<EntryTypeSelector> SelectFiles(string gameDirectoryPath)
        {
            var typeSelectors = new List<EntryTypeSelector>();
            var answers = new[] { "yes", "y", "no", "n" };

            // Files without entries
            var entryFileBlackList = new[] { "BloodTypes", "Data", "ItemColor", "ItemProgressionNames", "ItemProgressionVisuals", "XPData" };

            var validFolderFileList = Directory
                .EnumerateFiles($"{gameDirectoryPath}{StatObjectService.SharedStatsPath}\\Data", "*.txt")
                .Where(fileName => !entryFileBlackList.Contains(Path.GetFileNameWithoutExtension(fileName)))
                .Select(Path.GetFileNameWithoutExtension)
                .ToList();

            Console.WriteLine("Extract entries from all the following files ? (y/n) :");
            Console.WriteLine(FormatChoiceList(validFolderFileList));

            var extractFromAllFilesInput = "";
            while (!answers.Contains(extractFromAllFilesInput))
            {
                extractFromAllFilesInput = Console.ReadLine()?.Trim().ToLower();
            }
            var extractFromAllFiles = (extractFromAllFilesInput is "yes" or "y");

            foreach (var fileType in validFolderFileList)
            {
                var selectFileType = "";
                if (!extractFromAllFiles)
                {
                    Console.WriteLine($"{fileType} (y/n) :");
                    while (!answers.Contains(selectFileType))
                    {
                        selectFileType = Console.ReadLine()?.Trim().ToLower();
                    }
                }

                if (extractFromAllFiles || selectFileType is "yes" or "y")
                {
                    typeSelectors.Add(new EntryTypeSelector(fileType));
                }
            }

            return typeSelectors;
        }

        private static string FormatChoiceList(IReadOnlyList<string> choices)
        {
            var formattedString = "";
            for (var i = 0; i < choices.Count; i++)
            {
                formattedString += (i> 3 && i % 4 == 0) ? $"{choices[i]}\n" : $"{choices[i]}\t";
            }

            return formattedString + "\n";
        }

        private static void BrowseSubtypes(IDictionary<string, List<StatDataEntry>> entriesByType)
        {
            var answers = new[] { "yes", "y", "no", "n" };
            foreach (var (type, entries) in entriesByType)
            {
                var subTypes = entries.Select(e => e.UsingReference).Distinct().ToList();
                if (subTypes.Count <= 1) continue;

                Console.WriteLine($"The {type} entries possess the following subtypes :\n");
                Console.WriteLine(FormatChoiceList(subTypes));
                Console.WriteLine("Would you like to pick specific ones ? (y/n) :");

                var browseSubTypes = "";
                while (!answers.Contains(browseSubTypes))
                {
                    browseSubTypes = Console.ReadLine()?.Trim().ToLower();
                }

                if (browseSubTypes is not ("yes" or "y")) continue;

                var subTypeWhiteList = new List<string>();
                foreach (var subType in subTypes)
                {
                    var pickSubType = "";
                    Console.WriteLine($"{type}: Select subtype {subType} (y/n) :");
                    while (!answers.Contains(pickSubType))
                    {
                        pickSubType = Console.ReadLine()?.Trim().ToLower();
                    }

                    if (browseSubTypes is not ("yes" or "y")) continue;
                    subTypeWhiteList.Add(subType);
                }

                entriesByType[type] = StatObjectService.FilterEntrySubtypes(entries, subTypeWhiteList.ToArray(), false, false);
            }
        }

        private static void Main(string[] args)
        {
            Console.WriteLine("Enter unpacked BG3 data root directory");
            var gameDirectoryPath = Console.ReadLine()?.Trim();

            Console.WriteLine("Enter Mod Stats/Generated directory");
            var modDirectoryPath = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(gameDirectoryPath) || string.IsNullOrWhiteSpace(modDirectoryPath))
            {
                Console.WriteLine("Empty Line not allowed");
                Console.Write($"{Environment.NewLine}Press any key to exit...");
                Console.ReadKey(true);
            }

            var typeSelectors = new List<EntryTypeSelector>{
                new("Armor"),
                new("Weapon"),
                new("Object", new[] {"_MagicScroll", "_Poison",
                    "_Grenade", "_Potion", "_Arrow", "_Kit",
                    "_Potion_Of_Resistance" })
            };

            
            var answers = new[] {"yes", "y", "no", "n"};

            Console.WriteLine("Use preset extraction data (Armor/Weapon/Useable Objects) ? (y/n) :");
            var usePreset = "";
            while (!answers.Contains(usePreset))
            {
                usePreset = Console.ReadLine()?.Trim().ToLower();
            }

            if (usePreset is "no" or "n")
            {
                typeSelectors = SelectFiles(gameDirectoryPath);
            }

            var entriesByType = StatObjectService.ProcessStatsEntries(gameDirectoryPath, typeSelectors);

            if (usePreset is "no" or "n")
            {
                BrowseSubtypes(entriesByType);
            }

            if (!Directory.Exists($"{modDirectoryPath}\\Data"))
            {
                Directory.CreateDirectory($"{modDirectoryPath}\\Data");
            }
            foreach (var (type, entries) in entriesByType)
            {
                FileService.WriteModStatFile($"{modDirectoryPath}\\Data\\{type}.txt", entries);
            }

            var currentLootTypes = entriesByType.Keys.Where(t => StatObjectService.LootTypes.Contains(t)).ToArray();

            if (currentLootTypes.Any())
            {
                Console.WriteLine($"Loot entry types detected : {string.Join(" ", currentLootTypes)}");
                Console.WriteLine("Generating TreasureTable");
                var stackSizes = new Dictionary<string, int>();
                foreach (var entryType in currentLootTypes)
                {
                    Console.WriteLine($"Enter Size of the {entryType} stacks");
                    int.TryParse(Console.ReadLine()?.Trim(), out var quantity);
                    stackSizes.Add(entryType, quantity > 0 ? quantity : 1);
                }

                StatObjectService.GenerateTreasureTable(modDirectoryPath, entriesByType, stackSizes);
            }
            

            Console.Write($"{Environment.NewLine}Press any key to exit...");
            Console.ReadKey(true);
        }
    }
}
