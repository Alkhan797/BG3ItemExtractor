using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using GetAllItemsBG3.Services;

namespace GetAllItemsBG3
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            Console.WriteLine("Enter unpacked BG3 data root directory");
            var gameDirectoryPath = Console.ReadLine()?.Trim();

            Console.WriteLine("Enter Mod Data file directory");
            var modDirectoryPath = Console.ReadLine()?.Trim();

            Console.WriteLine("Enter Size of the Armor stacks");
            int.TryParse(Console.ReadLine()?.Trim(), out var armorQuantity);

            Console.WriteLine("Enter Size of the Weapon stacks");
            int.TryParse(Console.ReadLine()?.Trim(), out var weaponQuantity);

            Console.WriteLine("Enter Size of the Item stacks");
            int.TryParse(Console.ReadLine()?.Trim(), out var itemQuantity);

            if (string.IsNullOrWhiteSpace(gameDirectoryPath) || string.IsNullOrWhiteSpace(modDirectoryPath))
            {
                Console.Write("Empty Line not allowed");
                Console.Write($"{Environment.NewLine}Press any key to exit...");
                Console.ReadKey(true);
            }

            var quantities = new Dictionary<string, int>
            {
                {"ALL_ARMORS", armorQuantity > 0 ? armorQuantity : 1},
                {"ALL_WEAPONS", weaponQuantity > 0 ? weaponQuantity : 1},
                {"ALL_ITEMS", itemQuantity > 0 ? itemQuantity : 1}
            };

            var gameObjects = GameObjectService.ProcessGameFiles(gameDirectoryPath, modDirectoryPath);
            GameObjectService.GenerateTreasureTable(modDirectoryPath, gameObjects, quantities);

            Console.Write($"{Environment.NewLine}Press any key to exit...");
            Console.ReadKey(true);
        }
    }
}
