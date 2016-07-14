using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IS3.SimpleStructureTools.Helper.FEM.FEMModel
{
    public class Boundary
    {
        public virtual string AnsysOutput()
        {
            return "";
        }
    }

    public class BoundaryNode : Boundary
    {
        public Node node { get; set; }
        public int ux { get; set; }
        public int uy { get; set; }
        public int uz { get; set; }
        public int rx { get; set; }
        public int ry { get; set; }
        public int rz { get; set; }
        public bool isAll
        {
            get 
            {
                if (ux == 0 || uy == 0 || uz == 0 ||
                    rx == 0 || ry == 0 || rz == 0)
                    return false;
                else
                    return true;
            }
        }
        public override string AnsysOutput()
        {
            string s = "";
            string s_pre = "d," + node.nid + ",";
            if (isAll)
                return s_pre + "all\r\n";
            if (ux == 1)
                s += s_pre + "ux\r\n";
            if (uy == 1)
                s += s_pre + "uy\r\n";
            if (uz == 1)
                s += s_pre + "uz\r\n";
            if (rx == 1)
                s += s_pre + "rotx\r\n";
            if (ry == 1)
                s += s_pre + "roty\r\n";
            if (rz == 1)
                s += s_pre + "rotz\r\n";
            return s;
        }
    }
}
