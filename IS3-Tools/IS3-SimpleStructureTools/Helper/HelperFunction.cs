using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Data;
using System.Windows.Forms;
using System.Threading.Tasks;

using IS3.Core;
using IS3.Core.Geometry;
using IS3.Core.Graphics;
using IS3.Geology;

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
    public class HelperFunction
    {
        #region geometry and graphic functions
        public static IGraphic NewCircle(double x,double y, double r, ISpatialReference sp)
        {
            int NUM = 128;
            double[] px = new double[NUM];
            double[] py = new double[NUM];
            GeometryAlgorithms.CircleToPoints(x, y, r, NUM, px, py, AngleDirection.CounterClockwise);

            IPointCollection pc = Runtime.geometryEngine.newPointCollection();
            for (int i = 0; i < NUM; i++)
            {
                IMapPoint p = Runtime.geometryEngine.newMapPoint(px[i], py[i], sp);
                pc.Add(p);
            }
            pc.Add(pc[0]);

            return Runtime.graphicEngine.newPolygon(pc);
        }
        #endregion

        #region view, layer, domain functions
        public static List<Tuple<IGraphic, string>> GetPolylines(IGraphicsLayer gLayer)
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
                if (geom.GeometryType == GeometryType.Polyline)
                {
                    Tuple<IGraphic, string> turple = new Tuple<IGraphic, string>(
                        g, "Polyline#" + i.ToString());
                    i++;
                    result.Add(turple);
                }
            }
            return result;
        }

        //the layer loaded from geodatabase is readonly
        //getLayer function return a layer from geodatabse
        //geteLayer function return a layer that the draw graphics can be saved in
        //there is a letter e before layerID of elayer
        public static IGraphicsLayer GeteLayer(IView view, string layerID)
        {
            string elayerID = "e" + layerID;
            return GetLayer(view, elayerID);
        }
        public static IGraphicsLayer GetLayer(IView view, string layerID)
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
            return gLayer;
        }

        public static HashSet<IGraphicsLayer> GetAllLayerID(IView view, Domain domain, string typeName)
        {
            HashSet<IGraphicsLayer> graphicLayers = new HashSet<IGraphicsLayer>();
            DGObjectsCollection dgObjColl = domain.getObjects(typeName);
            List<DGObject> objList = dgObjColl.merge();
            foreach (DGObject obj in objList)
            {
                string layerID = obj.parent.definition.GISLayerName;
                IGraphicsLayer gLayer = HelperFunction.GetLayer(view, layerID);
                graphicLayers.Add(gLayer);
            }
            return graphicLayers;
        }

        public static Domain GetAnalysisDomain()
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

        public static DGObjects GetDGObjsByName(DGObjectsCollection dgObjsColl, string name)
        {
            if (dgObjsColl == null)
                return null;
            
            DGObjects dgObjs = null;
            foreach (DGObjects objs in dgObjsColl)
            {
                if (objs.definition.Name == name)
                {
                    dgObjs = objs;
                    break;
                }
            }
            return dgObjs;
        }

        public static DGObjects NewObjsInAnalysisDomain(string Type, string Name, 
            bool HasGeometry = false, string GISLayerName = "")
        {
            DGObjectsDefinition eDef = new DGObjectsDefinition();
            eDef.Type = Type;
            eDef.Name = Name;
            eDef.HasGeometry = HasGeometry;
            string layerName = "";
            if (HasGeometry)
                layerName = "e" + GISLayerName;
            eDef.GISLayerName = layerName;
            DGObjects dgObjects = new DGObjects(eDef);

            Domain analysisDomain = GetAnalysisDomain();
            dgObjects.parent = analysisDomain;
            analysisDomain.objsDefinitions[eDef.Name] = eDef;
            analysisDomain.objsContainer[eDef.Name] = dgObjects;

            return dgObjects;
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
