using System;
using System.Runtime.InteropServices;
using System.Text;
namespace SigningService.Extensions
{
    internal static class ByteArrayExt
    {
        public static void ReverseInplace(this byte[] bytes)
        {
            int i = 0, j = bytes.Length - 1;
            while (i < j)
            {
                byte c = bytes[i];
                bytes[i] = bytes[j];
                bytes[j] = c;
                i++;
                j--;
            }
        }

        public static byte[] Reverse(this byte[] bytes)
        {
            byte[] ret = new byte[bytes.Length];
            for (int i = 0, j = bytes.Length - 1; j >= 0; i++, j--)
            {
                ret[i] = bytes[j];
            }
            return ret;
        }

        public static bool IsEquivalentTo(this byte[] a, byte[] b)
        {
            if (a == b)
            {
                return true;
            }

            if (a == null || b == null)
            {
                // since they are not equal
                return false;
            }

            if (a.Length != b.Length)
            {
                return false;
            }

            for (int i = 0; i < a.Length; i++)
            {
                if (a[i] != b[i])
                {
                    return false;
                }
            }

            return true;
        }

        public static string ToHex(this byte[] t)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < t.Length; i++)
            {
                sb.Append(string.Format("{0:x2}", t[i]));
            }
            return sb.ToString();
        }

        // Dummy implementation
        // No need for anything faster
        public static byte[] FromHexToByteArray(this string hex)
        {
            byte[] ret = new byte[hex.Length / 2];
            for (int i = 0, j = 0; i < ret.Length; i++, j += 2)
            {
                string hexByte = hex.Substring(j, 2);
                ret[i] = Convert.ToByte(hexByte, 16);
            }
            return ret;
        }
    }
}