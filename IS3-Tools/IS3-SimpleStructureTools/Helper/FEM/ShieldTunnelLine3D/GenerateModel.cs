using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.IO;

using IS3.ShieldTunnel;
using IS3.SimpleStructureTools.Helper.Analysis;
using IS3.SimpleStructureTools.Helper.FEM.FEMModel;

namespace IS3.SimpleStructureTools.Helper.FEM.ShieldTunnelLine3D
{
    public class ShieldTunnel3DResult
    {
        public string changeLocal;
        public List<Node> nodes;
        public Dictionary<int, List<Element>> PreElements; //elements to be added before nrotat
        public Dictionary<int, List<Element>> PostElements; //elements to be added after nrotat
        public List<int> rotateNodeID;

        public ShieldTunnel3DResult()
        {
            nodes = new List<Node>();
            PreElements = new Dictionary<int, List<Element>>();
            PostElements = new Dictionary<int, List<Element>>();
            rotateNodeID = new List<int>();
        }
    }
    
    public class GenerateModel
    {
        public static void Build(ModelSetting modelSetting, Model model, string path)
        {
            if (modelSetting.isCurve == false)
                BuildStraightModel(modelSetting, model);
            else
                BuildCurveModel(modelSetting, model, path);
        }
        #region this region is to create the curve shield tunnel
        //you don't need to assign modelSetting.axis as it won't be used
        public static void BuildStraightModel(ModelSetting modelSetting, Model model)
        {       
            SingleRingResult ringResult = new SingleRingResult();
            GenerateNodes.GenerateSingleRingNode(modelSetting, ringResult);
            GenerateElements.GenerateSingleRingElement(modelSetting, ringResult);

            //generate all nodes
            for(int i = 0; i < modelSetting.num_ring; i++)
            {
                for(int j = 0; j < modelSetting.num_node_all; j++)
                {
                    Node node = ringResult.nodes[j];
                    Node newNode = new Node(node.nid + i * modelSetting.num_node_all,
                        node.x, node.y, node.z + modelSetting.width * i);
                    model.AddNode(newNode);
                }
            }

            int elemenCount = 0;

            //generate all shells
            for (int i = 0; i < modelSetting.num_ring; i++)
            {
                foreach (Element element in ringResult.elements[1])
                {
                    elemenCount++;
                    ElementShell shell = element as ElementShell;
                    ElementShell newShell = new ElementShell(elemenCount,
                        shell.pid, shell.n1 + modelSetting.num_node_all * i, shell.n2 + modelSetting.num_node_all * i,
                        shell.n3 + modelSetting.num_node_all * i, shell.n4 + modelSetting.num_node_all * i);
                    model.AddElement(newShell);
                }
            }

            //generate radial ground spring
            for (int i = 0; i < modelSetting.num_ring; i++)
            {
                foreach (Element element in ringResult.elements[2])
                {
                    elemenCount++;
                    ElementLink link = element as ElementLink;
                    ElementLink newLink = new ElementLink(elemenCount,
                        link.pid, link.n1 + modelSetting.num_node_all * i, link.n2 + modelSetting.num_node_all * i);
                    model.AddElement(newLink);
                }
            }

            //generate tangential ground spring
            for (int i = 0; i < modelSetting.num_ring; i++)
            {
                foreach (Element element in ringResult.elements[3])
                {
                    elemenCount++;
                    ElementLink link = element as ElementLink;
                    ElementLink newLink = new ElementLink(elemenCount,
                        link.pid, link.n1 + modelSetting.num_node_all * i, link.n2 + modelSetting.num_node_all * i);
                    model.AddElement(newLink);
                }
            }

            
            //generate the longitudinal joint spring
            for (int i = 0; i < modelSetting.num_ring; i++)
            {
                for (int j = 0; j <= modelSetting.num_longit; j++)
                {
                    for (int k = 0; k < modelSetting.pos_joint.Count; k++)
                    {
                        elemenCount++;
                        ElementCombin combinZ = new ElementCombin(elemenCount, 5,
                            modelSetting.pos_joint[k] + 1 + j * modelSetting.num_node_face + i * modelSetting.num_node_all,
                            k + 1 + modelSetting.num_circum + j * modelSetting.num_node_face + i * modelSetting.num_node_all);
                        model.AddElement(combinZ);
                    }
                }                
            }
            
            //generate circumferential joint spring
            for (int i = 1; i < modelSetting.num_ring; i++)
            {
                for (int j = 0; j < modelSetting.num_node_face; j++ )
                {
                    elemenCount++;
                    ElementCombin combinCircumZ = new ElementCombin(elemenCount, 4,
                         j + 1 + (i - 1) * modelSetting.num_node_all + modelSetting.num_node_face * modelSetting.num_longit,
                         j + 1 + i * modelSetting.num_node_all);
                    elemenCount++;
                    ElementCombin combinCircumX = new ElementCombin(elemenCount, 6,
                         j + 1 + (i - 1) * modelSetting.num_node_all + modelSetting.num_node_face * modelSetting.num_longit,
                         j + 1 + i * modelSetting.num_node_all);
                    elemenCount++;
                    ElementCombin combinCircumY = new ElementCombin(elemenCount, 7,
                        j + 1 + (i - 1) * modelSetting.num_node_all + modelSetting.num_node_face * modelSetting.num_longit,
                         j + 1 + i * modelSetting.num_node_all);
                    model.AddElement(combinCircumZ);
                    model.AddElement(combinCircumX);
                    model.AddElement(combinCircumY);
                }
                    
            }

            
            //add longitudinal joint nodes couple-constrain
            for (int i = 0; i < modelSetting.num_ring; i++)
            {
                for (int j = 0; j <= modelSetting.num_longit; j++)
                {
                    for (int k = 0; k < modelSetting.pos_joint.Count; k++)
                    {
                        ConstrainedNodes constrain = new ConstrainedNodes();
                        constrain.nodes.Add(model.nodes[modelSetting.pos_joint[k] + 1 + j * modelSetting.num_node_face + i * modelSetting.num_node_all]);
                        constrain.nodes.Add(model.nodes[k + 1 + modelSetting.num_circum + j * modelSetting.num_node_face + i * modelSetting.num_node_all]);
                        constrain.ux = 1;
                        constrain.uy = 1;
                        model.AddConstrained(constrain);
                    }
                }
            }

            //add boundary
            for (int i = 0; i < modelSetting.num_node_face; i++)
            {
                BoundaryNode boundary = new BoundaryNode();
                boundary.node = model.nodes[i + 1];
                boundary.ux = 1;
                boundary.uy = 1;
                boundary.uz = 1;
                boundary.rx = 1;
                boundary.ry = 1;
                boundary.rz = 1;
                model.AddBoundary(boundary);
            }
            for (int i = 0; i < modelSetting.num_node_face; i++)
            {
                BoundaryNode boundary = new BoundaryNode();
                boundary.node = model.nodes[i + 1 + modelSetting.num_node_all * (modelSetting.num_ring - 1) 
                    + modelSetting.num_longit * modelSetting.num_node_face];
                boundary.ux = 1;
                boundary.uy = 1;
                boundary.uz = 1;
                boundary.rx = 1;
                boundary.ry = 1;
                boundary.rz = 1;
                model.AddBoundary(boundary);
            }
            
            //apply load
        }
        #endregion

