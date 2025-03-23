using System.Runtime.InteropServices;
using System.Security.Cryptography;
using System.Text;
using System.Diagnostics;
using System.Text.Json;

namespace DataSerializer
{
    // Author: Alvar  
    // Written on: 2025-03-19 21:53  
    // Last Modified: 2025-03-23 19:51
    // Version: 1.2.2
    /// <summary>
    /// The DataSerializer class provides methods for serializing and deserializing objects,
    /// with optional AES encryption. It supports pretty-printing JSON output and cross-platform
    /// file path handling for writing data to a writable location.
    /// </summary>
    public class DataSerializer
    {
        // AES encryption key and IV used for encrypting/decrypting data.
        private readonly byte[] _aesKey;
        private readonly byte[] _aesIV;

        // Determines if the serialized data should be encrypted.
        private readonly bool _useEncryption;

        // Determines if the JSON output should be formatted in a human-readable (pretty-printed) style.
        private readonly bool _prettyPrint;

        /// <summary>
        /// A list of log messages and exceptions that occurred during serialization or deserialization.
        /// This requires Tuple support in C# 7.0 or later, or a custom class for the same purpose.
        /// </summary>
        public readonly List<(string? message, Exception? ex)> Log = new List<(string? message, Exception? ex)>();

        #region Constructors

        #region Encryption Constructors

        /// <summary>
        /// Initializes a new instance of the DataSerializer class with the specified encryption settings.
        /// </summary>
        /// <param name="prettyPrint">If true, JSON output will be indented (pretty printed).</param>
        /// <param name="aesKey">The AES encryption key as a byte array (32 Bytes = AES128, 16 Bytes = AES256).</param>
        /// <param name="aesIV">The AES encryption initialization vector as a byte array.</param>
        public DataSerializer(bool prettyPrint, byte[] aesKey, byte[] aesIV)
        {
            _useEncryption = true;
            _prettyPrint = prettyPrint;
            _aesKey = aesKey;
            _aesIV = aesIV;
        }

        /// <summary>
        /// Initializes a new instance of the DataSerializer class using encryption with string representations of the key and IV.
        /// </summary>
        /// <param name="superSecure">If true, a full 32-byte hash is used; otherwise, only the first 16 bytes are used.</param>
        /// <param name="prettyPrint">If true, JSON output will be indented (pretty printed).</param>
        /// <param name="aesKey">The AES encryption key as a string.</param>
        /// <param name="aesIV">The AES encryption initialization vector as a string.</param>
        public DataSerializer(bool superSecure, bool prettyPrint, string aesKey, string aesIV)
            : this(prettyPrint, ComputeSHA2Hash(aesKey, superSecure), ComputeSHA2Hash(aesIV, false))
        {
            _useEncryption = true;
        }

        /// <summary>
        /// Initializes a new instance of the DataSerializer class using encryption with integer representations of the key and IV.
        /// </summary>
        /// <param name="superSecure">If true, a full 32-byte hash is used; otherwise, only the first 16 bytes are used.</param>
        /// <param name="prettyPrint">If true, JSON output will be indented (pretty printed).</param>
        /// <param name="aesKey">The AES encryption key as an integer.</param>
        /// <param name="aesIV">The AES encryption initialization vector as an integer.</param>
        public DataSerializer(bool superSecure, bool prettyPrint, int aesKey, int aesIV)
            : this(prettyPrint, ComputeSHA2Hash(aesKey.ToString(), superSecure), ComputeSHA2Hash(aesIV.ToString(), false))
        {
            _useEncryption = true;
        }

        #endregion Encryption Constructors

        #region Non-Encryption Constructors

        /// <summary>
        /// Initializes a new instance of the DataSerializer class without encryption.
        /// </summary>
        /// <param name="prettyPrint">If true, JSON output will be indented (pretty printed).</param>
        public DataSerializer(bool prettyPrint) : this(prettyPrint, new byte[0], new byte[0])
        {
            _useEncryption = false;
        }

        #endregion Non-Encryption Constructors

        #endregion Constructors

        #region Encryption/Decryption Methods

