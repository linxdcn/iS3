using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;
using System.Collections;

using IS3.Core;
using IS3.Core.Geometry;
using IS3.Core.Graphics;
using IS3.ShieldTunnel;

using IS3.SimpleStructureTools.DrawTools;

namespace IS3.SimpleStructureTools.Helper
{
    #region Copyright Notice
    //************************  Notice  **********************************
    //** This file is part of iS3
    //**
    //** Copyright (c) 2015 Tongji University iS3 Team. All rights reserved.
    //**
    //** This library is free software; you can redistribute it and/or
    //** modify it under the terms of the GNU Lesser General Public
    //** License as published by the Free Software Foundation; either
    //** version 3 of the License, or (at your option) any later version.
    //**
    //** This library is distributed in the hope that it will be useful,
    //** but WITHOUT ANY WARRANTY; without even the implied warranty of
    //** MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the GNU
    //** Lesser General Public License for more details.
    //**
    //** In addition, as a special exception,  that plugins developed for iS3,
    //** are allowed to remain closed sourced and can be distributed under any license .
    //** These rights are included in the file LGPL_EXCEPTION.txt in this package.
    //**
    //**************************************************************************
    #endregion
    public class LPResult : DGObject
    {
        public DGObject Obj { get; set; }
        public IGraphic Graphic { get; set; }

        public TunnelAxis PlanAxis { get; set; }           // axis of the tunnel in map
        public TunnelAxis ProfileAxis { get; set; }        // axis of the tunnel in profile map
        public DrawTunnelsSettings Setting { get; set; }    // settings for analysis
    }
    
    public class CSAnalysisSettings
    {
        public bool DrawProjectionLine { get; set; }
        public bool FillInterior { get; set; }
        public double XOffset { get; set; }
        public int Interval { get; set; }

        public CSAnalysisSettings()
        {
            DrawProjectionLine = true;
            FillInterior = true;
            XOffset = 0.0;
            Interval = 10;
        }
    }

    public class LSAnalysisSettings
    {
        public bool DrawProjectionLine { get; set; }
        public bool ReverseAxisDir { get; set; }
        public double XOffset { get; set; }
        public double ZScale { get; set; }
        public int Interval { get; set; }

        public LSAnalysisSettings()
        {
            DrawProjectionLine = true;
            ReverseAxisDir = false;
            XOffset = 0.0;
            ZScale = 1.0;
            Interval = 100;
        }
    }

    public static class TunnelMappingUtility
    {
        // Project tunnels to longitudinal section
        //
        public static List<LPResult> ProjectTunnels_LS(EngineeringMap targetEMap,
            IList objList, ISpatialReference sp,
            DrawTunnelsSettings lsSettings)
        {
            List<LPResult> results = new List<LPResult>();

            foreach (DGObject obj in objList)
            {
                LPResult result = ProjectTunnel_LS(targetEMap, obj, sp, lsSettings);
                if (result != null)
                {
                    results.Add(result);
                }
            }
            return results;
        }

