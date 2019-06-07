using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;

namespace MBCore.Model
{
    public static class HashFileGenerator
    {
        public const string MD5_TYPE = "md5";
        public const string SHA1_TYPE = "sha1";
        public const string SHA256_TYPE = "sha256";

        public static string hash(string hashType, string fileName)
        {
            HashAlgorithm hash;
            switch (hashType)
            {
                case MD5_TYPE:
                    hash = MD5.Create();
                    break;
                case SHA1_TYPE:
                    hash = SHA1.Create();
                    break;
                case SHA256_TYPE:
                    hash = SHA256.Create();
                    break;
                default:
                    throw new Exception("Unknow type of hash");
            }

            // We declare a variable to be an array of bytes
            byte[] hashValue;
            // We create a FileStream for the file passed as a parameter
            var fileStream = File.OpenRead(fileName);
            // We position the cursor at the beginning of stream
            fileStream.Position = 0;
            // We calculate the hash of the file
            hashValue = hash.ComputeHash(fileStream);
            // The array of bytes is converted into hexadecimal before it can be read easily
            var hashHex = "";
            hashValue.ToList().ForEach(b => hashHex += b.ToString("X2"));

            fileStream.Close();

            return hashHex.ToLower();
        }

        public static string md5(string fileName)
        {
            return hash(MD5_TYPE, fileName);
        }
        public static string sha1(string fileName)
        {
            return hash(SHA1_TYPE, fileName);
        }
        public static string sha256(string fileName)
        {
            return hash(SHA256_TYPE, fileName);
        }
    }
}