        /// <summary>
        /// Encrypts the given byte array using AES encryption with the configured key and IV.
        /// </summary>
        /// <param name="data">The plaintext data to encrypt.</param>
        /// <returns>The encrypted data as a byte array.</returns>
        private byte[] EncryptData(byte[] data)
        {
            using Aes aesAlg = Aes.Create();
            aesAlg.Key = _aesKey;
            aesAlg.IV = _aesIV;
            using MemoryStream ms = new();
            using CryptoStream cs = new(ms, aesAlg.CreateEncryptor(), CryptoStreamMode.Write);
            cs.Write(data, 0, data.Length);
            cs.FlushFinalBlock();
            return ms.ToArray();
        }

        /// <summary>
        /// Decrypts the given byte array using AES decryption with the configured key and IV.
        /// </summary>
        /// <param name="data">The encrypted data to decrypt.</param>
        /// <returns>The decrypted data as a byte array.</returns>
        private byte[] DecryptData(byte[] data)
        {
            using Aes aesAlg = Aes.Create();
            aesAlg.Key = _aesKey;
            aesAlg.IV = _aesIV;
            using MemoryStream ms = new();
            using CryptoStream cs = new(ms, aesAlg.CreateDecryptor(), CryptoStreamMode.Write);
            cs.Write(data, 0, data.Length);
            cs.FlushFinalBlock();
            return ms.ToArray();
        }

        #endregion Encryption/Decryption Methods

        #region Hashing Method

        /// <summary>
        /// Computes the SHA-256 hash of the specified input string.
        /// If superSecure is false, the hash is truncated to the first 16 bytes.
        /// </summary>
        /// <param name="input">The input string to hash.</param>
        /// <param name="superSecure">Determines whether to use the full 32-byte hash.</param>
        /// <returns>A byte array representing the computed hash.</returns>
        private static byte[] ComputeSHA2Hash(string input, bool superSecure)
        {
            using SHA256 sha256 = SHA256.Create();
            byte[] hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
            if (!superSecure)
            {
                // Truncate to 16 bytes (128 bits) if not in super secure mode.
                Array.Resize(ref hashBytes, 16);
            }
            return hashBytes;
        }

        #endregion Hashing Method

        #region Serialization Methods

        /// <summary>
        /// Serializes the specified object to a file at the given path, optionally encrypting the data.
        /// The JSON output can be formatted (pretty-printed) based on the configuration.
        /// </summary>
        /// <typeparam name="T">The type of the object to serialize.</typeparam>
        /// <param name="data">The object to serialize.</param>
        /// <param name="filePath">The relative file path where the data will be saved.</param>
        /// <param name="useEncryption">If true, the serialized data will be encrypted before saving.</param>
        /// <param name="success">
        /// Out parameter set to true if serialization was successful; otherwise false.
        /// </param>
        public void SerializeData<T>(T data, string filePath, out bool success)
        {
            // Ensure the directory structure exists before attempting to write the file.
            EnsureDirectoryExists(filePath);

            try
            {
                using FileStream fs = new(filePath, FileMode.Create);
                // Set JSON serializer options to enable pretty-printing if configured.
                var options = new JsonSerializerOptions { WriteIndented = _prettyPrint };
                byte[] dataToSerialize = JsonSerializer.SerializeToUtf8Bytes(data, options);

                if (_useEncryption)
                {
                    // Encrypt the data and write the encrypted bytes.
                    byte[] encryptedData = EncryptData(dataToSerialize);
                    fs.Write(encryptedData, 0, encryptedData.Length);
                    Log.Add(($"Data successfully encrypted and serialized to {filePath}", null));
                }
                else
                {
                    // Write the unencrypted data directly.
                    fs.Write(dataToSerialize, 0, dataToSerialize.Length);
                    Log.Add(($"Data successfully serialized to {filePath}", null));
                }

                success = true;
            }
            catch (Exception ex)
            {
                Log.Add(($"An error occurred while serializing data to {filePath}", ex));
                success = false;
            }
        }

        /// <summary>
        /// Overload of SerializeData that does not require an out bool parameter.
        /// </summary>
        /// <typeparam name="T">The type of the object to serialize.</typeparam>
        /// <param name="data">The object to serialize.</param>
        /// <param name="filePath">The relative file path where the data will be saved.</param>
        /// <param name="useEncryption">If true, the serialized data will be encrypted before saving.</param>
        public void SerializeData<T>(T data, string filePath)
        {
            // Call the overload that returns an out bool and ignore the flag.
            SerializeData(data, filePath, out bool _);
        }