        public static LPResult ProjectTunnel_LS(EngineeringMap targetEMap, DGObject obj, 
            ISpatialReference sp, DrawTunnelsSettings lsSettings)
        {
            IPolyline projLine = targetEMap.profileLine;
            if (projLine == null)
                return null;
            
            Tunnel tunnel = obj as Tunnel;
            if (tunnel.LineNo == null)
                return null;
            int lineNo = tunnel.LineNo.Value;
            double mapScale = targetEMap.Scale;

            Domain domain = Globals.project.getDomain(DomainType.Structure);
            DGObjectsCollection allAxes = domain.getObjects("TunnelAxis");
            TunnelAxis axis = allAxes[tunnel.id] as TunnelAxis;
            if (axis == null)
                return null;

            double distance = 0;
            IMapPoint pt = Runtime.geometryEngine.newMapPoint(0, 0, sp);
            IMapPoint prjPt = Runtime.geometryEngine.newMapPoint(0, 0,sp);
            List<IMapPoint> upperPnts = new List<IMapPoint>();
            List<IMapPoint> lowerPnts = new List<IMapPoint>();
            int num = axis.AxisPoints.Count;
            double height = 0.0;
            if (tunnel.Height != null)
                height = tunnel.Height.Value;

            TunnelAxis profileAxis = new TunnelAxis();
            List<TunnelAxisPoint> pts = new List<TunnelAxisPoint>();
            profileAxis.AxisPoints = pts;
            profileAxis.LineNo = lineNo;

            for (int i = 0; i < num; ++i)
            {
                TunnelAxisPoint axisPt = axis.AxisPoints[i];
                pt = Runtime.geometryEngine.newMapPoint(axisPt.X, axisPt.Y, sp);

                GeomUtil.ProjectPointToPolyline(pt,
                    projLine.GetPoints(), ref distance, ref prjPt);
                distance /= mapScale;
                distance += lsSettings.xOffset;

                double X = distance;
                double Y = (axisPt.Z + height / 2.0) * lsSettings.zScale;
                IMapPoint upperPnt = Runtime.geometryEngine.newMapPoint(X, Y, sp);
                upperPnts.Add(upperPnt);

                X = distance;
                Y = (axisPt.Z - height / 2.0) * lsSettings.zScale;
                IMapPoint lowerPnt = Runtime.geometryEngine.newMapPoint(X, Y, sp);
                lowerPnts.Add(lowerPnt);

                TunnelAxisPoint newPt = new TunnelAxisPoint();
                newPt.X = distance;
                newPt.Z = axisPt.Z * lsSettings.zScale;
                newPt.Mileage = axisPt.Mileage;
                profileAxis.AxisPoints.Add(newPt);
            }
            lowerPnts.Reverse();
            upperPnts.AddRange(lowerPnts);
            upperPnts.Add(upperPnts[0]);

            IPointCollection tunnelPnts = Runtime.geometryEngine.newPointCollection();
            foreach (IMapPoint mp in upperPnts)
                tunnelPnts.Add(mp);
            IGraphic g = Runtime.graphicEngine.newPolygon(tunnelPnts);

            ISimpleLineSymbol linesymbol = Runtime.graphicEngine.newSimpleLineSymbol(
                                Colors.Black, Core.Graphics.SimpleLineStyle.Solid, 0.5);
            ISymbol symbol = Runtime.graphicEngine.newSimpleFillSymbol(
                                Colors.Red, SimpleFillStyle.Solid, linesymbol);
            g.Symbol = symbol;

            LPResult result = new LPResult();
            result.id = obj.id;
            result.name = obj.id.ToString() + targetEMap.MapID;
            result.Obj = obj;
            result.Graphic = g;
            result.PlanAxis = axis;
            result.ProfileAxis = profileAxis;
            result.Setting = lsSettings;
            return result;
        }

        // Clip the axes by mileage1 and mileage2
        //
        public static TunnelAxis ClipAxis(TunnelAxis axis, double mileage1, double mileage2)
        {
            TunnelAxis result = new TunnelAxis();
            List<TunnelAxisPoint> pts = new List<TunnelAxisPoint>();
            result.AxisPoints = pts;

            foreach (TunnelAxisPoint pt in axis.AxisPoints)
            {
                if (pt.Mileage < mileage1 || pt.Mileage > mileage2)
                    continue;
                result.AxisPoints.Add(pt);
            }
            return result;
        }


