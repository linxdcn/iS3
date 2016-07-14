using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IS3.SimpleStructureTools.Helper.FEM.FEMModel
{
    public class Element
    {
        private int _eid; //element id
        private int _pid; //part id

        public int eid { get { return _eid; } }
        public int pid { get { return _pid; } }

        public Element(int aeid, int apid)
        {
            _eid = aeid;
            _pid = apid;
        }

        public virtual string AnsysOutput()
        {
            return "";
        }
    }

    public class ElementLink : Element
    {
        private int _n1; //end of link
        private int _n2; //end of link

        public ElementLink(int eid, int pid, int n1, int n2)
            : base(eid, pid)
        {
            _n1 = n1;
            _n2 = n2;
        }

        public int n1 { get { return _n1; } }
        public int n2 { get { return _n2; } }

        public override string AnsysOutput()
        {
            return "e," + _n1 + "," + _n2 + "\r\n";
        }
    }

    public class ElementBeam : Element
    {
        private int _n1; //end of beam
        private int _n2; //end of beam
        private int _n3; //this nodal point is for direction

        public ElementBeam(int eid, int pid, int n1, int n2, int n3 = 0) : base(eid, pid)
        {
            _n1 = n1;
            _n2 = n2;
            _n3 = n3;
        }

        public int n1 { get { return _n1; } }
        public int n2 { get { return _n2; } }
        public int n3 { get { return _n3; } }

        public override string AnsysOutput()
        {
            return "e," + _n1 + "," + _n2 + "\r\n";
        }
    }

    public class ElementCombin : Element
    {
        private int _n1;
        private int _n2;

        public ElementCombin(int eid, int pid, int n1, int n2) : base(eid, pid)
        {
            _n1 = n1;
            _n2 = n2;
        }

        public int n1 { get { return _n1; } }
        public int n2 { get { return _n2; } }

        public override string AnsysOutput()
        {
            return "e," + _n1 + "," + _n2 + "\r\n";
        }
    }

    public class ElementShell : Element
    {
        private int _n1;
        private int _n2;
        private int _n3;
        private int _n4;

        public ElementShell(int eid, int pid, int n1, int n2, int n3, int n4) : base(eid, pid)
        {
            _n1 = n1;
            _n2 = n2;
            _n3 = n3;
            _n4 = n4;
        }

        public int n1 { get { return _n1; } }
        public int n2 { get { return _n2; } }
        public int n3 { get { return _n3; } }
        public int n4 { get { return _n4; } }

        public override string AnsysOutput()
        {
            return "e," + _n1 + "," + _n2 + "," + _n3 + "," + _n4 + "\r\n";
        }
    }
}
