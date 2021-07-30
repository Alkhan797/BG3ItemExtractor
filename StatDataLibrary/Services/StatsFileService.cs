using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using StatDataLibrary.GameObjects;

namespace StatDataLibrary.Services
{
    /// <summary>
    /// Service to read and write from the unpacked Stats folder extracted from Baldur's Gate 3 Shared.pak and Gustav.pak
    /// </summary>
    public class StatsFileService
    {
        #region Object Parsing methods

        /// <summary>
        /// Extract the last line of an entry given its start line
        /// </summary>
        /// <param name="fileLines">The .txt file as lines</param>
        /// <param name="entryStartLine">The start line of the entry</param>
        /// <returns></returns>
        private static int GetEntryEndLine(IReadOnlyList<string> fileLines, int entryStartLine)
        {
            for (var i = entryStartLine + 1; i < fileLines.Count; i++)
            {
                if (fileLines[i] == string.Empty || fileLines[i].Contains("new "))
                {
                    return i;
                }
            }

            return fileLines.Count;
        }

        private static StatDataEntry ReadStatDataEntry(IReadOnlyList<string> entryLines)
        {
            var quoteContents = new Regex("(?<= \\\")(.*?)(?=\\s*\\\")");

            // Every entry always has at least a name and a type
            var name = quoteContents.Match(entryLines[0]).Value;
            var type = quoteContents.Match(entryLines[1]).Value;
            var usingLine = entryLines.FirstOrDefault(line => line.Contains("using "));
            var usingReference = usingLine != null ? quoteContents.Match(usingLine).Value : null;

            var bgObject = StatDataEntry.Create(name, type, usingReference);

            var dataLines = entryLines.Where(line => line.Contains("data "));
            foreach (var data in dataLines)
            {
                var key = quoteContents.Matches(data)[0].Value;
                var value = quoteContents.Matches(data)[1].Value;
                bgObject.DataFields.Add(key, value);
            }

            return bgObject;
        }

        #endregion

        /// <summary>
        /// Extract all entries from a given .txt file
        /// </summary>
        /// <param name="inputPath"></param>
        /// <returns>All valid entries present in the file</returns>
        public static IEnumerable<StatDataEntry> ReadFileDataEntries(string inputPath)
        {
            if (!File.Exists(inputPath))
            {
                var message = $"Attempted to parse an non-existent file : {inputPath}";
                Console.WriteLine(message);
                throw new Exception(message);
            }

            var objectList = new List<StatDataEntry>();
            var fileLines = File.ReadAllLines(inputPath);

            for (var i = 0; i < fileLines.Length; i++)
            {
                var line = fileLines[i];

                if (!line.Contains("new entry")) continue;
                var entryEndLine = GetEntryEndLine(fileLines, i);
                var entryLines = fileLines.Skip(i).Take(entryEndLine - i).ToArray();

                try
                {
                    var entry = ReadStatDataEntry(entryLines);
                    entry.SourceFile = Path.GetFileNameWithoutExtension(inputPath);
                    objectList.Add(entry);
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{e.Message}; at lines {i}-{entryEndLine}");
                    throw;
                }
            }

            return objectList;
        }

        /// <summary>
        /// Outputs the processed entries as mod entries with a unique name and guid/rootemplate id
        /// </summary>
        /// <param name="outputPath">The path to the file you wish to populate</param>
        /// <param name="bgObjects">The list of entries you wish to output</param>
        /// <param name="modName">The name of your mod</param>
        public static void WriteModStatFile(string outputPath, IEnumerable<StatDataEntry> bgObjects, string modName = null)
        {
            modName ??= "MOD";
            using StreamWriter file = new(outputPath);
            foreach (var bgObject in bgObjects.Where(o => o.DataFields.Count > 0))
            {
                var objectAsString = $"new entry \"{modName}_{bgObject.Name}\"\n";
                objectAsString += $"type \"{bgObject.Type}\"\n";
                objectAsString += $"using \"{bgObject.Name}\"\n";
                objectAsString += $"data \"RootTemplate \"{Guid.NewGuid()}\"\n";

                if (StatObjectService.LootTypes.Contains(bgObject.Type))
                {
                    objectAsString += "data \"MinLevel\" \"1\"\n";
                }

                file.WriteLine(objectAsString);
            }
        }

        /// <summary>
        /// Output a given list of treasure/loot table into TreasureTable.txt
        /// </summary>
        /// <param name="outputPath">The path of the folder you wish to populate</param>
        /// <param name="tables">The list of tables you wish to insert in the generated file</param>
        public static void WriteTreasureTable(string outputPath, IEnumerable<TreasureTable> tables)
        {
            using StreamWriter file = new($"{outputPath}\\TreasureTable.txt");
            file.WriteLine("treasure itemtypes \"Common\",\"Uncommon\",\"Rare\",\"Epic\",\"Legendary\",\"Divine\",\"Unique\"\n");

            foreach (var table in tables)
            {
                file.WriteLine($"new treasuretable \"{table.Name}\"");

                foreach (var lootObject in table.Objects)
                {
                    file.WriteLine($"new subtable \"{table.ItemQuantity},1\"");
                    file.WriteLine($"object category \"{lootObject}\",1,0,0,0,0,0,0,0");
                }

                file.WriteLine("\n");
            }
        }
    }
}
