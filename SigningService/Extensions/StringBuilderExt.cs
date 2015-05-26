using System.Text;

namespace SigningService.Extensions
{
    internal static class StringBuilderExt
    {
        public static void AppendLine(this StringBuilder sb, string format, params object[] args)
        {
            sb.AppendLine(string.Format(format, args));
        }
    }
}