using System;
using System.Globalization;
using System.IO;

namespace Lab5
{
    internal static class ConsoleInput
    {
        public static int ReadInt32(string prompt, int minValue, int maxValue)
        {
            bool isNumber;
            int value;
            while (true)
            {
                Console.Write(prompt);
                isNumber = int.TryParse(ReadLine(), CultureInfo.InvariantCulture, out value);
                if (isNumber && value >= minValue && value <= maxValue)
                {
                    return value;
                }

                Console.WriteLine($"Нужно целое число от {minValue} до {maxValue}.");
            }
        }

        public static string ReadText(string prompt)
        {
            string text;
            while (true)
            {
                Console.Write(prompt);
                text = ReadLine().Trim();
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
