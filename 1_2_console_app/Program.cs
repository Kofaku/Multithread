using System;
using System.Reflection;
using System.Linq;

class Program
{
    static void Main()
    {
        try
        {
            string dllPath = @"C:\Users\masha\source\Multithread\1_2_Library\bin\Debug\net8.0\1_2_Library.dll";
            Assembly assembly = Assembly.LoadFrom(dllPath);

            Type[] types = assembly.GetTypes();

            Console.WriteLine("Классы в библиотеке:\n");
            foreach (Type type in types)
            {
                if (!type.IsClass) continue;

                Console.WriteLine($"Класс: {type.Name}");

                var properties = type.GetProperties(
                    BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance
                );

                Console.WriteLine("  Свойства:");
                foreach (var prop in properties)
                {
                    string accessModifier = GetAccessModifier(prop);
                    Console.WriteLine($"    {accessModifier} {prop.PropertyType.Name} {prop.Name}");
                }
                Console.WriteLine();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }
    }

    static string GetAccessModifier(PropertyInfo prop)
    {
        var getMethod = prop.GetMethod;
        if (getMethod == null) getMethod = prop.SetMethod;

        if (getMethod == null) return "неизвестно";

        if (getMethod.IsPublic) return "public";
        if (getMethod.IsPrivate) return "private";

        return "другой";
    }
}