using System;
using System.Reflection;
using System.Linq;

class Program
{
    static void Main()
    {
        string dllPath = @"C:\Users\masha\source\Multithread\1_2_Library\bin\Debug\net8.0\1_2_Library.dll";

        try
        {
            Assembly assembly = Assembly.LoadFrom(dllPath);

            Type[] types = assembly.GetTypes();

            foreach (Type type in types)
            {
                if (!type.IsClass) continue;

                MethodInfo createMethod = type.GetMethod("Create",
                    BindingFlags.Public | BindingFlags.Static);
                if (createMethod == null)
                {
                    Console.WriteLine("  Метод Create не найден, пропускаем.");
                    continue;
                }

                ParameterInfo[] createParams = createMethod.GetParameters();
                Console.WriteLine($"  Метод Create имеет параметры: {string.Join(", ", createParams.Select(p => p.ParameterType.Name + " " + p.Name))}");

                object[] createArgs = new object[createParams.Length];
                for (int i = 0; i < createParams.Length; i++)
                {
                    Console.Write($"    Введите значение для {createParams[i].ParameterType.Name} {createParams[i].Name}: ");
                    string input = Console.ReadLine();
                    createArgs[i] = Convert.ChangeType(input, createParams[i].ParameterType);
                }

                object instance = createMethod.Invoke(null, createArgs);
                Console.WriteLine("  Объект создан.");

                MethodInfo printMethod = type.GetMethod("PrintObject",
                    BindingFlags.Public | BindingFlags.Instance);
                if (printMethod == null)
                {
                    Console.WriteLine("  Метод PrintObject не найден.");
                    continue;
                }

                Console.Write("  Результат PrintObject: ");
                printMethod.Invoke(instance, null);
            }
        }
        catch (FileNotFoundException)
        {
            Console.WriteLine("Ошибка: файл библиотеки не найден. Проверьте путь.");
        }
        catch (TargetInvocationException ex)
        {
            Console.WriteLine($"Ошибка в вызванном методе: {ex.InnerException?.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Общая ошибка: {ex.Message}");
        }
    }
}