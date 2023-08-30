// See https://aka.ms/new-console-template for more information
using System.Diagnostics;

namespace ReadStreamSpeedTests
{
    public static class Program
    {
        public static void Main(string[] args)
        {
            string filePath = @"C:\Users\BuPh\Desktop\GIMTransfer\CAD Files\some cityjsons\09dz1_01.json";
            using (FileStream fileStream = new(filePath, FileMode.Open, FileAccess.Read))
            {
                for (int i = 0; i < 5; i++)
                {
                    Stopwatch bufferedReadWatch = Stopwatch.StartNew();
                    string first = FileStreamTests.ReadJsonAttributeFromEndOfFileBuffered(fileStream, "vertices");
                    bufferedReadWatch.Stop();
                    long bufferedTime = bufferedReadWatch.ElapsedMilliseconds;
                    Console.WriteLine((i+1)+"-Reading buffered took " + bufferedTime + "ms");
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