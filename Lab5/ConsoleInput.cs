using System;
using System.Globalization;
using System.IO;

namespace Lab5
{
    internal static class ConsoleInput
    {
        public static int ReadInt32(string prompt, int minValue, int maxValue)
        {
            while (true)
            {
                Console.Write(prompt);
                var isNumber = int.TryParse(ReadLine(), CultureInfo.InvariantCulture, out var value);
                if (isNumber && value >= minValue && value <= maxValue)
                {
                    return value;
                }

                Console.WriteLine($"Нужно целое число от {minValue} до {maxValue}.");
            }
        }

        public static string ReadText(string prompt)
        {
            while (true)
            {
                Console.Write(prompt);
                var text = ReadLine().Trim();
                if (text.Length > 0)
                {
                    return text;
                }

                Console.WriteLine("Строка не должна быть пустой.");
            }
        }

        public static string ReadTextOrDefault(string prompt, string defaultValue)
        {
            Console.Write(prompt);
            var text = ReadLine().Trim();
            if (text.Length == 0)
            {
                return defaultValue;
            }

            return text;
        }

        private static string ReadLine()
        {
            return Console.ReadLine() ?? throw new EndOfStreamException("Ввод с клавиатуры закончился.");
        }
    }
}
