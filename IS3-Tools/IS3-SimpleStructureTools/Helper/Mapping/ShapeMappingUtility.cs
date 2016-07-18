using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

using IS3.Core;
using IS3.Core.Graphics;
using IS3.Core.Geometry;

namespace IS3.SimpleStructureTools.Helper.Mapping
{
    public class ShapeMappingUtility
    {
        static int NUM = 128;

        public static IGraphic NewLine(double x1, double y1, double x2, double y2, ISpatialReference spatialRefe)
        {
            IMapPoint p1 = Runtime.geometryEngine.newMapPoint(x1, y1, spatialRefe);
            IMapPoint p2 = Runtime.geometryEngine.newMapPoint(x2, y2, spatialRefe);
            return NewLine(p1, p2);
        }
        public static IGraphic NewLine(IMapPoint p1, IMapPoint p2)
        {
            IPointCollection pc = Runtime.geometryEngine.newPointCollection();
            pc.Add(p1);
            pc.Add(p2);
            return NewPolyline(pc);
        }
        public static IGraphic NewPolyline(IPointCollection pc)
        {
            IPolyline polyline = Runtime.geometryEngine.newPolyline(pc);
            IGraphic g = Runtime.graphicEngine.newGraphic();
            g.Geometry = polyline;
            return g;
        }
        public static IGraphic NewCircle(double x, double y, double r, ISpatialReference spatialRefe)
        {
            double[] px = new double[NUM];
            double[] py = new double[NUM];
            GeometryAlgorithms.CircleToPoints(x, y, r, NUM, px, py, AngleDirection.CounterClockwise);

            IPointCollection pc = Runtime.geometryEngine.newPointCollection();
            for (int i = 0; i < NUM; i++)
            {
                IMapPoint p = Runtime.geometryEngine.newMapPoint(px[i], py[i], spatialRefe);
                pc.Add(p);
            }
            pc.Add(pc[0]);

            return NewPolygon(pc);
        }

        public static IGraphic NewRectangle(double left, double top, double right, double bottom, ISpatialReference spatialRefe)
        {
            IMapPoint p1 = Runtime.geometryEngine.newMapPoint(left, top, spatialRefe);
            IMapPoint p2 = Runtime.geometryEngine.newMapPoint(right, top, spatialRefe);
            IMapPoint p3 = Runtime.geometryEngine.newMapPoint(right, bottom, spatialRefe);
            IMapPoint p4 = Runtime.geometryEngine.newMapPoint(left, bottom, spatialRefe);
            return NewQuadrilateral(p1, p2, p3, p4);
        }

        public static IGraphic NewPolygon(IPointCollection pc)
        {
            IPolygon polygon = Runtime.geometryEngine.newPolygon(pc);
            IGraphic g = Runtime.graphicEngine.newGraphic();
            g.Geometry = polygon;
            return g;
        }

        public static IGraphic NewQuadrilateral(IMapPoint p1, IMapPoint p2, IMapPoint p3, IMapPoint p4)
        {
            IPointCollection pc = Runtime.geometryEngine.newPointCollection();
            pc.Add(p1);
            pc.Add(p2);
            pc.Add(p3);
            pc.Add(p4);
            //pc.Add(p1);
            return NewPolygon(pc);
        }
    }
}
