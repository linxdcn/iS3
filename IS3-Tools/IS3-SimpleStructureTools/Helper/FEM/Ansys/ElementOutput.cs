using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;

using IS3.SimpleStructureTools.Helper.FEM.FEMModel;

namespace IS3.SimpleStructureTools.Helper.FEM.Ansys
{
    public class ElementOutput
    {
        public static void Output(Model model, string path)
        {
            FileStream stream = new FileStream(path, FileMode.Append);
            StreamWriter sw = new StreamWriter(stream);
            sw.WriteLine("/prep7");
            foreach (int partID in model.partIDtoElements.Keys)
            {
                Part part = model.parts[partID];
                if (model.elementTypes.ContainsKey(part.eid))
                    sw.WriteLine("type," + part.eid);
                if (model.mats.ContainsKey(part.mid))
                    sw.WriteLine("mat," + part.mid);
                if (model.sections.ContainsKey(part.secid))
                    sw.WriteLine("real," + part.secid);
                foreach (Element element in model.partIDtoElements[partID].Values)
                {
                    sw.Write(element.AnsysOutput());
                }
            }
            sw.Close();
        }

        public static void Output(Model model, List<Element> elements, string path)
        {
            FileStream stream = new FileStream(path, FileMode.Append);
            StreamWriter sw = new StreamWriter(stream);
            sw.WriteLine("/prep7");
            int partID = elements[0].pid;
            if (!model.parts.ContainsKey(partID))
                return;
            Part part = model.parts[partID];
            if (model.elementTypes.ContainsKey(part.eid))
                sw.WriteLine("type," + part.eid);
            if (model.mats.ContainsKey(part.mid))
                sw.WriteLine("mat," + part.mid);
            if (model.sections.ContainsKey(part.secid))
                sw.WriteLine("real," + part.secid);
            foreach (Element element in elements)
            {
                sw.Write(element.AnsysOutput());
            }
            sw.Close();
        }
    }
}
