using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using GetAllItemsBG3.Objects;

namespace GetAllItemsBG3.Services
{
    public class FileService
    {
        #region Object Parsing methods

        private static int GetEntryLineEnd(IReadOnlyList<string> fileLines, int entryLineStart)
        {
            for (var i = entryLineStart + 1; i < fileLines.Count; i++)
            {
                if (fileLines[i] == string.Empty || fileLines[i].Contains("new "))
                {
                    return i;
                }
            }

            return fileLines.Count;
        }

        private static StatDataEntry ReadBaldursGateObjectEntry(IReadOnlyList<string> entry)
        {
            var quoteContents = new Regex("(?<= \\\")(.*?)(?=\\s*\\\")");

            // Every object always has at least a name and a type
            var name = quoteContents.Match(entry[0]).Value;
            var type = quoteContents.Match(entry[1]).Value;
            var usingLine = entry.FirstOrDefault(line => line.Contains("using "));
            var usingReference = usingLine != null ? quoteContents.Match(usingLine).Value : null;

            var bgObject = StatDataEntry.Create(name, type, usingReference);

            var dataLines = entry.Where(line => line.Contains("data "));
            foreach (var data in dataLines)
            {
                var key = quoteContents.Matches(data)[0].Value;
                var value = quoteContents.Matches(data)[1].Value;
                bgObject.Data.Add(key, value);
            }

            return bgObject;
        }

        #endregion

        public static IEnumerable<StatDataEntry> GetObjectFileAsObjects(string inputPath)
        {
            var objectList = new List<StatDataEntry>();
            var fileLines = File.ReadAllLines(inputPath);

            for (var i = 0; i < fileLines.Length; i++)
            {
                var line = fileLines[i];

                if (!line.Contains("new ")) continue;
                var entryLineEnd = GetEntryLineEnd(fileLines, i);
                var entry = fileLines.Skip(i).Take(entryLineEnd - i).ToArray();

                try
                {
                    objectList.Add(ReadBaldursGateObjectEntry(entry));
                }
                catch (Exception e)
                {
                    Console.WriteLine($"{e.Message}; at lines {i}-{entryLineEnd}");
                    throw;
                }
            }

            return objectList;
        }

        public static void WriteModObjectFile(string outputPath, IEnumerable<StatDataEntry> bgObjects)
        {
            using StreamWriter file = new(outputPath);
            foreach (var bgObject in bgObjects.Where(o => o.Data.Count > 0))
            {
                var objectAsString = $"new entry \"MOD_{bgObject.Name}\"\n";
                objectAsString += $"type \"{bgObject.Type}\"\n";
                objectAsString += $"using \"{bgObject.Name}\"\n";
                objectAsString += $"data \"RootTemplate \"{Guid.NewGuid()}\"\n";
                objectAsString += "data \"MinLevel\" \"1\"\n";

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

                foreach (var objectCategory in table.Objects)
                {
                    file.WriteLine($"new subtable \"{table.ItemQuantity},1\"");
                    file.WriteLine($"object category \"{objectCategory}\",1,0,0,0,0,0,0,0");
                }

                file.WriteLine("\n");
            }
        }
    }
}
