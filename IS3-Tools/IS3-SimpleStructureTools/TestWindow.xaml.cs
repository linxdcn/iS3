using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

using IS3.ShieldTunnel;
using IS3.SimpleStructureTools.Helper;
using IS3.SimpleStructureTools.Helper.FEM.FEMModel;
using IS3.SimpleStructureTools.Helper.FEM.Ansys;
using IS3.SimpleStructureTools.Helper.FEM.ShieldTunnelLine3D;

namespace IS3.SimpleStructureTools
{
    /// <summary>
    /// Interaction logic for TestWindow.xaml
    /// </summary>
    public partial class TestWindow : Window
    {
        public Model model;
        public ModelSetting modelSetting;
        public MatSetting matSetting;
        public const double PI = Math.PI;

        public TestWindow()
        {
            model = new Model();

            InitializeComponent();
        }

        private void SettingInitial()
        {
            //Model Setting
            modelSetting = new ModelSetting();
            modelSetting.outerRadius = 3.1;
            modelSetting.thickness = 0.35;
            modelSetting.width = 1.2;
            modelSetting.num_ring = 2;
            modelSetting.num_longit = 1;
            modelSetting.num_circum = 90;
            modelSetting.pos_joint.Add(8);
            modelSetting.pos_joint.Add(73);
            modelSetting.pos_joint.Add(138);
            modelSetting.pos_joint.Add(222);
            modelSetting.pos_joint.Add(287);
            modelSetting.pos_joint.Add(352);
            modelSetting.axialForce = 15000;
            modelSetting.circumferential_joint_length = 0.4;
            modelSetting.circumferential_joint_diameter = 0.03;
            modelSetting.circumferential_joint_Es = 200000000;
            modelSetting.circumferential_joint_Ec = 35000000;
            modelSetting.circumferential_ring_friction_factor = 0.6;

            modelSetting.isCurve = true;
            modelSetting.axis = TunnelTools.GetTunnelFootprintAxis(121800);

            modelSetting.Initial();


            //Mat Setting
            matSetting = new MatSetting();
            matSetting.shell_coe = 35000000;
            matSetting.shell_pr = 0.25;
            matSetting.shell_dens = 2.5;
            matSetting.shell_thickness = modelSetting.thickness;

            matSetting.radial_spring_coe = 5000;
            matSetting.radial_spring_pr = 0.25;
            matSetting.radial_spring_area = 2 * PI * modelSetting.modelRadius
                / modelSetting.num_circum * modelSetting.width / modelSetting.num_longit;

            matSetting.tangential_spring_coe = 100;
            matSetting.tangential_spring_pr = 0.25;
            matSetting.tangential_spring_area = matSetting.radial_spring_area / 3;

            matSetting.circumferential_joint_tensile = modelSetting.circumferential_joint_Es
                * modelSetting.circumferential_joint_area / modelSetting.circumferential_joint_length;
            matSetting.circumferential_joint_compression = matSetting.circumferential_joint_tensile * 1000;

            matSetting.longitudinal_joint_rotation_k_1 = 200000 / modelSetting.num_longit;
            matSetting.longitudinal_joint_rotation_strain_1 = 0.0015;
            matSetting.longitudinal_joint_rotation_k_2 = 20000 / modelSetting.num_longit;
            matSetting.longitudinal_joint_rotation_strain_2 = 0.002;

            matSetting.longitudinal_joint_shear_force = modelSetting.axialForce * modelSetting.circumferential_ring_friction_factor / modelSetting.num_circum;
        }

        private void Test_Click(object sender, RoutedEventArgs e)
        {
            SettingInitial();
            string path = "D:/test.txt";
            if (modelSetting.isCurve == false)
            {
                GenerateParts.GenerateAllParts(matSetting, model);
                GenerateModel.Build(modelSetting, model, path);              
                PartOutput.Output(model, path);
                NodeOutput.Output(model, path);
                ElementOutput.Output(model, path);
                AnsysOutput.ConstrainedOutput(model, path);
                AnsysOutput.BoundaryOutput(model, path);
            }
            else
            {
                GenerateParts.GenerateAllParts(matSetting, model);
                GenerateModel.Build(modelSetting, model, path);
            }       
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            DateTime beforDT = System.DateTime.Now;

            DateTime afterDT = System.DateTime.Now;
            TimeSpan ts = afterDT.Subtract(beforDT);
            MessageBox.Show(String.Format("DateTime总共花费{0}s.", ts.TotalMilliseconds / 1000));
            Close();
        }
    }
}