        #region this region is to create the curve shield tunnel
        //you don't need to assign modelSetting.num_longit as it will calculated by axis
        public static void BuildCurveModel(ModelSetting modelSetting, Model model, string path)
        {
            //get the coordinary of each ring
            double length = Math.Abs(modelSetting.axis.AxisPoints.First().Mileage -
                modelSetting.axis.AxisPoints.Last().Mileage);
            modelSetting.num_ring = (int)(length / modelSetting.width);
            List<RingInfo> ringInfos = GetRingInfo(modelSetting);

            //get single ring result
            SingleRingResult ringResult = new SingleRingResult();
            GenerateNodes.GenerateSingleRingNode(modelSetting, ringResult);
            GenerateElements.GenerateSingleRingElement(modelSetting, ringResult);

            //initial 3D result
            List<ShieldTunnel3DResult> results = new List<ShieldTunnel3DResult>();

            for (int i = 0; i < ringInfos.Count - 1; i++)
            //for (int i = 0; i < 10; i++)
            {
                ShieldTunnel3DResult result = new ShieldTunnel3DResult();
                ChangeLocal(ringInfos[i], result);
                GetNodes(i, modelSetting, ringResult, result);
                GetShellElements(i, modelSetting, ringResult, result);
                RotateNode(i, modelSetting, result);
                results.Add(result);
            }

            Output(model, results, path);
        }

