using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IS3.SimpleStructureTools.Helper.FEM.FEMModel
{
    public enum EType
    {
        none,
        link180,
        shell143,
        combin39
    };
    
    public class ElementType
    {
        public EType elementType { get; set; }
        public int eid { get; set; } //element id

        public ElementType(int _eid, EType _elementType)
        {
            elementType = _elementType;
            eid = _eid;
        }

        public string AnsysOutput()
        {
            return "et," + eid + "," + elementType.ToString() + "\r\n";
        }
    }
}
