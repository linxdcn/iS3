using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Text;
using System.Threading.Tasks;

using IS3.SimpleStructureTools.Helper.FEM.FEMModel;

namespace IS3.SimpleStructureTools.Helper.FEM.Ansys
{
    public class NodeOutput
    {
        public static void Output(Model model, string path)
        {
            FileStream stream = new FileStream(path, FileMode.Append);
            StreamWriter sw = new StreamWriter(stream);
            sw.WriteLine("/prep7");
            foreach (Node node in model.nodes.Values)
            {
                sw.Write(node.AnsysOutput());
            }
            sw.Close();
        }

        public static void Output(List<Node> nodes, string path)
        {
            FileStream stream = new FileStream(path, FileMode.Append);
            StreamWriter sw = new StreamWriter(stream);
            sw.WriteLine("/prep7");
            foreach (Node node in nodes)
            {
                sw.Write(node.AnsysOutput());
            }
            sw.Close();
        }
    }
}
