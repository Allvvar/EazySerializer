using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace DataSerializer
{


    // Author: Alvar  
    // Written on: 2025-03-19 21:53  
    // Last Modified: 2025-03-19 23:52  
    // Version: 1.1
    /// <summary>
    /// Provides functionality to serialize and deserialize data with optional AES encryption.
    /// </summary>
    public class DataSerializer
    {
        // Encryption settings (can be set externally)
        private byte[] _aesKey;
        private byte[] _aesIV;

        /// <summary>
        /// Private: Gets or sets a value indicating whether a super secure hash is used for the encryption key.
        /// </summary>
        private bool _superSecure { get; set; } = false;

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSerializer"/> class with the specified encryption settings.
        /// </summary>
        /// <param name="superSecure">Indicates whether a super secure hash is used.</param>
        /// <param name="aesKey">The AES encryption key as a byte array.</param>
        /// <param name="aesIV">The AES encryption initialization vector as a byte array.</param>
        public DataSerializer(bool superSecure, byte[] aesKey, byte[] aesIV) => (_superSecure, _aesKey, _aesIV) = (superSecure, aesKey, aesIV);

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSerializer"/> class using string representations of the key and IV.
        /// </summary>
        /// <param name="superSecure">Indicates whether a super secure hash is used.</param>
        /// <param name="aesKey">The AES encryption key as a string.</param>
        /// <param name="aesIV">The AES encryption initialization vector as a string.</param>
        public DataSerializer(bool superSecure, string aesKey, string aesIV)
            : this(superSecure, ComputeSHA2Hash(aesKey, superSecure), ComputeSHA2Hash(aesIV, false)) { }

        /// <summary>
        /// Initializes a new instance of the <see cref="DataSerializer"/> class using integer representations of the key and IV.
        /// </summary>
        /// <param name="superSecure">Indicates whether a super secure hash is used.</param>
        /// <param name="aesKey">The AES encryption key as an integer.</param>
        /// <param name="aesIV">The AES encryption initialization vector as an integer.</param>
        public DataSerializer(bool superSecure, int aesKey, int aesIV)
            : this(superSecure, ComputeSHA2Hash(aesKey.ToString(), superSecure), ComputeSHA2Hash(aesIV.ToString(), false)) { }

        /// <summary>
        /// Private: Encrypts the provided byte array using AES encryption with the configured key and IV.
        /// </summary>
        /// <param name="data">The data to encrypt.</param>
        /// <returns>The encrypted data as a byte array.</returns>
        private byte[] EncryptData(byte[] data)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = _aesKey;
                aesAlg.IV = _aesIV;
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, aesAlg.CreateEncryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(data, 0, data.Length);
                        cs.FlushFinalBlock();
                    }
                    return ms.ToArray();
                }
            }
        }

        /// <summary>
        /// Private: Decrypts the provided byte array using AES decryption with the configured key and IV.
        /// </summary>
        /// <param name="data">The encrypted data.</param>
        /// <returns>The decrypted data as a byte array.</returns>
        private byte[] DecryptData(byte[] data)
        {
            using (Aes aesAlg = Aes.Create())
            {
                aesAlg.Key = _aesKey;
                aesAlg.IV = _aesIV;
                using (MemoryStream ms = new MemoryStream())
                {
                    using (CryptoStream cs = new CryptoStream(ms, aesAlg.CreateDecryptor(), CryptoStreamMode.Write))
                    {
                        cs.Write(data, 0, data.Length);
                        cs.FlushFinalBlock();
                    }
                    return ms.ToArray();
                }
            }
        }

        /// <summary>
        /// Computes the SHA-256 hash of the given input string. Optionally reduces the hash to 16 bytes if superSecure is false.
        /// </summary>
        /// <param name="input">The input string to hash.</param>
        /// <param name="superSecure">If set to true, the full 32-byte hash is returned; otherwise, only the first 16 bytes are returned.</param>
        /// <returns>A byte array representing the computed hash.</returns>
        public static byte[] ComputeSHA2Hash(string input, bool superSecure)
        {
            using (SHA256 sha256 = SHA256.Create())
            {
                byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));

                if (!superSecure)
                {
                    // Return only the first 16 bytes (128 bits)
                    Array.Resize(ref hashBytes, 16);
                }

                return hashBytes;
            }
        }

        /// <summary>
        /// Serializes a list of objects to a specified file. Optionally encrypts the serialized data using AES.
        /// </summary>
        /// <typeparam name="T">The type of objects in the list.</typeparam>
        /// <param name="data">The list of objects to serialize.</param>
        /// <param name="filePath">The file path where the data will be written.</param>
        /// <param name="useEncryption">Determines whether to encrypt the data before writing to the file.</param>
        public void SerializeListData<T>(List<T> data, string filePath, bool useEncryption = true)
        {
            try
            {
                // Create a FileStream to write to the specified file.
                using (FileStream fs = new FileStream(filePath, FileMode.Create))
                {
                    byte[] dataToSerialize = JsonSerializer.SerializeToUtf8Bytes(data);

                    if (useEncryption)
                    {
                        // Encrypt data before writing to the file.
                        byte[] encryptedData = EncryptData(dataToSerialize);
                        fs.Write(encryptedData, 0, encryptedData.Length);
                        Console.WriteLine($"Data successfully encrypted and serialized to {filePath}");
                    }
                    else
                    {
                        // Write unencrypted data directly.
                        fs.Write(dataToSerialize, 0, dataToSerialize.Length);
                        Console.WriteLine($"Data successfully serialized to {filePath}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while serializing data: {ex.Message}");
            }
        }

        /// <summary>
        /// Deserializes a list of objects from a specified file. Optionally decrypts the data before deserialization.
        /// </summary>
        /// <typeparam name="T">The type of objects in the list.</typeparam>
        /// <param name="filePath">The file path from which the data will be read.</param>
        /// <param name="useEncryption">Determines whether to decrypt the data after reading from the file.</param>
        /// <returns>A list of deserialized objects, or null if an error occurs.</returns>
        public List<T> DeserializeListData<T>(string filePath, bool useEncryption = false)
        {
            try
            {
                // Create a FileStream to read from the specified file.
                using (FileStream fs = new FileStream(filePath, FileMode.Open))
                {
                    byte[] fileData = new byte[fs.Length];
                    fs.Read(fileData, 0, (int)fs.Length);

                    byte[] dataToDeserialize;

                    if (useEncryption)
                    {
                        // Decrypt data before deserialization.
                        dataToDeserialize = DecryptData(fileData);
                        Console.WriteLine($"Data successfully decrypted from {filePath}");
                    }
                    else
                    {
                        dataToDeserialize = fileData;
                        Console.WriteLine($"Data successfully deserialized from {filePath}");
                    }

                    // Deserialize the data from the byte array and return it.
                    List<T> data = JsonSerializer.Deserialize<List<T>>(dataToDeserialize);
                    return data;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while deserializing data: {ex.Message}");
                return null;
            }
        }

        /// <summary>
        /// Serializes any serializable object to a specified file. Optionally encrypts the serialized data using AES.
        /// </summary>
        /// <typeparam name="T">The type of the object to serialize.</typeparam>
        /// <param name="data">The object to serialize.</param>
        /// <param name="filePath">The file path where the data will be written.</param>
        /// <param name="useEncryption">Determines whether to encrypt the data before writing to the file.</param>
        public void SerializeData<T>(T data, string filePath, bool useEncryption = false)
        {
            try
            {
                // Create a FileStream to write to the specified file.
                using (FileStream fs = new FileStream(filePath, FileMode.Create))
                {
                    byte[] dataToSerialize = JsonSerializer.SerializeToUtf8Bytes(data);

                    if (useEncryption)
                    {
                        // Encrypt data before writing to the file.
                        byte[] encryptedData = EncryptData(dataToSerialize);
                        fs.Write(encryptedData, 0, encryptedData.Length);
                        Console.WriteLine($"Data successfully encrypted and serialized to {filePath}");
                    }
                    else
                    {
                        // Write unencrypted data directly.
                        fs.Write(dataToSerialize, 0, dataToSerialize.Length);
                        Console.WriteLine($"Data successfully serialized to {filePath}");
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while serializing data: {ex.Message}");
            }
        }

        /// <summary>
        /// Deserializes an object of type T from a specified file. Optionally decrypts the data before deserialization.
        /// </summary>
        /// <typeparam name="T">The type of the object to deserialize.</typeparam>
        /// <param name="filePath">The file path from which the data will be read.</param>
        /// <param name="useEncryption">Determines whether to decrypt the data after reading from the file.</param>
        /// <returns>The deserialized object, or the default value of T if an error occurs.</returns>
        public T DeserializeData<T>(string filePath, bool useEncryption = false)
        {
            try
            {
                // Create a FileStream to read from the specified file.
                using (FileStream fs = new FileStream(filePath, FileMode.Open))
                {
                    byte[] fileData = new byte[fs.Length];
                    fs.Read(fileData, 0, (int)fs.Length);

                    byte[] dataToDeserialize;

                    if (useEncryption)
                    {
                        // Decrypt data before deserialization.
                        dataToDeserialize = DecryptData(fileData);
                        Console.WriteLine($"Data successfully decrypted from {filePath}");
                    }
                    else
                    {
                        dataToDeserialize = fileData;
                        Console.WriteLine($"Data successfully deserialized from {filePath}");
                    }

                    // Deserialize the data from the byte array and return it.
                    T data = JsonSerializer.Deserialize<T>(dataToDeserialize);
                    return data;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while deserializing data: {ex.Message}");
                return default(T);
            }
        }

        /// <summary>
        /// Checks whether a file exists at the specified file path.
        /// </summary>
        /// <param name="filePath">The file path to check.</param>
        /// <returns>True if the file exists; otherwise, false.</returns>
        public bool DoesFileExist(string filePath)
        {
            return File.Exists(filePath);
        }

        /// <summary>
        /// Determines the operating system of the host and returns an integer identifier.
        /// </summary>
        /// <returns>
        /// 0 for Linux, 1 for Windows, 2 for macOS, 3 for FreeBSD, and 4 for other platforms.
        /// </returns>
        public int GetOperatingSystem()
        {
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
                return 2;  // macOS
            }
            else if (RuntimeInformation.IsOSPlatform(OSPlatform.FreeBSD))
            {
                return 3;  // FreeBSD
            }
            else
            {
                return 4;  // Other platforms
            }
        }
    }
}
