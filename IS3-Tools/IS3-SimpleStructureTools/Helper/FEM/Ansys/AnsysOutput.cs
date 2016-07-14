using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using IS3.SimpleStructureTools.Helper.FEM.FEMModel;

namespace IS3.SimpleStructureTools.Helper.FEM.Ansys
{
    public class AnsysOutput
    {
        public static void ConstrainedOutput(Model model, string path)
        {
            FileStream stream = new FileStream(path, FileMode.Append);
            StreamWriter sw = new StreamWriter(stream);
            sw.WriteLine("/prep7");
            foreach (Constrained constrained in model.constradineds)
            {
                sw.Write(constrained.AnsysOutput());
            }
            sw.Close();
        }
        public static void BoundaryOutput(Model model, string path)
        {
            FileStream stream = new FileStream(path, FileMode.Append);
            StreamWriter sw = new StreamWriter(stream);
            sw.WriteLine("/prep7");
            foreach (Boundary boundary in model.boundaries)
            {
                sw.Write(boundary.AnsysOutput());
            }
            sw.Close();
        }

        public static void SingleStringOutput(string context, string path)
        {
            FileStream stream = new FileStream(path, FileMode.Append);
            StreamWriter sw = new StreamWriter(stream);
            sw.WriteLine(context);
            sw.Close();
        }

        public static void TestOutput(string path)
        {
            FileStream stream = new FileStream(path, FileMode.Append);
            StreamWriter sw = new StreamWriter(stream);
            for (int i = 0; i < 100000; i++ )
            {
                sw.WriteLine(i);
            }
            sw.Close();
        }

        public static void TestOutput2(string path)
        {
            FileStream stream = new FileStream(path, FileMode.Append);
            StreamWriter sw = new StreamWriter(stream);
            for (int i = 0; i < 100; i++)
            {
                sw.WriteLine(i);
            }
            sw.Close();
        }
    }
}
