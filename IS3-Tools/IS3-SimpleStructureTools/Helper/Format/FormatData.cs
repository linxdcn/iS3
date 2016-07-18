using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace IS3.SimpleStructureTools.Helper.Format
{
    public class FormatData
    {
        public static double ToNumber(string text)
        {
            text.Trim();
            text.EndsWith("");
            string[] result = text.Split(new Char[] { ' ' });
            double d = 0;
            try
            {
                d = double.Parse(result[0]);
            }
            catch (FormatException)
            {
                MessageBox.Show("Input error", "error", MessageBoxButtons.OK);
                d = 0;
            }
            return d;
        }

        public static string SpaceToComma(string str)
        {
            //transfer muti-space to single comma
            string[] ss = str.Split();
            string Result = "";
            for (int i = 0; i < ss.Length; i++)
            {
                if (ss[i] != "")
                {
                    Result += ss[i].ToString().Trim() + ",";
                }
            }
            return Result;
        }

        public static double ScientificToDouble(string str)
        {
            string[] ss = str.Split('E');
            double b = double.Parse(ss[0]);
            double i = 0;
            if (ss.Count() > 1)
                i = double.Parse(ss[1]);
            double res = b * Math.Pow(10, i);
            return res;
        }
    }
}
