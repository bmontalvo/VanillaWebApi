using System;
using System.Text;

namespace VanillaWebApi.Helpers
{
    public static class StringExtension
    {
        public static string Base64Encode(this string str)
        {
            return Convert.ToBase64String(Encoding.ASCII.GetBytes(str));
        }

        public static string Base64Decode(this string str)
        {
            return Encoding.Default.GetString(Convert.FromBase64String(str));
        }
    }
}