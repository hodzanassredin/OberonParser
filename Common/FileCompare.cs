using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Common
{
    public class StringCompare
    {
        public static bool CompareFiles(string name, String name2)
        {
            using (var sr = new StreamReader(name))
            {
                using (var sr_out = new StreamReader(name2))
                {
                    var txt1 = sr.ReadToEnd();
                    var txt2 = sr_out.ReadToEnd();
                    return CompareStrings(txt1, txt2);
                }
            }
        }
        public static bool CompareStrings(String txt1, String txt2)
        {

            var i = 0; var j = 0;
            while (true)
            {
                var firstIsFinished = i == txt1.Length;
                var secondIsFinished = j == txt2.Length;
                if (firstIsFinished && secondIsFinished) break;
                else if (!firstIsFinished && (txt1[i] == '(' && txt1[i + 1] == '*')) i = SkipComment(txt1, i+2);
                else if (!secondIsFinished && (txt2[j] == '(' && txt2[j + 1] == '*')) j = SkipComment(txt2, j+2);
                else if (!firstIsFinished && Char.IsWhiteSpace(txt1[i]))
                {
                    i++;
                }
                else if (!secondIsFinished && Char.IsWhiteSpace(txt2[j]))
                {
                    j++;
                }
                else if (txt1[i] == ';')
                {
                    i++;
                }
                else if (txt2[j] == ';')
                {
                    j++;
                }
                else if (txt1[i] == txt2[j])
                {
                    i++;
                    j++;
                }
                else
                {
                    var istart = i - 100;
                    var jstart = j - 100;
                    if (istart < 0) istart = 0;
                    if (jstart < 0) jstart = 0;
                    Console.WriteLine($"Cant compare, found a diff. {Environment.NewLine} ORIGINAL: {Environment.NewLine} {txt1.Substring(istart,i-istart+10)} {Environment.NewLine} RESULT: {Environment.NewLine}  {txt2.Substring(jstart, j - jstart+10)} ");
                    return false;
                }
            }
            return true;
        }


        static int SkipComment(string str, int i)
        {
            while (true)
            {
                i++;
                if (i == str.Length) return i;

                if (str[i] == '(' && str[i + 1] == '*')
                {
                    i = SkipComment(str, i+2);
                }
                if (str[i] == '*' && str[i + 1] == ')')
                {
                    return i + 2;
                }
            }
        }
    }
}
