﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using StatDataLibrary.GameObjects;
using StatDataLibrary.Services;
using StatDataLibrary.Services.Dto;

namespace GetAllItemsBG3
{
    internal class Program
    {
        #region Misc Methods

        private static string FormatChoiceList(IReadOnlyList<string> choices)
        {
            var formattedString = "";
            for (var i = 0; i < choices.Count; i++)
            {
                formattedString += (i > 3 && i % 4 == 0) ? $"{choices[i]}\n" : $"{choices[i]}\t";
            }

            return formattedString + "\n";
        }

        #endregion

        #region File Selection

        private static List<EntryTypeSelector> SelectFiles(string gameDirectoryPath)
        {
            var typeSelectors = new List<EntryTypeSelector>();
            var answers = new[] { "yes", "y", "no", "n" };

            // Files without entries
            var entryFileBlackList = new[] { "BloodTypes", "DataFields", "ItemColor", "ItemProgressionNames", "ItemProgressionVisuals", "XPData" };

            var validFolderFileList = Directory
                .EnumerateFiles($"{gameDirectoryPath}{StatObjectService.SharedStatsPath}\\DataFields", "*.txt")
                .Where(fileName => !entryFileBlackList.Contains(Path.GetFileNameWithoutExtension(fileName)))
                .Select(Path.GetFileNameWithoutExtension)
                .ToList();

            Console.Clear();
            Console.WriteLine("Extract entries from all the following files ? (y/n) :\n");
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

        #endregion

        #region Type Selection

        private static void BrowseType(IDictionary<string, List<StatDataEntry>> entriesByType)
        {
            var answers = new[] { "yes", "y", "no", "n" };
            var filteredResult = new List<StatDataEntry>();

            foreach (var (type, entries) in entriesByType)
            {
                var categories = new Dictionary<string, List<StatDataEntry>>
                {
                    { "Base (To create from scratch)", entries.Where(e => e.IsBase()).ToList() },
                    { "Reference (Only useful for dev and testing)", entries.Where(e => e.IsReference()).ToList() },
                    { "Template (for new versions of an already existing item)", entries.Where(e => !e.IsReference() && !e.IsBase()).ToList() }
                };

                var categoryNames = categories.Where(c => c.Value.Any())
                    .Select(c => c.Key)
                    .OrderBy(c => c);
                
                Console.Clear();
                Console.WriteLine($"The type {type} possess the following categories of entries :\n");
                foreach (var categoryName in categoryNames)
                {
                    Console.WriteLine(categoryName);
                }
                Console.WriteLine("\nWould you like to pick or discard specific ones ? (y/n) :");
                var browseCategory = "";
                while (!answers.Contains(browseCategory))
                {
                    browseCategory = Console.ReadLine()?.Trim().ToLower();
                }

                if (browseCategory is "no" or "n") continue;

                BrowseCategories(categories, type);
                entriesByType[type] = categories.SelectMany(c => c.Value).ToList();
            }
        }

        #endregion

        #region Category Selection

        private static void BrowseCategories(IDictionary<string, List<StatDataEntry>> entriesByCategories, string type)
        {
            var answers = new[] { "yes", "y", "no", "n", "discard", "d" };

            foreach (var (category, entries) in entriesByCategories.Where(e => e.Value.Any()))
            {
                var subTypes = entries.Select(e => e.UsingReference ?? $"[Base/Root {type})]")
                    .Distinct().OrderBy(st => st).ToArray();

                if (subTypes.Length <= 1) continue;

                Console.Clear();
                Console.WriteLine($"The category {category} for the type {type} possess the following subtypes :\n");
                Console.WriteLine(FormatChoiceList(subTypes));
                Console.WriteLine("\nWould you like to pick specific ones ? (y/n/d discard category) :");

                var browseSubTypes = "";
                while (!answers.Contains(browseSubTypes))
                {
                    browseSubTypes = Console.ReadLine()?.Trim().ToLower();
                }

                switch (browseSubTypes)
                {
                    case "no" or "n":
                        continue;
                    case "discard" or "d":
                        entriesByCategories[category] = new List<StatDataEntry>();
                        continue;
                }

                var subTypeWhiteList = new List<string>();
                foreach (var subType in subTypes)
                {
                    var pickSubType = "";
                    Console.WriteLine($"{category}: Select subtype {subType} (y/n) :");
                    while (!answers.Contains(pickSubType))
                    {
                        pickSubType = Console.ReadLine()?.Trim().ToLower();
                    }

                    if (pickSubType is not ("yes" or "y")) continue;
                    subTypeWhiteList.Add(subType);
                }

                entriesByCategories[category] = StatObjectService.FilterEntrySubtypes(entries, subTypeWhiteList.ToArray());
            }
        }

        #endregion

        private static void Main(string[] args)
        {
            Console.WriteLine("Enter unpacked BG3 data root directory");
            var gameDirectoryPath = Console.ReadLine()?.Trim();

            Console.WriteLine("Enter Mod root directory");
            var modDirectoryPath = Console.ReadLine()?.Trim();

            if (string.IsNullOrWhiteSpace(gameDirectoryPath) || string.IsNullOrWhiteSpace(modDirectoryPath))
            {
                Console.WriteLine("Empty Line not allowed");
                Console.Write($"{Environment.NewLine}Press any key to exit...");
                Console.ReadKey(true);
            }

            var modName = Path.GetFileNameWithoutExtension(modDirectoryPath);

            var presetSelectors = new List<EntryTypeSelector>{
                new("Armor", useBases:false, useReferences:false),
                new("Weapon", useBases:false, useReferences:false),
                new("Object", new[] {"_MagicScroll", "_Poison",
                    "_Grenade", "_Potion", "_Arrow", "_Kit",
                    "_Potion_Of_Resistance" }, false, false)
            };

            var baseSelectors = new List<EntryTypeSelector>{
                new("Armor", useBases:true),
                new("Weapon", useBases:true),
                new("Object", useBases:true)
            };

            var usePreset = "";
            var useBasePreset = "";
            var answers = new[] {"yes", "y", "no", "n"};
            var modStatsOutput = $"{modDirectoryPath}\\Public\\{modName}\\Stats\\Generated";

            
            Console.WriteLine("Use base item (WARNING ! For from scratch creation purposes only !) ? (y/n) :");
            while (!answers.Contains(useBasePreset))
            {
                useBasePreset = Console.ReadLine()?.Trim().ToLower();
            }

            if (useBasePreset is "no" or "n")
            {
                Console.WriteLine("Use duplication preset (Armor/Weapon/Useable Objects) ? (y/n) :");
                while (!answers.Contains(usePreset))
                {
                    usePreset = Console.ReadLine()?.Trim().ToLower();
                }

                baseSelectors = usePreset is "no" or "n" ? SelectFiles(gameDirectoryPath) : presetSelectors;
            }

            var entriesByType = StatObjectService.ProcessStatsEntries(gameDirectoryPath, baseSelectors);

            // Manual selection mode
            if (usePreset is "no" or "n" && useBasePreset is "no" or "n")
            {
                BrowseType(entriesByType);
            }

            if (!Directory.Exists($"{modStatsOutput}\\Data"))
            {
                Directory.CreateDirectory($"{modStatsOutput}\\Data");
            }
            foreach (var (type, entries) in entriesByType)
            {
                StatsFileService.WriteModStatFile($"{modStatsOutput}\\Data\\{type}.txt", entries, modName);
            }

            var currentLootTypes = entriesByType.Keys.Where(t => StatObjectService.LootTypes.Contains(t)).ToArray();

            if (currentLootTypes.Any())
            {
                Console.Clear();
                Console.WriteLine($"Loot entry types detected : {string.Join(" ", currentLootTypes)}");
                Console.WriteLine("Generating TreasureTable");
                var stackSizes = new Dictionary<string, int>();
                foreach (var entryType in currentLootTypes)
                {
                    Console.WriteLine($"Enter Size of the {entryType} stacks");
                    int.TryParse(Console.ReadLine()?.Trim(), out var quantity);
                    stackSizes.Add(entryType, quantity > 0 ? quantity : 1);
                }

                var tables = StatObjectService.GenerateTreasureTable(entriesByType, stackSizes, modName);
                StatsFileService.WriteTreasureTable($"{modStatsOutput}", tables);
            }
            

            Console.Write($"{Environment.NewLine}Press any key to exit...");
            Console.ReadKey(true);
        }
    }
}
