using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Media;

using IS3.Core;
using IS3.Core.Geometry;
using IS3.Core.Graphics;
using IS3.ShieldTunnel;

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
    public class SegmentGraphicBase : DGObject
    {
        public DGObject Obj { get; set; }

        public IGraphic Interior { get; set; }

        public double CenterX { get; set; }
        public double CenterZ { get; set; }         // this maybe scaled by a z-factor
        public double OuterDiameter { get; set; }   // this maybe scaled by a z-factor
    }

    public class CSGraphics : SegmentGraphicBase
    {
        public bool IsSegmental { get; set; }   // if true, Segments property will hold data, Outline property will be null
        public IGraphicCollection Segments { get; set; }
        public IGraphic Outline { get; set; }

    }

    public class LSGraphics : SegmentGraphicBase
    {
        public IGraphic Segment { get; set; }
    }
    
    public class ShieldTunnelMappingUtility
    {
        static ISymbol[] _segFillSymbols;

        public static IGraphicCollection NewSegments(double x, double z,
            double innerRadius, double outerRadius,
            List<Segment> segments, int keyPos, int totalKeyPos, ISpatialReference sp)
        {
            IGraphicCollection gc = Runtime.graphicEngine.newGraphicCollection();

            double a = 0.0;
            if (keyPos >= 1)
                a = (360.0 / totalKeyPos) * (keyPos - 1);

            foreach (Segment seg in segments)
            {
                double startAngle = seg.StartAngle + a;
                double centralAngle = seg.CentralAngle;
                IPointCollection pc = NewSegment(x, z, innerRadius, outerRadius, startAngle, centralAngle,sp);
                IGraphic g = Runtime.graphicEngine.newPolygon(pc);
                g.Symbol = GetSegFillSymbol(seg);
                gc.Add(g);
            }
            return gc;
        }

        public static ISymbol GetSegFillSymbol(Segment seg)
        {
            if (_segFillSymbols == null)
            {
                _segFillSymbols = new ISimpleFillSymbol[2];
                ISimpleLineSymbol lineSymbol = GraphicsUtil.GetDefaultLineSymbol();
                ISimpleFillSymbol symbol = Runtime.graphicEngine.newSimpleFillSymbol(
                                Colors.Red, SimpleFillStyle.Solid, lineSymbol);
                _segFillSymbols[0] = symbol;
                symbol = Runtime.graphicEngine.newSimpleFillSymbol(
                                Colors.Cyan, SimpleFillStyle.Solid, lineSymbol);
                _segFillSymbols[1] = symbol;
            }
            if (seg.Code.Contains("F") || seg.Code.Contains("K"))
                return _segFillSymbols[0];
            else
                return _segFillSymbols[1];
        }

        static IPointCollection NewSegment(double x, double z,
            double innerRadius, double outerRadius, double startAngle, double centralAngle, ISpatialReference sp)
        {
            IPointCollection pc = Runtime.geometryEngine.newPointCollection();
            int NUM = 10;
            double delta = centralAngle / NUM;
            double angle;
            IMapPoint p;
            for (int i = 0; i <= NUM; ++i)
            {
                angle = i * delta + startAngle;
                if (angle >= 360.0)
                    angle -= 360.0;
                angle = angle * Math.PI / 180.0;
                double X = outerRadius * Math.Sin(angle) + x;
                double Y = outerRadius * Math.Cos(angle) + z;
                p = Runtime.geometryEngine.newMapPoint(X, Y, sp);
                pc.Add(p);
            }
            for (int i = 0; i <= NUM; ++i)
            {
                angle = -1.0 * i * delta + startAngle + centralAngle;
                if (angle >= 360.0)
                    angle -= 360.0;
                angle = angle * Math.PI / 180.0;
                double X = innerRadius * Math.Sin(angle) + x;
                double Y = innerRadius * Math.Cos(angle) + z;
                p = Runtime.geometryEngine.newMapPoint(X, Y, sp);
                pc.Add(p);
            }
            pc.Add(pc[0]);

            return pc;
        }
    }
}