        /*
        // Convert axis points in map coordinates to TunnelAxis format
        // Note: A new TunneAxis will be returned and the result is still in map coordinates.
        //
        public static TunnelAxis ConvertMapPointsToAxis(
            ESRI.ArcGIS.Client.Geometry.PointCollection axisPts,
            double startMileage, double endMileage)
        {
            double axisLen = ArcGISMappingUtility.Length(axisPts);
            double mapScale = axisLen / (endMileage - startMileage);

            TunnelAxis mapAxis = new TunnelAxis();
            List<TunnelAxisPoint> pts = new List<TunnelAxisPoint>();
            mapAxis.AxisPoints = pts;

            double m = startMileage;
            TunnelAxisPoint pt = new TunnelAxisPoint();
            MapPoint p1 = axisPts[0];
            pt.X = p1.X;
            pt.Y = p1.Y;
            pt.Mileage = startMileage;
            mapAxis.AxisPoints.Add(pt);
            for (int i = 1; i < axisPts.Count; ++i)
            {
                pt = new TunnelAxisPoint();
                MapPoint p2 = axisPts[i];
                pt.X = p2.X;
                pt.Y = p2.Y;
                m += ArcGISMappingUtility.Distance(p1, p2) / mapScale;
                pt.Mileage = m;
                mapAxis.AxisPoints.Add(pt);

                p1 = p2;
            }
            return mapAxis;
        }

        public enum CoordinateType { XY, Z };

        // Interpolate the coordinates of one axis using the data of another axis based on the mileage.
        // For example, you can evaluate the z coordinates of a map axis based on the z coordinate of real axis.
        //
        public static void InterpolateAxis(TunnelAxis axis, TunnelAxis axisRef,
            CoordinateType evType)
        {
            TunnelAxisPoint p1, p2;
            p1 = axisRef.AxisPoints[0];
            p2 = axisRef.AxisPoints[1];
            double m1, m2;             // mileage -> m
            m1 = p1.Mileage;
            m2 = p2.Mileage;

            int i = 0, j = 0;
            for (; i < axis.AxisPoints.Count; ++i)
            {
                TunnelAxisPoint p = axis.AxisPoints[i];
                double m = p.Mileage;
                if (m < m1)
                {
                    // if m<m1, p1,p2,m1,m2 is ready to approximate p.X,p.Y,p.Z
                    // don't need to do anything
                    ;
                }
                else
                {
                    while (!(m >= m1 && m < m2) && j < axisRef.AxisPoints.Count - 1)
                    {
                        p1 = axisRef.AxisPoints[j];
                        p2 = axisRef.AxisPoints[j + 1];
                        m1 = p1.Mileage;
                        m2 = p2.Mileage;
                        j++;
                    }

                }
                double scale = (m - m1) / (m2 - m1);

                if (evType == CoordinateType.Z)
                {
                    double z1, z2;
                    z1 = p1.Z;
                    z2 = p2.Z;
                    p.Z = z1 + (z2 - z1) * scale;
                }
                else
                {
                    double x1, x2, y1, y2;
                    x1 = p1.X;
                    y1 = p1.Y;
                    x2 = p2.X;
                    y2 = p2.Y;
                    p.X = x1 + (x2 - x1) * scale;
                    p.Y = y1 + (y2 - y1) * scale;
                }
            }
        }

        // Interpolate/Extrapolate the axis according to the given start and end mileage.
        //
        public static void InterpolateAxis(
            ESRI.ArcGIS.Client.Geometry.PointCollection axisPts,
            double axisStartMileage, double axisEndMileage,
            double startMileage, double endMileage)
        {
            if (startMileage > endMileage)
            {
                double t = startMileage;
                startMileage = endMileage;
                endMileage = t;
            }

            #region Code for fast processing
            if (Math.Abs(startMileage - axisStartMileage) < GeometryAlgorithms.Zero &&
                Math.Abs(endMileage - axisEndMileage) < GeometryAlgorithms.Zero)
                return;
            if (endMileage < axisStartMileage || startMileage > axisEndMileage)
            {
                MapPoint pt1, pt2;
                pt1 = MileageToAxisPoint(startMileage, axisPts,
                    axisStartMileage, axisEndMileage);
                pt2 = MileageToAxisPoint(endMileage, axisPts,
                    axisStartMileage, axisEndMileage);
                axisPts.Clear();
                axisPts.Add(pt1);
                axisPts.Add(pt2);
                return;
            }
            #endregion

            TunnelAxis axis = ConvertMapPointsToAxis(axisPts,
                axisStartMileage, axisEndMileage);
            TunnelAxisPoint p1, p2;
            p1 = MileageToAxisPoint(startMileage, axis);
            p2 = MileageToAxisPoint(endMileage, axis);

            int index;
            int count = 0;

            if (Math.Abs(startMileage - axisStartMileage) < GeometryAlgorithms.Zero)
            {
                // do nothing
            }
            else if (startMileage < axisStartMileage)
            {
                axis.AxisPoints.Insert(0, p1);
            }
            else
            {
                for (index = 0; index < axis.AxisPoints.Count - 1; ++index)
                {
                    count++;
                    double m1 = axis.AxisPoints[index].Mileage;
                    double m2 = axis.AxisPoints[index + 1].Mileage;
                    if ((startMileage >= m1 && startMileage < m2))
                        break;
                }
                axis.AxisPoints.RemoveRange(0, count);
                axis.AxisPoints.Insert(0, p1);
            }

            if (Math.Abs(endMileage - axisEndMileage) < GeometryAlgorithms.Zero)
            {
                // do nothing
            }
            else if (endMileage > axisEndMileage)
            {
                axis.AxisPoints.Add(p2);
            }
            else
            {
                for (index = 0; index < axis.AxisPoints.Count - 1; ++index)
                {
                    double m1 = axis.AxisPoints[index].Mileage;
                    double m2 = axis.AxisPoints[index + 1].Mileage;
                    if ((endMileage > m1 && endMileage <= m2))
                        break;
                }
                count = axis.AxisPoints.Count - index - 1;
                if (count > 0)
                {
                    axis.AxisPoints.RemoveRange(index + 1, count);
                    axis.AxisPoints.Add(p2);
                }
            }

            SpatialReference sr = axisPts[0].SpatialReference;
            axisPts.Clear();
            for (int i = 0; i < axis.AxisPoints.Count; ++i)
            {
                MapPoint pt = new MapPoint();
                pt.SpatialReference = sr;
                pt.X = axis.AxisPoints[i].X;
                pt.Y = axis.AxisPoints[i].Y;
                axisPts.Add(pt);
            }
        }

        // Merge two axes according to the mileage of axis points
        //
        public static TunnelAxis MergeAxis(TunnelAxis axis1, TunnelAxis axis2)
        {
            TunnelAxis axis = new TunnelAxis();
            List<TunnelAxisPoint> pts = new List<TunnelAxisPoint>();
            axis.AxisPoints = pts;

            axis.LineNo = axis1.LineNo;
            axis.AxisPoints.AddRange(axis1.AxisPoints);
            axis.AxisPoints.AddRange(axis2.AxisPoints);
            axis.AxisPoints.Sort(CompareAxisPoints);
            return axis;
        }
        */
        
