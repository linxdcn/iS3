using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections;
using System.Windows.Media;

using IS3.Core;
using IS3.Core.Geometry;
using IS3.Core.Graphics;
using IS3.Geology;

namespace IS3.SimpleGeologyTools
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

    public class ProjectBoreholeResult
    {
        public Dictionary<int,IGraphicCollection> bhGraphics { get; set; }
        public Dictionary<int, IGraphicCollection> stGraphics { get; set; }
        public IEnvelope Extent { get; set; }

        public ProjectBoreholeResult()
        {
            bhGraphics = new Dictionary<int, IGraphicCollection>();
            stGraphics = new Dictionary<int, IGraphicCollection>();
        }
    }

    public class SimpleProfileAnalysis
    {
        // sorted borehole
        class ProjectedBorehole
        {
            public double Distance;
            public Borehole Borehole;
            public IMapPoint Pos;
        }

        // stratum graphic
        class STGraphic
        {
            public int StratumID { get; set; }
            public IGraphic Graphic { get; set; }
        }

        #region helper funtions
        static IMapPoint NewMapPoint(double x, double y)
        {
            return Runtime.geometryEngine.newMapPoint(x, y);
        }
        static IGraphic NewTriangle(IMapPoint p1, IMapPoint p2, IMapPoint p3)
        {
            return Runtime.graphicEngine.newTriangle(p1, p2, p3);
        }
        static IGraphic NewQuadrilateral(IMapPoint p1, IMapPoint p2, IMapPoint p3, IMapPoint p4)
        {
            return Runtime.graphicEngine.newQuadrilateral(p1, p2, p3, p4);
        }
        static IGraphic NewPentagon(IMapPoint p1, IMapPoint p2, IMapPoint p3, IMapPoint p4, IMapPoint p5)
        {
            return Runtime.graphicEngine.newPentagon(p1, p2, p3, p4, p5);
        }
        static IGraphic NewPolygon(IPointCollection part)
        {
            return Runtime.graphicEngine.newPolygon(part);
        }
        static IPointCollection NewPointCollection()
        {
            return Runtime.geometryEngine.newPointCollection();
        }
        static IGraphicCollection NewGraphicCollection()
        {
            return Runtime.graphicEngine.newGraphicCollection();
        }
        static ISymbol GetDefaultFillSymbols(int index)
        {
            return GraphicsUtil.GetDefaultFillSymbols(index);
        }
        #endregion

        public static ProjectBoreholeResult
            ProjectBoreholes(List<Tuple<Borehole, IMapPoint>> input,
            IPolyline projLine, GeoProjSettings geoProjSettings)
        {
            if (input == null || projLine == null)
                return null;

            // remove the boreholes that do not have geology infos,
            // so called 'empty' boreholes
            // 
            input.RemoveAll(x => 
                x.Item1.Geologies == null || x.Item1.Geologies.Count == 0
                || x.Item1.Top == x.Item1.Base);

            // sort the boreholes according to their projected position on the projection line
            //
            List<ProjectedBorehole> sortedList = new List<ProjectedBorehole>();
            foreach (var tuple in input)
            {
                Borehole bh = tuple.Item1;
                IMapPoint p = tuple.Item2;

                double distance = 0;
                IMapPoint prjPnt = null;
                bool canProject = GeomUtil.ProjectPointToPolyline(p,
                    projLine.GetPoints(), ref distance, ref prjPnt);
                if (geoProjSettings.clipInProjectionLine == true && canProject == false)
                    continue;

                distance /= geoProjSettings.scale;
                distance += geoProjSettings.xOffset;
                ProjectedBorehole prjBorehole = new ProjectedBorehole();
                prjBorehole.Borehole = bh;
                prjBorehole.Distance = distance;
                prjBorehole.Pos = p;
                sortedList.Add(prjBorehole);
            }
            if (sortedList.Count == 0)
                return null;

            sortedList.Sort((x, y) => x.Distance.CompareTo(y.Distance));

            // extend borholes to same depth
            //
            if (geoProjSettings.extendBorehole)
            {
                ExtendBoreholes(input);
            }

            // perform the projection
            //
            ProjectBoreholeResult result = new ProjectBoreholeResult();

            List<STGraphic> stResults = new List<STGraphic>();
            ProjectedBorehole previousBh = null;
            foreach (ProjectedBorehole projectedBorehole in sortedList)
            {
                Borehole bh = projectedBorehole.Borehole;

                // draw strata
                if (geoProjSettings.drawStratum)
                {
                    if (previousBh == null)
                    {
                        previousBh = projectedBorehole;
                    }
                    else
                    {
                        List<STGraphic> stGraphics = LinkBorehole(
                            previousBh.Borehole, projectedBorehole.Borehole,
                            previousBh.Distance, projectedBorehole.Distance,
                            geoProjSettings.zScale);
                        previousBh = projectedBorehole;
                        stResults.AddRange(stGraphics);
                    }
                }

                // draw borehole 
                if (geoProjSettings.drawBorehole)
                {
                    IGraphicCollection gc = ProjectBorehole(bh,
                        projectedBorehole.Distance, geoProjSettings.zScale);
                    result.bhGraphics[bh.id] = gc;
                }
            }

            // transfrom strata results
            foreach (STGraphic stGraphic in stResults)
            {
                int id = stGraphic.StratumID;
                if (result.stGraphics.ContainsKey(id))
                {
                    IGraphicCollection gc = result.stGraphics[id];
                    gc.Add(stGraphic.Graphic);
                }
                else
                {
                    IGraphicCollection gc = NewGraphicCollection();
                    gc.Add(stGraphic.Graphic);
                    result.stGraphics[id] = gc;
                }
            }

            // calculate extent
            IEnvelope ext = null;
            foreach (IGraphicCollection gc in result.bhGraphics.Values)
            {
                IEnvelope itemExt = GraphicsUtil.GetGraphicsEnvelope(gc);
                if (ext == null)
                    ext = itemExt;
                else
                    ext = ext.Union(itemExt);
            }
            result.Extent = ext;

            return result;
        }

        static int CompareProjectedBorehole(ProjectedBorehole projectedBh1,
            ProjectedBorehole projectedBh2)
        {
            return projectedBh1.Distance.CompareTo(projectedBh2.Distance);
        }

        static void ExtendBoreholes(List<Tuple<Borehole, IMapPoint>> bhList)
        {
            if (bhList.Count == 0)
                return;

            int maxBaseIndex = 0;
            double maxBase = 1e10;
            for (int i = 0; i < bhList.Count; ++i)
            {
                Borehole bh = bhList[i].Item1;
                if (bh.Base < maxBase)
                {
                    maxBase = bh.Base;
                    maxBaseIndex = i;
                }
            }

            Borehole bhMax = bhList[maxBaseIndex].Item1;
            for (int i = 0; i < bhList.Count; ++i)
                ExtendBorehole(bhList[i].Item1, bhMax);
        }

        static void ExtendBorehole(Borehole bh, Borehole bhMax)
        {
            List<BoreholeGeology> bhGeos = bh.Geologies;
            List<BoreholeGeology> bhMaxGeos = bhMax.Geologies;

            double bhBase = bh.Base;
            double bhMaxBase = bhMax.Base;
            if (bhGeos.Count == 0 || bhBase <= bhMaxBase)
                return;
            int index;
            for (index = 0; index < bhMaxGeos.Count; ++index)
            {
                if (bhMaxGeos[index].Base <= bhBase)
                    break;
            }
            if (index == bhMaxGeos.Count)
                index--;
            if (bhGeos[bhGeos.Count - 1].StratumID == bhMaxGeos[index].StratumID)
                bhGeos[bhGeos.Count - 1].Base = bhMaxGeos[index].Base;
            else if (bhGeos[bhGeos.Count - 1].StratumID < bhMaxGeos[index].StratumID)
            {
                BoreholeGeology geo = new BoreholeGeology();
                geo.StratumID = bhMaxGeos[index].StratumID;
                geo.Top = bh.Base;
                geo.Base = bhMaxGeos[index].Base;
                bhGeos.Add(geo);
            }
            else
            {
                while (bhGeos[bhGeos.Count - 1].StratumID > bhMaxGeos[index].StratumID
                    && index < bhMaxGeos.Count - 1)
                    index++;
                bhGeos[bhGeos.Count - 1].Base = bhMaxGeos[index].Base;
            }

            for (int i = index + 1; i < bhMaxGeos.Count; ++i)
            {
                bhGeos.Add(bhMaxGeos[i]);
            }
            bh.Base = bhMaxBase;
        }

        static List<STGraphic> LinkBorehole(Borehole bh1, Borehole bh2,
            double x1, double x2, double zScale)
        {
            List<BoreholeGeology> bhGeos1 = new List<BoreholeGeology>();
            List<BoreholeGeology> bhGeos2 = new List<BoreholeGeology>();
            bhGeos1.AddRange(bh1.Geologies);
            bhGeos2.AddRange(bh2.Geologies);

            int firstStratum = bhGeos1[0].StratumID;
            if (firstStratum > bhGeos2[0].StratumID)
                firstStratum = bhGeos2[0].StratumID;
            int lastStratum = bhGeos1[bhGeos1.Count - 1].StratumID;
            if (lastStratum < bhGeos2[bhGeos2.Count - 1].StratumID)
                lastStratum = bhGeos2[bhGeos2.Count - 1].StratumID;

            List<int> containers1 = new List<int>();
            List<int> containers2 = new List<int>();
            bool simple1 = FillBoreholeGeology(bhGeos1, firstStratum, lastStratum, containers1);
            bool simple2 = FillBoreholeGeology(bhGeos2, firstStratum, lastStratum, containers2);

            List<STGraphic> stGraphics = new List<STGraphic>();
            if (simple1 && simple2 || !simple1 && !simple2)
            {
                List<STGraphic> results = LinkSimpleBoreholeGeology(
                    bhGeos1, bhGeos2, x1, x2, zScale);
                stGraphics.AddRange(results);
            }
            else if (simple1 && !simple2)
            {
                List<STGraphic> results = LinkBoreholeGeologyWithLens(
                    bhGeos1, bhGeos2, x1, x2, containers2, true, zScale);
                stGraphics.AddRange(results);
            }
            else
            {
                List<STGraphic> results = LinkBoreholeGeologyWithLens(
                    bhGeos1, bhGeos2, x1, x2, containers1, false, zScale);
                stGraphics.AddRange(results);
            }

            return stGraphics;
        }

        // Fill borehole geology with zero-thickness strata
        // e.g.
        //      #1: Sequence of simple strata : (2,3,8) -> (2,3,4,5,6,7,8)
        //      where (4,5,6,7) are inserted strata with 0 thickness
        //      #2: Sequence of strata with lens: (2,3,2,8) -> (2,3,2,3,4,5,6,7,8)
        //      where (3,4,5,6,7) are inserted strata with 0 thickness
        //      In case #2, container stratum 2 will be returned.
        static bool FillBoreholeGeology(List<BoreholeGeology> bhGeos, int first, int last,
            List<int> containers)
        {
            bool simple = true;
            for (int i = first, index = 0; i <= last; ++i, ++index)
            {
                if (index == bhGeos.Count)
                    return simple;

                int current = bhGeos[index].StratumID;
                if (current > i)
                {
                    for (int j = i; j < current; ++j)
                    {
                        BoreholeGeology bhGeo = new BoreholeGeology();
                        bhGeo.StratumID = j;
                        bhGeo.Top = bhGeos[index].Top;
                        bhGeo.Base = bhGeo.Top;
                        bhGeos.Insert(index++, bhGeo);
                    }
                }
                else if (current < i)
                {
                    // for stratum lens, see #2
                    // move the next stratum (index+1)
                    index++;
                    int container = current;    // this is a stratum that contain lens
                    if (index < bhGeos.Count)
                    {
                        current = bhGeos[index].StratumID;
                        for (int j = container + 1; j < current; ++j)
                        {
                            BoreholeGeology bhGeo = new BoreholeGeology();
                            bhGeo.StratumID = j;
                            bhGeo.Top = bhGeos[index].Top;
                            bhGeo.Base = bhGeo.Top;
                            bhGeos.Insert(index++, bhGeo);
                        }
                    }
                    simple = false;
                    containers.Add(container);
                }
                i = current;
            }
            return simple;
        }

        static List<STGraphic> LinkSimpleBoreholeGeology(
            List<BoreholeGeology> bhGeos1, List<BoreholeGeology> bhGeos2,
            double x1, double x2, double zScale)
        {
            int count = bhGeos1.Count;
            if (count > bhGeos2.Count)
                count = bhGeos2.Count;
            List<STGraphic> stGraphics = new List<STGraphic>();

            IMapPoint p1 = NewMapPoint(x1, bhGeos1[0].Top * zScale);
            IMapPoint p2 = NewMapPoint(x2, bhGeos2[0].Top * zScale);
            IMapPoint MidOfp1p2 = NewMapPoint((p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2);
            IMapPoint p3, p4;
            bool useMidPnt = false;
            IGraphic g;
            for (int i = 0; i < count; ++i)
            {
                BoreholeGeology bhGeo1 = bhGeos1[i];
                BoreholeGeology bhGeo2 = bhGeos2[i];
                if (bhGeo1.Top == bhGeo1.Base && bhGeo2.Top == bhGeo2.Base)
                    continue;

                if (bhGeo1.Top == bhGeo1.Base)
                {
                    p3 = p1;
                    p4 = NewMapPoint(x2, bhGeo2.Base * zScale);
                    g = NewTriangle(MidOfp1p2, p2, p4);
                    useMidPnt = true;
                }
                else if (bhGeo2.Top == bhGeo2.Base)
                {
                    p3 = NewMapPoint(x1, bhGeo1.Base * zScale);
                    p4 = p2;
                    g = NewTriangle(p1, MidOfp1p2, p3);
                    useMidPnt = true;
                }
                else
                {
                    p3 = NewMapPoint(x1, bhGeo1.Base * zScale);
                    p4 = NewMapPoint(x2, bhGeo2.Base * zScale);
                    if (useMidPnt)
                        g = NewPentagon(p1, MidOfp1p2, p2, p4, p3);
                    else
                        g = NewQuadrilateral(p1, p2, p4, p3);
                    MidOfp1p2 = NewMapPoint((p3.X + p4.X) / 2, (p3.Y + p4.Y) / 2);
                    useMidPnt = false;
                }
                p1 = p3;
                p2 = p4;
                g.Symbol = GetDefaultFillSymbols(bhGeo1.StratumID);

                STGraphic stGraphic = new STGraphic();
                stGraphic.StratumID = bhGeo1.StratumID;
                stGraphic.Graphic = g;
                stGraphics.Add(stGraphic);
            }

            return stGraphics;
        }

        static List<STGraphic> LinkBoreholeGeologyWithLens(
            List<BoreholeGeology> bhGeos1, List<BoreholeGeology> bhGeos2,
            double x1, double x2,
            List<int> containers, bool rightBorehole, double zScale)
        {
            int count = bhGeos1.Count;
            if (count > bhGeos2.Count)
                count = bhGeos2.Count;
            List<STGraphic> stGraphics = new List<STGraphic>();
            STGraphic stGraphic = null;

            IMapPoint p1 = NewMapPoint(x1, bhGeos1[0].Top * zScale);
            IMapPoint p2 = NewMapPoint(x2, bhGeos2[0].Top * zScale);
            IMapPoint MidOfp1p2 = NewMapPoint((p1.X + p2.X) / 2, (p1.Y + p2.Y) / 2);
            IMapPoint p3, p4;
            bool useMidPnt = false;
            IGraphic g;
            int index1 = 0, index2 = 0;
            for (int i = 0; i < count; ++i)
            {
                if (index1 == bhGeos1.Count || index2 == bhGeos2.Count)
                    break;

                BoreholeGeology bhGeo1 = bhGeos1[index1++];
                BoreholeGeology bhGeo2 = bhGeos2[index2++];
                if (bhGeo1.Top == bhGeo1.Base && bhGeo2.Top == bhGeo2.Base)
                    continue;

                BoreholeGeology bhGeo;
                if (rightBorehole)
                    bhGeo = bhGeo2;
                else
                    bhGeo = bhGeo1;

                if (containers.Contains(bhGeo.StratumID))
                {
                    BoreholeGeology lens;
                    BoreholeGeology bhGeo_part2;
                    IMapPoint plens_top, plens_base, plens_mid;
                    if (rightBorehole)
                    {
                        lens = bhGeos2[index2++];
                        bhGeo_part2 = bhGeos2[index2++];
                        p3 = NewMapPoint(x1, bhGeo1.Base * zScale);
                        p4 = NewMapPoint(x2, bhGeo_part2.Base * zScale);
                        plens_top = NewMapPoint(x2, lens.Top * zScale);
                        plens_base = NewMapPoint(x2, lens.Base * zScale);
                    }
                    else
                    {
                        lens = bhGeos1[index1++];
                        bhGeo_part2 = bhGeos1[index1++];
                        p3 = NewMapPoint(x1, bhGeo_part2.Base * zScale);
                        p4 = NewMapPoint(x2, bhGeo2.Base * zScale);
                        plens_top = NewMapPoint(x1, lens.Top * zScale);
                        plens_base = NewMapPoint(x1, lens.Base * zScale);
                    }
                    if (bhGeo.Top == bhGeo.Base)
                        plens_mid = MidOfp1p2;
                    else
                        plens_mid = NewMapPoint((x1 + x2) / 2, (lens.Top + lens.Base) * zScale / 2);
                    IPointCollection pc = NewPointCollection();
                    if (rightBorehole)
                    {
                        pc.Add(p1);
                        if (useMidPnt)
                            pc.Add(MidOfp1p2);
                        if (bhGeo.Top != bhGeo.Base)
                        {
                            pc.Add(p2);
                            pc.Add(plens_top);
                        }
                        pc.Add(plens_mid);
                        pc.Add(plens_base);
                        pc.Add(p4);
                        pc.Add(p3);
                        pc.Add(p1);
                    }
                    else
                    {
                        pc.Add(p2);
                        pc.Add(p4);
                        pc.Add(p3);
                        pc.Add(plens_base);
                        pc.Add(plens_mid);
                        if (bhGeo.Top != bhGeo.Base)
                        {
                            pc.Add(plens_top);
                            pc.Add(p1);
                        }
                        if (useMidPnt)
                            pc.Add(MidOfp1p2);
                        pc.Add(p2);
                    }

                    g = NewPolygon(pc);
                    g.Symbol = GetDefaultFillSymbols(bhGeo.StratumID);
                    stGraphic = new STGraphic();
                    stGraphic.StratumID = bhGeo.StratumID;
                    stGraphic.Graphic = g;
                    stGraphics.Add(stGraphic);

                    g = NewTriangle(plens_mid, plens_top, plens_base);
                    g.Symbol = GetDefaultFillSymbols(lens.StratumID);
                    stGraphic = new STGraphic();
                    stGraphic.StratumID = lens.StratumID;
                    stGraphic.Graphic = g;
                    stGraphics.Add(stGraphic);

                    p1 = p3;
                    p2 = p4;
                    MidOfp1p2 = NewMapPoint((p3.X + p4.X) / 2, (p3.Y + p4.Y) / 2);
                    useMidPnt = false;
                    continue;
                }
                if (bhGeo1.Top == bhGeo1.Base)
                {
                    p3 = p1;
                    p4 = NewMapPoint(x2, bhGeo2.Base * zScale);
                    g = NewTriangle(MidOfp1p2, p2, p4);
                    useMidPnt = true;
                }
                else if (bhGeo2.Top == bhGeo2.Base)
                {
                    p3 = NewMapPoint(x1, bhGeo1.Base * zScale);
                    p4 = p2;
                    g = NewTriangle(p1, MidOfp1p2, p3);
                    useMidPnt = true;
                }
                else
                {
                    p3 = NewMapPoint(x1, bhGeo1.Base * zScale);
                    p4 = NewMapPoint(x2, bhGeo2.Base * zScale);
                    if (useMidPnt)
                        g = NewPentagon(p1, MidOfp1p2, p2, p4, p3);
                    else
                        g = NewQuadrilateral(p1, p2, p4, p3);
                    MidOfp1p2 = NewMapPoint((p3.X + p4.X) / 2, (p3.Y + p4.Y) / 2);
                    useMidPnt = false;
                }
                p1 = p3;
                p2 = p4;
                g.Symbol = GetDefaultFillSymbols(bhGeo1.StratumID);
                stGraphic = new STGraphic();
                stGraphic.StratumID = bhGeo1.StratumID;
                stGraphic.Graphic = g;
                stGraphics.Add(stGraphic);
            }

            return stGraphics;
        }

        static IGraphicCollection ProjectBorehole(Borehole bh,
            double distance, double zScale)
        {
            double d = distance;
            IGraphicCollection graphics = NewGraphicCollection();

            double top = bh.Geologies[0].Top * zScale;
            double bottom;
            double width = 0.5;
            IMapPoint p1 = NewMapPoint(d - width, top);
            IMapPoint p2 = NewMapPoint(d + width, top);
            for (int i = 0; i < bh.Geologies.Count; ++i)
            {
                bottom = bh.Geologies[i].Base * zScale;
                IMapPoint p3 = NewMapPoint(d - width, bottom);
                IMapPoint p4 = NewMapPoint(d + width, bottom);
                IGraphic g = NewQuadrilateral(p1, p2, p4, p3);
                p1 = p3;
                p2 = p4;
                g.Symbol = GetDefaultFillSymbols(bh.Geologies[i].StratumID);
                graphics.Add(g);
            }

            return graphics;
        }

    }
}
