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

            if (value > 4.8)
                color = Color.FromArgb(255,0,255,0);
            else if (value > 4.6)
                color = Color.FromArgb(255, 0, 255, 51);
            else if (value > 4.4)
                color = Color.FromArgb(255, 0, 255, 102);
            else if (value > 4.2)
                color = Color.FromArgb(255, 0, 255, 153);
            else if (value > 4.0)
                color = Color.FromArgb(255, 0, 255, 204);
            else if (value > 3.8)
                color = Color.FromArgb(255, 0, 255, 255);
            else if (value > 3.6)
                color = Color.FromArgb(255, 51, 255, 255);
            else if (value > 3.4)
                color = Color.FromArgb(255, 102, 255, 204);
            else if (value > 3.2)
                color = Color.FromArgb(255, 153, 255, 204);
            else if (value > 3.0)
                color = Colors.Blue;
            else if (value > 2)
                color = Colors.LightYellow;
            else if (value > 1)
                color = Colors.Orange;
            else
                color = Colors.Red;

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
