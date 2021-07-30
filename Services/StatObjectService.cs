using System.Collections.Generic;
using System.IO;
using System.Linq;
using GetAllItemsBG3.GameObjects;
using GetAllItemsBG3.Services.Dto;

namespace GetAllItemsBG3.Services
{
    public class StatObjectService
    {
        public const string SharedStatsPath = "\\Shared\\Public\\Shared\\Stats\\Generated";
        public const string GustavStatsPath = "\\Gustav\\Public\\Gustav\\Stats\\Generated";
        public static string[] LootTypes = { "Armor", "Weapon", "Object" };

        public static IEnumerable<StatDataEntry> ProcessStatsDataEntryFolder(string compiledPath, ICollection<string> fileTypeFilter = null)
        {
            var compiledEntries = new List<StatDataEntry>();
            foreach (var file in Directory.EnumerateFiles($"{compiledPath}\\Data", "*.txt"))
            {
                if (fileTypeFilter != null && fileTypeFilter.Contains(Path.GetFileNameWithoutExtension(file)))
                {
                    compiledEntries.AddRange(FileService.ReadFileDataEntries(file));
                }
                else if (fileTypeFilter == null)
                {
                    compiledEntries.AddRange(FileService.ReadFileDataEntries(file));
                }
            }

            return compiledEntries;
        }

        public static IDictionary<string, List<StatDataEntry>> ProcessStatsEntries(string rootPath, List<EntryTypeSelector> typeSelectors = null)
        {
            var filesToProcess = typeSelectors?.Select(t => t.FileTypeName).ToList();
            var compiledEntries = ProcessStatsDataEntryFolder($"{rootPath}{SharedStatsPath}", filesToProcess).ToList();
            compiledEntries.AddRange(ProcessStatsDataEntryFolder($"{rootPath}{GustavStatsPath}", filesToProcess));

            var entriesByFileType = new Dictionary<string, List<StatDataEntry>>();
            
            if (typeSelectors == null)
            {
                var entryFileTypes = compiledEntries.Select(e => e.SourceFile).Distinct().ToList();
                entriesByFileType = entryFileTypes.ToDictionary(
                    type => type,
                    type => compiledEntries.Where(e => e.SourceFile == type).ToList());
            }
            else
            {
                foreach (var typeSelector in typeSelectors)
                {
                    entriesByFileType.Add(typeSelector.FileTypeName, compiledEntries
                        .Where(e =>
                            e.SourceFile == typeSelector.FileTypeName &&
                            (typeSelector.SubTypeWhiteList == null || typeSelector.SubTypeWhiteList.Contains(e.UsingReference)) &&
                            (typeSelector.UseReferences == null || e.IsReference() == typeSelector.UseReferences) &&
                            (typeSelector.UseBase == null || e.IsBase() == typeSelector.UseBase)
                            ).ToList());
                }
            }

            return entriesByFileType;
        }

        public static List<StatDataEntry> FilterEntrySubtypes(List<StatDataEntry> entries, string[] subTypeWhiteList, bool? includeTemplates = null, bool? includeReferences = null)
        {
            return entries.Where(e =>
                (subTypeWhiteList == null || subTypeWhiteList.Contains(e.UsingReference)) &&
                (includeReferences == null || e.IsReference() == includeReferences) &&
                (includeTemplates == null || e.IsBase() == includeTemplates)
            ).ToList();
        }

        public static void GenerateTreasureTable(string outputPath, IDictionary<string, List<StatDataEntry>> entriesByType, IDictionary<string, int> quantities)
        {
            var lootEntries = entriesByType.Where(e => LootTypes.Contains(e.Key));

            var tables = new List<TreasureTable>();
            foreach (var (type, entries) in lootEntries)
            {
                tables.Add(TreasureTable.Create($"ALL_{type.ToUpper()}S", entries, quantities[type]));
            }

            FileService.WriteTreasureTable(outputPath, tables);
        }
    }
}
