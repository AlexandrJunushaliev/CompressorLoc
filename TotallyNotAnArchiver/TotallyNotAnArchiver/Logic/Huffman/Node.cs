using System.Collections.Generic;

namespace TotallyNotAnArchiver.Logic.Huffman
{
    public class Node
    {
        public char Character { get; set; }
        public int Rate { get; set; }
        public Node Right { get; set; }
        public Node Left { get; set; }

        public List<bool> Traverse(char character, List<bool> data)
        {
            //обход во все стороны дерева по потомкам с целью найти путь до листа с символом, который ищем
            if (Right == null && Left == null)
            {
                return character.Equals(Character) ? data : null;
            }

            List<bool> left = null;

            if (Left != null)
            {
                List<bool> leftPath = new List<bool>();
                leftPath.AddRange(data);
                leftPath.Add(false);

                left = Left.Traverse(character, leftPath);
            }

            if (Right == null) return left;
            var rightPath = new List<bool>();
            rightPath.AddRange(data);
            rightPath.Add(true);
            var right = Right.Traverse(character, rightPath);

            return left ?? right;
        }
    }

}