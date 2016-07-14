using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using IS3.SimpleStructureTools.Helper.FEM.FEMModel;

namespace IS3.SimpleStructureTools.Helper.FEM.ShieldTunnelLine3D
{
    
    public class GenerateNodes
    {
        public static void GenerateSingleRingNode(ModelSetting sett, SingleRingResult result)
        {
            double r = sett.outerRadius - sett.thickness / 2; // radius of the model
            
            double pi = Math.PI;

            //generate the nodes of front border
            //first segment
            for (int i = 0; i < sett.num_segment_element[0]; i++ )
            {
                double detal = sett.pos_joint[0] * 1.0 / sett.num_segment_element[0];
                double angle = pi / 180 * i * detal;
                Node node = new Node(i + 1, 0, Math.Sin(angle) * r, Math.Cos(angle) * r);
                result.nodes.Add(node);
            }
            //middle segment
            for (int i = 1; i < sett.pos_joint.Count; i++ )
            {
                for (int j = 0; j < (sett.num_segment_element[i] - sett.num_segment_element[i - 1]); j++)
                {
                    double detal = (sett.pos_joint[i] - sett.pos_joint[i - 1]) * 1.0 / (sett.num_segment_element[i] - sett.num_segment_element[i - 1]);
                    double angle = (sett.pos_joint[i - 1] + j * detal) * pi / 180;
                    Node node = new Node(sett.num_segment_element[i - 1] + j + 1, 0, Math.Sin(angle) * r, Math.Cos(angle) * r);
                    result.nodes.Add(node);
                }
            }
            //last segment
            for (int i = 0; i < sett.num_segment_element[0]; i++)
            {
                double detal = sett.pos_joint[0] * 1.0 / sett.num_segment_element[0];
                double angle = (sett.pos_joint.Last() + i * detal) * pi / 180;
                Node node = new Node(sett.num_segment_element.Last() + i + 1, 0, Math.Sin(angle) * r, Math.Cos(angle) * r);
                result.nodes.Add(node);
            }
            
            //joint node
            for(int i = 0; i < sett.pos_joint.Count; i++)
            {
                double angle = pi / 180 * sett.pos_joint[i];
                Node node = new Node(sett.num_circum + i + 1, 0, Math.Sin(angle) * r, Math.Cos(angle) * r);
                result.nodes.Add(node);
            }

            //generate the nodes of lining in longitudinal direction
            double width = sett.width * 0.97;
            double delta = width / sett.num_longit;

            for(int j = 0; j < sett.num_longit; j++)
            {
                for (int i = 0; i < sett.num_node_face; i++)
                {
                    Node tmp = result.nodes[i];
                    Node node = new Node(tmp.nid + (j + 1) * sett.num_node_face, delta * (j + 1), tmp.y, tmp.z);
                    result.nodes.Add(node);
                }
            }

            //generate the radial ground node
            for(int i = 0; i < sett.num_node_ring; i++)
            {
                Node slNode = result.nodes[i];
                Node node = new Node(slNode.nid + sett.num_node_ring, slNode.x, slNode.y * (1 + 1 / r),
                    slNode.z * (1 + 1 / r));
                result.nodes.Add(node);
            }

            //gegerate the tangential ground node
            for (int i = 0; i < sett.num_node_ring; i++)
            {
                Node slNode = result.nodes[i];
                Node node = new Node(slNode.nid + sett.num_node_ring * 2, slNode.x, slNode.y + slNode.z / r,
                    slNode.z - slNode.y / r);
                result.nodes.Add(node);
            }
        }
    }
}
