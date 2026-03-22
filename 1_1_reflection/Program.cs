using System;
using System.Linq;
using System.Reflection;

class Program
{
    static void Main()
    {
        try
        {
            Console.Write("Введите имя класса (с пространством имён): ");
            string className = Console.ReadLine();
            Type type = Type.GetType(className);
            if (type == null)
            {
                Console.WriteLine("Класс не найден.");
                return;
            }

            Console.Write("Введите имя метода: ");
            string methodName = Console.ReadLine();

            Console.Write("Введите аргументы через пробел: ");
            string argsLine = Console.ReadLine();
            string[] inputArgs = argsLine?.Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries) ?? Array.Empty<string>();

            MethodInfo method = null;
            object[] methodArgs = null;

            var candidates = type.GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Static)
                                 .Where(m => m.Name == methodName)
                                 .ToList();

            if (candidates.Count == 0)
            {
                Console.WriteLine("Метод не найден.");
                return;
            }

            foreach (var candidate in candidates)
            {
                ParameterInfo[] parameters = candidate.GetParameters();
                if (parameters.Length != inputArgs.Length) continue;

                bool ok = true;
                object[] args = new object[parameters.Length];
                for (int i = 0; i < parameters.Length; i++)
                {
                    try
                    {
                        args[i] = Convert.ChangeType(inputArgs[i], parameters[i].ParameterType);
                    }
                    catch
                    {
                        ok = false;
                        break;
                    }
                }
                if (ok)
                {
                    method = candidate;
                    methodArgs = args;
                    break;
                }
            }

            if (method == null)
            {
                Console.WriteLine("Не удалось найти метод с подходящей сигнатурой для введённых аргументов.");
                return;
            }

            object instance = null;
            if (!method.IsStatic)
            {
                try
                {
                    instance = Activator.CreateInstance(type);
                }
                catch (MissingMethodException)
                {
                    Console.WriteLine("Нет доступного конструктора без параметров для создания экземпляра.");
                    return;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Ошибка при создании экземпляра: {ex.Message}");
                    return;
                }
            }

            object result = method.Invoke(instance, methodArgs);
            Console.WriteLine($"Результат: {result}");
        }
        catch (TargetInvocationException ex)
        {
            Console.WriteLine($"Ошибка в вызванном методе: {ex.InnerException?.Message}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }
    }
}