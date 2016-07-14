using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IS3.SimpleStructureTools.Helper.FEM.FEMModel
{
    public class Node
    {
        private int _nid;
        private double _x;
        private double _y;
        private double _z;

        public Node(int nid, double x, double y, double z)
        {
            _nid = nid;
            _x = x;
            _y = y;
            _z = z;
        }

        public int nid
        {
            get { return _nid; }
        }
        public double x
        {
            get { return _x; }
        }
        public double y
        {
            get { return _y; }
        }
        public double z
        {
            get { return _z; }
        }

        public string AnsysOutput()
        {
            return "n," + _nid + "," + _x.ToString("0.0000") + "," 
                + _y.ToString("0.0000") + "," + _z.ToString("0.0000") + "\r\n";
        }
    }

    public class NodeSet
    {
        private List<Node> _nodes;
        
        public NodeSet()
        {
            _nodes = new List<Node>();
        }

        public Node this[int index] { get { return _nodes[index]; } }

        public void Add(Node node)
        {
            _nodes.Add(node);
        }
        public int Count()
        {
            return _nodes.Count();
        }
    }
}
