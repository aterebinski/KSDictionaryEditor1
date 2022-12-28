using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KSDictionaryEditor
{
    class AsciiConverter
    {
        public static string HEX2ASCII(string hex)
        {
            string res = String.Empty;
            for (int a = 0; a < hex.Length; a = a + 2)
            {
                string Char2Convert = hex.Substring(a, 2);
                int n = Convert.ToInt32(Char2Convert, 16);
                char c = (char)n;
                res += c.ToString();
            }
            return res;
        }

        public static string ASCIITOHex(string ascii)
        {
            StringBuilder sb = new StringBuilder();
            byte[] inputBytes = Encoding.UTF8.GetBytes(ascii);
            foreach (byte b in inputBytes)
            {
                sb.Append(string.Format("{0:x2}", b));
            }
            return sb.ToString();
        }
    }
}
