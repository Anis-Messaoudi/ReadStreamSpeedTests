using Newtonsoft.Json.Linq;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace ReadStreamSpeedTests
{
    public class GIMCityJsonParser
    {
        public JsonParserBase Parse(string json)
        {
            int position = 0;
            return ParseRecursive(json, ref position, null);
        }

        public JsonParserBase ParseRecursive(string json, ref int position, JsonParserBase parent)
        {
            JsonParserBase property = null;

            SkipWhiteSpaces(json, ref position);

            if (json[position] == '"')
            {
                property = new JsonParserString(ReadString(json, ref position));
                SkipWhiteSpaces(json, ref position);
                return property;
            }
            if (json[position] == '{') 
            {
                position++;

                SkipWhiteSpaces(json, ref position);

                if (json[position] == '}')
                {
                    return new JsonParserObject();
                }

                JsonParserObject currentObj = new();

                bool moreProperties = false;
                do
                {
                    if (json[position] == '"')
                    {
                        string propertyName = ReadString(json, ref position);
                        position++; //skipping the ':' character
                        currentObj[propertyName] = ParseRecursive(json, ref position, currentObj);
                        SkipWhiteSpaces(json, ref position);
                        if (json[position] == ',') moreProperties = true;
                        if (json[position] == '}') moreProperties = false;
                        position++;
                    }
                    else
                    {
                        moreProperties = false;
                    }
                } while (moreProperties);
                SkipWhiteSpaces(json, ref position);
                return currentObj;
            }

            if (json[position] == '[')
            {
                position++;

                SkipWhiteSpaces(json, ref position);

                if (json[position] == ']')
                {
                    return new JsonParserArray();
                }

                JsonParserArray currentArray = new();

                bool moreValues = false;
                
                do
                {
                    var value = ParseRecursive(json, ref position, currentArray);
                    currentArray.Add(value);
                    SkipWhiteSpaces(json, ref position);
                    if (json[position] == ',') moreValues = true;
                    if (json[position] == ']') moreValues = false;
                    position++;
                } while(moreValues);

                SkipWhiteSpaces(json, ref position);
                return currentArray;
            }

            if (char.IsDigit(json[position]))
            {
                string charString = "";
                while (char.IsDigit(json[position]) || json[position] == '.') 
                { 
                    charString += json[position];
                    position++;
                }

                return new JsonParserNumber(charString);
            }

            if (json[position] == 't' || json[position] == 'f')
            {

                if (json[position..(position + 4)] == "true")
                {
                    position += 4;
                    SkipWhiteSpaces(json, ref position);
                    return new JsonParserBool(true);
                }

                if (json[position..(position+5)] == "false") {
                    position += 5;
                    SkipWhiteSpaces(json, ref position);
                    return new JsonParserBool(false);
                }
            }

            if (json[position] == 'n')
            {
                if (json[position..(position+4)] == "null")
                {
                    position += 4;
                    return new JsonParserNull();
                }
            }

            return new JsonParserNull();
        }

        private void SkipWhiteSpaces(string json, ref int position)
        {
            if (position >= json.Length) return;
            while (char.IsWhiteSpace(json[position]))
            {
                position++;
            }
        }

        public string ReadString(string json, ref int position)
        {
            position++;
            int stringBegin = position;
            int stringEnd = stringBegin;

            bool ignoreNextChar = false;
            bool closingQuoteFound = false;

            StringBuilder propertyString = new();
            while (position < json.Length)
            {

                if (json[position] == '\\') // we're ignoring the function of the next character
                {
                    propertyString.Append(json[position]);
                    position++;
                }

                else if (json[position] == '"')
                {
                    closingQuoteFound = true;
                    break;
                }

                propertyString.Append(json[position]);
                position++;
            }
            position++;
            return propertyString.ToString();
        }
    }

    public enum JsonParserType
    {
        Object, Array, String, Number, Bool, Null
    }

    public abstract class JsonParserBase
    {
        public abstract JsonParserType Type { get; }

        public abstract JsonParserBase this[object indexer]
        {
            get;
            set;
        }

    }

    public class JsonParserObject: JsonParserBase, IEnumerator<KeyValuePair<string, JsonParserBase>>
    {
        public override JsonParserType Type { get => JsonParserType.Object; }
        Dictionary<string, JsonParserBase> values = new();

        public IEnumerator<KeyValuePair<string, JsonParserBase>> GetEnumerator()
        {
            return values.GetEnumerator();
        }

        public void Reset()
        {
            GetEnumerator().Reset();
        }

        public bool MoveNext()
        {
            return GetEnumerator().MoveNext();
        }

        public void Dispose()
        {
            GetEnumerator().Dispose();
        }


        public KeyValuePair<string, JsonParserBase> Current
        {
            get => GetEnumerator().Current;
        }

        object IEnumerator.Current
        {
            get
            {
                return Current;
            }
        }



        public override JsonParserBase this[object indexer]
        {
            get
            {
                if (indexer is string)
                {
                    string key = (string)indexer;
                    return values[key];
                }
                else
                {
                    return values[indexer.ToString()];
                }
            }
            set
            {
                if (indexer is string)
                {
                    string key = (string)indexer;
                    values[key] = value;
                }
                else
                {
                    values[indexer.ToString()] = value;
                }
            }
        }
    }

    public class JsonParserArray: JsonParserBase, IEnumerable<JsonParserBase>
    {
        public override JsonParserType Type { get => JsonParserType.Array; }
        List<JsonParserBase> values = new List<JsonParserBase>();

        public IEnumerator<JsonParserBase> GetEnumerator()
        {
            return values.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }


        public override JsonParserBase this[object indexer] 
        {
            get
            {
                if (indexer is int)
                {
                    int key = (int)indexer;
                    return values[key];
                }
                else
                {
                    return values[(int)indexer];
                }
            }
            set
            {
                if (indexer is int)
                {
                    int key = (int)indexer;
                    values[key] = value;
                }
                else
                {
                    values[(int)indexer] = value;
                }
            }
        }

        public int Length
        {
            get
            {
                return values.Count;
            }
        }

        public void Add(JsonParserBase obj)
        {
            values.Add(obj);
        }

    }

    public class JsonParserString: JsonParserBase
    {
        public string Value;

        public override JsonParserType Type { get => JsonParserType.String; }

        public JsonParserString(string value)
        {
            Value = value;
        }

        public override JsonParserBase this[object indexer] {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException(); 
        }

        public int Length
        {
            get
            {
                return Value.Length;
            }
        }

    }

    public class JsonParserNumber : JsonParserBase
    {
        public double Value;
        public override JsonParserType Type { get => JsonParserType.Number; }

        public JsonParserNumber(double value)
        {
            Value = value;
        }
        
        public JsonParserNumber(string value)
        {
            Value = double.Parse(value);
        }

        public override JsonParserBase this[object indexer]
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

        public int ToInt()
        {
            return (int)Value;
        }

        public double ToDouble()
        {
            return Value;
        }

    }

    public class JsonParserBool : JsonParserBase
    {
        public bool Value;

        public override JsonParserType Type { get => JsonParserType.Bool; }
        public JsonParserBool(bool value)
        {
            Value = value;
        }

        public JsonParserBool(string value)
        {
            Value = bool.Parse(value);
        }

        public override JsonParserBase this[object indexer]
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

    }

    public class JsonParserNull : JsonParserBase
    {
        public override JsonParserType Type { get => JsonParserType.Null; }
        
        public override JsonParserBase this[object indexer]
        {
            get => throw new NotImplementedException();
            set => throw new NotImplementedException();
        }

    }


}
