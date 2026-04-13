using _2_Serialization;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;

namespace _2_Serialization { 
class Program
{
    static string filePath = "devices.xml";
    static List<Device> devices = new List<Device>();
    public static void Main(string[] args)
    {
        while (true)
        {
            Console.WriteLine("\nМеню:");
            Console.WriteLine("1. Создать 10 экземпляров и вывести в консоль");
            Console.WriteLine("2. Сериализовать объекты в XML-файл");
            Console.WriteLine("3. Прочитать XML-файл и вывести содержимое");
            Console.WriteLine("4. Десериализовать объекты из файла");
            Console.WriteLine("5. Найти все значения атрибута Model (XDocument)");
            Console.WriteLine("6. Найти все значения атрибута Model (XmlDocument)");
            Console.WriteLine("7. Изменить значение атрибута (XDocument)");
            Console.WriteLine("8. Изменить значение атрибута (XmlDocument)");
            Console.WriteLine("0. Выход");
            Console.Write("Выберите пункт: ");
            string choice = Console.ReadLine();

            switch (choice)
            {
                case "1": CreateAndPrintDevices(); break;
                case "2": SerializeToXml(); break;
                case "3": ReadXmlRaw(); break;
                case "4": DeserializeFromXml(); break;
                case "5": FindModelsXDocument(); break;
                case "6": FindModelsXmlDocument(); break;
                case "7": ModifyAttributeXDocument(); break;
                case "8": ModifyAttributeXmlDocument(); break;
                case "0": return;
                default: Console.WriteLine("Неверный выбор"); break;
            }
        }
    }

    static void CreateAndPrintDevices()
    {
        devices.Clear();
        for (int i = 1; i <= 10; i++)
        {
            devices.Add(new Device(
                model: $"Model_{i}",
                manufacturer: $"Brand_{i % 3 + 1}",
                year: 2015 + i,
                price: 500 * i
            ));
        }
        Console.WriteLine("Создано 10 устройств:");
        foreach (var d in devices)
            Console.WriteLine(d);
    }

    static void SerializeToXml()
    {
        if (devices.Count == 0)
        {
            Console.WriteLine("Сначала создайте объекты (пункт 1)");
            return;
        }
        try
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<Device>));
            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                serializer.Serialize(fs, devices);
            }
            Console.WriteLine($"Сериализация завершена. Файл: {filePath}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }
    }

    static void ReadXmlRaw()
    {
        if (!File.Exists(filePath))
        {
            Console.WriteLine("Файл не найден. Сначала выполните сериализацию (пункт 2)");
            return;
        }
        string content = File.ReadAllText(filePath);
        Console.WriteLine("Содержимое XML-файла:");
        Console.WriteLine(content);
    }

    static void DeserializeFromXml()
    {
        if (!File.Exists(filePath))
        {
            Console.WriteLine("Файл не найден. Сначала выполните сериализацию (пункт 2)");
            return;
        }
        try
        {
            XmlSerializer serializer = new XmlSerializer(typeof(List<Device>));
            using (FileStream fs = new FileStream(filePath, FileMode.Open))
            {
                devices = (List<Device>)serializer.Deserialize(fs);
            }
            Console.WriteLine("Десериализация выполнена. Полученные объекты:");
            foreach (var d in devices)
                Console.WriteLine(d);
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Ошибка: {ex.Message}");
        }
    }

    static void FindModelsXDocument()
    {
        if (!File.Exists(filePath))
        {
            Console.WriteLine("Файл не найден.");
            return;
        }
        XDocument doc = XDocument.Load(filePath);
        var models = doc.Descendants("Device")
                        .Attributes("Model")
                        .Select(a => a.Value);
        Console.WriteLine("Найденные значения Model (XDocument):");
        foreach (var m in models)
            Console.WriteLine(m);
    }

    static void FindModelsXmlDocument()
    {
        if (!File.Exists(filePath))
        {
            Console.WriteLine("Файл не найден.");
            return;
        }
        XmlDocument doc = new XmlDocument();
        doc.Load(filePath);
        XmlNodeList nodes = doc.SelectNodes("//Device/@Model");
        Console.WriteLine("Найденные значения Model (XmlDocument):");
        foreach (XmlNode node in nodes)
            Console.WriteLine(node.Value);
    }

    static void ModifyAttributeXDocument()
    {
        if (!File.Exists(filePath))
        {
            Console.WriteLine("Файл не найден.");
            return;
        }
        Console.Write("Введите имя атрибута (например, Model): ");
        string attrName = Console.ReadLine();
        Console.Write("Введите номер элемента (начиная с 0): ");
        if (!int.TryParse(Console.ReadLine(), out int index))
        {
            Console.WriteLine("Неверный номер");
            return;
        }
        Console.Write("Введите новое значение: ");
        string newValue = Console.ReadLine();

        XDocument doc = XDocument.Load(filePath);
        var elements = doc.Descendants("Device").ToList();
        if (index < 0 || index >= elements.Count)
        {
            Console.WriteLine("Элемента с таким номером нет");
            return;
        }
        XElement element = elements[index];
        XAttribute attr = element.Attribute(attrName);
        if (attr == null)
        {
            Console.WriteLine($"Атрибут '{attrName}' не найден у элемента {index}");
            return;
        }
        attr.Value = newValue;
        doc.Save(filePath);
        Console.WriteLine("Атрибут изменён (через XDocument)");
    }

    static void ModifyAttributeXmlDocument()
    {
        if (!File.Exists(filePath))
        {
            Console.WriteLine("Файл не найден.");
            return;
        }
        Console.Write("Введите имя атрибута (например, Model): ");
        string attrName = Console.ReadLine();
        Console.Write("Введите номер элемента (начиная с 0): ");
        if (!int.TryParse(Console.ReadLine(), out int index))
        {
            Console.WriteLine("Неверный номер");
            return;
        }
        Console.Write("Введите новое значение: ");
        string newValue = Console.ReadLine();

        XmlDocument doc = new XmlDocument();
        doc.Load(filePath);
        XmlNodeList nodes = doc.SelectNodes("//Device");
        if (index < 0 || index >= nodes.Count)
        {
            Console.WriteLine("Элемента с таким номером нет");
            return;
        }
        XmlNode deviceNode = nodes[index];
        if (deviceNode.Attributes[attrName] == null)
        {
            Console.WriteLine($"Атрибут '{attrName}' не найден у элемента {index}");
            return;
        }
        deviceNode.Attributes[attrName].Value = newValue;
        doc.Save(filePath);
        Console.WriteLine("Атрибут изменён (через XmlDocument)");
    }
}
}