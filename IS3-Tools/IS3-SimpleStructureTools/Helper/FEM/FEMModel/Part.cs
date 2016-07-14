using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IS3.SimpleStructureTools.Helper.FEM.FEMModel
{ 
    public class Part
    {
        public int pid { get; set; } //part id
        public int eid { get; set; } //element id
        public int mid { get; set; } //material id
        public int secid { get; set; } //section id

        public Part()
        {
        }

        public Part(int _pid, int _eid, int _mid, int _secid)
        {
            pid = _pid;
            eid = _eid;
            mid = _mid;
            secid = _secid;
        }
    }
}
