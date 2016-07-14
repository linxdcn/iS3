using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

using IS3.SimpleStructureTools.Helper.FEM.FEMModel;

namespace IS3.SimpleStructureTools.Helper.FEM.Ansys
{
    public class PartOutput
    {
        public static void Output(Model model, string path)
        {
            FileStream stream = new FileStream(path, FileMode.Append);
            StreamWriter sw = new StreamWriter(stream);
            sw.WriteLine("/prep7");
            sw.WriteLine("!!");
            foreach (Part part in model.parts.Values)
            {
                if (model.elementTypes.ContainsKey(part.eid))
                    sw.Write(model.elementTypes[part.eid].AnsysOutput());
                if (model.mats.ContainsKey(part.mid))
                    sw.Write(model.mats[part.mid].AnsysOutput());
                if (model.sections.ContainsKey(part.secid))
                    sw.Write(model.sections[part.secid].AnsysOutput());
                sw.WriteLine("!!");
            }
            sw.Close();
        }
    }
}
