using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using IS3.SimpleStructureTools.Helper.FEM.FEMModel;

namespace IS3.SimpleStructureTools.Helper.FEM.ShieldTunnelLine3D
{
    public class GenerateParts
    {
        public static void GenerateAllParts(MatSetting matSetting, Model model)
        {
            //lining element, shell143
            Part partShell = new Part(1, 1, 1, 1);
            ElementType typeShell = new ElementType(1, EType.shell143);
            Mat matShell = new Mat(1, matSetting.shell_coe, matSetting.shell_pr, matSetting.shell_dens);
            Shell143 sectionShell = new Shell143(1);
            sectionShell.thickness = matSetting.shell_thickness;
            model.AddPart(partShell);
            model.AddElementType(typeShell);
            model.AddMat(matShell);
            model.AddSection(sectionShell);

            //radial ground spring, link180
            Part partRadialGround = new Part(2, 2, 2, 2);
            ElementType typeRadialGround = new ElementType(2, EType.link180);
            Mat matRadialGround = new Mat(2, matSetting.radial_spring_coe, matSetting.radial_spring_pr, 0);
            Link180 linkRadialGround = new Link180(2);
            linkRadialGround.area = matSetting.radial_spring_area;
            linkRadialGround.keyOption1 = 3;
            linkRadialGround.keyOption2 = 2;
            model.AddPart(partRadialGround);
            model.AddElementType(typeRadialGround);
            model.AddMat(matRadialGround);
            model.AddSection(linkRadialGround);

            //tangential ground spring, link180
            Part partTangentialGround = new Part(3, 3, 3, 3);
            ElementType typeTangentialGround = new ElementType(3, EType.link180);
            Mat matTangentialGround = new Mat(3, matSetting.tangential_spring_coe, matSetting.tangential_spring_pr, 0);
            Link180 linkTangentialGround = new Link180(3);
            linkTangentialGround.area = matSetting.tangential_spring_area;
            model.AddPart(partTangentialGround);
            model.AddElementType(typeTangentialGround);
            model.AddMat(matTangentialGround);
            model.AddSection(linkTangentialGround);

            //circumferential joint
            Part partCircumferentialJoint = new Part(4, 4, 0, 4);
            ElementType typeCircumferentialJoint = new ElementType(4, EType.combin39);
            //Mat matCircumferentialJoint = new Mat(4, 0, 0, 0);
            Combin39 combinCircumferentialJoint = new Combin39(4);
            combinCircumferentialJoint.keyOption1 = 3;
            combinCircumferentialJoint.keyOption2 = 1;
            combinCircumferentialJoint.stiffness = GeneratePairs(
                -0.001 * matSetting.circumferential_joint_compression, -0.001,
                0, 0,
                0.001 * matSetting.circumferential_joint_tensile, 0.001);           
            model.AddPart(partCircumferentialJoint);
            model.AddElementType(typeCircumferentialJoint);
            //model.AddMat(matCircumferentialJoint);
            model.AddSection(combinCircumferentialJoint);

            //longitudinal joint rotation X
            Part partLongitJointZ = new Part(5, 5, 0, 5);
            ElementType typeLongitJointZ = new ElementType(5, EType.combin39);
            //Mat matLongitJointZ = new Mat(5, 0, 0, 0);
            Combin39 combinLongitJointZ = new Combin39(5);
            combinLongitJointZ.keyOption1 = 3;
            combinLongitJointZ.keyOption2 = 4;
            combinLongitJointZ.stiffness = GeneratePairs(GenerateCoordinate(
                matSetting.longitudinal_joint_rotation_k_1, matSetting.longitudinal_joint_rotation_strain_1,
                matSetting.longitudinal_joint_rotation_k_2, matSetting.longitudinal_joint_rotation_strain_2));
            model.AddPart(partLongitJointZ);
            model.AddElementType(typeLongitJointZ);
            //model.AddMat(matLongitJointZ);
            model.AddSection(combinLongitJointZ);

            //circumferential joint Y
            Part partLongitJointX = new Part(6, 6, 0, 6);
            ElementType typeLongitJointX = new ElementType(6, EType.combin39);
            //Mat matLongitJointX = new Mat(6);
            Combin39 combinLongitJointX = new Combin39(6);
            combinLongitJointX.keyOption1 = 3;
            combinLongitJointX.keyOption2 = 2;
            combinLongitJointX.stiffness = GeneratePairs(GenerateCoordinate(
                matSetting.longitudinal_joint_rotation_k_1 / 2, matSetting.longitudinal_joint_rotation_strain_1,
                matSetting.longitudinal_joint_rotation_k_2 / 2, matSetting.longitudinal_joint_rotation_strain_2));
            model.AddPart(partLongitJointX);
            model.AddElementType(typeLongitJointX);
            //model.AddMat(matLongitJointX);
            model.AddSection(combinLongitJointX);

            //circumferential joint Z
            Part partLongitJointY = new Part(7, 7, 0, 7);
            ElementType typeLongitJointY = new ElementType(7, EType.combin39);
            //Mat matLongitJointY = new Mat(7);
            Combin39 combinLongitJointY = new Combin39(7);
            combinLongitJointY.keyOption1 = 3;
            combinLongitJointY.keyOption2 = 3;
            combinLongitJointY.stiffness = GeneratePairs(GenerateCoordinate(
                matSetting.longitudinal_joint_rotation_k_1 / 2, matSetting.longitudinal_joint_rotation_strain_1,
                matSetting.longitudinal_joint_rotation_k_2 / 2, matSetting.longitudinal_joint_rotation_strain_2));
            model.AddPart(partLongitJointY);
            model.AddElementType(typeLongitJointY);
            //model.AddMat(matLongitJointY);
            model.AddSection(combinLongitJointY);

            //pre-load element, in axial direction, link180
            Part partAxial = new Part(8, 8, 8, 8);
            ElementType typeAxial = new ElementType(8, EType.link180);
            Mat matAxial = new Mat(8, matSetting.radial_spring_coe, matSetting.radial_spring_pr, 0);
            Link180 linkAxial = new Link180(8);
            linkRadialGround.area = matSetting.radial_spring_area;
            linkRadialGround.keyOption1 = 3;
            linkRadialGround.keyOption2 = 2;
            model.AddPart(partRadialGround);
            model.AddElementType(typeRadialGround);
            model.AddMat(matRadialGround);
            model.AddSection(linkRadialGround);
        }

        private static List<KeyValuePair<double, double>> GeneratePairs(params double[] values)
        {
            List<KeyValuePair<double, double>> result = new List<KeyValuePair<double, double>>();
            if (values.Length % 2 != 0)
                return result;
            for (int i = 0; i < values.Length; i += 2)
                result.Add(new KeyValuePair<double, double>(values[i], values[i + 1]));
            return result;
        }

        private static double[] GenerateCoordinate(double k1, double t1, double k2, double t2)
        {
            double[] result = new double[10];
            result[0] = -(t2 - t1) * k2 - t1 * k1;
            result[1] = -t2;
            result[2] = -t1 * k1;
            result[3] = -t1;
            result[4] = 0;
            result[5] = 0;
            result[6] = t1 * k1;
            result[7] = t1;
            result[8] = (t2 - t1) * k2 + t1 * k1;
            result[9] = t2;
            return result;
        }
    }
}
