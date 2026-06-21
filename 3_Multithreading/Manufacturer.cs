using System.Collections.Generic;

namespace _3_Multithreading
{
    public class Manufacturer
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Country { get; set; }
        public ICollection<Phone> Phones { get; set; } = new List<Phone>();
    }
}