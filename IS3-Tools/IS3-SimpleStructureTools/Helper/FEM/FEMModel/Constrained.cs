using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IS3.SimpleStructureTools.Helper.FEM.FEMModel
{
    public class Constrained
    {
        public virtual string AnsysOutput()
        {
            return "";
        }
    }

    public class ConstrainedNodes : Constrained
    {
        public NodeSet nodes;
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

        public ConstrainedNodes()
        {
            nodes = new NodeSet();
        }

        public override string AnsysOutput()
        {
            string s = "";
            string s_pre = "cp,next,";
            string s_post = "";
            for (int i = 0; i < nodes.Count(); i++)
                s_post += "," + nodes[i].nid;
            s_post += "\r\n";
            if (isAll)
                return s_pre + "all" + s_post;
            if (ux == 1)
                s += s_pre + "ux" + s_post;
            if (uy == 1)
                s += s_pre + "uy" + s_post;
            if (uz == 1)
                s += s_pre + "uz" + s_post;
            if (rx == 1)
                s += s_pre + "rotx" + s_post;
            if (ry == 1)
                s += s_pre + "roty" + s_post;
            if (rz == 1)
                s += s_pre + "rotz" + s_post;
            return s;

        }
    }
}
