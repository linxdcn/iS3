using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Windows.Forms;
using System.Threading.Tasks;

using IS3.Core;
using IS3.ArcGIS;
using IS3.Core.Geometry;
using IS3.Core.Graphics;
using IS3.Geology;

using Esri.ArcGISRuntime.Layers;
using Esri.ArcGISRuntime.Geometry;

namespace IS3.SimpleStructureTools.Helper
{
    public class HelperFunction
    {
        #region geometry and graphic functions
        public static IMapPoint NewMapPoint(double x, double y)
        {
            return Runtime.geometryEngine.newMapPoint(x, y);
        }
        public static IGraphic NewTriangle(IMapPoint p1, IMapPoint p2, IMapPoint p3)
        {
            return Runtime.graphicEngine.newTriangle(p1, p2, p3);
        }
        public static IGraphic NewQuadrilateral(IMapPoint p1, IMapPoint p2, IMapPoint p3, IMapPoint p4)
        {
            return Runtime.graphicEngine.newQuadrilateral(p1, p2, p3, p4);
        }
        public static IGraphic NewPentagon(IMapPoint p1, IMapPoint p2, IMapPoint p3, IMapPoint p4, IMapPoint p5)
        {
            return Runtime.graphicEngine.newPentagon(p1, p2, p3, p4, p5);
        }
        public static IGraphic NewPolygon(IPointCollection part)
        {
            return Runtime.graphicEngine.newPolygon(part);
        }
        public static IPointCollection NewPointCollection()
        {
            return Runtime.geometryEngine.newPointCollection();
        }
        public static IGraphicCollection NewGraphicCollection()
        {
            return Runtime.graphicEngine.newGraphicCollection();
        }
        public static ISymbol GetDefaultFillSymbols(int index)
        {
            return GraphicsUtil.GetDefaultFillSymbols(index);
        }
        public static IGraphic NewCircle(double x,double y, double r)
        {
            return ArcGISMappingUtility.NewCircle(x, y, r) as IGraphic;
        }
        public static IGraphic NewLine(double x1, double y1, double x2, double y2)
        {
            return Runtime.graphicEngine.newLine(x1, y1, x2, y2);
        }
        #endregion

        #region view and layer functions
        public static List<Tuple<IGraphic, string>> getPolylines(IGraphicsLayer gLayer)
        {
            if (gLayer == null)
                return null;

            List<Tuple<IGraphic, string>> result = new List<Tuple<IGraphic, string>>();
            int i = 1;
            foreach (IGraphic g in gLayer.graphics)
            {
                IGeometry geom = g.Geometry;
                if (geom == null)
                    continue;
                if (geom.GeometryType == IS3.Core.Geometry.GeometryType.Polyline)
                {
                    Tuple<IGraphic, string> turple = new Tuple<IGraphic, string>(
                        g, "Polyline#" + i.ToString());
                    i++;
                    result.Add(turple);
                }
            }
            return result;
        }

        public static IGraphicsLayer getLayer(IView view, string layerID)
        {
            IGraphicsLayer gLayer = view.getLayer(layerID);  
            if (gLayer == null)
            {
                gLayer = Runtime.graphicEngine.newGraphicsLayer(
                    layerID, layerID);
                var sym_fill = GraphicsUtil.GetDefaultFillSymbol();
                var renderer = Runtime.graphicEngine.newSimpleRenderer(sym_fill);
                gLayer.setRenderer(renderer);
                view.addLayer(gLayer);
            }

            GraphicsLayer GLayer = gLayer as GraphicsLayer;
            if(GLayer.Graphics.IsReadOnly == true)
            {
                string newID = "e" + layerID;
                gLayer = Runtime.graphicEngine.newGraphicsLayer(
                    newID, newID);
                var sym_fill = GraphicsUtil.GetDefaultFillSymbol();
                var renderer = Runtime.graphicEngine.newSimpleRenderer(sym_fill);
                gLayer.setRenderer(renderer);
                view.addLayer(gLayer);
            }
            return gLayer;
        }

        public static IGraphicCollection ChangeSpatailReference(IGraphicCollection igc, IView view)
        {
            SpatialReference sr = null;
            IEnumerable<IGraphicsLayer> layers = view.layers;
            foreach(IGraphicsLayer igl in layers)
            {
                GraphicsLayer gl = igl as GraphicsLayer;
                foreach(Graphic g in gl.Graphics)
                {
                    sr = g.Geometry.SpatialReference;
                    if (sr != null)
                        break;
                }
                if (sr != null)
                    break;
            }
            
            IGraphicCollection gc = NewGraphicCollection();
            foreach(IGraphic ig in igc)
            {
                Geometry geometry = ig.Geometry as Geometry;
                geometry = GeometryEngine.Project(geometry, sr);
                IGraphic g = Runtime.graphicEngine.newGraphic(geometry as IGeometry);
                gc.Add(g);
            }
            return gc;
        }

        public static Domain getAnalysisDomain()
        {
            Project prj = Globals.project;
            if (prj.domains.ContainsKey("Analysis"))
                return prj.domains["Analysis"];
            else
            {
                Domain analysisDomain = new Domain("Analysis", DomainType.Unknown);
                analysisDomain.parent = prj;
                return analysisDomain;
            }
        }

        public static Tree generateTree()
        {
            Tree tree = new Tree();
            DataView dataView = tree.ObjectsView;
            

            return tree;
        }
        #endregion

        #region Format data
        public static double ToNumber(string text)
        {
            text.Trim();
            text.EndsWith("");
            string[] result = text.Split(new Char[] { ' ' });
            double d = 0;
            try
            {
                d = double.Parse(result[0]);
            }
            catch (FormatException)
            {
                MessageBox.Show("Input error", "error", MessageBoxButtons.OK);
                d = 0;
            }
            return d;
        }
        #endregion
    }
}
