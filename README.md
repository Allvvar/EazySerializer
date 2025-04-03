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
public class MyClass
{
    public string Property1 { get; set; }
    public int Property2 { get; set; }
}

// Create an instance of EazySerializer with encryption enabled and pretty-printing turned on.
EazySerializer serializer = new EazySerializer(
    superSecure: true,
    prettyPrint: true,
    aesKey: "YourEncryptionKey",
    aesIV: "YourEncryptionIV"
);

// Create sample data to serialize
List<MyClass> myData = new List<MyClass>
{
    new MyClass { Property1 = "Value1", Property2 = 123 },
    new MyClass { Property1 = "Value2", Property2 = 456 }
};

// Serialize data to file with encryption
serializer.SerializeData(myData, "data.json", useEncryption: true);

// Deserialize data from file; using the overload that returns a success flag.
bool operationSuccess;
List<MyClass> deserializedData = serializer.DeserializeData<List<MyClass>>("data.json", useEncryption: true, out operationSuccess);

if (operationSuccess)
{
    Console.WriteLine("Deserialization succeeded!");
}
else
{
    Console.WriteLine("Deserialization failed.");
}
```

### MIT License Explanation:
The MIT License is a permissive free software license. It allows you to freely use, modify, distribute, and sublicense the software, even in commercial applications. The only conditions are:
- The license must be included in all copies or substantial portions of the software.
- The software is provided "as is", without warranty of any kind, express or implied, including but not limited to the warranties of merchantability, fitness for a particular purpose, or non-infringement.
