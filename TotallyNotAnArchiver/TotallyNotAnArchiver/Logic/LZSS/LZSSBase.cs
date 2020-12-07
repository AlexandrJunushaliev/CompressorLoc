namespace TotallyNotAnArchiver.Logic.LZSS
{
    public class LZSSBase
    {
        protected static readonly char[] SearchBuffer = new char[Constants.SearchBufferSize]; // Буфер словаря
        protected static readonly int?[] HashTable = new int?[Constants.HashTableSize]; // Оптимизация на хешах
        protected static readonly int?[] Next = new int?[Constants.SearchBufferSize]; // Связный список
        protected static readonly char[] NonEncodedBuffer = new char[Constants.NonEncodedBufferSize]; // Незакодированное окно

        public LZSSBase()
        {
            InitializeData();
        }

        private static void InitializeData()
        {
            // инициализируем поисковый буфер
            for (int i = 0; i < Constants.SearchBufferSize; ++i)
            {
                SearchBuffer[i] = '\0';
                Next[i] = i + 1;
            }
            
            Next[Constants.SearchBufferSize - 1] = null;

            for (var i = 0; i < Constants.HashTableSize; i++)
            {
                HashTable[i] = null;
            }

            HashTable[GetHashKey(0, false)] = 0;
        }
        
        protected static int GetHashKey(int offset, bool isInLookahead)
        {
            var hashKey = 0;

            if (isInLookahead)
            {
                for (var i = 0; i < Constants.MatchThreshold + 1; i++)
                {
                    hashKey = (hashKey << 5) ^ NonEncodedBuffer[offset];
                    hashKey %= Constants.HashTableSize;
                    offset = (offset + 1) % Constants.NonEncodedBufferSize;
                }
            }
            else
            {
                for (var i = 0; i < Constants.MatchThreshold + 1; i++)
                {
                    hashKey = (hashKey << 5) ^ SearchBuffer[offset];
                    hashKey %= Constants.HashTableSize;
                    offset = (offset + 1) % Constants.SearchBufferSize;
                }
            }

            return hashKey;
        }
    }
}
