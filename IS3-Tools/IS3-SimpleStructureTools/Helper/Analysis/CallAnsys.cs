using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using IS3.SimpleStructureTools.Helper.File;


namespace IS3.SimpleStructureTools.Helper.Analysis
{
    public class CallAnsys
    {
        public static Task CalllRemoteAsync(string inputFilePath, string outputPath)
        {
            return Task.Run(() =>
            {
                //get file
                byte[] buffer = FileHelper.GetByteFromFile(inputFilePath);
                string base64Str = Convert.ToBase64String(buffer);

                //remote analysis
                IS3Web.soap client = new IS3Web.soap();
                base64Str = client.DoAnsys(base64Str);

                //save result
                byte[] result = Convert.FromBase64String(base64Str);
                string outputFilePath;
                string tempPath;
                if (outputPath.EndsWith("/"))
                {
                    outputFilePath = outputPath + "output.txt";
                    tempPath = outputPath + "temp.txt";
                } 
                else
                {
                    outputFilePath = outputPath + "/output.txt";
                    tempPath = outputPath + "/temp.txt";
                }
                System.IO.File.WriteAllBytes(tempPath, result);  

                //replace "\n" in linux with "\r\n" in windows
                StreamReader reader = new System.IO.StreamReader(tempPath);
                FileStream fs = new FileStream(outputFilePath, FileMode.OpenOrCreate, FileAccess.Write);
                StreamWriter sr = new StreamWriter(fs);
                string ss;
                while ((ss = reader.ReadLine()) != null)
                {
                    ss.Replace("\n", "\r\n");
                    sr.WriteLine(ss);
                }
                reader.Close();
                sr.Close();
                fs.Close();
            });
        }

        public static Task CalllLocalAsync(string ansysPath, string inputFilePath, string outputPath)
        {
            return Task.Run(() =>
            {
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                string outputFilePath;
                if (outputPath.EndsWith("/"))
                    outputFilePath = outputPath + "output.txt";
                else
                    outputFilePath = outputPath + "/output.txt";
                proc.StartInfo.FileName = ansysPath;
                proc.StartInfo.Arguments = "-b -p ansysds -i " + inputFilePath + " -o " + outputFilePath;
                proc.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Hidden;
                proc.StartInfo.WorkingDirectory = outputPath;
                proc.Start();
                proc.WaitForExit();
            });
        }
    }
}
