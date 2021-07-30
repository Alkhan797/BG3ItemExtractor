using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using StatDataLibrary.GameObjects;

namespace StatDataLibrary.Services
{
    public class FileService
    {
        #region Object Parsing methods

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
                bgObject.Data.Add(key, value);
            }

            return bgObject;
        }

        #endregion

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

        public static void WriteModStatFile(string outputPath, IEnumerable<StatDataEntry> bgObjects)
        {
            using StreamWriter file = new(outputPath);
            foreach (var bgObject in bgObjects.Where(o => o.Data.Count > 0))
            {
                var objectAsString = $"new entry \"MOD_{bgObject.Name}\"\n";
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
