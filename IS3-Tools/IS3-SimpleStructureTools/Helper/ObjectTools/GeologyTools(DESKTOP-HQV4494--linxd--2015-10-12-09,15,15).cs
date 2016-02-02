using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using IS3.Core;
using IS3.Core.Geometry;
using IS3.Core.Graphics;
using IS3.Geology;
using IS3.SimpleStructureTools.Helper;

namespace IS3.SimpleStructureTools.Helper
{
    public class GeologyTools
    {
        public static SoilProperty GetSoilProperty(string name, double mileage)
        {
            IApp app = Application.Current as IApp;
            List<Tree> spTrees = app.activeMainFrame.Project.GeoTree.FindTreeContainName("SoilProperty");

            foreach (Tree tree in spTrees)
            {
                List<SubCategory> subObj = tree.SubCategories as List<SubCategory>;
                List<DGObject> spObj = tree.Objs as List<DGObject>;
                if (subObj == null)
                    continue;
                StratumSection straSec = subObj[0] as StratumSection;
                if (mileage < straSec.StartMileage ||
                    mileage > straSec.EndMileage)
                    continue;

                foreach (SoilProperty sp in spObj)
                {
                    if (sp.Name == name)
                    {
                        return sp;
                    }
                }
            }
            return null;
        }
    }
}
