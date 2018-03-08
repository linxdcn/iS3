using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

namespace IS3.SimpleStructureTools.Helper.ColorTools
{
    public class GradeColor
    {
        public static Color GetTSIColor(double value)
        {
            Color color = new Color();

            if (value <= 1.5)
                color = Color.FromArgb(255,0,255,0);
            else if (value <= 2.5)
                color = Color.FromArgb(255, 0, 0, 255);
            else if (value <= 3.5)
                color = Color.FromArgb(255, 255, 255, 0);
            else if (value <= 4.5)
                color = Color.FromArgb(255, 255, 127, 0);
            else
                color = Color.FromArgb(255, 255, 0, 0);

            return color;
        }

        public static Color GetFEMColor(double max, double min, double x)
        {
            Color result = Colors.Black;
            double n = (max - min) / 9.0;
            if (x <= min + n)
                result = Color.FromArgb(255, 0, 0, 255);
            else if (x <= min + 2 * n)
                result = Color.FromArgb(255, 0, 179, 255);
            else if (x <= min + 3 * n)
                result = Color.FromArgb(255, 0, 255, 255);
            else if (x <= min + 4 * n)
                result = Color.FromArgb(255, 0, 255, 179);
            else if (x <= min + 5 * n)
                result = Color.FromArgb(255, 0, 255, 0);
            else if (x <= min + 6 * n)
                result = Color.FromArgb(255, 179, 255, 0);
            else if (x <= min + 7 * n)
                result = Color.FromArgb(255, 255, 255, 0);
            else if (x <= min + 8 * n)
                result = Color.FromArgb(255, 255, 179, 0);
            else
                result = Color.FromArgb(255, 255, 0, 0);
            return result;
        }
    }
}
