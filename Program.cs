// See https://aka.ms/new-console-template for more information
using System;
using System.Diagnostics;
using System.Threading.Tasks;

namespace ReadStreamSpeedTests
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            string filePath = @"C:\Users\BuPh\Desktop\GIMTransfer\CAD Files\some cityjsons\09dz1_04.json";
            using (FileStream fileStream = new(filePath, FileMode.Open, FileAccess.Read))
            {


                int positionFirst = 0;
                for (int i = 0; i < 5; i++)
                {
                    Stopwatch bufferedReadWatch = Stopwatch.StartNew();
                    positionFirst = FileStreamTests.FindAttributeStartBuffered(fileStream, "CityObjects");
                    bufferedReadWatch.Stop();
                    long bufferedTime = bufferedReadWatch.ElapsedMilliseconds;
                    Console.WriteLine((i + 1) + "-Finding Attribute buffered took " + bufferedTime + "ms");
                }

                int positionSecond = 0;
                for (int i = 0; i < 5; i++)
                {
                    Stopwatch bufferedReadWatch = Stopwatch.StartNew();
                    positionSecond = FileStreamTests.FindAttributeStartPureStream(fileStream, "CityObjects");
                    bufferedReadWatch.Stop();
                    long bufferedTime = bufferedReadWatch.ElapsedMilliseconds;
                    Console.WriteLine((i + 1) + "-Finding Attribute buffered took " + bufferedTime + "ms");
                }

                for(int count = 0; count < 5; count++)
                {
                    Stopwatch allJsonObjectsBuffered = Stopwatch.StartNew();
                    {
                        int objectPosition = positionFirst;
                        bool objectNotEmpty = true;
                        bool moreObjects = true;
                        List<Task> tasks = new List<Task>();

                        do
                        {
                            string objectJsonText = FileStreamTests.ReadSingleJsonAttributeBuffered(fileStream, ref objectPosition);
                            if (objectJsonText != "")
                            {
                                objectNotEmpty = true;
                                // read async here
                                if (tasks.Count > 31)
                                {
                                    int taskFinishedIndex = Task.WaitAny(tasks.ToArray());


                                    Task task = Task.Run(() =>
                                    {
                                        Dictionary<char, int> charCount = new();
                                        for (int i = 0; i < objectJsonText.Length; i++)
                                        {
                                            char c = objectJsonText[i];
                                            if (!charCount.ContainsKey(c))
                                            {
                                                charCount[c] = 0;
                                            }

                                            charCount[c]++;
                                        }
                                    });
                                    tasks[taskFinishedIndex] = task;
                                }

                                else
                                {

                                    Task task = Task.Run(() =>
                                    {
                                        Dictionary<char, int> charCount = new();
                                        for (int i = 0; i < objectJsonText.Length; i++)
                                        {
                                            char c = objectJsonText[i];
                                            if (!charCount.ContainsKey(c))
                                            {
                                                charCount[c] = 0;
                                            }

                                            charCount[c]++;
                                        }
                                    });
                                    tasks.Add(task);
                                }

                                byte[] buffer = new byte[1];

                                objectPosition++;
                                fileStream.Seek(objectPosition, SeekOrigin.Begin);
                                fileStream.Read(buffer, 0, 1);

                                if ((char)buffer[0] == ',')
                                {
                                    moreObjects = true;
                                    objectPosition++;
                                }
                                else
                                {
                                    moreObjects = false;
                                }
                            }
                            else
                            {
                                objectNotEmpty = false;
                            }
                        } while (objectNotEmpty && moreObjects);

                        Task.WaitAll(tasks.ToArray());
                    }

                    allJsonObjectsBuffered.Stop();

                    Console.WriteLine("Reading all json objects buffered took " + allJsonObjectsBuffered.ElapsedMilliseconds + "ms");


                }

                for(int count = 0; count < 5; count++)
                {
                    Stopwatch allJsonObjectsPureStream = Stopwatch.StartNew();
                    {
                        int objectPosition = positionSecond;
                        bool objectNotEmpty = true;
                        bool moreObjects = true;
                        List<Task> tasks = new List<Task>();

                        do
                        {
                            string objectJsonText = FileStreamTests.ReadSingleJsonAttributePureStream(fileStream, ref objectPosition);
                            if (objectJsonText != "")
                            {
                                objectNotEmpty = true;
                                // read async here
                                if (tasks.Count > 15)
                                {
                                    Task.WaitAll(tasks.ToArray());
                                    tasks = new();
                                }
                                Task task = Task.Run(() =>
                                {
                                    Dictionary<char, int> charCount = new();
                                    for (int i = 0; i < objectJsonText.Length; i++)
                                    {
                                        char c = objectJsonText[i];
                                        if (!charCount.ContainsKey(c))
                                        {
                                            charCount[c] = 0;
                                        }

                                        charCount[c]++;
                                    }
                                });
                                tasks.Add(task);

                                byte[] buffer = new byte[1];

                                objectPosition++;
                                fileStream.Seek(objectPosition, SeekOrigin.Begin);
                                fileStream.Read(buffer, 0, 1);

                                if ((char)buffer[0] == ',')
                                {
                                    moreObjects = true;
                                    objectPosition++;
                                }
                                else
                                {
                                    moreObjects = false;
                                }
                            }
                            else
                            {
                                objectNotEmpty = false;
                            }
                        } while (objectNotEmpty && moreObjects);

                        Task.WaitAll(tasks.ToArray());
                    }

                    allJsonObjectsPureStream.Stop();


                    Console.WriteLine("Reading all json objects pure stream took " + allJsonObjectsPureStream.ElapsedMilliseconds + "ms");

                }

                for (int i = 0; i < 5; i++)
                {
                    Stopwatch bufferedReadWatch = Stopwatch.StartNew();
                    string first = FileStreamTests.ReadJsonAttributeFromEndOfFileBuffered(fileStream, "vertices");
                    bufferedReadWatch.Stop();
                    long bufferedTime = bufferedReadWatch.ElapsedMilliseconds;
                    Console.WriteLine((i+1) + "-Reading buffered took " + bufferedTime + "ms");
                }
             
                
                for (int i = 0; i < 5; i++)
                {
                    Stopwatch pureStreamReadWatch = Stopwatch.StartNew();
                    string second = FileStreamTests.ReadJsonAttributeFromEndOfFilePureStream(fileStream, "vertices");
                    pureStreamReadWatch.Stop();
                    long bufferedTime = pureStreamReadWatch.ElapsedMilliseconds;
                    Console.WriteLine((i + 1) + "-Reading purely from stream took " + bufferedTime + "ms");
                }
            }
        }
    }
}