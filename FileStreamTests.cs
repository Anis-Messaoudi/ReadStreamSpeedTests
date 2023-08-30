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

                int finalPosition = keywordPosition;
                for (int i = keywordPosition; i < builtString.Length; i++)
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

        private static string ReadPreviousChars(FileStream fs, long position, int count)
        {
            byte[] buffer = new byte[count];
            fs.Seek(position - count, SeekOrigin.Begin);
            fs.Read(buffer, 0, count);
            return Encoding.UTF8.GetString(buffer);
        }
    }
}
