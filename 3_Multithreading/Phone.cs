using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace _3_Multithreading
{
    public class Phone
    {
        public int Id { get; set; }
        public string Model { get; set; }
        public decimal Price { get; set; }
        public int ManufacturerId { get; set; }
        public Manufacturer Manufacturer { get; set; }
    }
}
