using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IS3.SimpleStructureTools.Helper.FEM.FEMModel
{
    public class SetNode
    {
        public int snid { get; set; }
        public List<int> nodes { get; set; }

        public SetNode(int _snid)
        {
            snid = _snid;
            nodes = new List<int>();
        }

        public SetNode(int _snid, List<int> _nodes)
        {
            snid = _snid;
            nodes = _nodes;
        }
    }
}
