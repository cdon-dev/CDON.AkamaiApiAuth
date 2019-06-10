using System;
using System.IO;
using System.Security.Cryptography;
using System.Text;

namespace CDON.AkamaiApiAuth
{
    internal static class Extensions
    {
        internal static string ToIso8601(this DateTime timestamp)
        {
            return timestamp.ToUniversalTime().ToString("yyyyMMdd'T'HH:mm:ss+0000");
        }

        internal static byte[] ToByteArray(this string data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));
            return Encoding.UTF8.GetBytes(data);
        }

        internal static byte[] ComputeHash(this Stream stream, string hashType, int? maxBodySize = null)
        {
            if (hashType == null) throw new ArgumentNullException(nameof(hashType));

            if (stream == null) return Array.Empty<byte>();

            using (var algorithm = HashAlgorithm.Create(hashType))
            {
                return maxBodySize.HasValue && maxBodySize > 0
                    ? algorithm.ComputeHash(stream.ReadExactly(maxBodySize.Value))
                    : algorithm.ComputeHash(stream);
            }
        }

        internal static byte[] ComputeKeyedHash(this byte[] data, string key, string hashType)
        {
            if (key == null) throw new ArgumentNullException(nameof(key));
            if (hashType == null) throw new ArgumentNullException(nameof(hashType));

            if (data == null) return Array.Empty<byte>();

            using (var algorithm = HMAC.Create(hashType))
            {
                algorithm.Key = key.ToByteArray();
                return algorithm.ComputeHash(data);
            }
        }

        internal static byte[] ReadExactly(this Stream stream, int maxCount)
        {
            using (var result = new MemoryStream())
            {
                var buffer = new byte[1024 * 1024];
                int bytesRead;
                var leftToRead = maxCount;

                while ((bytesRead = stream.Read(buffer, 0, leftToRead)) != 0)
                {
                    leftToRead -= bytesRead;
                    result.Write(buffer, 0, bytesRead);
                }

                return result.ToArray();
            }
        }

        internal static string ToBase64(this byte[] data)
        {
            if (data == null) throw new ArgumentNullException(nameof(data));

            return Convert.ToBase64String(data);
        }
    }
}