        /*
        static int CompareAxisPoints(TunnelAxisPoint p1, TunnelAxisPoint p2)
        {
            return p1.Mileage.CompareTo(p2.Mileage);
        }

        // Evaluate the axis points of a tunnel, give its geometry.
        // The geometry can only be a Polygon with 4 corner points and 4 sides.
        // The points on one side should be the same with the points on the opposite side.
        // Otherwise, the function will fail and a NULL value will be returned.
        //
        public static ESRI.ArcGIS.Client.Geometry.PointCollection
            EvaluateTunnelAxisPoints(ESRI.ArcGIS.Client.Geometry.Geometry geom)
        {
            ESRI.ArcGIS.Client.Geometry.Polygon polygon = geom
                as ESRI.ArcGIS.Client.Geometry.Polygon;
            if (polygon == null)
                return null;

            ESRI.ArcGIS.Client.Geometry.PointCollection result =
                new ESRI.ArcGIS.Client.Geometry.PointCollection();

            ESRI.ArcGIS.Client.Geometry.PointCollection pts = polygon.Rings[0];
            List<int> corners = new List<int>();     // corner points index
            corners.Add(0);
            for (int i = 1; i < pts.Count - 1; ++i)
            {
                MapPoint p0 = pts[i - 1];
                MapPoint p1 = pts[i];
                MapPoint p2 = pts[i + 1];
                double angle = GeometryAlgorithms.AngleBetweenLines(p0.X, p0.Y, p1.X, p1.Y,
                    p1.X, p1.Y, p2.X, p2.Y);
                if (Math.Abs(angle) > 30.0 * Math.PI / 180.0)
                {
                    corners.Add(i);
                }
            }

            if (corners.Count == 4)
            {
                int pts1 = corners[1] - corners[0] + 1;     // points on line1
                int pts2 = corners[2] - corners[1] + 1;     // points on line2
                int pts3 = corners[3] - corners[2] + 1;     // points on line3
                int pts4 = pts.Count - corners[3];          // points on line4

                if (pts1 > pts2)                    // use long lines: line1,line3
                {
                    int min = pts1 < pts3 ? pts1 : pts3;
                    int index1, index2;
                    MapPoint p;
                    for (int i = 0; i < min - 1; ++i)
                    {
                        index1 = i;
                        index2 = corners[3] - i;
                        p = ArcGISMappingUtility.MidPoint(pts[index1], pts[index2]);
                        result.Add(p);
                    }
                    index1 = corners[1];
                    index2 = corners[2];
                    p = ArcGISMappingUtility.MidPoint(pts[index1], pts[index2]);
                    result.Add(p);
                }
                else
                {
                    int min = pts2 < pts4 ? pts2 : pts4;
                    int index1, index2;
                    MapPoint p;
                    for (int i = 0; i < min - 1; ++i)  // use long lines: line2,line4
                    {
                        index1 = corners[1] + i;
                        index2 = pts.Count - 1 - i;
                        p = ArcGISMappingUtility.MidPoint(pts[index1], pts[index2]);
                        result.Add(p);
                    }
                    index1 = corners[2];
                    index2 = corners[3];
                    p = ArcGISMappingUtility.MidPoint(pts[index1], pts[index2]);
                    result.Add(p);
                }
                return result;
            }
            else
                return null;
        }

        public class LineSegment
        {
            public MapPoint StartPt;
            public MapPoint EndPt;
            public LineSegment()
            {
                StartPt = new MapPoint();
                EndPt = new MapPoint();
            }
        }

        public class AxisSegment : LineSegment
        {
            public double StartMileage;
            public double EndMileage;
        }
        */
        // Translate a point represented by mileage to map coordinate. 
        // Input mileage: (mileage)
        // Tunnel axis is given by (axisPts, startMileage, endMileage).
        // return value: (1) a MapPoint
        //               (2) (outSegment), the segment that contains the map point.  
        //
        /*
        public static MapPoint MileageToAxisPoint(double mileage,
            ESRI.ArcGIS.Client.Geometry.PointCollection axisPts,
            double startMileage, double endMileage,
            AxisSegment outSegment = null)
        {
            double axisLen = ArcGISMappingUtility.Length(axisPts);  // this is map length (often greater)
            double len = endMileage - startMileage;                 // this is real length
            double mapScale = axisLen / len;
            MapPoint p;

            // case 1: mileage < startMileage
            MapPoint p1 = axisPts[0];
            MapPoint p2 = axisPts[1];
            double m1 = startMileage;
            double m2 = m1 + ArcGISMappingUtility.Distance(p1, p2) / mapScale;
            double scale = 0.0;
            if (mileage < m1)
            {
                scale = (mileage - m1) / (m2 - m1);
                p = ArcGISMappingUtility.InterpolatePointOnLine(p1, p2, scale);
                if (outSegment != null)
                {
                    outSegment.StartPt = p1;
                    outSegment.EndPt = p2;
                    outSegment.StartMileage = m1;
                    outSegment.EndMileage = m2;
                }
                return p;
            }

            // case 2: startMileage <= mileage <= endMileage
            m1 = startMileage;
            m2 = startMileage;
            for (int i = 0; i < axisPts.Count - 1; ++i)
            {
                p1 = axisPts[i];
                p2 = axisPts[i + 1];
                m1 = m2;
                m2 = m1 + ArcGISMappingUtility.Distance(p1, p2) / mapScale;
                if (mileage >= m1 && mileage <= m2)
                {
                    scale = (mileage - m1) / (m2 - m1);
                    p = ArcGISMappingUtility.InterpolatePointOnLine(p1, p2, scale);
                    if (outSegment != null)
                    {
                        outSegment.StartPt = p1;
                        outSegment.EndPt = p2;
                        outSegment.StartMileage = m1;
                        outSegment.EndMileage = m2;
                    }
                    return p;
                }
            }

            // case 3: mileage > endMileage
            scale = (mileage - m1) / (m2 - m1);
            p = ArcGISMappingUtility.InterpolatePointOnLine(p1, p2, scale);
            if (outSegment != null)
            {
                outSegment.StartPt = p1;
                outSegment.EndPt = p2;
                outSegment.StartMileage = m1;
                outSegment.EndMileage = m2;
            }
            return p;
        }
        */
        // Translate a point represented by mileage to engineering coordinate: (x,y,z,m)
        //
        public static TunnelAxisPoint MileageToAxisPoint(double mileage,
            TunnelAxis axis)
        {
            TunnelAxisPoint p = new TunnelAxisPoint();
            double x = 0.0, y = 0.0, z = 0.0;

            // case 1: mileage < startMileage
            TunnelAxisPoint p1 = axis.AxisPoints[0];
            TunnelAxisPoint p2 = axis.AxisPoints[1];
            double m1 = p1.Mileage;
            double m2 = p2.Mileage;
            double scale = 0.0;
            if (mileage < m1)
            {
                scale = (mileage - m1) / (m2 - m1);
                GeometryAlgorithms.InterpolatePointOnLine(p1.X, p1.Y, p1.Z,
                    p2.X, p2.Y, p2.Z, scale, ref x, ref y, ref z);
                p.X = x;
                p.Y = y;
                p.Z = z;
                p.Mileage = mileage;
                return p;
            }

            // case 2: startMileage <= mileage <= endMileage
            for (int i = 0; i < axis.AxisPoints.Count - 1; ++i)
            {
                p1 = axis.AxisPoints[i];
                p2 = axis.AxisPoints[i + 1];
                m1 = p1.Mileage;
                m2 = p2.Mileage;
                if (mileage >= m1 && mileage <= m2)
                {
                    scale = (mileage - m1) / (m2 - m1);
                    GeometryAlgorithms.InterpolatePointOnLine(p1.X, p1.Y, p1.Z,
                        p2.X, p2.Y, p2.Z, scale, ref x, ref y, ref z);
                    p.X = x;
                    p.Y = y;
                    p.Z = z;
                    p.Mileage = mileage;
                    return p;
                }
            }

            // case 3: mileage > endMileage
            scale = (mileage - m1) / (m2 - m1);
            GeometryAlgorithms.InterpolatePointOnLine(p1.X, p1.Y, p1.Z,
                p2.X, p2.Y, p2.Z, scale, ref x, ref y, ref z);
            p.X = x;
            p.Y = y;
            p.Z = z;
            p.Mileage = mileage;
            return p;
        }

