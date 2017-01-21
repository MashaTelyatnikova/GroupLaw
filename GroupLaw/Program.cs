using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace GroupLaw
{
    public class Program
    {
        public static string PrepareText(string[] text)
        {
            return Regex.Replace(string.Join(" ",
                text.Select(
                    x =>
                        x.Trim().ToLower()
                            .Replace(".", "")
                            .Replace(",", "")
                            .Replace("!", "")
                            .Replace("?", "")
                            .Replace(";", "")
                            .Replace(":", "")
                )), @"\s+", " ");
        }

        public static void Main(string[] args)
        {
            var text = PrepareText(File.ReadAllLines(args[0]));
            var openText = File.ReadAllText(args[1]).ToLower();
            var count = int.Parse(args[2]);
            var key = DESKeyInfo.Generate();

            var txt = DESHelper.EncryptStringToBytes(Encoding.UTF8.GetBytes(openText), key);

            var trigrams = GetTrigrams(text);
            var bigrams = GetBiigrams(text);
            var letters = GetLetters(text);

            var populationSize = 100;
            var startPopulation =
                Enumerable.Range(0, populationSize).Select(x => new Chromosome(DESKeyInfo.Generate(), DESKeyInfo.Generate())).ToList();

            var random = new Random(new Guid().GetHashCode());
            Console.WriteLine("START");
            for (var i = 0; i < count; ++i)
            {
                Console.WriteLine(i);
                for (var j = 0; j < 5; ++j)
                {
                    var x = random.Next(0, populationSize);
                    var y = random.Next(0, populationSize);
                    while (y == x)
                    {
                        y = random.Next(0, populationSize);
                    }
                    startPopulation.Add(startPopulation[x].Cross(startPopulation[y]));
                }

                startPopulation = startPopulation.OrderBy(x => x.GetFittness(trigrams, bigrams, letters, txt))
                    .Take(populationSize)
                    .ToList();
            }

            var result = new StringBuilder();
            for (var i = 0; i < populationSize; ++i)
            {
                var chr = startPopulation[i];
                result.AppendLine($"K2 = {ByteArrayToString(chr.K2)}");
                result.AppendLine($"K3 = {ByteArrayToString(chr.K3)}");
                result.AppendLine($"P = {chr.P}");
                result.AppendLine(chr.Apply(txt));
                result.AppendLine("----------------------------------------------");
            }

            File.WriteAllText("result.txt", result.ToString());
        }

        public static string ByteArrayToString(byte[] ba)
        {
            StringBuilder hex = new StringBuilder(ba.Length * 2);
            foreach (byte b in ba)
                hex.AppendFormat("{0:x2}", b);
            return hex.ToString();
        }
        public static Dictionary<string, double> GetTrigrams(string text)
        {
            var result = new Dictionary<string, double>();
            for (var i = 0; i < text.Length - 2; i++)
            {
                var word = text.Substring(i, 3);
                if (!result.ContainsKey(word))
                {
                    result[word] = 0;
                }

                result[word]++;
            }

            return result.ToDictionary(x => x.Key, x => x.Value/(text.Length - 2));
        }

        public static Dictionary<string, double> GetBiigrams(string text)
        {
            var result = new Dictionary<string, double>();
            for (var i = 0; i < text.Length - 1; i++)
            {
                var word = text.Substring(i, 2);
                if (!result.ContainsKey(word))
                {
                    result[word] = 0;
                }

                result[word]++;
            }

            return result.ToDictionary(x => x.Key, x => x.Value/(text.Length - 1));
        }

        public static Dictionary<string, double> GetLetters(string text)
        {
            var result = new Dictionary<string, double>();
            for (var i = 0; i < text.Length; i++)
            {
                var word = text.Substring(i, 1);
                if (!result.ContainsKey(word))
                {
                    result[word] = 0;
                }

                result[word]++;
            }

            return result.ToDictionary(x => x.Key, x => x.Value/(text.Length));
        }
    }
}