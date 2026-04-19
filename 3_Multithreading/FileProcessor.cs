using System.Diagnostics;
using System.Text.Json;

namespace _3_Multithreading
{
    public class FileProcessor
    {
        private readonly object _lockObj = new object();
        private int _currentIndex = 0;
        private bool _isFirstFileTurn = true;
        private List<string> _linesFile1;
        private List<string> _linesFile2;
        private StreamWriter _writerFile3;

        public void WriteTwoFiles(List<Person> people)
        {
            var firstHalf = people.Take(10).ToList();
            var secondHalf = people.Skip(10).Take(10).ToList();

            Thread t1 = new Thread(() => WritePeopleToFile(firstHalf, "file1.json"));
            Thread t2 = new Thread(() => WritePeopleToFile(secondHalf, "file2.json"));

            t1.Start();
            t2.Start();
            t1.Join();
            t2.Join();
        }

        private void WritePeopleToFile(List<Person> people, string filePath)
        {
            using (StreamWriter writer = new StreamWriter(filePath, append: false))
            {
                foreach (var person in people)
                {
                    string json = JsonSerializer.Serialize(person);
                    writer.WriteLine(json);
                }
            }
            Console.WriteLine($"Запись в {filePath} завершена.");
        }

        public void ReadAndMergeAlternately()
        {
            Thread read1 = new Thread(() => _linesFile1 = ReadLinesFromFile("file1.json"));
            Thread read2 = new Thread(() => _linesFile2 = ReadLinesFromFile("file2.json"));
            read1.Start(); read2.Start();
            read1.Join(); read2.Join();

            using (_writerFile3 = new StreamWriter("file3.json", append: false))
            {
                Thread write1 = new Thread(WriteFromFile1);
                Thread write2 = new Thread(WriteFromFile2);
                write1.Start(); write2.Start();
                write1.Join(); write2.Join();
            }
            Console.WriteLine("Слияние в file3.json завершено.");
        }

        private List<string> ReadLinesFromFile(string path)
        {
            if (File.Exists(path))
                return File.ReadAllLines(path).ToList();
            return new List<string>();
        }

        private void WriteFromFile1()
        {
            while (true)
            {
                string lineToWrite = null;
                bool shouldExit = false;
                lock (_lockObj)
                {
                    while (!_isFirstFileTurn && _currentIndex < _linesFile1.Count)
                        Monitor.Wait(_lockObj);
                    if (_currentIndex >= _linesFile1.Count)
                        shouldExit = true;
                    else
                    {
                        lineToWrite = _linesFile1[_currentIndex];
                        _isFirstFileTurn = false;
                        Monitor.PulseAll(_lockObj);
                    }
                }
                if (shouldExit) break;
                if (lineToWrite != null)
                    lock (_lockObj) { _writerFile3.WriteLine(lineToWrite); }
            }
            lock (_lockObj) { Monitor.PulseAll(_lockObj); }
        }

        private void WriteFromFile2()
        {
            while (true)
            {
                string lineToWrite = null;
                bool shouldExit = false;
                lock (_lockObj)
                {
                    while (_isFirstFileTurn && _currentIndex < _linesFile2.Count)
                        Monitor.Wait(_lockObj);
                    if (_currentIndex >= _linesFile2.Count)
                        shouldExit = true;
                    else
                    {
                        lineToWrite = _linesFile2[_currentIndex];
                        _isFirstFileTurn = true;
                        _currentIndex++;
                        Monitor.PulseAll(_lockObj);
                    }
                }
                if (shouldExit) break;
                if (lineToWrite != null)
                    lock (_lockObj) { _writerFile3.WriteLine(lineToWrite); }
            }
            lock (_lockObj) { Monitor.PulseAll(_lockObj); }
        }

        public void ReadFileSingleThread()
        {
            Stopwatch sw = Stopwatch.StartNew();
            string content = File.ReadAllText("file3.json");
            sw.Stop();
            Console.WriteLine("Содержимое file3.json:");
            Console.WriteLine(content);
            Console.WriteLine($"Время чтения (1 поток): {sw.ElapsedMilliseconds} мс");
        }

        public void ReadFileTwoThreads()
        {
            string[] allLines = File.ReadAllLines("file3.json");
            int mid = allLines.Length / 2;
            Stopwatch sw = Stopwatch.StartNew();
            Thread t1 = new Thread(() => ProcessLines(allLines.Take(mid).ToArray(), "Поток 1"));
            Thread t2 = new Thread(() => ProcessLines(allLines.Skip(mid).ToArray(), "Поток 2"));
            t1.Start(); t2.Start();
            t1.Join(); t2.Join();
            sw.Stop();
            Console.WriteLine($"Время обработки (2 потока): {sw.ElapsedMilliseconds} мс");
        }

        private void ProcessLines(string[] lines, string threadName)
        {
            foreach (var line in lines)
                lock (_lockObj) { Console.WriteLine($"{threadName}: {line}"); }
        }

        public void ReadFileWithSemaphore()
        {
            string[] allLines = File.ReadAllLines("file3.json");
            SemaphoreSlim semaphore = new SemaphoreSlim(5, 5);
            List<Thread> threads = new List<Thread>();
            Stopwatch sw = Stopwatch.StartNew();
            for (int i = 0; i < 10; i++)
            {
                int threadNum = i + 1;
                Thread t = new Thread(() =>
                {
                    semaphore.Wait();
                    try
                    {
                        int chunkSize = allLines.Length / 10;
                        int start = (threadNum - 1) * chunkSize;
                        int end = (threadNum == 10) ? allLines.Length : start + chunkSize;
                        for (int j = start; j < end; j++)
                            lock (_lockObj) { Console.WriteLine($"Поток {threadNum}: {allLines[j]}"); }
                        Thread.Sleep(10);
                    }
                    finally { semaphore.Release(); }
                });
                threads.Add(t);
                t.Start();
            }
            foreach (var t in threads) t.Join();
            sw.Stop();
            Console.WriteLine($"Время выполнения (10 потоков, семафор 5): {sw.ElapsedMilliseconds} мс");
        }
    }
}
