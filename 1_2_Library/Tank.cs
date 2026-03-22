namespace _1_2_Library
{
    public class Tank
    {
        private int ID { get; set; }         
        public string Model { get; set; }    
        public string SerialNumber { get; set; }
        public string TankType { get; set; }

        public static Tank Create(int id, string model, string serialNumber, string tankType)
        {
            return new Tank
            {
                ID = id,
                Model = model,
                SerialNumber = serialNumber,
                TankType = tankType
            };
        }

        public void PrintObject()
        {
            Console.WriteLine($"Tank: ID={ID}, Model={Model}, Serial={SerialNumber}, Type={TankType}");
        }
    }
}
