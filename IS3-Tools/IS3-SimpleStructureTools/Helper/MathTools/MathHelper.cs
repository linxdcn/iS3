using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace IS3.SimpleStructureTools.Helper.MathTools
{
    public class MathHelper
    {
        public static double Mean(List<double> data)
        {
            double sum = 0;
            foreach (double val in data)
                sum += val;
            return sum /= data.Count();
        }

        public static double Var(List<double> data)
        {
            double sum = 0;
            double aver = Mean(data);
            foreach (double val in data)
                sum += (val - aver) * (val - aver);
            return Math.Sqrt(sum / (data.Count() - 1));
        }

        public static List<double> RemoveOutliers(List<double> data)
        {
            data.Sort();
            int count = data.Count();
            double Q1 = 0, Q2 = 0, Q3 = 0;
            if (count % 4 == 0)
            {
                Q1 = (data[count / 4] + data[count / 4 + 1]) * 0.5;
                Q2 = (data[count / 2] + data[count / 2 + 1]) * 0.5;
                Q3 = (data[count * 3 / 4]) + data[count * 3 / 4 + 1] * 0.5;
            }
            else if (count % 4 == 1)
            {
                Q1 = (data[(count - 1) / 4] + data[(count - 1) / 4 + 1]) * 0.5;
                Q2 = data[(count + 1) / 2];
                Q3 = (data[(count - 1) * 3 / 4 + 2] + data[(count - 1) * 3 / 4 + 1]) * 0.5;
            }
            else if (count % 4 == 2)
            {
                Q1 = data[(count + 2) / 4];
                Q2 = (data[count / 2] + data[count / 2 + 1]) * 0.5;
                Q3 = data[(count + 2) * 3 / 4 - 1];
            }
            else if (count % 4 == 3)
            {
                Q1 = data[(count + 1) / 4];
                Q2 = data[(count + 1) / 2];
                Q3 = data[(count + 1) * 3 / 4];
            }

            double lower = Q1 - 1.5 * (Q3 - Q1);
            double uper = Q3 + 1.5 * (Q3 - Q1);

            for (int i = 0; i != data.Count; i++)
            {
                if (data[i] < lower || data[i] > uper)
                {
                    data.RemoveAt(i);
                    i--;
                }
            }
            return data;
        }

        public static List<int> FindOutlierIndex(List<double> inputData)
        {
            List<double> data = new List<double>();
            foreach (double input in inputData)
                data.Add(input);
            data.Sort();
            int count = data.Count();
            double Q1 = 0, Q2 = 0, Q3 = 0;
            if (count % 4 == 0)
            {
                Q1 = (data[count / 4] + data[count / 4 + 1]) * 0.5;
                Q2 = (data[count / 2] + data[count / 2 + 1]) * 0.5;
                Q3 = (data[count * 3 / 4] + data[count * 3 / 4 + 1]) * 0.5;
            }
            else if (count % 4 == 1)
            {
                Q1 = (data[(count - 1) / 4] + data[(count - 1) / 4 + 1]) * 0.5;
                Q2 = data[(count + 1) / 2];
                Q3 = (data[(count - 1) * 3 / 4 + 2] + data[(count - 1) * 3 / 4 + 1]) * 0.5;
            }
            else if (count % 4 == 2)
            {
                Q1 = data[(count + 2) / 4];
                Q2 = (data[count / 2] + data[count / 2 + 1]) * 0.5;
                Q3 = data[(count + 2) * 3 / 4 - 1];
            }
            else if (count % 4 == 3)
            {
                Q1 = data[(count + 1) / 4];
                Q2 = data[(count + 1) / 2];
                Q3 = data[(count + 1) * 3 / 4];
            }

            double lower = Q1 - 1.5 * (Q3 - Q1);
            double uper = Q3 + 1.5 * (Q3 - Q1);

            List<int> indexs = new List<int>();
            for (int i = 0; i != inputData.Count; i++)
            {
                if (inputData[i] < lower || inputData[i] > uper)
                {
                    indexs.Add(i);
                }
            }
            return indexs;
        }
    }
}
