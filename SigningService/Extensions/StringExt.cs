using System;
using System.Text;

namespace SigningService.Extensions
{
    internal static class StringExt
    {
        // Workaround for https://github.com/dotnet/corefx/issues/1805
        public static string RemoveSpecialCharacters(this string s, bool substituteSpecialCharacters = false)
        {
            StringBuilder sb = new StringBuilder();
            for (int i = 0; i < s.Length; i++)
            {
                char c = s[i];
                if ((int)c >= 32 && (int)c <= 127)
                {
                    sb.Append(c);
                }
                else
                {
                    if (substituteSpecialCharacters)
                    {
                        sb.Append("&#");
                        sb.Append(((int)c).ToString("x"));
                        sb.Append(";");
                    }
                }
            }

            return sb.ToString();
        }
    }
}