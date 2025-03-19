using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;

public class DataSerializer
{
    // Author: Alvar
    // Written on: 2025-03-19 21:53
    // Last Modified: 2025-03-19 21:53
    // Version: 1.0

    /// <summary>
    /// Serializes a list of data and writes it to the specified file.
    /// </summary>
    /// <typeparam name="T">The type of the data to be serialized.</typeparam>
    /// <param name="data">The list of data to be serialized.</param>
    /// <param name="filePath">The path where the serialized data will be written.</param>
    public static void SerializeListData<T>(List<T> data, string filePath)
    {
        try
        {
            // Create a FileStream to write to the specified file
            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                // Serialize the data and write it to the file using JsonSerializer
                JsonSerializer.Serialize(fs, data);
                Console.WriteLine($"Data successfully serialized to {filePath}");
            }
        }
        catch (Exception ex)
        {
            // Print error message if an exception occurs during serialization
            Console.WriteLine($"An error occurred while serializing data: {ex.Message}");
        }
    }

    /// <summary>
    /// Deserializes data from a file and returns it as a list of objects.
    /// </summary>
    /// <typeparam name="T">The type of the objects in the list.</typeparam>
    /// <param name="filePath">The path to the file that contains the serialized data.</param>
    /// <returns>A list of deserialized objects.</returns>
    public static List<T> DeserializeListData<T>(string filePath)
    {
        try
        {
            // Create a FileStream to read from the specified file
            using (FileStream fs = new FileStream(filePath, FileMode.Open))
            {
                // Deserialize the data from the file and return it as a list of objects
                List<T> data = JsonSerializer.Deserialize<List<T>>(fs);
                Console.WriteLine($"Data successfully deserialized from {filePath}");
                return data;
            }
        }
        catch (Exception ex)
        {
            // Print error message if an exception occurs during deserialization
            Console.WriteLine($"An error occurred while deserializing data: {ex.Message}");
            return null;
        }
    }

    /// <summary>
    /// Serializes any serializable object and writes it to the specified file path.
    /// </summary>
    /// <typeparam name="T">The type of the object to serialize.</typeparam>
    /// <param name="data">The object to serialize.</param>
    /// <param name="filePath">The path where the serialized data will be written.</param>
    public static void SerializeData<T>(T data, string filePath)
    {
        try
        {
            // Create a FileStream to write to the specified file
            using (FileStream fs = new FileStream(filePath, FileMode.Create))
            {
                // Serialize the data and write it to the file using JsonSerializer
                JsonSerializer.Serialize(fs, data);
                Console.WriteLine($"Data successfully serialized to {filePath}");
            }
        }
        catch (Exception ex)
        {
            // Print error message if an exception occurs during serialization
            Console.WriteLine($"An error occurred while serializing data: {ex.Message}");
        }
    }

    /// <summary>
    /// Deserializes data from a file and returns it as an object of type T.
    /// </summary>
    /// <typeparam name="T">The type of the object to deserialize.</typeparam>
    /// <param name="filePath">The path to the file that contains the serialized data.</param>
    /// <returns>The deserialized object of type T, or null if deserialization fails.</returns>
    public static T DeserializeData<T>(string filePath)
    {
        try
        {
            // Create a FileStream to read from the specified file
            using (FileStream fs = new FileStream(filePath, FileMode.Open))
            {
                // Deserialize the data from the file and return it as an object of type T
                T data = JsonSerializer.Deserialize<T>(fs);
                Console.WriteLine($"Data successfully deserialized from {filePath}");
                return data;
            }
        }
        catch (Exception ex)
        {
            // Print error message if an exception occurs during deserialization
            Console.WriteLine($"An error occurred while deserializing data: {ex.Message}");
            return default(T); // Return default value for the type if deserialization fails
        }
    }

    /// <summary>
    /// Checks if a file already exists at the specified file path.
    /// </summary>
    /// <param name="filePath">The path of the file to check for existence.</param>
    /// <returns>True if the file exists, otherwise false.</returns>
    public static bool DoesFileExist(string filePath)
    {
        // Return true if the file exists at the given path, otherwise false
        return File.Exists(filePath);
    }

    /// <summary>
    /// Returns an integer based on the host operating system.
    /// </summary>
    /// <returns>
    /// 0: Linux
    /// 1: Windows
    /// 2: macOS/OSX
    /// 3: FreeBSD
    /// 4: Android/iOS/Other
    /// </returns>
    public static int GetOperatingSystem()
    {
        // Check the platform and return an integer corresponding to the operating system
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return 0;  // Linux
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return 1;  // Windows
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return 2;  // macOS/OSX
        }
        else if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
        {
            return 3;  // FreeBSD
        }
        else
        {
            return 4;  // Other platforms (Android, iOS, or somthing else that manage to run C# code somehow?)
        }
    }
}
