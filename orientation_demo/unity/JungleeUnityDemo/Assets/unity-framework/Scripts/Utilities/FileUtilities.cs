using System;
using System.Globalization;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Xml.Serialization;
using UnityEngine;
using XcelerateGames.AssetLoading;

namespace XcelerateGames
{
    /// <summary>
    /// Utility class to handle all File IO related functionality
    /// </summary>
    public static class FileUtilities
    {
        /// <summary>
        /// Serialize object to XML
        /// </summary>
        /// <param name="inData">Object to serialize</param>
        /// <returns>serialized xml string</returns>
        public static string Serialize(object inData)
        {
            StringWriter writer = new StringWriter(CultureInfo.InvariantCulture);
            XmlSerializer serializer = new XmlSerializer(inData.GetType());
            serializer.Serialize(writer, inData);
            return writer.ToString();
        }

        /// <summary>
        /// Deserialize xml string to given Object type
        /// </summary>
        /// <typeparam name="TYPE">Type to which object to deserialise to</typeparam>
        /// <param name="inData">xml string</param>
        /// <returns>Deserialized object of type T</returns>
        public static TYPE Deserialize<TYPE>(string inData)
        {
            using (StringReader sr = new StringReader(inData))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(TYPE));
                try
                {
                    XDebug.Log("Deserialize to String : " + inData);
                    return (TYPE)xmlSerializer.Deserialize(sr);
                }
                catch (System.Exception inException)
                {
                    XDebug.LogError("Deserialize error = " + inException.ToString() + " \n " + inData);
                    return default(TYPE);
                }
            }
        }

        public static TYPE DeserializeFromXml<TYPE>(string inData, string key, bool rethrow = false)
        {
            if (inData == null)
            {
                XDebug.LogError("Deserialize error inData is null!");
                return default(TYPE);
            }

            //make object from Deserializer
            using (StringReader sr = new StringReader(inData))
            {
                XmlSerializer xmlSerializer = new XmlSerializer(typeof(TYPE));
                try
                {
                    //Debug.Log("*****Deserialize from xml\n" + inData);
                    return (TYPE)xmlSerializer.Deserialize(sr);
                }
                catch (System.Exception inException)
                {
                    XDebug.LogError("Deserialize error = " + inException.ToString() + " \n " + inData);
                    //May be the xml in PlayerPrefs is corrupted, delete & hope it works on re-launch.
                    if (!string.IsNullOrEmpty(key))
                    {
                        //We will re-throw only if the xml was cached, else we wont.
                        rethrow = UnityEngine.PlayerPrefs.HasKey(key);
                        ResourceManager.RemoveFromCache(key);
                        UnityEngine.PlayerPrefs.DeleteKey(key);
                    }

                    if (rethrow)
                        throw;
                    else
                        return default(TYPE);
                }
            }
        }

        public class StringWriterUtf8 : StringWriter
        {
            public override Encoding Encoding
            {
                get { return Encoding.UTF8; }
            }
        }

        public static string SerializeToXMLWithUTF8(object inData)
        {
            StringWriterUtf8 writer = new StringWriterUtf8();
            XmlSerializer serializer = new XmlSerializer(inData.GetType());
            serializer.Serialize(writer, inData);
            string text = writer.ToString();
            return text;
        }

#if !UNITY_WEBPLAYER

        /// <summary>
        /// Returns MD5 for the given file if the file exists, else returns "NA"
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetMD5OfFile(string fileName)
        {
            if (!File.Exists(fileName))
                return "NA";
            return GetMD5OfText(File.ReadAllText(fileName));
        }

