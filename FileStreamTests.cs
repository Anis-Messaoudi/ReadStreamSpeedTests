using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReadStreamSpeedTests
{
    public static class FileStreamTests
    {
        public static string ReadJsonAttributeFromEndOfFilePureStream(FileStream fs, string attributeName, bool fixUpJson = true)
        {
            StringBuilder finalString = new();
            if (fixUpJson) finalString = new("{\"");
            string keyword = attributeName;
            bool foundKeyword = false;
            char finalChar = keyword[^1];
            // Loop 1: Find the keyword "Transform" from the end of the file
            long position = fs.Length - 1;
            byte[] buffer = new byte[1];
            while (position >= 0)
            {
                fs.Seek(position, SeekOrigin.Begin);
                fs.Read(buffer, 0, 1);

                char currentChar = (char)buffer[0];

                if (currentChar == finalChar)
                {
                    string candidateKeyword = ReadPreviousChars(fs, position, keyword.Length - 1) + currentChar;
                    if (candidateKeyword == keyword)
                    {
                        foundKeyword = true;
                        position++;
                        break;
                    }
                }

                position--;
            }

            // Loop 2: Extract information following the keyword until closing bracket
            if (foundKeyword)
            {
                int openBrackets = 0; // Start with 0 as the initial open bracket is not part of the count
                int closeBrackets = 0;

                bool firstOpenBracketFound = false; // To skip the first open bracket after "Transform"

                finalString.Append(keyword);

                while (position < fs.Length && (openBrackets > closeBrackets || !firstOpenBracketFound))
                {
                    fs.Seek(position, SeekOrigin.Begin);
                    fs.Read(buffer, 0, 1);
                    char currentChar = (char)buffer[0];

                    if (currentChar == '{')
                    {
                        openBrackets++;
                        firstOpenBracketFound = true;
                    }
                    else if (currentChar == '}')
                        closeBrackets++;

                    if (openBrackets > closeBrackets || !firstOpenBracketFound)
                        finalString.Append(currentChar);

                    position++;
                }
            }


            if (fixUpJson) finalString.Append("}}");
            return finalString.ToString();
        }

        public static string ReadJsonAttributeFromEndOfFileBuffered(FileStream fs, string attributeName)
        {
            StringBuilder finalString = new StringBuilder("{\"");
            string keyword = attributeName;
            bool foundKeyword = false;
            char finalChar = keyword[^1];
            char firstChar = keyword[0];
            char secondChar = keyword[1];
            // Loop 1: Find the keyword "Transform" from the end of the file
            int bufferSize = 1000;
            long position = Math.Max(0, fs.Length - bufferSize);

            int keywordPosition = -1;

            List<string> stringBuffer = new();
            string previousString = "";

            while (position >= 0)
            {
                fs.Seek(position, SeekOrigin.Begin);

                int charactersToRead = (int)Math.Min(bufferSize, fs.Length - position);

                byte[] buffer = new byte[charactersToRead];
                fs.Read(buffer, 0, charactersToRead);

                string bufferedString = Encoding.UTF8.GetString(buffer);
                stringBuffer.Add(bufferedString);

                string totalString = bufferedString + previousString;

                for(int i=0; i < bufferedString.Length - 1; i++)
                {
                    char currentChar = totalString[i];
                    char secondCurrentChar = totalString[i + 1];
                    if(currentChar == firstChar && secondCurrentChar == secondChar) {
                        string candidateKeyword = totalString[i..(i + keyword.Length)];
                        if(candidateKeyword == keyword)
                        {
                            foundKeyword = true;
                            keywordPosition = i;
                            break;
                        }
                    }
                }

                if (foundKeyword)
                {
                    break;
                }

                previousString = bufferedString;

                //char currentChar = (char)buffer[0];

                //if (currentChar == finalChar)
                //{
                //    string candidateKeyword = ReadPreviousChars(fs, position, keyword.Length - 1) + currentChar;
                //    if (candidateKeyword == keyword)
                //    {
                //        foundKeyword = true;
                //        position++;
                //        break;
                //    }
                //}

                position -= bufferSize;
            }

            if(foundKeyword)
            {
                StringBuilder stringBuilder= new StringBuilder();
                for (int i = stringBuffer.Count - 1; i >= 0; i--)
                {
                    stringBuilder.Append(stringBuffer[i]);
                }

                string builtString = stringBuilder.ToString();

                int openBrackets = 0; // Start with 0 as the initial open bracket is not part of the count
                int closeBrackets = 0;

                bool firstOpenBracketFound = false; // To skip the first open bracket after "Transform"

                int startingPosition = keywordPosition + keyword.Length;
                int finalPosition = startingPosition;
                for (int i = startingPosition; i < builtString.Length; i++)
                {
                    char currentChar = builtString[i];

                    if (currentChar == '{')
                    {
                        openBrackets++;
                        firstOpenBracketFound = true;
                    }
                    else if (currentChar == '}')
                        closeBrackets++;

                    if (openBrackets <= closeBrackets && firstOpenBracketFound)
                    {
                        finalPosition = i + 1;
                        break;
                    }
                }


                finalString.Append(builtString[keywordPosition..finalPosition]); 
                finalString.Append("}");
            }

            // Loop 2: Extract information following the keyword until closing bracket

            return finalString.ToString();
        }


        public static int FindAttributeStartPureStream(FileStream fileStream, string attributeName)
        {
            int position = -1;
            string keyword = attributeName;
            fileStream.Seek(0, SeekOrigin.Begin);
            StreamReader reader = new StreamReader(fileStream, Encoding.UTF8);

            char[] bufferword = new char[keyword.Length];
            int charactersRead = 0;

            bool keywordFound = false;
            while (!reader.EndOfStream)
            {
                char currentCharacter = (char)reader.Read();
                if (charactersRead < keyword.Length)
                {
                    bufferword[charactersRead] = currentCharacter;
                    charactersRead++;
                }
                else
                {
                    for (int i = 0; i < bufferword.Length - 1; i++)
                    {
                        bufferword[i] = bufferword[i + 1];
                    }
                    bufferword[^1] = currentCharacter;
                    charactersRead++;
                }

                if (bufferword[0] == keyword[0] && charactersRead > keyword.Length) // potential candidate
                {
                    string candidateWord = new(bufferword);
                    if (candidateWord == keyword)
                    {
                        keywordFound = true;
                        position = charactersRead + 3;
                        break;
                    }
                }
            }

            if (keywordFound) return position;

            return -1;
        }

        public static string ReadSingleJsonAttributePureStream(FileStream fileStream, ref int startPosition, bool fixUpJson = true)
        {
            StringBuilder finalString = new();
            if (fixUpJson) finalString = new("{\"");

            fileStream.Seek(startPosition, SeekOrigin.Begin);
            byte[] buffer = new byte[1];
            fileStream.Read(buffer, 0, 1);
            char character = (char)buffer[0];

            if (character == '}')
            {
                return "";
            }

            while (character != '"')
            {
                startPosition++;
                fileStream.Seek(startPosition, SeekOrigin.Begin);
                fileStream.Read(buffer, 0, 1);
                character = (char)buffer[0];
            }

            StreamReader reader = new StreamReader(fileStream);

            bool openingBracketEncountered = false;
            int openBracketCount = 0;
            int closedBracketCount = 0;
            while (!reader.EndOfStream)
            {
                character = (char)reader.Read();
                finalString.Append(character);

                if (character == '{')
                {
                    openingBracketEncountered = true;
                    openBracketCount++;
                }
                else if (character == '}')
                {
                    closedBracketCount++;
                }

                startPosition++;

                if (openingBracketEncountered && (closedBracketCount >= openBracketCount))
                {
                    break;
                }

            }

            if (fixUpJson) finalString.Append("}");

            return finalString.ToString();
        }


        public static int FindAttributeStartBuffered(FileStream fileStream, string attributeName)
        {
            int position = 0;
            int keywordPosition = 0;
            string keyword = attributeName;
            
            char firstChar = keyword[0];
            char secondChar = keyword[1];

            int bufferSize = 1000;

            bool keywordFound = false;
            string previousString = "";
            while(position < fileStream.Length)
            {
                int charactersToRead = (int)Math.Min(bufferSize, fileStream.Length - position);
                byte[] buffer = new byte[charactersToRead];
                fileStream.Seek(position, SeekOrigin.Begin);
                fileStream.Read(buffer, 0, charactersToRead);

                string bufferedString = Encoding.UTF8.GetString(buffer);
                string totalString = previousString + bufferedString;

                int startIndex = previousString.Length - keyword.Length;
                if(startIndex < 0) { startIndex = 0; }

                for(int i =startIndex; i < totalString.Length - keyword.Length; i++)
                {
                    char currentChar = totalString[i];
                    char currentSecondChar = totalString[i + 1];

                    if(currentChar == firstChar && currentSecondChar == secondChar)
                    {
                        string candidateKeyWord = totalString[i..(i + keyword.Length)];
                        if(candidateKeyWord == keyword)
                        {
                            keywordPosition = position + i - previousString.Length + keyword.Length + 3;
                            keywordFound = true;
                            break;
                        }
                    }
                }

                if (keywordFound) break;


                position += bufferSize;
            }


            if (keywordFound) return keywordPosition;

            return -1;
        }

        public static string ReadSingleJsonAttributeBuffered(FileStream fileStream, ref int startPosition, bool fixUpJson = true)
        {
            StringBuilder finalString = new("{\"");

            fileStream.Seek(startPosition, SeekOrigin.Begin);
            byte[] singleCharBuffer = new byte[1];
            fileStream.Read(singleCharBuffer, 0, 1);
            char character = (char)singleCharBuffer[0];

            if (character == '}')
            {
                return "";
            }

            while (character != '"')
            {
                startPosition++;
                fileStream.Seek(startPosition, SeekOrigin.Begin);
                fileStream.Read(singleCharBuffer, 0, 1);
                character = (char)singleCharBuffer[0];
            }

            bool openingBracketEncountered = false;
            int openBracketCount = 0;
            int closedBracketCount = 0;

            int position = startPosition;
            int bufferSize = 1000;

            while(position < fileStream.Length)
            {
                int charactersToRead = (int)Math.Min(bufferSize, fileStream.Length - position);
                byte[] buffer = new byte[charactersToRead];
                fileStream.Read(buffer, 0, charactersToRead);
                string bufferedString = UTF8Encoding.UTF8.GetString(buffer);

                bool breakLoop = false;
                int breakPosition = 0;
                for(int i = 0; i < bufferedString.Length; i++)
                {
                    character = bufferedString[i];

                    if (character == '{')
                    {
                        openingBracketEncountered = true;
                        openBracketCount++;
                    }
                    else if (character == '}')
                    {
                        closedBracketCount++;
                    }

                    if (openingBracketEncountered && (closedBracketCount >= openBracketCount))
                    {
                        breakLoop = true;
                        breakPosition = i + 1;
                        break;
                    }
                }

                if(breakLoop)
                {
                    finalString.Append(bufferedString[0..breakPosition]);
                    startPosition += breakPosition;
                    break;
                }
                else
                {
                    finalString.Append(bufferedString);
                    startPosition += bufferedString.Length;
                }

                position += bufferSize;
            }

            if (fixUpJson) finalString.Append("}");

            return finalString.ToString();
        }


        private static string ReadPreviousChars(FileStream fs, long position, int count)
        {
            byte[] buffer = new byte[count];
            fs.Seek(position - count, SeekOrigin.Begin);
            fs.Read(buffer, 0, count);
            return Encoding.UTF8.GetString(buffer);
        }
    }
}