        /// <summary>
        /// Deserializes an object of type T from the specified file, optionally decrypting the data.
        /// </summary>
        /// <typeparam name="T">The type of the object to deserialize.</typeparam>
        /// <param name="filePath">The relative file path from which the data will be read.</param>
        /// <param name="useEncryption">If true, the data will be decrypted after reading from the file.</param>
        /// <param name="success">
        /// Out parameter set to true if deserialization was successful; otherwise false.
        /// </param>
        /// <returns>The deserialized object, or the default value of T if an error occurs.</returns>
        public T? DeserializeData<T>(string filePath, out bool success)
        {
            try
            {
                using FileStream fs = new(filePath, FileMode.Open);
                byte[] fileData = new byte[fs.Length];
                fs.Read(fileData, 0, (int)fs.Length);

                byte[] dataToDeserialize = _useEncryption ? DecryptData(fileData) : fileData;
                Log.Add((_useEncryption ? $"Data successfully decrypted from {filePath}" : $"Data successfully deserialized from {filePath}", null));

                success = true;
                return JsonSerializer.Deserialize<T>(dataToDeserialize);
            }
            catch (Exception ex)
            {
                Log.Add(($"An error occurred while deserializing data from {filePath}", ex));
                success = false;
                return default;
            }
        }

        /// <summary>
        /// Overload of DeserializeData that does not require an out bool parameter.
        /// </summary>
        /// <typeparam name="T">The type of the object to deserialize.</typeparam>
        /// <param name="filePath">The relative file path from which the data will be read.</param>
        /// <param name="useEncryption">If true, the data will be decrypted after reading from the file.</param>
        /// <returns>The deserialized object, or the default value of T if an error occurs.</returns>
        public T? DeserializeData<T>(string filePath)
        {
            // Call the overload that returns an out bool and ignore the flag.
            return DeserializeData<T>(filePath, out bool _);
        }

        #endregion Serialization Methods

        #region File and Directory Utilities

        /// <summary>
        /// Checks whether a file exists at the specified path.
        /// </summary>
        /// <param name="filePath">The relative file path to check.</param>
        /// <returns>True if the file exists; otherwise, false.</returns>
        public bool DoesFileExist(string filePath)
        {
            return File.Exists(GetWritableAbsolutePath(filePath));
        }

        /// <summary>
        /// Ensures that the directory structure for the specified file path exists.
        /// If any directories in the path are missing, they will be created.
        /// </summary>
        /// <param name="filePath">The absolute file path for which to ensure directory existence.</param>
        public void EnsureDirectoryExists(string filePath)
        {
            try
            {
                string? directoryPath = Path.GetDirectoryName(filePath);
                if (!string.IsNullOrEmpty(directoryPath) && !Directory.Exists(directoryPath))
                {
                    Directory.CreateDirectory(directoryPath);
                    Log.Add(($"Created missing directories: {filePath}", null));
                }
            }
            catch (Exception ex)
            {
                Log.Add(($"An error occurred while ensuring directory existence for {filePath}", ex));
            }
        }

        /// <summary>
        /// Returns an absolute file path that is writable, based on the current operating system.
        /// If 'useCurrent' is true and running on a desktop OS, the path is based on the executable's directory.
        /// Otherwise, a user-writable directory (e.g., LocalApplicationData) is used.
        /// </summary>
        /// <param name="filePath">A relative file path.</param>
        /// <param name="useCurrent">If true, use the current directory (for desktop OS); otherwise, use a user-writable directory.</param>
        /// <returns>A fully qualified, cross-platform absolute file path.</returns>
        public string GetWritableAbsolutePath(string filePath = "", bool useCurrent = false)
        {
            if (useCurrent && IsDesktopOS())
            {
                return Path.Combine(AppDomain.CurrentDomain.BaseDirectory, filePath);
            }
            else
            {
                return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData), filePath);
            }
        }

        /// <summary>
        /// Determines if the current operating system is a desktop OS (Windows, Linux, macOS, or FreeBSD).
        /// </summary>
        /// <returns>True if running on a desktop OS; otherwise, false.</returns>
        public bool IsDesktopOS()
        {
            return GetOperatingSystem() < 4;
        }

        /// <summary>
        /// Determines the operating system of the host and returns an integer identifier:
        /// 0 for Linux, 1 for Windows, 2 for macOS, 3 for FreeBSD, and 4 for other platforms.
        /// </summary>
        /// <returns>An integer representing the operating system.</returns>
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

        #endregion File and Directory Utilities
    }
}
