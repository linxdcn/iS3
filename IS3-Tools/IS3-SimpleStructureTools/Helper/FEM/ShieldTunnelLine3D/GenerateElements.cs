using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using IS3.SimpleStructureTools.Helper.FEM.FEMModel;

namespace IS3.SimpleStructureTools.Helper.FEM.ShieldTunnelLine3D
{  
    public class GenerateElements
    {
        public static void GenerateSingleRingElement(ModelSetting sett, SingleRingResult result)
        {
            GenerateSingleRingShell(sett, result);
            GenerateGroundSpring(sett,result);
        }
        
        private static void GenerateSingleRingShell(ModelSetting sett, SingleRingResult result)
        {
            double r = sett.outerRadius - sett.thickness / 2; // radius of the model
            int count = 0;
            int shellID = 1;
            if (!result.elements.ContainsKey(shellID))
                result.elements[shellID] = new List<Element>();

            //generate the shell elements of a ring
            for (int i = 0; i < sett.num_longit; i++)
            {
                // element from 0 degree to pos_joint.first degree
                for(int j = 0; j < sett.num_segment_element[0]; j++)
                {
                    count++;
                    ElementShell shell = new ElementShell(count, shellID, j + 1 + i * sett.num_node_face, 
                        j + 2 + i * sett.num_node_face, j + 2 + sett.num_node_face * (i + 1),
                        j + 1 + sett.num_node_face * (i + 1));
                    result.elements[shellID].Add(shell);
                }

                //element from pos_joint.first degree to pos_joint.last degree
                for(int j = 0; j < sett.pos_joint.Count - 1; j++)
                {
                    count++;
                    ElementShell shell = new ElementShell(count, shellID, sett.num_circum + j + 1 + i * sett.num_node_face,
                        sett.num_segment_element[j] + 2 + i * sett.num_node_face, sett.num_segment_element[j] + 2 + (i + 1) * sett.num_node_face,
                        sett.num_circum + j + 1 + (i + 1) * sett.num_node_face);
                    result.elements[shellID].Add(shell);
                    for (int k = sett.num_segment_element[j] + 2; k <= sett.num_segment_element[j + 1]; k++)
                    {
                        count++;
                        ElementShell shell_tmp = new ElementShell(count, shellID, k + i * sett.num_node_face,
                            k + 1 + i * sett.num_node_face, k + 1 + (i + 1) * sett.num_node_face,
                            k + (i + 1) * sett.num_node_face);
                        result.elements[shellID].Add(shell_tmp);
                    }
                }
                
                //element from pos_joint.last degree to 360 degree
                count++;
                ElementShell shell1 = new ElementShell(count, shellID, sett.num_circum + sett.pos_joint.Count + i * sett.num_node_face,
                    sett.num_segment_element.Last() + 2 + i * sett.num_node_face, sett.num_segment_element.Last() + 2 + (i + 1) * sett.num_node_face,
                    sett.num_circum + sett.pos_joint.Count + (i + 1) * sett.num_node_face);
                result.elements[shellID].Add(shell1);
                for (int j = sett.num_segment_element.Last() + 2; j < sett.num_circum; j++)
                {
                    count++;
                    ElementShell shell2 = new ElementShell(count, shellID, j + i * sett.num_node_face,
                        j + 1 + i * sett.num_node_face, j + 1 + (i + 1) * sett.num_node_face,
                        j + (i + 1) * sett.num_node_face);
                    result.elements[shellID].Add(shell2);
                }
                count++;
                ElementShell shell3 = new ElementShell(count, shellID, sett.num_circum + i * sett.num_node_face,
                    1 + i * sett.num_node_face, 1 + (i + 1) * sett.num_node_face, sett.num_circum + (i + 1) * sett.num_node_face);
                result.elements[shellID].Add(shell3);
            }
        }

        private static void GenerateGroundSpring(ModelSetting sett, SingleRingResult result)
        {
            double r = sett.outerRadius - sett.thickness / 2; // radius of the model
            double pi = Math.PI;
            int count = result.elements[1].Count;
            int ground_radius_ID = 2;
            int ground_tangential_ID = 3;
            if (!result.elements.ContainsKey(ground_radius_ID))
                result.elements[ground_radius_ID] = new List<Element>();
            if (!result.elements.ContainsKey(ground_tangential_ID))
                result.elements[ground_tangential_ID] = new List<Element>();

            for (int i = 0; i < sett.num_node_ring; i++)
            {
                count++;
                ElementLink spring = new ElementLink(count, ground_radius_ID,
                    i + 1, i + 1 + sett.num_node_ring);
                result.elements[ground_radius_ID].Add(spring);

                count++;
                spring = new ElementLink(count, ground_tangential_ID,
                    i + 1, i + 1 + 2 * sett.num_node_ring);
                result.elements[ground_tangential_ID].Add(spring);
            }
        }
    }
}
