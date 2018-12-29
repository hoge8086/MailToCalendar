using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Text.RegularExpressions;

namespace AppointmentParser
{
    class Utils
    {
        //テキストから数値の配列を取得する関数
        static public int[] ParseNumbers(string textOfNumbers)
        {
            Regex regexp = new Regex(@"[0-9]+", RegexOptions.IgnoreCase | RegexOptions.Singleline);
            MatchCollection mc = regexp.Matches(textOfNumbers);

            List<int> numbers = new List<int>();
            foreach (Match m in mc)
            {
                try
                {
                    int num = Int32.Parse(m.Value);
                    numbers.Add(num);
                }
                catch(Exception)
                {
                    return new int[0];
                }
            }

            return numbers.ToArray();
        }
    }
}
