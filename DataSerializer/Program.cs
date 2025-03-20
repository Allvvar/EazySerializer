namespace DataSerializer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            DataSerializer dataSerializer = new DataSerializer(true, true, "Hello", "Moahahah");

            string filePath = dataSerializer.GetWritableAbsolutePath("Test/Life");
            List<string> input = new List<string>();

            for (int i = 0; i < 3; i++)
            {
                input.Add(Console.ReadLine());
            }
            dataSerializer.SerializeData<List<string>>(input, filePath, false);

            List<string> output = new List<string>();
            output = dataSerializer.DeserializeData<List<string>>(filePath, false);
            foreach (string item in output)
            {
                Console.WriteLine(item);
            }

            Console.WriteLine("The OS is: " + dataSerializer.GetOperatingSystem());
        }
    }
}
