using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using TotallyNotAnArchiver.Logic.Huffman;
using TotallyNotAnArchiver.Logic.LZSS;

namespace TotallyNotAnArchiver.CommandsRunner
{
    public static class PipeLine
    {
        public static void CreateEncodedPipeLine(string inFileName, string outFileName,
            string dictFileName = "HuffmanDict.dict") =>
            ReadAllTextFromFile(inFileName)
                .PipeTo(EncodeWithLzss)
                .PipeTo(EncodeWithHuffman)
                .FinalPipeTo(x => WriteToHuffmanEncodedFile(outFileName, dictFileName, x));

        public static void CreateDecodedPipeLine(string inFileName, string outFileName,
            string dictFileName = "HuffmanDict.dict") =>
            ReadAllBytesFromFile(inFileName)
                .PipeTo(x => DecodeWithHuffman(dictFileName, x))
                .PipeTo(DecodeWithLzss)
                .FinalPipeTo(x => WriteToAllTextToFile(outFileName, x));

        public static void CreateLzssOnlyEncodedPipeLine(string inFileName, string outFileName) =>
            ReadAllTextFromFile(inFileName)
                .PipeTo(EncodeWithLzss)
                .FinalPipeTo(x => WriteToAllTextToFile(outFileName, x));

        public static void CreateLzssOnlyDecodedPipeLine(string inFileName, string outFileName)
        {
            ReadAllTextFromFile(inFileName)
                .PipeTo(DecodeWithLzss)
                .FinalPipeTo(x => WriteToAllTextToFile(outFileName, x));
        }


        private static string EncodeWithLzss(string stringToEncode)
            => new LZSSEncoder().Encode(stringToEncode);

        private static string DecodeWithLzss(string stringToDecode)
            => new LZSSDecoder().Decode(stringToDecode);

        private static string ReadAllTextFromFile(string filePath)
        {
            return File.ReadAllText(filePath);
        }

        private static byte[] ReadAllBytesFromFile(string filePath)
        {
            return File.ReadAllBytes(filePath);
        }


        private static void WriteToHuffmanEncodedFile(string outFileName, string dictFileName,
            HuffmanEncodeReturn encoded)
        {
            File.WriteAllText(dictFileName,
                string.Join('操', encoded.HuffmanTree.Rates.Select(x => $"{x.Key}{x.Value}").ToArray()) + '\n');
            byte[] bytes = new byte[encoded.BitArray.Length / 8 + (encoded.BitArray.Length % 8 == 0 ? 0 : 1)];
            encoded.BitArray.CopyTo(bytes, 0);
            File.WriteAllBytes(outFileName, bytes);
        }

        private static void WriteToAllTextToFile(string outFileName, string encoded)
        {
            File.WriteAllText(outFileName, encoded);
        }

        private static HuffmanEncodeReturn EncodeWithHuffman(string stringToEncode)
        {
            var huffmanTree = new HuffmanEncoder();
            huffmanTree.Build(stringToEncode);
            return new HuffmanEncodeReturn {BitArray = huffmanTree.Encode(stringToEncode), HuffmanTree = huffmanTree};
        }

        private static string DecodeWithHuffman(string dictFilename, byte[] bytes)
        {
            var serializedDict = File.ReadAllText(dictFilename);
            var rates = serializedDict.Split('操').Select(x =>
            {
                var chr = x[0];
                var rate = int.Parse(x.Substring(1, x.Length - 1));
                return new {chr, rate};
            }).ToDictionary(x => x.chr, x => x.rate);
            var huffmanTree = new HuffmanDecoder();
            huffmanTree.BuildWithRates(rates);
            return huffmanTree.Decode(new BitArray(bytes));
        }
    }

    internal class HuffmanEncodeReturn
    {
        internal BitArray BitArray;
        internal BaseHuffmanTree HuffmanTree;
    }
}