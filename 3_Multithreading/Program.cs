using System.Text;

namespace _3_Multithreading
{
    class Program
    {
        static async Task Main()
        {
            Console.OutputEncoding = Encoding.UTF8;

            List<Person> people = GeneratePeople(20);
            TaskFileProcessor taskProcessor = new TaskFileProcessor();

            taskProcessor.WriteTwoFilesWithTasks(people);

            taskProcessor.ReadAndMergeWithTasks();

            await taskProcessor.ReadAndOutputAsync();

            Assignment5Runner.Run();
        }

        static List<Person> GeneratePeople(int count)
        {
            var list = new List<Person>();
            for (int i = 1; i <= count; i++)
                list.Add(new Person { Id = i, Name = $"Person_{i}", Age = 20 + i % 30 });
            return list;
        }
    }
}