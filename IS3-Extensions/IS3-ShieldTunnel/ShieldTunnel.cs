using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Collections.ObjectModel;
using System.Data;
using System.Windows;

using IS3.Core;
using IS3.Core.Serialization;
using IS3.ShieldTunnel.Serialization;

namespace IS3.ShieldTunnel
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
    public enum SLCategoryEnum { PrecastConcrete, CastIron, SteelPlate };    // 管片类型: 预制管片, 铸铁管片, 钢管片
    public enum SLShapeEnum { Straight, Tapered, Universal };                // 管片形状: 直管片，楔形管片，通用管片
    public enum InnerStructureTypeEnum { RoadSlab, Gallery, Corbel, Ballast, FluePlate } // 内部结构：路面板, 内部廊道（轨道交通），牛腿，压重块，烟道板

    public class Segment
    {
        public double StartAngle { get; set; }
        public double CentralAngle { get; set; }
        public string Code { get; set; }
    }

    public class SLBaseType : DGObject
    {
        public int? SLBaseTypeID { get; set; }
        public SLCategoryEnum SLCategory { get; set; }
        public SLShapeEnum SLShape { get; set; }
        public int NumberOfSegments { get; set; }
        public double Conicity { get; set; }
        public double Thickness { get; set; }
        public double Width { get; set; }
        public double OuterDiameter { get; set; }
        public double InnerDiameter { get; set; }
        public int TotalKeyPos { get; set; }
        public List<Segment> Segments { get; set; }

        public SLBaseType()
        {
            Segments = new List<Segment>();
        }

        public SLBaseType(DataRow dataRow)
            : base(dataRow)
        {
            Segments = new List<Segment>();
        }
    }

    public class SLType : SLBaseType
    {
        public string Description_CN { get; set; }

        public override string key
        {
            get
            {
                return id.ToString();
            }
        }

        public SLType()
        {
            Segments = new List<Segment>();
        }

        public SLType(DataRow rawData)
            : base(rawData)
        {
            Segments = new List<Segment>();
        }

        public override bool LoadObjs(DGObjects objs, DbContext dbContext)
        {
            TunnelDGObjectLoader loader2 = new TunnelDGObjectLoader(dbContext);
            bool success = loader2.LoadSLType(objs);
            return success;
        }
    }

    public class SegmentLining : TunnelEntity
    {
        public int? SLTypeID { get; set; }
        public int LineNo { get; set; }
        public int RingNo { get; set; }
        public double? CentroidZ { get; set; }
        public SLConstructionRecordType ConstructionRecord { get; set; }
        public SLInspectionRecordType InspectionRecords { get; set; }
        public Collection<SLRehabilitationRecordType> RehabilitationRecords { get; set; }

        // Use LineNo:RingNo as the key
        public override string key
        {
            get
            {
                return LineNo.ToString() + ":" + RingNo.ToString();
            }
        }

        public SegmentLining()
        {
            ConstructionRecord = new SLConstructionRecordType();
            InspectionRecords = new SLInspectionRecordType();
            //RehabilitationRecords = new Collection<SLRehabilitationRecordType>();
        }

        public SegmentLining(DataRow rawData)
            :base(rawData)
        {
            ConstructionRecord = new SLConstructionRecordType();
            InspectionRecords = new SLInspectionRecordType();
            //RehabilitationRecords = new Collection<SLRehabilitationRecordType>();
        }

        public override bool LoadObjs(DGObjects objs, DbContext dbContext)
        {
            ShieldTunnelDGObjectLoader loader2 =
                new ShieldTunnelDGObjectLoader(dbContext);
            bool success = loader2.LoadSegmentLinings(objs);
            return success;
        }

        public override List<DataView> tableViews(IEnumerable<DGObject> objs)
        {
            List<DataView> dataViews = base.tableViews(objs);
            DataTable table = parent.rawDataSet.Tables["dbo_SLConstructionRecords"];
            if(table != null)
            {
                string filter = LineNoRingNoFilter(objs);
                DataView view = new DataView(table, filter, "[LineNo],[RingNo]",
                    DataViewRowState.CurrentRows);
                dataViews.Add(view);
            }

            table = parent.rawDataSet.Tables["dbo_TBMDrivingRecords"];
            if(table != null)
            {
                string filter = LineNoRingNoFilter(objs);
                DataView view = new DataView(table, filter, "[LineNo],[RingNo]",
                    DataViewRowState.CurrentRows);
                dataViews.Add(view);
            }

            table = parent.rawDataSet.Tables["dbo_TBMPostureRecords"];
            if (table != null)
            {
                string filter = LineNoRingNoFilter(objs);
                DataView view = new DataView(table, filter, "[LineNo],[RingNo]",
                    DataViewRowState.CurrentRows);
                dataViews.Add(view);
            }

            table = parent.rawDataSet.Tables["dbo_SLPostureRecords"];
            if (table != null)
            {
                string filter = LineNoRingNoFilter(objs);
                DataView view = new DataView(table, filter, "[LineNo],[RingNo]",
                    DataViewRowState.CurrentRows);
                dataViews.Add(view);
            }

            table = parent.rawDataSet.Tables["dbo_SLSettlement"];
            if (table != null)
            {
                string filter = LineNoRingNoFilter(objs);
                DataView view = new DataView(table, filter, "[LineNo],[RingNo]",
                    DataViewRowState.CurrentRows);
                dataViews.Add(view);
            }

            table = parent.rawDataSet.Tables["dbo_SLConvergence"];
            if (table != null)
            {
                string filter = LineNoRingNoFilter(objs);
                DataView view = new DataView(table, filter, "[LineNo],[RingNo]",
                    DataViewRowState.CurrentRows);
                dataViews.Add(view);
            }

            return dataViews;
        }

        string LineNoRingNoFilter(IEnumerable<DGObject> objs)
        {
            HashSet<int> lineNos = new HashSet<int>();
            foreach(DGObject obj in objs)
            {
                SegmentLining sl = obj as SegmentLining;
                lineNos.Add(sl.LineNo);
            }
            string sql = "LineNo in (";
            foreach (int lineNo in lineNos)
            {
                sql += lineNo.ToString();
                sql += ",";
            }
            sql += ") and RingNo in (";
            foreach (DGObject obj in objs)
            {
                SegmentLining sl = obj as SegmentLining;
                sql += sl.RingNo.ToString();
                sql += ",";
            }
            sql += ")";
            return sql;
        }

        /*
        public override List<FrameworkElement> chartViews(
            IEnumerable<DGObject> objs, double width, double height)
        {
            List<FrameworkElement> charts = new List<FrameworkElement>();

            SegmentLining sl = objs.Last() as SegmentLining;

            SegmentLiningView slsView = new SegmentLiningView();
            slsView.Name = "Segmentlining";
            slsView.ViewerHeight = height;
            charts.Add(slsView);

            return charts;
        }
         */
    }
}
