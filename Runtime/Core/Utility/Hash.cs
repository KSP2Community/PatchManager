using System;
using System.Buffers;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using Newtonsoft.Json;

namespace PatchManager.Core.Utility
{
    /// <summary>
    /// Hashing utility class.
    /// Implements FNV-1a-64 for faster hashing speed, rather than a crypto hash
    /// And for possible thread safety in the future
    /// </summary>
    public static class Hash
    {
        private const ulong FnvOffset = 14695981039346656037UL;
        private const ulong FnvPrime  = 1099511628211UL;

        /// <summary>
        /// Gets the FNV-1a-64 hash of the input string.
        /// </summary>
        /// <param name="input">Input string.</param>
        /// <returns>FNV-1a-64 hash of the input string.</returns>
        public static string FromString(string input)
        {
            var maxBytes = Encoding.UTF8.GetMaxByteCount(input.Length);
            var buf = ArrayPool<byte>.Shared.Rent(maxBytes);
            try
            {
                var n = Encoding.UTF8.GetBytes(input, 0, input.Length, buf, 0);
                return HashBytes(buf, 0, n).ToString("X16");
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buf);
            }
        }

        /// <summary>
        /// Gets the FNV-1a-64 hash of the input file.
        /// </summary>
        /// <param name="path">Path to the file.</param>
        /// <returns>FNV-1a-64 hash of the input file.</returns>
        public static string FromFile(string path)
        {
            var h = FnvOffset;
            using var stream = new FileStream(path, FileMode.Open, FileAccess.Read, FileShare.Read);
            var buf = ArrayPool<byte>.Shared.Rent(8192);
            try
            {
                int read;
                while ((read = stream.Read(buf, 0, buf.Length)) > 0)
                {
                    for (var i = 0; i < read; i++)
                    {
                        h ^= buf[i];
                        h *= FnvPrime;
                    }
                }
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buf);
            }
            return h.ToString("X16");
        }

        /// <summary>
        /// Gets the FNV-1a-64 hash of the input JSON-serializable object.
        /// </summary>
        /// <param name="input">JSON-serializable object.</param>
        /// <returns>FNV-1a-64 hash of the JSON-serializable object.</returns>
        public static string FromJsonObject(object input)
        {
            var json = JsonConvert.SerializeObject(input, Formatting.None);
            return FromString(json);
        }
        
        private static ulong HashBytes(byte[] buf, int offset, int count)
        {
            var h = FnvOffset;
            for (var i = offset; i < offset + count; i++)
            {
                h ^= buf[i];
                h *= FnvPrime;
            }
            return h;
        }
    }
}