        public static IPointCollection ComputeTunnelCrossLine(double mileage, double len1, double len2,
            TunnelAxis axis, ISpatialReference sp)
        {
            IPointCollection result = Runtime.geometryEngine.newPointCollection();

            TunnelAxisPoint axisPt = MileageToAxisPoint(mileage, axis);
            TunnelAxisPoint axisPt1 = MileageToAxisPoint(mileage - 1, axis);
            TunnelAxisPoint axisPt2 = MileageToAxisPoint(mileage + 1, axis);
            double a1 = GeometryAlgorithms.LineOrientation(axisPt1.X, axisPt1.Y, axisPt.X, axisPt.Y);
            double a2 = GeometryAlgorithms.LineOrientation(axisPt.X, axisPt.Y, axisPt2.X, axisPt2.Y);
            double orient = (a1 + a2) / 2.0;

            double d1 = orient + Math.PI / 2;
            double d2 = orient - Math.PI / 2;
            double x = len1 * Math.Cos(d1) / 2;
            double y = len1 * Math.Sin(d1) / 2;
            IMapPoint p1 = Runtime.geometryEngine.newMapPoint(axisPt.X + x, axisPt.Y + y, sp);

            x = len2 * Math.Cos(d2) / 2;
            y = len2 * Math.Sin(d2) / 2;
            IMapPoint p2 = Runtime.geometryEngine.newMapPoint(axisPt.X + x, axisPt.Y + y, sp);

            result.Add(p1);
            result.Add(p2);
            return result;
        }
        /*
        // Translate an axis point to mileage. The point must lie on the axis.
        // Return value Double.NaN means the point does not line on the axis.
        // You can call ArcGISMappingUtility.ProjectPointToPolyline function to preform the projection.
        //
        public static double AxisPointToMileage(MapPoint p,
            ESRI.ArcGIS.Client.Geometry.PointCollection axisPts,
            double startMileage, double endMileage)
        {
            double axisLen = ArcGISMappingUtility.Length(axisPts);  // this is map length (often greater)
            double len = endMileage - startMileage;                 // this is real length
            double mapScale = axisLen / len;

            // case 1: startMileage <= mileage < endMileage
            double d12, d1p, d2p;
            double mileage = startMileage;
            MapPoint p1, p2;
            for (int i = 0; i < axisPts.Count - 1; ++i)
            {
                p1 = axisPts[i];
                p2 = axisPts[i + 1];
                d12 = ArcGISMappingUtility.Distance(p1, p2);
                d1p = ArcGISMappingUtility.Distance(p, p1);
                d2p = ArcGISMappingUtility.Distance(p, p2);
                if (Math.Abs(d12 - d1p - d2p) <= GeometryAlgorithms.Zero)
                {
                    mileage += d1p / mapScale;
                    return mileage;
                }
                mileage += d12 / mapScale;
            }

            // case 2: mileage < startMileage
            p1 = axisPts[0];
            p2 = axisPts[axisPts.Count - 1];
            double dToBegin = ArcGISMappingUtility.Distance(p1, p);
            double dToEnd = ArcGISMappingUtility.Distance(p2, p);
            if (dToBegin < dToEnd)
            {
                p1 = axisPts[0];
                p2 = axisPts[1];
                d12 = ArcGISMappingUtility.Distance(p1, p2);
                d1p = ArcGISMappingUtility.Distance(p, p1);
                d2p = ArcGISMappingUtility.Distance(p, p2);
                if (Math.Abs(d2p - d1p - d12) <= GeometryAlgorithms.Zero)
                {
                    mileage = startMileage - d1p / mapScale;
                    return mileage;
                }
                else
                    return Double.NaN;
            }

            // case 3: mileage > endMileage
            if (dToBegin > dToEnd)
            {
                p1 = axisPts[axisPts.Count - 2];
                p2 = axisPts[axisPts.Count - 1];
                d12 = ArcGISMappingUtility.Distance(p1, p2);
                d1p = ArcGISMappingUtility.Distance(p, p1);
                d2p = ArcGISMappingUtility.Distance(p, p2);
                if (Math.Abs(d1p - d2p - d12) <= GeometryAlgorithms.Zero)
                {
                    mileage = endMileage + d2p / mapScale;
                    return mileage;
                }
                else
                    return Double.NaN;
            }

            return Double.NaN;
        }

        // Compute tunnel cross line based on the mileage.
        // Input mileage: (mileage)
        // Input cross line length: (len1, len2), len1 is on the left, len2 is on the right
        // Tunnel axis is given by (axisPts, startMileage, endMileage).
        // 
        public static ESRI.ArcGIS.Client.Geometry.PointCollection
            ComputeTunnelCrossLine(double mileage, double len1, double len2,
            ESRI.ArcGIS.Client.Geometry.PointCollection axisPts,
            double startMileage, double endMileage)
        {
            ESRI.ArcGIS.Client.Geometry.PointCollection result =
                new ESRI.ArcGIS.Client.Geometry.PointCollection();

            AxisSegment segment = new AxisSegment();
            MapPoint p = MileageToAxisPoint(mileage, axisPts, startMileage, endMileage, segment);
            MapPoint p1 = segment.StartPt;
            MapPoint p2 = segment.EndPt;

            double orient = GeometryAlgorithms.LineOrientation(p1.X, p1.Y, p2.X, p2.Y);
            double d1 = orient + Math.PI / 2;
            double d2 = orient - Math.PI / 2;

            double x = len1 * Math.Cos(d1) / 2;
            double y = len1 * Math.Sin(d1) / 2;
            p1.X = p.X + x;
            p1.Y = p.Y + y;

            x = len2 * Math.Cos(d2) / 2;
            y = len2 * Math.Sin(d2) / 2;
            p2.X = p.X + x;
            p2.Y = p.Y + y;
            result.Add(p1);
            result.Add(p2);

            return result;
        }
         * */
        
