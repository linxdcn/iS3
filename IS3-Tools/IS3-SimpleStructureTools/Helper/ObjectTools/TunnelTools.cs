using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using IS3.Core;
using IS3.Core.Geometry;
using IS3.Core.Graphics;
using IS3.ShieldTunnel;
using IS3.SimpleStructureTools.Helper;

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
    public static class TunnelTools
    {
        // Load tunnel footprint map axis by query the map.
        // Note:
        //  1. Tunnel3DAxes layer must exist in the map.
        //  2. The axis of a tunnel must be a polyline with its [LineNo] be defined.
        //  3. The number of axis points must be the same as that of [TunnelAxesPoints] in the database.
        //
        /*
        public static void LoadTunnelMapAxis(ClientDGObject dgObj)
        {
            if (dgObj.Attributes.ContainsKey("Axis"))
                return;

            App app = Application.Current as App;
            MainPage mainPage = app.MainPage;
            EngineeringMap currentEMap = mainPage.ProjectMapping.CurrentEMap;
            EngineeringLayer eLayer = currentEMap.Structures.GetELayerByName("Tunnel3DAxes");
            if (eLayer == null)
            {
                currentEMap = currentEMap.RefEMap;
                eLayer = currentEMap.Structures.GetELayerByName("Tunnel3DAxes");
                if (eLayer == null)
                {
                    app.ReportError("ErrorString18");
                    return;
                }
            }

            DigitalGeotec.DataServiceReference.Tunnel tunnel = dgObj.Data
                as DigitalGeotec.DataServiceReference.Tunnel;
            if (tunnel == null || tunnel.LineNo == null)
            {
                app.ReportError("ErrorString0");
                return;
            }

            QueryTask task = new QueryTask(eLayer.FeatureServiceUrl);
            task.ExecuteCompleted += new EventHandler<QueryEventArgs>(task_ExecuteCompleted);
            task.Failed += new EventHandler<TaskFailedEventArgs>(task_Failed);

            Query query = new Query();
            query.OutFields.Add("*");
            query.ReturnGeometry = true;
            if (app.Project.ProjDef.UseGeographicMap)
            {
                Map map = currentEMap.GMap as Map;
                query.OutSpatialReference = map.SpatialReference;
            }
            else
            {
                query.OutSpatialReference = null;
            }
            query.Where = "LineNo=" + tunnel.LineNo.Value.ToString();

            task.ExecuteAsync(query, dgObj);
        }

        static void task_Failed(object sender, TaskFailedEventArgs e)
        {
            App app = Application.Current as App;
            string errorFmt = App.Current.Resources["ErrorString19"] as string;
            string error = string.Format(errorFmt, e.Error);
            app.ReportError(error, false);
        }

        static void task_ExecuteCompleted(object sender, QueryEventArgs e)
        {
            App app = Application.Current as App;
            ClientTunnels tunnels = GetClientTunnels();
            if (tunnels == null)
            {
                app.ReportError("ErrorString0");
                return;
            }

            ClientDGObject dgObj = e.UserState as ClientDGObject;
            DigitalGeotec.DataServiceReference.Tunnel tunnel = dgObj.Data
                as DigitalGeotec.DataServiceReference.Tunnel;
            if (tunnel.LineNo == null)
            {
                app.ReportError("ErrorString0");
                return;
            }

            TunnelAxis engineeringAxis = tunnels.TunnelAxes[tunnel.LineNo.Value];
            int count = engineeringAxis.AxisPoints.Count;

            FeatureSet featureSet = e.FeatureSet;
            if (featureSet.Features.Count != 1)
            {
                app.ReportError("ErrorString0");
                return;
            }

            Graphic g = featureSet.Features[0];
            ESRI.ArcGIS.Client.Geometry.Polyline pLine = g.Geometry
                as ESRI.ArcGIS.Client.Geometry.Polyline;
            if (pLine == null)
            {
                app.ReportError("ErrorString20");
                return;
            }
            ESRI.ArcGIS.Client.Geometry.PointCollection pc = pLine.Paths[0];
            if (pc.Count != count)
            {
                app.ReportError("ErrorString21");
                return;
            }

            TunnelAxis mapAxis = new TunnelAxis();
            mapAxis.LineNo = engineeringAxis.LineNo;
            mapAxis.AxisPoints = new System.Collections.Generic.List<TunnelAxisPoint>();
            for (int i = 0; i < count; i++)
            {
                TunnelAxisPoint p1 = engineeringAxis.AxisPoints[i];
                TunnelAxisPoint p2 = new TunnelAxisPoint();
                p2.Mileage = p1.Mileage;
                p2.X = pc[i].X;
                p2.Y = pc[i].Y;
                p2.Z = p1.Z;
                mapAxis.AxisPoints.Add(p2);
            }
            mapAxis = TunnelMappingUtility.ClipAxis(mapAxis, tunnel.StartMileage, tunnel.EndMileage);
            if (mapAxis.AxisPoints[0].Mileage != tunnel.StartMileage)
            {
                TunnelAxisPoint p = TunnelMappingUtility.MileageToAxisPoint(tunnel.StartMileage, mapAxis);
                mapAxis.AxisPoints.Insert(0, p);
            }
            count = mapAxis.AxisPoints.Count;
            if (mapAxis.AxisPoints[count - 1].Mileage != tunnel.EndMileage)
            {
                TunnelAxisPoint p = TunnelMappingUtility.MileageToAxisPoint(tunnel.EndMileage, mapAxis);
                mapAxis.AxisPoints.Add(p);
            }

            dgObj.Attributes["Axis"] = mapAxis;
        }
         */


        // Helper function
        // Get tunnel object
        //
        public static Tunnel GetTunnel(int lineNo)
        {
            Project prj = Globals.project;
            Domain strDomain = prj.getDomain(DomainType.Structure);
            DGObjectsCollection tunnels = strDomain.getObjects("Tunnel");
            if (tunnels == null)
            {
                return null;
            }

            Tunnel tunnel = tunnels[lineNo] as Tunnel;
            return tunnel;
        }

        // Helper function
        // Get tunnel footprint axis
        //
        public static TunnelAxis GetTunnelFootprintAxis(int lineNo)
        {
            Domain domain = Globals.project.getDomain(DomainType.Structure);
            DGObjectsCollection axes = domain.getObjects("TunnelAxis");
            if (axes == null)
            {
                return null;
            }
            //lineNo is the TunnelAxis id and key
            TunnelAxis ta = axes[lineNo] as TunnelAxis;
            return ta;
        }

        // Helper function
        // Get tunnel profile axis
        //
        public static TunnelAxis GetTunnelProfileAxis(int lineNo, EngineeringMap eMap)
        {
            Domain domain = HelperFunction.GetAnalysisDomain();
            DGObjectsCollection lps = domain.getObjects("LPResult");
            if (lps == null)
                return null;

            Tunnel tunnel = GetTunnel(lineNo);
            if (tunnel == null)
            {
                return null;
            }

            string name = tunnel.id.ToString() + eMap.MapID;

            LPResult lpResult = lps[name] as LPResult;
            if (lpResult == null)
            {
                return null;
            }
            TunnelAxis profileAxis = lpResult.ProfileAxis;
            return profileAxis;
        }

        public static double getAxisLength(TunnelAxis tunnelAxis)
        {
            double sum = 0;
            int n = tunnelAxis.AxisPoints.Count;
            for (int i = 0; i < n - 1; i++)
            {
                double x0 = tunnelAxis.AxisPoints[i].X;
                double y0 = tunnelAxis.AxisPoints[i].Y;
                double x1 = tunnelAxis.AxisPoints[i + 1].X;
                double y1 = tunnelAxis.AxisPoints[i + 1].Y;
                sum += GeometryAlgorithms.PointDistanceToPoint(x0, y0, x1, y1);
            }
            return sum;
        }

        // Helper function
        // get segment lining type
        public static SLType getSLType(int? slTypeID)
        {
            if (slTypeID == null)
                return null;
            Project prj = Globals.project;
            Domain strDomain = prj.getDomain(DomainType.Structure);
            DGObjectsCollection slColl = strDomain.getObjects("SLType");
            SLType slType = slColl[(int)slTypeID] as SLType;
            return slType;
        }

        /*
        // Helper function
        // Convert to a client segment lining object
        //
        public static ClientDGObject ToSLObject(object obj, bool reportErr = true)
        {
            App app = Application.Current as App;
            ClientDGObject cdgObj = obj as ClientDGObject;
            if (cdgObj == null || cdgObj.Data == null || cdgObj.Data.GetType() != typeof(SegmentLining))
            {
                if (reportErr)
                {
                    app.ReportError("ErrorString9");
                }
                return null;
            }
            return cdgObj;
        }

        // Helper function
        // Get SLType of a segment lining
        //
        public static SLType GetSLType(ClientDGObject cdgSL, int SLTypeID)
        {
            Tree tree = cdgSL.Owner;
            ClientSubCategories subCats = tree.SubCategories;
            SLType slType = subCats.GetSubCategoryByID(SLTypeID) as SLType;
            return slType;
        }

        // Helper function
        public static double GetSLMileage(SegmentLining sl)
        {
            double m1;
            if (sl.ConstructionRecord.MileageAsBuilt != null)
                m1 = sl.ConstructionRecord.MileageAsBuilt.Value;
            else
                m1 = sl.StartMileage;
            return m1;
        }
         */

        public static List<SegmentLining> getSLsByLineNo(int lineNo)
        {
            List<SegmentLining> sls = new List<SegmentLining>();

            Domain domain = Globals.project.getDomain(DomainType.Structure);
            DGObjectsCollection allSLs = domain.getObjects("SegmentLining");
            List<DGObject> allListSLs = allSLs.merge();
            foreach(DGObject dg in allListSLs)
            {
                SegmentLining sl = dg as SegmentLining;
                if (sl.LineNo == lineNo)
                    sls.Add(sl);
            }
            return sls;
        }
    }
}
