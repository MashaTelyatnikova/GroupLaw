using System;
using System.Collections.Generic;
using System.Text;

namespace GroupLaw
{
    public class Chromosome
    {
        private readonly DESKeyInfo k3;
        private readonly DESKeyInfo k2;
        public int P { get; set; }
        private double? fittness;

        public Chromosome(DESKeyInfo k2, DESKeyInfo k3)
        {
            this.k2 = k2;
            this.k3 = k3;
        }

        public byte[] K3 => k3.Key;
        public byte[] K2 => k2.Key;
        public void Mutate()
        {
            var random = new Random(new Guid().GetHashCode());
            var x = random.Next(0, k3.Key.Length);
            var y = random.Next(0, k3.Key.Length);
            while (y == x)
            {
                y = random.Next(0, k3.Key.Length);
            }

            var tmp = k3.Key[x];
            k3.Key[x] = k3.Key[y];
            k3.Key[y] = tmp;
        }

        public Chromosome Cross(Chromosome other)
        {
            var random = new Random(new Guid().GetHashCode());
            var childK2 = new byte[k2.Key.Length];

            for (var i = 0; i < childK2.Length; ++i)
            {
                var fromFirst = random.NextDouble() < 0.5;
                childK2[i] = fromFirst ? k2.Key[i] : other.k2.Key[i];
            }

            var childK2Info = new DESKeyInfo
            {
                Key = childK2,
                IV = random.NextDouble() < 0.5 ? k3.IV : other.k3.IV
            };

            var childK3 = new byte[k3.Key.Length];

            for (var i = 0; i < childK3.Length; ++i)
            {
                var fromFirst = random.NextDouble() < 0.5;
                childK3[i] = fromFirst ? k3.Key[i] : other.k3.Key[i];
            }

            var childK3Info = new DESKeyInfo
            {
                Key = childK3,
                IV = random.NextDouble() < 0.5 ? k3.IV : other.k3.IV
            };
            return new Chromosome(childK2Info, childK3Info);
        }

        public double GetFittness(Dictionary<string, double> trainTrigrams, Dictionary<string, double> trainBigrams,
            Dictionary<string, double> trainLetters, byte[] text)
        {
            if (fittness.HasValue)
            {
                return fittness.Value;
            }
            var p = 1;
            var min = double.MaxValue;
            text = DESHelper.EncryptStringToBytes(text, k2);
            for (var i = 1; i < 10; ++i)
            {
                var applyed = Apply(text, i);
                var chipcherTrigrams = Program.GetTrigrams(applyed);
                var chipperBigrams = Program.GetBiigrams(applyed);
                var chipperLetters = Program.GetLetters(applyed);

                var s1 = GetSum(trainTrigrams, chipcherTrigrams);
                s1 += GetSum(trainBigrams, chipperBigrams);
                s1 += GetSum(trainLetters, chipperLetters);
                if (s1 <= min)
                {
                    min = s1;
                    p = i;
                }
            }

            P = p;
            return min;
        }

        public string Apply(byte[] text, int p)
        {
            while (p != 0)
            {
                text = DESHelper.EncryptStringToBytes(text, k3);
                --p;
            }

            return Encoding.UTF8.GetString(text);
        }
        public string Apply(byte[] text)
        {
            text = DESHelper.EncryptStringToBytes(text, k2);
            var p = P;
            while (p != 0)
            {
                text = DESHelper.EncryptStringToBytes(text, k3);
                --p;
            }

            return Encoding.UTF8.GetString(text);
        }

        public double GetSum(Dictionary<string, double> source, Dictionary<string, double> target)
        {
            var result = 0.0;
            foreach (var val in source)
            {
                var r = val.Value - (target.ContainsKey(val.Key) ? target[val.Key] : 0);
                result += r*r;
            }

            return result;
        }
    }
}