using System.Collections.Generic;
using System.IO;
using System.Linq;
using StatDataLibrary.GameObjects;
using StatDataLibrary.Services.Dto;

namespace StatDataLibrary.Services
{
    /// <summary>
    /// Service to manipulate the entries read from the unpacked Stats folder, extracted from Baldur's Gate 3 Shared.pak and Gustav.pak
    /// </summary>
    public class StatObjectService
    {
        /// <summary>
        /// Lootable types of items regrouped by their "type" property
        /// </summary>
        public static string[] LootTypes = { "Armor", "Weapon", "Object" };

        /// <summary>
        /// Path to take from the root of the unpacked files to reach the data folder
        /// </summary>
        /// <remarks>Shared contains all of the common item types, regardless of their rarity</remarks>
        public const string SharedStatsPath = "\\Shared\\Public\\Shared\\Stats\\Generated";

        /// <summary>
        /// Path to take from the root of the unpacked files to reach the data folder
        /// </summary>
        /// <remarks>Gustav contains every unique and story items</remarks>
        public const string GustavStatsPath = "\\Gustav\\Public\\Gustav\\Stats\\Generated";

        /// <summary>
        /// Extract all entries from the .txt files in the stats Data folder
        /// </summary>
        /// <param name="compiledPath">The path to the folder containing the Data folder</param>
        /// <param name="fileFilter">The list of files to process (null = all)</param>
        /// <returns>The complete list of entries extracted from the Data directory</returns>
        private static IEnumerable<StatDataEntry> ProcessStatsDataEntryFolder(string compiledPath, ICollection<string> fileFilter = null)
        {
            var compiledEntries = new List<StatDataEntry>();
            foreach (var file in Directory.EnumerateFiles($"{compiledPath}\\Data", "*.txt"))
            {
                if (fileFilter != null && fileFilter.Contains(Path.GetFileNameWithoutExtension(file)))
                {
                    compiledEntries.AddRange(StatsFileService.ReadFileDataEntries(file));
                }
                else if (fileFilter == null)
                {
                    compiledEntries.AddRange(StatsFileService.ReadFileDataEntries(file));
                }
            }

            return compiledEntries;
        }

        /// <summary>
        /// Extract and filters entries in the stats Data folder
        /// </summary>
        /// <param name="rootPath">The path to the root of the unpacked game files</param>
        /// <param name="typeSelectors">The list of filters by entry types to be applied</param>
        /// <returns>The total list of entries sorted by the files from which they were extracted</returns>
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
                            (typeSelector.UseBases == null || e.IsBase() == typeSelector.UseBases)
                            ).ToList());
                }
            }

            return entriesByFileType;
        }

        /// <summary>
        /// Filters a given list of stat entries given their "using" property and name pattern
        /// </summary>
        /// <param name="entries">The list of entries to be filtered</param>
        /// <param name="subTypeWhiteList">The list of subtypes ("using" property value) to extract</param>
        /// <param name="useBases">Whether to use only (true), exclude (false) or include (null) base templates</param>
        /// <param name="useReferences">Whether to only use (true), exclude (false) or include (null) reference templates</param>
        /// <returns>The filtered list of entries</returns>
        public static List<StatDataEntry> FilterEntrySubtypes(List<StatDataEntry> entries, string[] subTypeWhiteList, bool? useBases = null, bool? useReferences = null)
        {
            return entries.Where(e =>
                (subTypeWhiteList == null || subTypeWhiteList.Contains(e.UsingReference)) &&
                (useReferences == null || e.IsReference() == useReferences) &&
                (useBases == null || e.IsBase() == useBases)
            ).ToList();
        }

        /// <summary>
        /// Generates a list of treasure tables, printable into the TreasureTable.txt file via StatsFileService.WriteTreasureTable
        /// </summary>
        /// <param name="entriesByType"></param>
        /// <param name="quantities"></param>
        /// <param name="modName">The name of your mod</param>
        /// <returns>The generated list of treasure tables</returns>
        public static List<TreasureTable> GenerateTreasureTable(IDictionary<string, List<StatDataEntry>> entriesByType, IDictionary<string, int> quantities, string modName = null)
        {
            var lootEntries = entriesByType.Where(e => LootTypes.Contains(e.Key));

            var tables = new List<TreasureTable>();
            foreach (var (type, entries) in lootEntries)
            {
                tables.Add(TreasureTable.Create($"ALL_{type.ToUpper()}S", entries, quantities[type], modName));
            }

            return tables;
        }
    }
}
