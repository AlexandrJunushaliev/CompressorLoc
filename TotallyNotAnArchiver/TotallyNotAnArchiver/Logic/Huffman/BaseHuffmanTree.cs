using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace TotallyNotAnArchiver.Logic.Huffman
{
    class BaseHuffmanTree
    {
        protected List<Node> _nodes = new List<Node>();
        protected Node _root { get; set; }
        public Dictionary<char, int> Rates = new Dictionary<char, int>();

        protected void AfterBuildPrep()
        {
            foreach (var (chr, rate) in Rates)
            {
                _nodes.Add(new Node() { Character = chr, Rate = rate });
            }
            //построение дерева ровно как по алгоритму, только основываясь на частотах, а не на вероятностях
            while (_nodes.Count > 1)
            {
                var orderedNodes = _nodes.OrderBy(node => node.Rate).ToList();

                if (orderedNodes.Count >= 2)
                {
                    var taken = orderedNodes.Take(2).ToList();

                    Node parent = new Node()
                    {
                        Character = '\0',
                        Rate = taken[0].Rate + taken[1].Rate,
                        Left = taken[0],
                        Right = taken[1]
                    };

                    _nodes.Remove(taken[0]);
                    _nodes.Remove(taken[1]);
                    _nodes.Add(parent);
                }

                _root = _nodes.FirstOrDefault();
            }
        }
    }
}