        /*
        public static void AddGraphic(LPResult dGraphic, EngineeringMap targetEMap)
        {
            Map csMap = targetEMap.GMap as Map;
            GraphicsLayer layer = ArcGISMappingUtility.GetDrawingGraphicsLayer(csMap);

            ClientDGObject dgObj = dGraphic.Obj;
            if (dGraphic.Graphic != null)
            {
                dgObj.AssociateObjectWithGraphic(dGraphic.Graphic, targetEMap);
                layer.Graphics.Add(dGraphic.Graphic);
            }
        }
         

        // tunnel axis analysis for plan map
        //
        public static TunnelAxis TunnelAxisAnalysis(DGObject cdgTunnel,
            EngineeringMap eMap, bool reverseAxisDir)
        {
            if (eMap.MapType != EngineeringMapType.FootPrintMap)
                return null;

            Tunnel tunnel = cdgTunnel as Tunnel;
            if (tunnel == null || tunnel.LineNo == null)
                return null;

            int lineNo = tunnel.LineNo.Value;
            double startMileage = tunnel.StartMileage;
            double endMileage = tunnel.EndMileage;
            double mapScale = eMap.Scale;

            // get the graphics of the tunnel
            List<object> graphics = cdgTunnel.Graphics[eMap.MapID];
            Graphic graphic = graphics[0] as Graphic;

            // get the planar axis points of the tunnel
            // note: the X,Y coordinates of axis points are in map coordinates
            ESRI.ArcGIS.Client.Geometry.Polyline polyline = graphic.Geometry
                as ESRI.ArcGIS.Client.Geometry.Polyline;
            ESRI.ArcGIS.Client.Geometry.PointCollection axisPts;
            if (polyline != null)
                axisPts = polyline.Paths[0];
            else
                axisPts = EvaluateTunnelAxisPoints(graphic.Geometry);
            if (axisPts == null)
                return null;
            if (reverseAxisDir)
                ArcGISMappingUtility.Reverse(axisPts);

            // convert the points to axis format
            TunnelAxis mapAxis = ConvertMapPointsToAxis(axisPts, startMileage, endMileage);
            mapAxis.LineNo = lineNo;

            // get the engineering axis points of the tunnel
            // note: the X,Y coordinates of axis points are NOT in map coordinates

            TunnelAxis engineeringAxis = tunnel.Axis;

            InterpolateAxis(mapAxis, engineeringAxis, CoordinateType.Z);
            InterpolateAxis(engineeringAxis, mapAxis, CoordinateType.XY);
            TunnelAxis mergedAxis = MergeAxis(mapAxis, engineeringAxis);
            TunnelAxis axis = ClipAxis(mergedAxis, mapAxis.AxisPoints[0].Mileage,
                mapAxis.AxisPoints[mapAxis.AxisPoints.Count - 1].Mileage);
            axis.LineNo = lineNo;

            return axis;
        }
        public static TunnelAxis TunnelAxisAnalysis(DGObject cdgTunnel,
            EngineeringMap eMap, bool reverseAxisDir, double start, double end)
        {
            TunnelAxis axis = TunnelAxisAnalysis(cdgTunnel, eMap, reverseAxisDir);
            if (axis == null)
                return null;
            TunnelAxis newAxis = ClipAxis(axis, start, end);
            return newAxis;
        }
        */

        public static IPointCollection
            AxisTo2DPoints(TunnelAxis axis, ISpatialReference sp)
        {
            IPointCollection pc = Runtime.geometryEngine.newPointCollection();
            foreach (TunnelAxisPoint pt in axis.AxisPoints)
            {
                IMapPoint p = Runtime.geometryEngine.newMapPoint(pt.X, pt.Y, sp);
                pc.Add(p);
            }
            return pc;
        }
    }
}
