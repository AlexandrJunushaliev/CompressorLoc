using System.Text;

namespace TotallyNotAnArchiver.Logic.LZSS
{
    public class LZSSEncoder : LZSSBase
    {
        public LZSSEncoder() : base()
        {

        }
        public string Encode(string stringToEncode)
        {
            var sb = new StringBuilder();
            var i = 0;
            
            int length;
            // заполняем буфер незакодированных символов
            int readChar;
            for (length = 0; length < Constants.NonEncodedBufferSize; length++)
            {
                if (i >= stringToEncode.Length)
                    break;
                readChar = stringToEncode[i];
                i++;

                NonEncodedBuffer[length] = (char)readChar;
            }

            if (length == 0)
                return sb.ToString(); // если файл был пустым

            int flags = 0;
            var flagPosition = 1;
            var encodedData = new char[16];
            var nextEncoded = 0;

            var searchHead = 0; 
            var nonEncoded = 0; 

            int j;

            var matchData = FindMatch(nonEncoded);
            
            while (length > 0)
            {
                // Конец строчки в конце матча будет мусорная дата - обрезаем
                if (matchData.Length > length)
                {
                    matchData.Length = length;
                }

                // Если не прошли минимальную границу (2 символа, так как длина и смещение займут 2 симовла)
                if (matchData.Length <= Constants.MatchThreshold)
                {
                    matchData.Length = 1; // устанавливаем длину совпадения на 1 - потом окно сдинется на 1 символ
                    flags += flagPosition; //помечаем флагом неужатых данных
                    encodedData[nextEncoded] = NonEncodedBuffer[nonEncoded]; // записываем символ
                    nextEncoded++;
                }
                else 
                {
                    // кодируем смещение и его длину, при этом используется знание, что смещение не может быть больше
                    // чем 4095 (3 квартета), а длина совпадения больше 15 (1 квартет) (что достигается засчет вычитания) 
                    // благодаря чему мы можем поделить квартеты поравну (2 и 2), закодировав смещение и длину в два символа
                    encodedData[nextEncoded] = (char)((matchData.Offset & (Constants.SearchBufferSize - 1)) >> 4);
                    nextEncoded++;

                    encodedData[nextEncoded] = (char)(((matchData.Offset & 15) << 4) +
                                                      (matchData.Length - (Constants.MatchThreshold + 1)));
                    nextEncoded++;
                }
                
                if (flagPosition == 128)
                {
                    sb.Append((char) flags);
                    for (j = 0; j < nextEncoded; j++)
                    {
                        sb.Append(encodedData[j]); // добавляем содержимое
                    }
                    
                    flags = 0;
                    flagPosition = 1;
                    nextEncoded = 0;
                }
                else
                {
                    flagPosition <<= 1;
                }

                // Двигаем окно
                j = 0;
                while (j < matchData.Length)
                {
                    if (i >= stringToEncode.Length)
                        break;

                    readChar = stringToEncode[i];
                    i++;

                    // перемещаем окно на 1
                    ReplaceChar(searchHead, NonEncodedBuffer[nonEncoded]);
                    // добавляем в незакодированный буфер новые данные
                    NonEncodedBuffer[nonEncoded] = (char)readChar;
                    searchHead = (searchHead + 1) % Constants.SearchBufferSize;
                    nonEncoded = (nonEncoded + 1) % Constants.NonEncodedBufferSize;
                    j++;
                }

                // Если данные закончились, добрасываем конец
                while (j < matchData.Length)
                {
                    ReplaceChar(searchHead, NonEncodedBuffer[nonEncoded]);

                    searchHead = (searchHead + 1) % Constants.SearchBufferSize;
                    nonEncoded = (nonEncoded + 1) % Constants.NonEncodedBufferSize;
                    length--;
                    j++;
                }
                
                matchData = FindMatch(nonEncoded);
            }

            // Выписываем остатки кодированных данных
            if (nextEncoded == 0) return sb.ToString();
            sb.Append((char)flags);
            for (i = 0; i < nextEncoded; i++)
            {
                sb.Append(encodedData[i]);
            }
            return sb.ToString();
        }
        
        private static EncodedString FindMatch(int nonEncodedHead)
        {
            var matchData = new EncodedString();

            var i = HashTable[GetHashKey(nonEncodedHead, true)];
            var j = 0;
            // если в хэш таблице есть совпадение
            while (i.HasValue)
            {
                // если оно совпадает с первым символом буфера
                if (SearchBuffer[i.Value] == NonEncodedBuffer[nonEncodedHead])
                {
                    j = 1;

                    while (SearchBuffer[(i.Value + j) % Constants.SearchBufferSize] ==
                        NonEncodedBuffer[(nonEncodedHead + j) % Constants.NonEncodedBufferSize])
                    {
                        if (j >= Constants.NonEncodedBufferSize)
                        {
                            break;
                        }

                        j++;
                    }

                    if (j > matchData.Length)
                    {
                        matchData.Length = j;
                        matchData.Offset = i.Value;
                    }
                }

                if (j >= Constants.NonEncodedBufferSize)
                {
                    matchData.Length = Constants.NonEncodedBufferSize;
                    break;
                }

                i = Next[i.Value];
            }

            return matchData;
        }
        
        private static void AddString(int charIndex)
        {
            // Вставляем в конец списка - у него не будет следующего
            Next[charIndex] = null;

            var hashKey = GetHashKey(charIndex, false);

            // Если не представлен совпадение с него не представлено в хеш таблице
            if (HashTable[hashKey] == null)
            {
                HashTable[hashKey] = charIndex;
                return;
            }
            // находим текущий конец
            var i = HashTable[hashKey];
            while (Next[i.Value].HasValue)
            {
                i = Next[i.Value];
            }

            // перебасываем ссылку
            Next[i.Value] = charIndex;
        }
        
        private static void RemoveString(int charIndex)
        {
            var nextIndex = Next[charIndex];
            Next[charIndex] = null;

            var hashKey = GetHashKey(charIndex, false);

            if (HashTable[hashKey] == charIndex)
            {
                HashTable[hashKey] = nextIndex;
                return;
            }

            // Находим, кто указывает на удаляемый и перебрасываем ссылки
            var i = HashTable[hashKey];
            while (Next[i.Value].Value != charIndex)
            {
                i = Next[i.Value].Value;
            }

            Next[i.Value] = nextIndex;
        }
        private static void ReplaceChar(int charIndex, char newChar)
        {
            //с таким смещением хранятся записи в хештаблице
            var firstIndex = charIndex - Constants.MatchThreshold - 1;
            if (firstIndex < 0)
            {
                firstIndex += Constants.SearchBufferSize;
            }

            // Удаляем все вхождения в хештаблице, которые могут содержать данный символ
            for (var i = 0; i < Constants.MatchThreshold + 1; i++)
            {
                RemoveString((firstIndex + i) % Constants.SearchBufferSize);
            }

            SearchBuffer[charIndex] = newChar;

            for (var i = 0; i < Constants.MatchThreshold + 1; i++)
            {
                AddString((firstIndex + i) % Constants.SearchBufferSize);
            }
        }
    }
}
