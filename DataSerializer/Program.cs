namespace DataSerializer
{
    internal class Program
    {
        static void Main(string[] args)
        {
            DataSerializer dataSerializer = new DataSerializer(true, "Hello", "Moahahah");

            string filePath = "file.txt";
            List<string> input = new List<string>();

            for (int i = 0; i < 3; i++)
            {
                input.Add(Console.ReadLine());
            }
            dataSerializer.SerializeListData<string>(input, filePath, true);

            List<string> output = new List<string>();
            output = dataSerializer.DeserializeData<List<string>>(filePath, true);
            foreach (string item in output)
            {
                Console.WriteLine(item);
            }

            Console.WriteLine("The OS is: " + dataSerializer.GetOperatingSystem());
        }
    }
}
