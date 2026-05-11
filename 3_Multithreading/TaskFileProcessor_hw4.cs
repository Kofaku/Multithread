using System.Text.Json;

namespace _3_Multithreading
{
    public class TaskFileProcessor
    {
        private readonly object _writeLock = new object();

        public void WriteTwoFilesWithTasks(List<Person> people)
        {
            var firstHalf = people.Take(10).ToList();
            var secondHalf = people.Skip(10).Take(10).ToList();

            Task task1 = Task.Run(() => WritePeopleToFile(firstHalf, "file1.json"));
            Task task2 = Task.Run(() => WritePeopleToFile(secondHalf, "file2.json"));

            Task.WaitAll(task1, task2);
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
            Console.WriteLine($"Задача записи в {filePath} завершена.");
        }

        public void ReadAndMergeWithTasks()
        {
            List<string> lines1 = null, lines2 = null;

            Task taskRead1 = Task.Run(() => lines1 = ReadLines("file1.json"));
            Task taskRead2 = Task.Run(() => lines2 = ReadLines("file2.json"));

            Task.WaitAll(taskRead1, taskRead2);

            using (StreamWriter writer = new StreamWriter("file3.json", append: false))
            {
                Task write1 = Task.Run(() =>
                {
                    lock (_writeLock)
                    {
                        foreach (var line in lines1)
                            writer.WriteLine(line);
                    }
                });

                Task write2 = Task.Run(() =>
                {
                    lock (_writeLock)
                    {
                        foreach (var line in lines2)
                            writer.WriteLine(line);
                    }
                });

                Task.WaitAll(write1, write2);
            }

            Console.WriteLine("Слияние в file3.json завершено (с использованием Task).");
        }

        private List<string> ReadLines(string path)
        {
            if (File.Exists(path))
                return File.ReadAllLines(path).ToList();
            return new List<string>();
        }

        public async Task ReadAndOutputAsync()
        {
            string[] allLines = await File.ReadAllLinesAsync("file3.json");

            int mid = allLines.Length / 2;

            Task outputTask1 = Task.Run(() =>
            {
                for (int i = 0; i < mid; i++)
                {
                    lock (_writeLock)
                    {
                        Console.WriteLine($"Task1: {allLines[i]}");
                    }
                }
            });

            Task outputTask2 = Task.Run(() =>
            {
                for (int i = mid; i < allLines.Length; i++)
                {
                    lock (_writeLock)
                    {
                        Console.WriteLine($"Task2: {allLines[i]}");
                    }
                }
            });

            await Task.WhenAll(outputTask1, outputTask2);
            Console.WriteLine("Асинхронный вывод завершён.");
        }
    }
}