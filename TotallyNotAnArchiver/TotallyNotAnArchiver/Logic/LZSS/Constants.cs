namespace TotallyNotAnArchiver.Logic.LZSS
{
    public static class Constants
    {
        public const int SearchBufferSize = 4096;

        public const int MatchThreshold = 2;

        public const int NonEncodedBufferSize = MatchThreshold + 16;

        public const int HashTableSize = 1024;

        public const int AlphabetSize = 255;
    }
}
