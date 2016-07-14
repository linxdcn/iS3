using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IS3.SimpleStructureTools.Helper.FEM.FEMModel
{
    public class Section
    {
        public int secid { get; set; }

        public Section(int _secid)
        {
            secid = _secid;
        }

        public virtual string AnsysOutput()
        {
            string s = "";
            return s;
        }
    }

    public class SectionShell : Section
    {
        public SectionShell(int _id) : base(_id)
        {

        }
    }

    public class SectionLink : Section
    {
        public SectionLink(int _id) : base(_id)
        {

        }
    }

    public class SectionCombin : Section
    {
        public SectionCombin(int _id) : base(_id)
        {

        }
    }

    public class Shell143 : SectionShell
    {
        public double thickness { get; set; }

        public Shell143(int _id) : base(_id)
        {

        }

        public override string AnsysOutput()
        {
            return "r," + secid + "," + thickness.ToString("0.00") + "\r\n";
        }
    }

    // Ansys 13.0 or later, link8 and link10 were discarded.
    // They are replaced by link180
    public class Link180 : SectionLink
    {
        /***********************************************************************************************************************
        KEYOPT(2)
        Cross-section scaling (applies only if large-deflection effects [NLGEOM,ON] apply ):
        0 -- Enforce incompressibility; cross section is scaled as a function of axial stretch (default).
        1 -- Section is assumed to be rigid.
        KEYOPT(3)
        Tension and/or compression option:
        0 -- Tension and compression (default).
        1 -- Tension only.
        2 -- Compression only.
        ***********************************************************************************************************************/
        public int keyOption1 { get; set; }
        public int keyOption2 { get; set; }
        public double area { get; set; }

        public Link180(int _id) : base(_id)
        {

        }

        public override string AnsysOutput()
        {
            string s = "";
            if (keyOption1 != 0)
                s += "keyopt," + secid + "," + keyOption1 + "," + keyOption2 + "\r\n";
            s += "r," + secid + "," + area.ToString("0.00") + "\r\n";
            return s;
        }
    }

    public class Combin39 : SectionCombin
    {
        /***********************************************************************************************************************
        KEYOPT(1)
        Unloading path:
        0 -- Unload along same loading curve
        1 -- Unload along line parallel to slope at origin of loading curve
        KEYOPT(2)
        Element behavior under compressive load:
        0 -- Compressive loading follows defined compressive curve (or reflected tensile curve if not defined)
        1 -- Element offers no resistance to compressive loading
        2 -- Loading initially follows tensile curve then follows compressive curve after buckling (zero or negative stiffness)
        KEYOPT(3)
        Element degrees of freedom (1-D) (KEYOPT(4) overrides KEYOPT(3)):
        0, 1 -- UX (Displacement along nodal X axes)
        2 -- UY (Displacement along nodal Y axes)
        3 -- UZ (Displacement along nodal Z axes)
        4 -- ROTX (Rotation about nodal X axes)
        5 -- ROTY (Rotation about nodal Y axes)
        6 -- ROTZ (Rotation about nodal Z axes)
        7 -- PRES
        8 -- TEMP
        KEYOPT(4)
        Element degrees of freedom (2-D or 3-D):
        0 -- Use any KEYOPT(3) option
        1 -- 3-D longitudinal element (UX, UY and UZ)
        2 -- 3-D torsional element (ROTX, ROTY and ROTZ)
        3 -- 2-D longitudinal element. (UX and UY) Element must lie in an X-Y plane
        KEYOPT(6)
        Element output:
        0 -- Basic element printout
        1 -- Also print force-deflection table for each element (only at first iteration of problem)
        ***********************************************************************************************************************/
        public int keyOption1 { get; set; }
        public int keyOption2 { get; set; }
        public List<KeyValuePair<double, double>> stiffness { get; set; } //<force, displacement>
        
        public Combin39(int _id) : base(_id)
        {

        }

        public override string AnsysOutput()
        {
            string s = "";
            if (keyOption1 != 0)
                s += "keyopt," + secid + "," + keyOption1 + "," + keyOption2 + "\r\n";
            if (stiffness.Count != 0)
            {
                s += "r," + secid;
                foreach(KeyValuePair<double, double> pair in stiffness)
                {
                    s += "," + pair.Value.ToString("0.000000") + "," + pair.Key.ToString("0.0");
                }
                s += "\r\n";
            }
            return s;
        }
    }
}
