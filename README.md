# EazySerializer Class

## Overview

The **EazySerializer** class is a robust and flexible utility for handling data persistence in .NET applications. It provides methods to serialize any JSON-serializable object—including individual objects, collections, and complex nested types—to a file and to deserialize data back into objects. Built entirely with standard .NET libraries, it leverages `System.Text.Json` for JSON processing and supports optional AES encryption for securing sensitive data.

I plan to add support for other formats like XML and Binary. For now, it only includes JSON serialization.

## Key Features

- **Versatile Serialization:**  
  Serialize and deserialize any serializable object, including lists and complex nested types, thanks to the use of the standard C# implementation for serialization.

- **Optional AES Encryption:**  
  Secure your data by optionally encrypting it using AES. Configure encryption to use either a full 32-byte hash ("super secure") or a truncated 16-byte hash.

- **Configurable JSON Formatting:**  
  Choose between compact or pretty-printed (indented) JSON output to facilitate debugging or data review.

- **Cross-Platform File Handling:**  
  Automatically resolves file paths to writable locations across different operating systems (Windows, Linux, macOS, etc.).

- **Built-In Logging:**  
  Maintains a log of operations and errors within a simple collection, providing a built-in mechanism for tracking serialization activities without external logging dependencies.

- **Resource Management:**  
  Implements proper disposal of resources using `using` statements to prevent memory leaks and optimize performance.

## .NET Version Compatibility

The EazySerializer class is compatible with the following .NET versions:

- **.NET Core:**  
  .NET Core 3.0 and higher (including .NET 5.0, .NET 6.0, .NET 7.0, and later), this is required due to the use of nullable reference types and other modern C# features such as Tuples.

- **.Net:**  
  .NET 5.0 and higher, fully supported for all features of the class, including AES encryption, logging, and serialization.

This class leverages features introduced in C# 7.0 (for Tuples) and C# 8.0 (for nullable reference types), which are available in .NET Core 3.0 and later versions. It will not work with versions prior to .NET Core 3.0 due to these dependencies.

## Usage Example

Below is a sample code snippet demonstrating how to use the **EazySerializer** class:

```csharp
// Example class to be serialized
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
```

### MIT License Explanation:
The MIT License is a permissive free software license. It allows you to freely use, modify, distribute, and sublicense the software, even in commercial applications. The only conditions are:
- The license must be included in all copies or substantial portions of the software.
- The software is provided "as is", without warranty of any kind, express or implied, including but not limited to the warranties of merchantability, fitness for a particular purpose, or non-infringement.
