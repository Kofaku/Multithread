using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace _3_Multithreading
{
    public class ThreadSafePersonList
    {
        private readonly List<Person> _list = new List<Person>();
        private readonly object _lock = new object();

        public void Add(Person person)
        {
            lock (_lock)
            {
                _list.Add(person);
            }
        }

        public List<Person> GetSortedCopy()
        {
            lock (_lock)
            {
                var copy = new List<Person>(_list);
                copy.Sort((a, b) => a.Id.CompareTo(b.Id));
                return copy;
            }
        }

        public void SortById()
        {
            lock (_lock)
            {
                _list.Sort((a, b) => a.Id.CompareTo(b.Id));
            }
        }

        public int Count
        {
            get
            {
                lock (_lock) return _list.Count;
            }
        }
    }

    public class SortingObserver
    {
        private readonly ConcurrentDictionary<string, ThreadSafePersonList> _dictionary;
        private readonly int _intervalMs;
        private volatile bool _isRunning = true;

        public SortingObserver(ConcurrentDictionary<string, ThreadSafePersonList> dictionary, int intervalMs = 3000)
        {
            _dictionary = dictionary;
            _intervalMs = intervalMs;
        }

        public void Start()
        {
            Thread worker = new Thread(Run);
            worker.IsBackground = true;
            worker.Start();
        }

        public void Stop() => _isRunning = false;

        private void Run()
        {
            while (_isRunning)
            {
                Thread.Sleep(_intervalMs);
                foreach (var kvp in _dictionary)
                {
                    kvp.Value.SortById();
                }
            }
        }
    }

    public class Assignment5Processor
    {
        private const int TotalPersons = 50;
        private const int FileCount = 5;
        private readonly string[] _fileNames;

        public Assignment5Processor()
        {
            _fileNames = Enumerable.Range(0, FileCount)
                                   .Select(i => $"data{i}.txt")
                                   .ToArray();
        }

        private List<Person> GeneratePeople()
        {
            var list = new List<Person>();
            for (int i = 1; i <= TotalPersons; i++)
            {
                list.Add(new Person
                {
                    Id = i,
                    Name = $"Person_{i}",
                    Age = 20 + (i % 30)
                });
            }
            return list;
        }

        public void WritePeopleToFiles()
        {
            var allPeople = GeneratePeople();
            int chunkSize = TotalPersons / FileCount;

            for (int i = 0; i < FileCount; i++)
            {
                var chunk = allPeople.Skip(i * chunkSize).Take(chunkSize).ToList();
                string filePath = _fileNames[i];
                using (StreamWriter writer = new StreamWriter(filePath, false, Encoding.UTF8))
                {
                    foreach (var person in chunk)
                    {
                        string json = JsonSerializer.Serialize(person);
                        writer.WriteLine(json);
                    }
                }
                Console.WriteLine($"Запись в {filePath} завершена. ({chunk.Count} записей)");
            }
        }

        public void ProcessFilesWithProgressAndSorting()
        {
            var dataDictionary = new ConcurrentDictionary<string, ThreadSafePersonList>();
            foreach (var fileName in _fileNames)
            {
                dataDictionary[fileName] = new ThreadSafePersonList();
            }

            var sorter = new SortingObserver(dataDictionary, 2000);
            sorter.Start();

            int totalRead = 0;
            object progressLock = new object();

            void UpdateProgressBar(int current, int total)
            {
                lock (progressLock)
                {
                    int barWidth = 50;
                    double percent = (double)current / total;
                    int filled = (int)(percent * barWidth);
                    string bar = new string('#', filled) + new string('-', barWidth - filled);
                    Console.Write($"\rПрогресс: [{bar}] {current}/{total} ({percent:P0})");
                }
            }

            Thread[] threads = new Thread[FileCount];
            for (int i = 0; i < FileCount; i++)
            {
                string fileName = _fileNames[i];
                threads[i] = new Thread(() =>
                {
                    if (File.Exists(fileName))
                    {
                        var lines = File.ReadAllLines(fileName);
                        foreach (var line in lines)
                        {
                            var person = JsonSerializer.Deserialize<Person>(line);
                            if (person != null)
                            {
                                dataDictionary[fileName].Add(person);
                                int newCount = Interlocked.Increment(ref totalRead);
                                UpdateProgressBar(newCount, TotalPersons);
                            }
                        }
                    }
                    Console.WriteLine($"\nПоток завершил чтение файла {fileName}");
                });
                threads[i].Start();
            }

            foreach (var t in threads) t.Join();

            Console.WriteLine("\n\nЧтение всех файлов завершено.");

            Thread.Sleep(1000);

            Console.WriteLine("\nСодержимое словаря (отсортировано по Id):");
            foreach (var fileName in _fileNames)
            {
                Console.WriteLine($"\nФайл: {fileName}");
                var sortedList = dataDictionary[fileName].GetSortedCopy();
                foreach (var person in sortedList)
                {
                    Console.WriteLine(person);
                }
            }

            sorter.Stop();
        }
    }

    public static class Assignment5Runner
    {
        public static void Run()
        {
            Console.OutputEncoding = Encoding.UTF8;
            var processor = new Assignment5Processor();

            processor.WritePeopleToFiles();

            processor.ProcessFilesWithProgressAndSorting();
        }
    }
}
