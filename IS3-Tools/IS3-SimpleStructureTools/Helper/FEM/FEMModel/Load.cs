using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IS3.SimpleStructureTools.Helper.FEM.FEMModel
{
    public class Load
    {
        public virtual string AnsysOutput()
        {
            return "";
        }
    }

    public class LoadNode : Load
    {
        public int nid;
        public double fx;
        public double fy;
        public double fz;
        public double mx;
        public double my;
        public double mz;

        public override string AnsysOutput()
        {
            string s = "";
            string s_pre = "f," + nid + ",";
            if (fx != 0)
                s += s_pre + "fx," + fx +"\r\n";
            if (fy != 0)
                s += s_pre + "fy," + fy + "\r\n";
            if (fz != 0)
                s += s_pre + "fz," + fz + "\r\n";
            if (mx != 0)
                s += s_pre + "mx," + mx + "\r\n";
            if (my != 0)
                s += s_pre + "my," + my + "\r\n";
            if (mz != 0)
                s += s_pre + "mz," + mz + "\r\n";
            return s;
        }
    }
}
