using System;
using System.Collections;
using System.Collections.Generic;

namespace TotallyNotAnArchiver.Logic.Huffman
{
    class HuffmanEncoder : BaseHuffmanTree
    {
        public void Build(string input)
        {
            foreach (var t in input)
            {
                if (!Rates.ContainsKey(t))
                {
                    Rates.Add(t, 0);
                }

                Rates[t]++;
            }

            AfterBuildPrep();
        }

        public BitArray Encode(string input)
        {
            var encoded = new List<bool>();
            for (var i = 0; i < input.Length; i++)
            {
                var encodedCharPath = _root.Traverse(input[i], new List<bool>());
                encoded.AddRange(encodedCharPath);
                if (i % 100000 == 0)
                    Console.WriteLine($"{i} of {input.Length}: {i * 100 / input.Length}%");
            }

            var bits = new BitArray(encoded.ToArray());

            return bits;
        }
    }
}
