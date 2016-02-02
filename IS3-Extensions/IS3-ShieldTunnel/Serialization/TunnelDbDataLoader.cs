using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data.Common;
using System.Data.Odbc;
using System.Data;

using IS3.Core;
using IS3.Core.Serialization;
using IS3.ShieldTunnel;

namespace IS3.ShieldTunnel.Serialization
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

    public class TunnelDbDataLoader : DbDataLoader
    {
        public TunnelDbDataLoader(DbContext dbContext)
            : base(dbContext)
        { }

        public bool ReadTunnels(
            DGObjects objs,
            string tableNameSQL,
            List<int> objsIDs)
        {
            string conditionSQL = WhereSQL(objsIDs);
            return ReadTunnels(objs, tableNameSQL, conditionSQL, null);
        }

        public bool ReadTunnels(
            DGObjects objs,
            string tableNameSQL,
            string conditionSQL,
            string orderSQL)
        {
            try
            {
                _ReadTunnels(objs, tableNameSQL,
                    conditionSQL, orderSQL);
            }
            catch (DbException ex)
            {
                string str = ex.ToString();
                ErrorReport.Report(str);
                return false;
            }
            return true;
        }

        void _ReadTunnels(
            DGObjects objs,
            string tableNameSQL,
            string conditionSQL,
            string orderSQL)
        {
            ReadRawData(objs, tableNameSQL, orderSQL, conditionSQL);
            DataTable table = objs.rawDataSet.Tables[0];
            foreach (DataRow reader in table.Rows)
            {
                if (IsDbNull(reader, "ID") || IsDbNull(reader, "StartMileage"))
                    continue;

                Tunnel tunnel = new Tunnel(reader);
                tunnel.id = ReadInt(reader, "ID").Value;
                tunnel.name = ReadString(reader, "Name");
                //tunnel.FullName = ReadString(reader, "");
                tunnel.description = ReadString(reader, "description");
                tunnel.LineNo = ReadInt(reader, "LineNo");
                tunnel.Width = ReadDouble(reader, "Width");
                tunnel.Height = ReadDouble(reader, "Height");
                tunnel.ShapeDesc = ReadString(reader, "ShapeDesc");

                tunnel.StartMileage = ReadDouble(reader, "StartMileage").Value;
                tunnel.EndMileage = ReadDouble(reader, "EndMileage").Value;

                tunnel.ConBeginDate = ReadDateTime(reader, "ConBeginDate");
                tunnel.ConEndDate = ReadDateTime(reader, "ConEndDate");

                tunnel.shape = ReadShape(reader);

                objs[tunnel.key] = tunnel;
            }
        }

        public bool ReadTunnelAxes(
            DGObjects objs,
            string tableNameSQL,
            string conditionSQL,
            string orderSQL)
        {
            try
            {
                _ReadTunnelAxes(objs, tableNameSQL,
                    conditionSQL, orderSQL);
            }
            catch (DbException ex)
            {
                string str = ex.ToString();
                ErrorReport.Report(str);
                return false;
            }
            return true;
        }

        void _ReadTunnelAxes(
            DGObjects objs,
            string tableNameSQL,
            string conditionSQL,
            string orderSQL)
        {
            ReadRawData(objs, tableNameSQL, orderSQL, conditionSQL);
            DataTable table = objs.rawDataSet.Tables[_dbContext.TableNamePrefix + "TunnelAxes"];
            if( table != null)
            {
                foreach (DataRow row in table.Rows)
                {
                    if (IsDbNull(row, "ID"))
                        continue;

                    TunnelAxis ta = new TunnelAxis(row);
                    ta.name = ReadString(row, "Name");
                    ta.id = ReadInt(row, "ID").Value;
                    ta.LineNo = ReadInt(row, "LineNo").Value;
                    ta.shape = ReadShape(row);

                    objs[ta.key] = ta;
                }

                DataTable table2 = objs.rawDataSet.Tables[_dbContext.TableNamePrefix + "TunnelAxesPoints"];
                Dictionary<int, TunnelAxis> dictAxes = new Dictionary<int, TunnelAxis>();
                if (table2 != null)
                {
                    TunnelAxis axis = null;
                    foreach (DataRow reader in table2.Rows)
                    {
                        if (IsDbNull(reader, "LineNo"))
                            continue;
                        int lineNo = ReadInt(reader, "LineNo").Value;

                        if (dictAxes.ContainsKey(lineNo))
                            axis = dictAxes[lineNo];
                        else
                        {
                            axis = new TunnelAxis();
                            axis.LineNo = lineNo;
                            axis.id = lineNo;
                            axis.name = lineNo.ToString();
                            dictAxes[lineNo] = axis;
                        }
                        TunnelAxisPoint tap = new TunnelAxisPoint();
                        tap.Mileage = ReadDouble(reader, "Milage").Value;
                        tap.X = ReadDouble(reader, "X").Value;
                        tap.Y = ReadDouble(reader, "Y").Value;
                        tap.Z = ReadDouble(reader, "Z").Value;
                        axis.AxisPoints.Add(tap);
                    }
                }

                foreach (TunnelAxis ta in objs.values)
                {
                    if (dictAxes.ContainsKey(ta.id))
                        ta.AxisPoints = dictAxes[ta.id].AxisPoints;
                }
            }
        }

        public bool ReadSLType(
            DGObjects objs,
            string tableNameSQL,
            string conditionSQL,
            string orderSQL)
        {
            try
            {
                _ReadSLType(objs, tableNameSQL,
                    conditionSQL, orderSQL);
            }
            catch (DbException ex)
            {
                string str = ex.ToString();
                ErrorReport.Report(str);
                return false;
            }
            return true;
        }

        void _ReadSLType(
            DGObjects objs,
            string tableNameSQL,
            string conditionSQL,
            string orderSQL)
        {
            ReadRawData(objs, tableNameSQL, orderSQL, conditionSQL);
            DataTable table = objs.rawDataSet.Tables[_dbContext.TableNamePrefix + "SLBaseType"];
            Dictionary<int, SLBaseType> slBaseTypes = new Dictionary<int, SLBaseType>();
            if (table != null)
            {
                foreach (DataRow reader in table.Rows)
                {
                    if (IsDbNull(reader, "SLBaseTypeID"))
                        continue;
                    SLBaseType type = new SLBaseType();
                    type.id = ReadInt(reader, "SLBaseTypeID").Value;
                    type.SLBaseTypeID = type.id;
                    string category = ReadString(reader, "SLCategory");
                    if (category == "PrecastConcrete")
                        type.SLCategory = SLCategoryEnum.PrecastConcrete;
                    else if (category == "CastIron")
                        type.SLCategory = SLCategoryEnum.CastIron;
                    else if (category == "SteelPlate")
                        type.SLCategory = SLCategoryEnum.SteelPlate;

                    string shape = ReadString(reader, "SLShape");
                    if (shape == "Stright")
                        type.SLShape = SLShapeEnum.Straight;
                    else if (shape == "Tapered")
                        type.SLShape = SLShapeEnum.Tapered;
                    else if (shape == "Universal")
                        type.SLShape = SLShapeEnum.Universal;

                    type.NumberOfSegments = ReadInt(reader, "SLNumOfSegments").Value;
                    type.Conicity = ReadDouble(reader, "SLConicity").Value;
                    type.Thickness = ReadDouble(reader, "SLThickness").Value;
                    type.Width = ReadDouble(reader, "SLWidth").Value;
                    type.OuterDiameter = ReadDouble(reader, "SLOutDiameter").Value;
                    type.InnerDiameter = ReadDouble(reader, "SLInnerDiameter").Value;
                    type.TotalKeyPos = ReadInt(reader, "SLTotalKeyPos").Value;

                    slBaseTypes[(int)type.SLBaseTypeID] = type;
                }

                table = objs.rawDataSet.Tables[_dbContext.TableNamePrefix + "SLType"];
                if (table != null)
                {
                    foreach (DataRow reader in table.Rows)
                    {
                        if (IsDbNull(reader, "SLTypeID"))
                            continue;
                        SLType type = new SLType(reader);
                        type.id = ReadInt(reader, "SLTypeID").Value;
                        type.name = ReadString(reader, "SLTypeName");
                        type.SLBaseTypeID = ReadInt(reader, "SLBaseTypeID");
                        type.description = ReadString(reader, "SLTypeDescription");
                        type.Description_CN = ReadString(reader, "SLTypeDescription_CN");

                        if (type.SLBaseTypeID != null && slBaseTypes.ContainsKey((int)type.SLBaseTypeID))
                        {
                            SLBaseType baseType = slBaseTypes[(int)type.SLBaseTypeID];
                            type.SLCategory = baseType.SLCategory;
                            type.SLShape = baseType.SLShape;
                            type.NumberOfSegments = baseType.NumberOfSegments;
                            type.Conicity = baseType.Conicity;
                            type.Thickness = baseType.Thickness;
                            type.Width = baseType.Width;
                            type.OuterDiameter = baseType.OuterDiameter;
                            type.InnerDiameter = baseType.InnerDiameter;
                            type.TotalKeyPos = baseType.TotalKeyPos;
                        }
                        objs[type.key] = type;
                    }
                }

                table = objs.rawDataSet.Tables[_dbContext.TableNamePrefix + "Segments"];
                if (table != null)
                {
                    Dictionary<int, List<Segment>> allSegments = new Dictionary<int, List<Segment>>();
                    List<Segment> segments;

                    foreach (DataRow reader in table.Rows)
                    {
                        Segment seg = new Segment();
                        int i = 0;
                        int SLBaseTypeID = ReadInt(reader, "SLBaseTypeID").Value;
                        seg.StartAngle = ReadDouble(reader, "StartAngle").Value;
                        seg.CentralAngle = ReadDouble(reader, "CentralAngle").Value;
                        seg.Code = ReadString(reader, "Code");

                        if (allSegments.ContainsKey(SLBaseTypeID))
                            segments = allSegments[SLBaseTypeID];
                        else
                        {
                            segments = new List<Segment>();
                            allSegments[SLBaseTypeID] = segments;
                        }
                        segments.Add(seg);
                    }

                    foreach (var obj in objs.values)
                    {
                        SLType slType = obj as SLType;
                        if (slType.SLBaseTypeID == null)
                            continue;
                        int SLBaseTypeID = (int)slType.SLBaseTypeID;
                        if (allSegments.ContainsKey(SLBaseTypeID))
                        {
                            segments = allSegments[SLBaseTypeID];
                            slType.Segments.AddRange(segments);
                        }
                    }
                }
            }
        }
    }
}
