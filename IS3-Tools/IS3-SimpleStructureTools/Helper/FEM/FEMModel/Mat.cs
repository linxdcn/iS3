using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IS3.SimpleStructureTools.Helper.FEM.FEMModel
{
    public class Mat
    {
        public int mid { get; set; }
        public double coe { get; set; }
        public double poisson { get; set; }
        public double density { get; set; }
        public double alpx { get; set; } //thermal expansion x
        public double alpy { get; set; } //thermal expansion x
        public double alpz { get; set; } //thermal expansion x

        public Mat()
        {
            
        }

        public Mat(int _mid, double _coe = 0, double _poisson = 0, double _density = 0)
        {
            mid = _mid;
            coe = _coe;
            poisson = _poisson;
            density = _density;
        }

        public string AnsysOutput()
        {
            string s = "";
            if (coe != 0)
                s += "mp,ex," + mid + "," + coe.ToString("0.00") + "\r\n";
            if (poisson != 0)
                s += "mp,prxy," + mid + "," + poisson.ToString("0.00") + "\r\n";
            if (density != 0)
                s += "mp,dens," + mid + "," + density.ToString("0.00") + "\r\n";
            return s;
        }
    }
}
