using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace CsLib
{
    public class SpecialChar
    {
        public static bool IsContainsSpecialChar(string str)
        {
            // - 허용
            string replaced = Regex.Replace(str, @"[^0-9a-zA-Z\-]{1,10}", "", RegexOptions.Singleline);
            var result = !str.Equals(replaced) ? true : false;
            return result;
        }

        public static bool IsContainUpperChar(string str)
        {
            string replaced = Regex.Replace(str, @"[^A-Z]{1,10}", "", RegexOptions.Singleline);
            var result = replaced.Length > 0 ? true : false;
            return result;
        }

        public static bool IsContainLowerChar(string str)
        {
            string replaced = Regex.Replace(str, @"[^a-z]{1,10}", "", RegexOptions.Singleline);
            var result = replaced.Length > 0 ? true : false;
            return result;
        }

        public static bool IsContainNumChar(string str)
        {
            string replaced = Regex.Replace(str, @"[^0-9]{1,10}", "", RegexOptions.Singleline);
            var result = replaced.Length > 0 ? true : false;
            return result;
        }
    }
}
