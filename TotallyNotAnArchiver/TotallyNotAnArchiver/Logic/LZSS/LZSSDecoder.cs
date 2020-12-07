using System.Text;

namespace TotallyNotAnArchiver.Logic.LZSS
{
    class LZSSDecoder : LZSSBase
    {
        public LZSSDecoder() : base()
        {

        }

        public string Decode(string stringToDecode)
        {
            var sb = new StringBuilder();

            int flag = 0;
            int flagCount = 7;
            int nextChar = 0;
            var encodedCode = new EncodedString();

            for (var i = 0;;)
            {
                flag >>= 1;
                flagCount++;

                int currentChar;
                if (flagCount == 8)
                {
                    if (i >= stringToDecode.Length)
                    {
                        break;
                    }

                    currentChar = stringToDecode[i];
                    i++;

                    flag = currentChar & Constants.AlphabetSize;
                    flagCount = 0;
                }
                // если флаг закодированных данных
                if (flag % 2 == 0)
                {
                    if (i >= stringToDecode.Length)
                    {
                        break;
                    }
                    // берем первые два квартета
                    encodedCode.Offset = stringToDecode[i];
                    i++;
                    
                    if (i >= stringToDecode.Length)
                    {
                        break;
                    }
                    // берем вторые два квартета
                    encodedCode.Length = stringToDecode[i];
                    i++;
                    //первые два квартета склеиваем с первым квартетом из второй пары - получаем смещение
                    encodedCode.Offset = (encodedCode.Offset << 4) + ((encodedCode.Length & 240) >> 4);
                    //длину совпадение получаем из 4ого квартета + то, что отрезали при кодировании, чтобы влезть в 15
                    encodedCode.Length = (encodedCode.Length & 15) + Constants.MatchThreshold + 1;
                    // ищем в скользящем окне соответствующее смещение и длину совпадения - выписываем
                    for (var j = 0; j < encodedCode.Length; j++)
                    {
                        currentChar = SearchBuffer[(encodedCode.Offset + j) % Constants.SearchBufferSize];
                        sb.Append((char)currentChar);
                        SearchBuffer[(nextChar + j) % Constants.SearchBufferSize] = (char)currentChar;
                    }

                    nextChar = (nextChar + encodedCode.Length) % Constants.SearchBufferSize;
                }
                else
                {
                    if (i >= stringToDecode.Length)
                    {
                        break;
                    }

                    currentChar = stringToDecode[i];
                    i++;

                    //просто выписываем незашифрованный символ
                    sb.Append((char)currentChar);
                    SearchBuffer[nextChar] = (char)currentChar;
                    nextChar = (nextChar + 1) % Constants.SearchBufferSize;
                }
            }

            return sb.ToString();
        }
    }
}
