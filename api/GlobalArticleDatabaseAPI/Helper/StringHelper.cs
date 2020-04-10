using System.Text;
using System.Text.RegularExpressions;

namespace GlobalArticleDatabaseAPI.Helpers
{
    public static class StringHelper
    {
        /// <summary>
        /// Normalize string to lowercase removing special characters and replacing '.' by '_'
        /// </summary>
        /// <param name="str">Input string</param>
        /// <returns>Normalized string</returns>
        public static string NormalizeCharacters(this string str)
        {
            str = str.ToLower();
            StringBuilder sb = new StringBuilder();
            foreach (char c in str)
            {
                if ((c >= '0' && c <= '9') || (c >= 'a' && c <= 'z') || c == '-' || c == '_')
                {
                    sb.Append(c);
                }
                else if (c == '.')
                {
                    sb.Append('_');
                }
            }
            return sb.ToString();
        }

        /// <summary>
        /// Check whether it is a base 64 string
        /// </summary>
        /// <param name="s">base64 string</param>
        /// <returns>true if it is a base 64 string</returns>
        public static bool IsBase64Image(this string s)
        {
            if (string.IsNullOrEmpty(s)) return false;

            s = s.Trim();

            return Regex.IsMatch(s, @"^data:image\/(jpg|jpeg|png);base64,[a-zA-Z0-9\+\/]*={0,3}$", RegexOptions.None);
        }
    }
}