        /// <summary>
        /// Get the coordinary and direction of each ring
        /// </summary>
        /// <param name="modelSetting"></param>
        /// <returns></returns>
        private static List<RingInfo> GetRingInfo(ModelSetting modelSetting)
        {
            List<RingInfo> ringInfos = new List<RingInfo>();
            for (int i = 0; i < modelSetting.num_ring; i++ )
            {
                double mStart = modelSetting.axis.AxisPoints[0].Mileage + modelSetting.width * i;
                double mEnd = mStart + modelSetting.width;
                TunnelAxisPoint axisP1 = TunnelMappingUtility.MileageToAxisPoint(mStart, modelSetting.axis);
                TunnelAxisPoint axisP2 = TunnelMappingUtility.MileageToAxisPoint(mEnd, modelSetting.axis);
                RingInfo info = new RingInfo();
                info.x = axisP1.X;
                info.y = axisP1.Y;
                info.z = axisP1.Z;
                info.vecX = axisP2.X - axisP1.X;
                info.vecY = axisP2.Y - axisP1.Y;
                info.vecZ = axisP2.Z - axisP1.Z;
                ringInfos.Add(info);
            }
            return ringInfos;
        }

        private static void ChangeLocal(RingInfo info, ShieldTunnel3DResult result)
        {
            result.changeLocal = String.Format("local,11,0,{0:0.00},{1:0.00},{2:0.00},{3:0.00},{4:0.00},{5:0.00}",
                info.x, info.y, info.z,
                Math.Atan(info.vecY / info.vecX) / Math.PI * 180, 0, Math.Atan(info.vecZ / info.vecX) / Math.PI * 180);
        }

        private static void GetNodes(int ringNo, ModelSetting modelSetting, SingleRingResult ringResult, ShieldTunnel3DResult result)
        {
            for (int j = 0; j < modelSetting.num_node_all; j++)
            {
                Node node = ringResult.nodes[j];
                Node newNode = new Node(node.nid + ringNo * modelSetting.num_node_all,
                    node.x, node.y, node.z);
                result.nodes.Add(newNode);
            }
        }

        private static void GetShellElements(int ringNo, ModelSetting modelSetting, SingleRingResult ringResult, ShieldTunnel3DResult result)
        {
            if (!result.PreElements.ContainsKey(1))
                result.PreElements[1] = new List<Element>();
            foreach (Element element in ringResult.elements[1])
            {
                ElementShell shell = element as ElementShell;
                ElementShell newShell = new ElementShell(1,
                    shell.pid, shell.n1 + modelSetting.num_node_all * ringNo, shell.n2 + modelSetting.num_node_all * ringNo,
                    shell.n3 + modelSetting.num_node_all * ringNo, shell.n4 + modelSetting.num_node_all * ringNo);
                result.PreElements[1].Add(newShell);
            }
        }

        private static void GetGroundElements(int ringNo, ModelSetting modelSetting, SingleRingResult ringResult, ShieldTunnel3DResult result)
        {
            if (!result.PreElements.ContainsKey(2))
                result.PreElements[2] = new List<Element>();
            //generate radial ground spring
            foreach (Element element in ringResult.elements[2])
            {
                ElementLink link = element as ElementLink;
                ElementLink newLink = new ElementLink(1,
                    link.pid, link.n1 + modelSetting.num_node_all * ringNo, link.n2 + modelSetting.num_node_all * ringNo);
                result.PreElements[2].Add(newLink);
            }

            if (!result.PreElements.ContainsKey(3))
                result.PreElements[3] = new List<Element>();
            //generate tangential ground spring
            foreach (Element element in ringResult.elements[3])
            {
                ElementLink link = element as ElementLink;
                ElementLink newLink = new ElementLink(1,
                    link.pid, link.n1 + modelSetting.num_node_all * ringNo, link.n2 + modelSetting.num_node_all * ringNo);
                result.PreElements[3].Add(newLink);
            }
        }

        private static void RotateNode(int ringNo, ModelSetting modelSetting, ShieldTunnel3DResult result)
        {
            //rotate the nodes of back border of the last ring
            if (ringNo != 0) //not the first ring
                for (int j = 0; j < modelSetting.num_node_face; j++)
                {
                    result.rotateNodeID.Add(j + 1 + (ringNo - 1) * modelSetting.num_node_all + modelSetting.num_node_face * modelSetting.num_longit);
                }

            //rotate all the nodes of this ring
            for (int j = 0; j < modelSetting.num_node_ring; j++)
            {
                result.rotateNodeID.Add(j + 1 + ringNo * modelSetting.num_node_all);
            }
        }

