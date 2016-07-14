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

            /*
            if (value > 4)
                color = Color.FromArgb(255, 149, 215, 174);
            else if (value > 3)
                color = Color.FromArgb(255, 131, 181, 209);
            else if (value > 2)
                color = Color.FromArgb(255, 240, 236, 87);
            else if (value > 1)
                color = Color.FromArgb(255, 241, 145, 67);
            else
                color = Color.FromArgb(255, 255, 105, 120);
             * */

            if (value > 4)
                color = Colors.LightGreen;
            else if (value > 3)
                color = Colors.Blue;
            else if (value > 2)
                color = Colors.LightYellow;
            else if (value > 1)
                color = Colors.Orange;
            else
                color = Colors.Red;

            return color;
        }
    }
}
