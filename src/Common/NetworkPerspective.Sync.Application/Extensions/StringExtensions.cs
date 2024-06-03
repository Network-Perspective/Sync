﻿using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace NetworkPerspective.Sync.Application.Extensions
{
    public static class StringExtensions
    {
        /// <summary>
        /// https://stackoverflow.com/a/36845864
        /// </summary>
        public static int GetStableHashCode(this string str)
        {
            unchecked
            {
                int hash1 = 5381;
                int hash2 = hash1;

                for (int i = 0; i < str.Length && str[i] != '\0'; i += 2)
                {
                    hash1 = ((hash1 << 5) + hash1) ^ str[i];
                    if (i == str.Length - 1 || str[i + 1] == '\0')
                        break;
                    hash2 = ((hash2 << 5) + hash2) ^ str[i + 1];
                }

                return hash1 + (hash2 * 1566083941);
            }
        }

        public static string GetMd5HashCode(this string str)
        {
            var inputBytes = Encoding.Unicode.GetBytes(str);
            var outputBytes = MD5.HashData(inputBytes);
            var outputStrings = outputBytes.Select(x => x.ToString("x2"));
            return string.Join(string.Empty, outputStrings);
        }
    }
}