#endif //UNITY_WEBPLAYER

        /// <summary>
        /// Get file size in bytes
        /// </summary>
        /// <param name="filePath">path of the file</param>
        /// <returns>no of bytes</returns>
        public static ulong GetFileSize(string filePath)
        {
            return (ulong)(new FileInfo(filePath).Length);
        }

        /// <summary>
        /// Generate SHA256 of given string
        /// </summary>
        /// <param name="data"></param>
        /// <returns>SHA256 string</returns>
        public static string GenerateSHA256(string data)
        {
            byte[] fileBytes = Encoding.ASCII.GetBytes(data);
            StringBuilder sb = new StringBuilder();
            using (SHA256Managed sha256 = new SHA256Managed())
            {
                byte[] hash = sha256.ComputeHash(fileBytes);
                foreach (Byte b in hash)
                    sb.Append(b.ToString("X2"));
            }
            return sb.ToString();
        }

        /// <summary>
        /// Get MD5 of given string
        /// </summary>
        /// <param name="inText"></param>
        /// <returns>MD5</returns>
        public static string GetMD5OfText(string inText)
        {
            using (var md5 = MD5.Create())
            {
                return BitConverter.ToString(md5.ComputeHash(System.Text.Encoding.UTF8.GetBytes(inText))).Replace("-", "").ToLower();
            }
        }

        /// <summary>
        /// Get MD5 of given bytes
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns>MD5</returns>
        public static string GetMD5OfBytes(byte[] bytes)
        {
            using (var md5 = MD5.Create())
            {
                return BitConverter.ToString(md5.ComputeHash(bytes)).Replace("-", "").ToLower();
            }
        }

        /// <summary>
        /// Write contents to file
        /// </summary>
        /// <param name="filePath"></param>
        /// <param name="data"></param>
        public static void WriteToFile(string filePath, string data)
        {
            string folder = Path.GetDirectoryName(filePath);
            if (!string.IsNullOrEmpty(folder) && !Directory.Exists(folder))
                Directory.CreateDirectory(folder);
            File.WriteAllText(filePath, data);
        }

        /// <summary>
        /// Write bytes to the given path with specified file name 
        /// </summary>
        /// <param name="fileName">File name</param>
        /// <param name="data">contents</param>
        /// <param name="fullPath">File path</param>
        public static void WriteToFile(string fileName, byte[] data, string fullPath)
        {
            try
            {
                Debug.Log($"fileName:{fileName}, fullPath:{fullPath}");
                fullPath += "/" + fileName;
                string folder = Path.GetDirectoryName(fullPath);
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);
                File.WriteAllBytes(fullPath, data);
            }
            catch (Exception e)
            {
                XDebug.LogException(e);
            }
        }

        /// <summary>
        /// Returns a file name that has time stamp added to the given input.
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetTimeStamp(string fileName)
        {
            string extn = Path.GetExtension(fileName);
            fileName = Path.GetFileNameWithoutExtension(fileName);
            fileName += "-" + DateTime.Now.Day + "-" + DateTime.Now.Month + "-" + DateTime.Now.Year + "(" + DateTime.Now.Hour + "-" + DateTime.Now.Minute + ")" + extn;
            return fileName;
        }

        /// <summary>
        /// Serialize an object & write as xml file
        /// </summary>
        /// <param name="inData">Object to serialize</param>
        /// <param name="fileName">Filename</param>
        public static void WriteToXMLFile(object inData, string fileName)
        {
            fileName = PlatformUtilities.GetPersistentDataPath() + "/" + fileName;
            File.WriteAllText(fileName, Serialize(inData));
        }

        public static void WriteToFile(string fileName, byte[] data)
        {
            try
            {
                string fullPath = (PlatformUtilities.GetPersistentDataPath() + Path.DirectorySeparatorChar + fileName).Replace('/', Path.DirectorySeparatorChar);
                string folder = Path.GetDirectoryName(fullPath);
                if (!Directory.Exists(folder))
                    Directory.CreateDirectory(folder);
                File.WriteAllBytes(fullPath, data);
            }
            catch (Exception e)
            {
                XDebug.LogException(e);
            }
        }

        public static string ReadFromFile(string fileName)
        {
            fileName = PlatformUtilities.GetPersistentDataPath() + Path.DirectorySeparatorChar + fileName;
            return File.ReadAllText(fileName);
        }

        public static bool FileExists(string fileName)
        {
            fileName = PlatformUtilities.GetPersistentDataPath() + "/" + fileName;
            return File.Exists(fileName);
        }

        public static void Delete(string fileName)
        {
            fileName = PlatformUtilities.GetPersistentDataPath() + "/" + fileName;
            File.Delete(fileName);
        }

        public static byte[] ReadBytes(string fileName)
        {
            if (FileExists(fileName))
            {
                fileName = PlatformUtilities.GetPersistentDataPath() + "/" + fileName;
                using (BinaryReader sr = new BinaryReader(File.OpenRead(fileName)))
                {
                    byte[] buffer = sr.ReadBytes((int)(sr.BaseStream.Length));
                    sr.Close();
                    return buffer;
                }
            }
            if(File.Exists(fileName))
            {
                using (BinaryReader sr = new BinaryReader(File.OpenRead(fileName)))
                {
                    byte[] buffer = sr.ReadBytes((int)(sr.BaseStream.Length));
                    sr.Close();
                    return buffer;
                }
            }
            Debug.LogError($"File does not exist: {fileName}");
            return null;
        }

        /// <summary>
        /// Create directories recursively.
        /// </summary>
        /// <param name="path"></param>
        public static void CreateDirectoryRecursively(string path)
        {
            Directory.CreateDirectory(path);
        }

        public static int CleanDirectory(string path, string searchPattern = "*.*")
        {
            if (!Directory.Exists(path))
                return 0;
            string[] files = Directory.GetFiles(path, searchPattern, SearchOption.AllDirectories);
            foreach (string file in files)
            {
                File.Delete(file);
            }
            return files.Length;
        }

        public static bool IsDirectory(string inPath)
        {
            FileAttributes attr = File.GetAttributes(inPath);
            if ((attr & FileAttributes.Directory) == FileAttributes.Directory)
                return true;
            return false; ;
        }

        public static void DeleteDirectory(string dirName)
        {
            if (Directory.Exists(dirName))
                Directory.Delete(dirName, true);
        }

        public static string Encode(string plainText)
        {
            var plainTextBytes = System.Text.Encoding.UTF8.GetBytes(plainText);
            return System.Convert.ToBase64String(plainTextBytes);
        }

        public static string Decode(string base64EncodedData)
        {
            var base64EncodedBytes = System.Convert.FromBase64String(base64EncodedData);
            return System.Text.Encoding.UTF8.GetString(base64EncodedBytes);
        }

        private const string mKey = "K0s!Gam3zS0kk3rL3g3nD";

        public static string EncryptOrDecrypt(string text)
        {
            var result = new StringBuilder();

            for (int c = 0; c < text.Length; c++)
                result.Append((char)((uint)text[c] ^ (uint)mKey[c % mKey.Length]));

            return result.ToString();
        }

        public static string EncryptAndEncode(string text)
        {
            return Encode(EncryptOrDecrypt(text));
        }

        public static string DecryptAndDecode(string text)
        {
            text = text.Replace("\"", "");
            return EncryptOrDecrypt(Decode(text));
        }

        //public static void AddToCache(string fileName, WWW obj)
        //{
        //    if (obj == null)
        //        return;
        //    XDebug.Log("Adding " + fileName + " to cache.", XDebug.Mask.Resources);
        //    if (ResourceManager.IsTextAsset(fileName))
        //    {
        //        //We will cache these files in player prefs. Key will be file name.
        //        XDebug.Log("loaded & cached " + fileName + " to PlayerPrefs.", XDebug.Mask.Resources | XDebug.Mask.UpdateManager);
        //        string txt = obj.text;
        //        //if (fileName.EndsWith(ResourceManager.mCompressedFileExtension))
        //        //    txt = StringCompression.Decompress(obj.bytes);
        //        UnityEngine.PlayerPrefs.SetString(fileName, txt);
        //        ResourceManager.AddToCache(fileName, ResourceManager.GetVersionAssetHash(fileName));
        //    }
        //    else
        //    {
        //        WriteToFile(fileName, obj.bytes);
        //        ResourceManager.AddToCache(fileName, ResourceManager.GetVersionAssetHash(fileName));
        //        XDebug.Log("loaded & cached : " + fileName, XDebug.Mask.Resources | XDebug.Mask.UpdateManager);
        //    }
        //}

        public static void AddToCache(string fileName, byte[] data)
        {
            XDebug.Log("Adding " + fileName + " to cache.", XDebug.Mask.Resources);
            WriteToFile(fileName, data);
            ResourceManager.AddToCache(fileName, ResourceManager.GetVersionAssetHash(fileName));
            XDebug.Log("loaded & cached : " + fileName, XDebug.Mask.Resources | XDebug.Mask.UpdateManager);
        }

        public static void AddToCache(string fileName, string text)
        {
            XDebug.Log("Adding " + fileName + " to cache.", XDebug.Mask.Resources);
            //We will cache these files in player prefs. Key will be file name.
            XDebug.Log("loaded & cached " + fileName + " to PlayerPrefs.", XDebug.Mask.Resources | XDebug.Mask.UpdateManager);
            UnityEngine.PlayerPrefs.SetString(fileName, text);
            ResourceManager.AddToCache(fileName, ResourceManager.GetVersionAssetHash(fileName));
        }

        //Returns true if the given file has BOM encoding
        public static bool HasBOM(string inFile)
        {
            bool hasBOM = false;
            using (FileStream fs = new FileStream(inFile, FileMode.Open))
            {
                byte[] bits = new byte[3];
                fs.Read(bits, 0, 3);

                // UTF8 byte order mark is: 0xEF,0xBB,0xBF
                if (bits[0] == 0xEF && bits[1] == 0xBB && bits[2] == 0xBF)
                    hasBOM = true;
            }
            return hasBOM;
        }

        /// <summary>
        /// Returns extension of the file without the leading "."
        /// @example if input if testFile.txt returns txt
        /// </summary>
        /// <param name="fileName"></param>
        /// <returns></returns>
        public static string GetExtension(string fileName)
        {
            return Path.GetExtension(fileName).Replace(".", "");
        }

        ////Returns true if the given string has BOM encoding
        //public static bool HasBOM_string(string inString)
        //{
        //    bool hasBOM = false;
        //    MemoryStream fs = GenerateStreamFromString(inString);
        //    byte[] bits = new byte[3];
        //    fs.Read(bits, 0, 3);

        //    // UTF8 byte order mark is: 0xEF,0xBB,0xBF
        //    if (bits[0] == 0xEF && bits[1] == 0xBB && bits[2] == 0xBF)
        //        hasBOM = true;
        //    return hasBOM;
        //}

        //public static MemoryStream GenerateStreamFromString(string value)
        //{
        //    return new MemoryStream(Encoding.UTF8.GetBytes(value ?? ""));
        //}
    }
}
