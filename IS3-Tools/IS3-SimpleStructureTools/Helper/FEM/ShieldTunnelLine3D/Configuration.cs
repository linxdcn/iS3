using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using IS3.ShieldTunnel;
using IS3.SimpleStructureTools.Helper.FEM.FEMModel;

namespace IS3.SimpleStructureTools.Helper.FEM.ShieldTunnelLine3D
{
    public class ModelSetting
    {
        public double outerRadius { get; set; } // external radius    
        public double thickness { get; set; } // lining thickness
        public double width { get; set; } // lining width
        public int num_ring { get; set; } // number of all rings
        public int num_circum { get; set; } //element num in circumferential direction
        public int num_longit { get; set; } //element num in longitudinal direction
        public List<int> pos_joint { get; set; } // position of longituinal joint, clock-wise is positive, 
                                                 //it should be symmetry, or it will cause errors
        public double axialForce { get; set; }
        public double circumferential_joint_length { get; set; }
        public double circumferential_joint_diameter { get; set; }
        public double circumferential_joint_Es { get; set; }
        public double circumferential_joint_Ec { get; set; }
        public double circumferential_ring_friction_factor { get; set; }

        public bool isCurve { get; set; }
        public TunnelAxis axis { get; set; }

        //calculated by Initial function automatically
        public double modelRadius { get; set; } // model radius
        public double circumferential_joint_area { get; set; }
        public int num_node_face { get; set; } //number of nodes in a cross section of lining
        public int num_node_ring { get; set; } //number of nodes in a lining
        public int num_node_all { get; set; } // number of all nodes in a ring
        public List<int> num_segment_element { get; set; }

        public ModelSetting()
        {
            pos_joint = new List<int>();
            num_segment_element = new List<int>();
        }

        public void Initial()
        {
            //calculate the element number in each segment. It may update the num_circum
            double num_temp = pos_joint[0] / 360.0 * num_circum;
            int r = (int)(num_temp + 0.5);
            int save_first_r = r;
            num_segment_element.Add(r);
            for (int i = 1; i < pos_joint.Count; i++ )
            {
                num_temp = (pos_joint[i] - pos_joint[i - 1]) / 360.0 * num_circum;
                r += (int)(num_temp + 0.5);
                num_segment_element.Add(r);
            }
            num_circum = num_segment_element.Last() + save_first_r;

            //initial the parameter that will be used later
            modelRadius = outerRadius - thickness / 2.0;
            circumferential_joint_area = Math.Pow(circumferential_joint_diameter, 2) / 4;
            num_node_face = num_circum + pos_joint.Count;
            num_node_ring = num_node_face * (num_longit + 1);
            num_node_all = 3 * num_node_ring;          
        }
    }

    public class MatSetting
    {
        //shell mat
        public double shell_coe { get; set; }
        public double shell_pr { get; set; }
        public double shell_dens { get; set; }
        public double shell_thickness { get; set; }

        //radial ground spring
        public double radial_spring_coe { get; set;}
        public double radial_spring_pr { get; set; }
        public double radial_spring_area { get; set; }

        //tangential ground spring
        public double tangential_spring_coe { get; set; }
        public double tangential_spring_pr { get; set; }
        public double tangential_spring_area { get; set; }

        //circumferential joint spring
        public double circumferential_joint_tensile { get; set; }
        public double circumferential_joint_compression { get; set; }

        //longitudinal joint spring
        public double longitudinal_joint_rotation_k_1 { get; set; } // 1st phase stiffness
        public double longitudinal_joint_rotation_strain_1 { get; set; }
        public double longitudinal_joint_rotation_k_2 { get; set; } // 2st phase stiffness
        public double longitudinal_joint_rotation_strain_2 { get; set; }

        public double longitudinal_joint_shear_force { get; set; }
    }

    public class SingleRingResult
    {
        public List<Node> nodes { get; set; }
        public Dictionary<int, List<ElementShell>> shells { get; set; }
        public Dictionary<int, List<Element>> elements { get; set; }

        public SingleRingResult()
        {
            nodes = new List<Node>();
            shells = new Dictionary<int, List<ElementShell>>();
            elements = new Dictionary<int, List<Element>>();
        }
    }
}
