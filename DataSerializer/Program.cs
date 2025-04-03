using EazyDataSerializer;
using System.Text;

namespace DataSerializer
{
    class SerializebleClass
    {
        public int? number { get; set; }
        public string? name { get; set; }
        public string? description { get; set; }
        
        public List<string> tags { get; set; } = new List<string>();

        public double floatNumber; // Field.

        public override string ToString()
        {
            StringBuilder sb = new StringBuilder();

            if (number != null)
                sb.AppendLine("Number: " + number.ToString());
            if (number != null)
                sb.AppendLine("Name: " + name.ToString());
            if (number != null)
                sb.AppendLine("Description: " + description.ToString());
            sb.AppendLine("FloatNumber: " + floatNumber.ToString());

            sb.AppendLine("Tags:");
            foreach (var tag in tags)
            {
                sb.AppendLine(tag.ToString());
            }

            return sb.ToString();
        }
    }


    internal class Program
    {
        static void Main(string[] args)
        {
            var dataSerializer = new EazySerializer(true, true, true, true); // Does not use encryption.

            string filePath = dataSerializer.GetWritableAbsolutePath("Test/SaveTest/Text.txt");

            var obj = new SerializebleClass();
            obj.number = 423;
            obj.name = "A PERSON";
            obj.description = "I think this is rather smart, what is this thing, but a text writen into a test class";
            obj.floatNumber = 3.141592653589793243243223542534234523424345432234423;

            obj.tags = new List<string>
            {
                "Thing",
                "Other Thing",
                "Pi is just half of Tau",
                "Tau is superior, if you dont know anything..."
            };

            dataSerializer.WriteData<SerializebleClass>(obj, filePath);

            var readObj = dataSerializer.ReadData<SerializebleClass>(filePath);

            Console.WriteLine(readObj.ToString());

            // Returns number representing OS.
            Console.WriteLine("The OS is: " + dataSerializer.GetOperatingSystem());
        }
    }
}
