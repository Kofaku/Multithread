using System;
using System.Collections.Generic;
using System.Xml.Serialization;

namespace _2_Serialization
{
    public class Device
    {
        [XmlAttribute]
        public string Model { get; set; }

        public string Manufacturer { get; set; }
        public int Year { get; set; }
        public decimal Price { get; set; }

        public Device() { }

        public Device(string model, string manufacturer, int year, decimal price)
        {
            Model = model;
            Manufacturer = manufacturer;
            Year = year;
            Price = price;
        }

        public override string ToString()
        {
            return $"Модель: {Model}, Производитель: {Manufacturer}, Год: {Year}, Цена: {Price}";
        }
    }
}
