using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using IS3.Core;
using IS3.Core.Geometry;
using IS3.Core.Graphics;
using IS3.Geology;

namespace IS3.SimpleStructureTools.Helper.Mapping
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
    public class StratumMappingUtility
    {
        public class Setting
        {
            public bool AllStrata { get; set; }
        }

        public class Result
        {
            public DGObject StratumObj { get; set; }

            public double Top { get; set; }                 // stratum top elevation
            public double Thickness { get; set; }           // stratum thickness above the tunnel
            public double ThicknessInside { get; set; }     // stratum thickness inside the tunnel
            public double ThicknessBelow { get; set; }      // stratum thickness below the tunnel
            public double Depth { get; set; }               // distance from the stratum top to (tunnelX, tunnelZ)

            public bool IsOverlap { get; set; }             // flag indicates if tunnel overlaps with stratum
            public double MinDistToTop { get; set; }        // minimum distance from stratum to tunnel top
            public double MinDistToBottom { get; set; }     // minimum distance from stratum to tunnel bottom

            public List<IMapPoint> intersectPnts { get; set; }       // used with pairs, e.g. (0-1),(2-3),(4-5),...
        }

        // tunnelX, tunnelZ is the coordinate of tunnel crown
        // h is the height of tunnel
        //
        public static List<Result> StrataOnTunnel(double tunnelX, double tunnelZ, double h,
            IView profileView, string stLayerID, Setting setting)
        {
            IGraphicsLayer gLayerSt = profileView.getLayer(stLayerID);
            if (gLayerSt == null)
                return null;
            
            List<Result> results = new List<Result>();

            Project prj = Globals.project;
            Domain geology = prj.getDomain(DomainType.Geology);
            DGObjectsCollection stratum = geology.getObjects("Stratum");
            var stratumList = stratum.merge();
            foreach (DGObject obj in stratumList)
            {
                Stratum strata = obj as Stratum;
                IGraphicCollection gc = gLayerSt.getGraphics(strata);

                if (gc == null)
                    continue;

                Result result = StratumOnTunnel(tunnelX, tunnelZ, h, gc, profileView.spatialReference);
                if (result != null)
                {
                    result.StratumObj = gLayerSt.getObject(gc[0]);
                    if (result.Depth > 0 || setting.AllStrata)
                        results.Add(result);
                }
            }
            return results;
        }

        public static Result StratumOnTunnel(double tunnelX, double tunnelZ, double h,
            IGraphicCollection sGraphics, ISpatialReference sp)
        {
            List<IMapPoint> intersectPnts = new List<IMapPoint>();
            foreach (object obj in sGraphics)
            {
                IGraphic sGraph = obj as IGraphic;
                if (sGraph == null)
                    continue;
                IPolygon sPoly = sGraph.Geometry as IPolygon;
                if (sPoly == null)
                    continue;
                IPointCollection sPts = sPoly.GetPoints();

                // compute the stratum's maximum coordinates
                double xmin, xmax, ymin, ymax;
                xmin = sPts[0].X;
                xmax = sPts[0].X;
                ymin = sPts[0].Y;
                ymax = sPts[0].Y;
                foreach (IMapPoint p in sPts)
                {
                    if (p.X < xmin) xmin = p.X;
                    if (p.X > xmax) xmax = p.X;
                    if (p.Y < ymin) ymin = p.Y;
                    if (p.Y > ymax) ymax = p.Y;
                }

                if (tunnelX < xmin || tunnelX > xmax)
                    continue;

                for (int i = 0; i < sPts.Count - 1; ++i)
                {
                    IMapPoint p0 = sPts[i];
                    IMapPoint p1 = sPts[i + 1];
                    double x = 0, y = 0;
                    bool intersect = GeometryAlgorithms.LineIntersectWithLine(p0.X, p0.Y, p1.X, p1.Y,
                        tunnelX, ymin, tunnelX, ymax, ref x, ref y, ExtendOption.None);
                    if (intersect)
                    {
                        IMapPoint p = Runtime.geometryEngine.newMapPoint(x, y,sp);
                        intersectPnts.Add(p);
                    }
                }
            }

            if (intersectPnts.Count > 0)
            {
                List<double> y = new List<double>();
                for (int i = 0; i < intersectPnts.Count; ++i)
                {
                    y.Add(intersectPnts[i].Y);
                }
                y.Sort();

                for (int i = 0; i < y.Count / 2; ++i)
                {
                    if (tunnelZ > y[2 * i] && tunnelZ < y[2 * i + 1])
                    {
                        y.Insert(2 * i + 1, tunnelZ);
                        y.Insert(2 * i + 1, tunnelZ);
                        break;
                    }
                }
                double bottom = tunnelZ - h;
                for (int i = 0; i < y.Count / 2; ++i)
                {
                    if (bottom > y[2 * i] && bottom < y[2 * i + 1])
                    {
                        y.Insert(2 * i + 1, bottom);
                        y.Insert(2 * i + 1, bottom);
                        break;
                    }
                }

                intersectPnts.Clear();
                for (int i = 0; i < y.Count; ++i)
                {
                    IMapPoint p = Runtime.geometryEngine.newMapPoint(tunnelX, y[i], sp);
                    intersectPnts.Add(p);
                }

                Result res = new Result();
                res.Depth = y[y.Count - 1] - tunnelZ;
                res.Top = y[y.Count - 1];
                res.Thickness = 0;
                res.ThicknessInside = 0;
                res.ThicknessBelow = 0;
                res.intersectPnts = intersectPnts;

                double y1, y2, t;
                for (int i = 0; i < y.Count / 2; ++i)
                {
                    y1 = y[i * 2];
                    y2 = y[i * 2 + 1];
                    t = y2 - y1;
                    if (y1 >= tunnelZ)
                        res.Thickness += t;
                    else if (y1 >= bottom)
                        res.ThicknessInside += t;
                    else
                        res.ThicknessBelow += t;
                }

                res.IsOverlap = false;
                res.MinDistToTop = 0;
                res.MinDistToBottom = 0;
                if (y[0] > tunnelZ)
                    res.MinDistToTop = y[0] - tunnelZ;
                else if (y[y.Count - 1] < bottom)
                    res.MinDistToBottom = bottom - y[y.Count - 1];
                else
                    res.IsOverlap = true;

                return res;
            }
            return null;
        }

        // Get strata top coordinate at the specified location
        //
        public static double? StratumTop(double tunnelX, IView profileView, string stLayerID)
        {
            IGraphicsLayer gLayerSt = profileView.getLayer(stLayerID);
            if (gLayerSt == null)
                return null;

            List<double> results = new List<double>();

            Project prj = Globals.project;
            Domain geology = prj.getDomain(DomainType.Geology);
            DGObjectsCollection stratum = geology.getObjects("Stratum");
            List<DGObject> stratumList = stratum.merge();
            foreach (DGObject obj in stratumList)
            {
                Stratum strata = obj as Stratum;
                IGraphicCollection gc = gLayerSt.getGraphics(strata);

                if (gc == null)
                    continue;

                double? top = StratumTop(tunnelX, gc, profileView.spatialReference);
                if (top != null)
                {
                    results.Add(top.Value);
                }
            }
            if (results.Count == 0)
                return null;

            results.Sort();
            return results[results.Count - 1];
        }
        public static double? StratumTop(double tunnelX, IGraphicCollection sGraphics, ISpatialReference sp)
        {
            List<IMapPoint> intersectPnts = new List<IMapPoint>();
            foreach (var obj in sGraphics)
            {
                IGraphic sGraph = obj as IGraphic;
                if (sGraph == null)
                    continue;
                IPolygon sPoly = sGraph.Geometry as IPolygon;
                if (sPoly == null)
                    continue;
                IPointCollection sPts = sPoly.GetPoints();

                // compute the stratum's maximum coordinates
                double xmin, xmax, ymin, ymax;
                xmin = sPts[0].X;
                xmax = sPts[0].X;
                ymin = sPts[0].Y;
                ymax = sPts[0].Y;
                foreach (IMapPoint p in sPts)
                {
                    if (p.X < xmin) xmin = p.X;
                    if (p.X > xmax) xmax = p.X;
                    if (p.Y < ymin) ymin = p.Y;
                    if (p.Y > ymax) ymax = p.Y;
                }

                if (tunnelX < xmin || tunnelX > xmax)
                    continue;

                for (int i = 0; i < sPts.Count - 1; ++i)
                {
                    IMapPoint p0 = sPts[i];
                    IMapPoint p1 = sPts[i + 1];
                    double x = 0, y = 0;
                    bool intersect = GeometryAlgorithms.LineIntersectWithLine(p0.X, p0.Y, p1.X, p1.Y,
                        tunnelX, ymin, tunnelX, ymax, ref x, ref y, ExtendOption.Other);
                    if (intersect)
                    {
                        IMapPoint p = Runtime.geometryEngine.newMapPoint(x, y,sp);
                        intersectPnts.Add(p);
                    }
                }
            }

            if (intersectPnts.Count > 0)
            {
                List<double> y = new List<double>();
                for (int i = 0; i < intersectPnts.Count; ++i)
                {
                    y.Add(intersectPnts[i].Y);
                }
                y.Sort();

                return y[y.Count - 1];
            }

            return null;
        }
    }
}