        private static void GetLongitudinalJoint(int ringNo, ModelSetting modelSetting, ShieldTunnel3DResult result)
        {
            if (!result.PostElements.ContainsKey(5))
                result.PostElements[5] = new List<Element>();
            for (int j = 0; j <= modelSetting.num_longit; j++)
            {
                for (int k = 0; k < modelSetting.pos_joint.Count; k++)
                {
                    ElementCombin combinZ = new ElementCombin(1, 5,
                        modelSetting.num_segment_element[k] + 1 + j * modelSetting.num_node_face + ringNo * modelSetting.num_node_all,
                        k + 1 + modelSetting.num_circum + j * modelSetting.num_node_face + ringNo * modelSetting.num_node_all);
                    result.PostElements[5].Add(combinZ);
                }
            }  
        }

        private static void GetCircumferentialJoint(int ringNo, ModelSetting modelSetting, ShieldTunnel3DResult result)
        {
            if (!result.PostElements.ContainsKey(4))
                result.PostElements[4] = new List<Element>();
            if (!result.PostElements.ContainsKey(6))
                result.PostElements[6] = new List<Element>();
            if (!result.PostElements.ContainsKey(7))
                result.PostElements[7] = new List<Element>();

            if(ringNo != 0)
                for (int j = 0; j < modelSetting.num_node_face; j++)
                {
                    ElementCombin combinCircumZ = new ElementCombin(1, 4,
                         j + 1 + (ringNo - 1) * modelSetting.num_node_all + modelSetting.num_node_face * modelSetting.num_longit,
                         j + 1 + ringNo * modelSetting.num_node_all);
                    result.PostElements[4].Add(combinCircumZ);

                    ElementCombin combinCircumX = new ElementCombin(1, 6,
                         j + 1 + (ringNo - 1) * modelSetting.num_node_all + modelSetting.num_node_face * modelSetting.num_longit,
                         j + 1 + ringNo * modelSetting.num_node_all);
                    result.PostElements[6].Add(combinCircumX);

                    ElementCombin combinCircumY = new ElementCombin(1, 7,
                        j + 1 + (ringNo - 1) * modelSetting.num_node_all + modelSetting.num_node_face * modelSetting.num_longit,
                         j + 1 + ringNo * modelSetting.num_node_all);
                    result.PostElements[7].Add(combinCircumY);
                }
        }

        private static void ApplyLoad(ModelSetting modelSetting, Model model, SoilInitalStress.IResult load, int ringNo)
        {

        }

        private static void Output(Model model, List<ShieldTunnel3DResult> results, string path)
        {
            FileStream stream = new FileStream(path, FileMode.Append);
            StreamWriter sw = new StreamWriter(stream);

            //output parts
            sw.WriteLine("/prep7");
            sw.WriteLine("!!");
            foreach (Part part in model.parts.Values)
            {
                if (model.elementTypes.ContainsKey(part.eid))
                    sw.Write(model.elementTypes[part.eid].AnsysOutput());
                if (model.mats.ContainsKey(part.mid))
                    sw.Write(model.mats[part.mid].AnsysOutput());
                if (model.sections.ContainsKey(part.secid))
                    sw.Write(model.sections[part.secid].AnsysOutput());
                sw.WriteLine("!!");
            }

            foreach(ShieldTunnel3DResult result in results)
            {
                //change local system
                sw.WriteLine(result.changeLocal);

                //nodes
                foreach (Node node in result.nodes)
                {
                    sw.Write(node.AnsysOutput());
                }

                //pre elements
                foreach (int partID in result.PreElements.Keys)
                {
                    Part part = model.parts[partID];
                    if (model.elementTypes.ContainsKey(part.eid))
                        sw.WriteLine("type," + part.eid);
                    if (model.mats.ContainsKey(part.mid))
                        sw.WriteLine("mat," + part.mid);
                    if (model.sections.ContainsKey(part.secid))
                        sw.WriteLine("real," + part.secid);
                    foreach (Element element in result.PreElements[partID])
                    {
                        sw.Write(element.AnsysOutput());
                    }
                }

                //rotate node
                foreach (int id in result.rotateNodeID)
                    sw.WriteLine("nrotat," + id);


            }
            sw.Close();
        }
        #endregion
    }
}
