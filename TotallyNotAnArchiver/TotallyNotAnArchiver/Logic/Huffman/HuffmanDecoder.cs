using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace TotallyNotAnArchiver.Logic.Huffman
{
    class HuffmanDecoder : BaseHuffmanTree
    {
        public void BuildWithRates(Dictionary<char, int> rates)
        {
            Rates = rates;

            
            AfterBuildPrep();
        }

        public string Decode(BitArray directions)
        {
            Node currentNode = _root;
            var output = new StringBuilder("");
            var count = 0;
            foreach (bool right in directions)
            {
                if (right)
                {
                    if (currentNode.Right != null)
                    {
                        currentNode = currentNode.Right;
                    }
                }
                else
                {
                    if (currentNode.Left != null)
                    {
                        currentNode = currentNode.Left;
                    }
                }

                //является листом
                if (currentNode.Left == null && currentNode.Right == null)
                {
                    output.Append(currentNode.Character);
                    currentNode = _root;
                }

                count++;
                if (count % 100000 == 0)
                    Console.WriteLine($"{count} of {directions.Length}: {count * 100 / directions.Length}%");
            }

            return output.ToString();
        }
    }
}
