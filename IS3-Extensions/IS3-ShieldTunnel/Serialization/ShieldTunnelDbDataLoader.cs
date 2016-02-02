using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Data;
using System.Data.Common;
using System.Data.Odbc;

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

    public class ShieldTunnelDbDataLoader : DbDataLoader
    {
        public ShieldTunnelDbDataLoader(DbContext dbContext)
            : base(dbContext)
        { }

        public bool ReadSegmentLinings(
            DGObjects objs,
            string tableNameSQL,
            List<int> objsIDs)
        {
            string conditionSQL = WhereSQL(objsIDs);

            return ReadSegmentLinings(objs, tableNameSQL, conditionSQL, null);
        }

        SegmentLining _GetSL(DGObjects objs, DataRow row)
        {
            if (IsDbNull(row, "LineNo") || IsDbNull(row, "RingNo"))
                return null;

            int lineNo = ReadInt(row, "LineNo").Value;
            int ringNo = ReadInt(row, "RingNo").Value;

            string key = lineNo.ToString() + ":" + ringNo.ToString();
            if (!objs.containsKey(key))
            {
                string error = string.Format(
                    "Segment Lining does not exist when reading [{0}]!!! " +
                    "LineNo={1}, RingNo={2}.", row.Table.TableName, lineNo, ringNo);
                ErrorReport.Report(error);
                return null;
            }
            SegmentLining SL = (SegmentLining)objs[key];
            return SL;
        }

        public bool ReadSegmentLinings(
            DGObjects objs,
            string tableNameSQL,
            string conditionSQL,
            string orderSQL)
        {
            try
            {
                _ReadSegmentLinings(objs, tableNameSQL,
                    conditionSQL, orderSQL);
                //_ReadSLConstructionRecords(conn, objs);
                //_ReadTBMDrivingRecords(conn, objs);
            }
            catch (DbException ex)
            {
                string str = ex.ToString();
                ErrorReport.Report(str);
                return false;
            }
            return true;
        }

        public void _ReadSegmentLinings(
            DGObjects objs,
            string tableNameSQL,
            string conditionSQL,
            string orderSQL)
        {
            ReadRawData(objs, tableNameSQL, orderSQL, conditionSQL);
            DataTable table = objs.rawDataSet.Tables[0];

            ///
            /// 0. [SegmentLinings]
            ///
            foreach (DataRow reader in table.Rows)
            {
                if (IsDbNull(reader, "ID"))
                    continue;

                SegmentLining SL = new SegmentLining(reader);
                SL.id = ReadInt(reader, "ID").Value;
                SL.name = ReadString(reader, "Name");
                SL.fullName = ReadString(reader, "FullName");
                SL.description = ReadString(reader, "Description");
                SL.StartMileage = ReadDouble(reader, "MilageAsDesign");
                SL.SLTypeID = ReadInt(reader, "SLTypeIDAsDesign");
                SL.LineNo = ReadInt(reader, "LineNo").Value;
                SL.RingNo = ReadInt(reader, "RingNo").Value;
                SL.CentroidZ = ReadDouble(reader, "Centroid_Z");

                SL.shape = ReadShape(reader);

                objs[SL.key] = SL;
            }

            /*
             * Don't write code like this because linq is too slow for large data.
             * 
            foreach (SegmentLining SL in objs.Values)
            {
                DataTable sub_table = objs.RawDataSet.Tables[1];
                var rows = from row in sub_table.AsEnumerable()
                           where !IsDbNull(row, "LineNo") &&
                                 !IsDbNull(row, "RingNo") &&
                                 Convert.ToInt32(row["LineNo"]) == SL.LineNo &&
                                 Convert.ToInt32(row["RingNo"]) == SL.RingNo
                           select row;
                ...
            }
            */
            ///
            ///1.[SLConstructionRecords]
            ///
            table = objs.rawDataSet.Tables[_dbContext.TableNamePrefix + "SLConstructionRecords"];
            if (table != null)
            {
                foreach (DataRow reader in table.Rows)
                {
                    SegmentLining SL = _GetSL(objs, reader);
                    if (SL == null)
                        continue;

                    SLConstructionRecordType rec = SL.ConstructionRecord;
                    rec.LineNo = SL.LineNo;
                    rec.RingNo = SL.RingNo;
                    rec.ConstructionDate = ReadDateTime(reader, "ConstructionDate");
                    rec.MileageAsBuilt = ReadDouble(reader, "MilageAsBuilt");
                    rec.SLTypeIDAsBuilt = ReadInt(reader, "SLTypeIDAsBuilt");
                    rec.KeySegmentPosition = ReadInt(reader, "KeySegementPositionNum");
                    rec.Remarks = ReadString(reader, "Remarks");
                }
            }
            ///
            ///2. [TBMDrivingRecords]
            ///
            table = objs.rawDataSet.Tables[_dbContext.TableNamePrefix + "TBMDrivingRecords"];
            if (table != null)
            {
                foreach (DataRow reader in table.Rows)
                {
                    SegmentLining SL = _GetSL(objs, reader);
                    if (SL == null)
                        continue;

                    SLConstructionRecordType rec = SL.ConstructionRecord;
                    rec.TBMDrivingRecord.DataAquisitionNum = ReadInt(reader, "DataAquisitionNum");
                    rec.TBMDrivingRecord.DrivingDate = ReadDateTime(reader, "DrivingDate");
                    rec.TBMDrivingRecord.DrivingSpeed = ReadDouble(reader, "DrivingSpeed");
                    // time detail
                    rec.TBMDrivingRecord.TimeDetail.DrivingStartTime = ReadString(reader, "DrivingStartTime");
                    rec.TBMDrivingRecord.TimeDetail.DrivingEndTime = ReadString(reader, "DrivingEndTime");
                    rec.TBMDrivingRecord.TimeDetail.ExcavTime = ReadInt(reader, "ExcavTime");
                    rec.TBMDrivingRecord.TimeDetail.ExcavStopTime = ReadInt(reader, "ExcavStopTime");
                    rec.TBMDrivingRecord.TimeDetail.ExcavWaitingTime = ReadInt(reader, "ExcavWaitingTime");
                    rec.TBMDrivingRecord.TimeDetail.AssemTime = ReadInt(reader, "AssemTime");
                    rec.TBMDrivingRecord.TimeDetail.AssemStopTime = ReadInt(reader, "AssemStopTime");
                    // grouting detail
                    rec.TBMDrivingRecord.GroutingDetail.GroutingAmountOfPumps = new double?[6];
                    rec.TBMDrivingRecord.GroutingDetail.GroutingPressureOfPumps = new double?[6];
                    for (int index = 0; index < 6; ++index)
                        rec.TBMDrivingRecord.GroutingDetail.GroutingAmountOfPumps[index] = ReadDouble(reader, "GroutingAmountOfPump" + (index + 1).ToString());
                    for (int index = 0; index < 6; ++index)
                        rec.TBMDrivingRecord.GroutingDetail.GroutingPressureOfPumps[index] = ReadDouble(reader, "GroutingPressureOfPump" + (index + 1).ToString());
                    rec.TBMDrivingRecord.GroutingDetail.TotalGroutingAmountOfAllPumps = ReadDouble(reader, "TotalGroutingAmountOfAllPumps");
                    rec.TBMDrivingRecord.GroutingDetail.GroutingOilAmountAtTBMTail = ReadDouble(reader, "GroutingOilAmountAtTBMTail");
                    rec.TBMDrivingRecord.GroutingDetail.GroutingOilAmountAtFrontChamber = ReadDouble(reader, "GroutingOilAmountAtFrontChamber");
                    rec.TBMDrivingRecord.GroutingDetail.GroutingOilAmountAtMiddleChamber = ReadDouble(reader, "GroutingOilAmountAtMiddleChamber");
                    rec.TBMDrivingRecord.GroutingDetail.GroutingOilAmountAtRearChamber = ReadDouble(reader, "GroutingOilAmountAtRearChamber");
                    // driving force detail
                    rec.TBMDrivingRecord.ForceDetail.TotalDrivingForce = ReadDouble(reader, "TotalDrivingForce");
                    rec.TBMDrivingRecord.ForceDetail.RearFrameTractionForce = ReadDouble(reader, "RearFrameTractionForce");
                    // pressure detail
                    rec.TBMDrivingRecord.PressureDetail.AirBubblePressure = ReadDouble(reader, "AirBubblePressure");
                    rec.TBMDrivingRecord.PressureDetail.FrontChamberPressure = ReadDouble(reader, "FrontChamberPressure");
                    // mud detail
                    rec.TBMDrivingRecord.MudDetail.MudLiquidLevel = ReadDouble(reader, "MudLiquidLevel");
                    rec.TBMDrivingRecord.MudDetail.IncomingMudAmount = ReadDouble(reader, "IncomingMudAmount");
                    rec.TBMDrivingRecord.MudDetail.DischargingMudAmount = ReadDouble(reader, "DischargingMudAmount");
                    rec.TBMDrivingRecord.MudDetail.IncomingMudDensity = ReadDouble(reader, "IncomingMudDensity");
                    rec.TBMDrivingRecord.MudDetail.DischargingMudDensity = ReadDouble(reader, "DischargingMudDensity");
                    // cutter header detail
                    rec.TBMDrivingRecord.CutterHeadDetail.RotationSpeedOfCutterHead = ReadDouble(reader, "RotationSpeedOfCutterHead");
                    rec.TBMDrivingRecord.CutterHeadDetail.PenetrationOfCutterHead = ReadDouble(reader, "PenetrationOfCutterHead");
                    // deviation detail
                    rec.TBMDrivingRecord.DeviationDetail.ExcavDeviation = ReadDouble(reader, "ExcavDeviation");
                    rec.TBMDrivingRecord.DeviationDetail.AbsoluteDeviationX = ReadDouble(reader, "AbsoluteDeviationX");
                    rec.TBMDrivingRecord.DeviationDetail.AbsoluteDeviationY = ReadDouble(reader, "AbsoluteDeviationY");
                    rec.TBMDrivingRecord.DeviationDetail.BeginFrontHorizontalDeviation = ReadDouble(reader, "BeginFrontHorizontalDeviation");
                    rec.TBMDrivingRecord.DeviationDetail.BeginFrontElevationalDeviation = ReadDouble(reader, "BeginFrontElevationalDeviation");
                    rec.TBMDrivingRecord.DeviationDetail.EndFrontHorizontalDeviation = ReadDouble(reader, "EndFrontHorizontalDeviation");
                    rec.TBMDrivingRecord.DeviationDetail.EndFrontElevationalDeviation = ReadDouble(reader, "EndFrontElevationalDeviation");
                    rec.TBMDrivingRecord.DeviationDetail.BeginRearHorizontalDeviation = ReadDouble(reader, "BeginRearHorizontalDeviation");
                    rec.TBMDrivingRecord.DeviationDetail.BeginRearElevationalDeviation = ReadDouble(reader, "BeginRearElevationalDeviation");
                    rec.TBMDrivingRecord.DeviationDetail.EndRearHorizontalDeviation = ReadDouble(reader, "EndRearHorizontalDeviation");
                    rec.TBMDrivingRecord.DeviationDetail.EndRearElevationalDeviation = ReadDouble(reader, "EndRearElevationalDeviation");
                    // angle detail
                    rec.TBMDrivingRecord.AngleDetail.FrontRotationAngleOfTBM = ReadDouble(reader, "FrontRotationAngleOfTBM");
                    rec.TBMDrivingRecord.AngleDetail.DipAngle = ReadDouble(reader, "DipAngle");
                    rec.TBMDrivingRecord.AngleDetail.RotationAngle = ReadDouble(reader, "RotationAngle");
                    rec.TBMDrivingRecord.AngleDetail.AzimuthAngle = ReadDouble(reader, "AzimuthAngle");
                    // travel detail
                    rec.TBMDrivingRecord.TravelDetail.BeginTravelDistance = ReadDouble(reader, "BeginTravelDistance");
                    rec.TBMDrivingRecord.TravelDetail.EndTravelDistance = ReadDouble(reader, "EndTravelDistance");
                    rec.TBMDrivingRecord.TravelDetail.DrivingMilage = ReadDouble(reader, "DrivingMilage");
                }
            }
            ///
            ///3.[TBMPostureRecords]
            ///
            table = objs.rawDataSet.Tables[_dbContext.TableNamePrefix + "TBMPostureRecords"];
            if (table != null)
            {
                foreach (DataRow reader in table.Rows)
                {
                    SegmentLining SL = _GetSL(objs, reader);
                    if (SL == null)
                        continue;

                    SLConstructionRecordType rec = SL.ConstructionRecord;
                    rec.TBMPostureRecord.CutterHeadHorizontalDeviation = ReadDouble(reader, "CutterHeadHorizontalDeviation");
                    rec.TBMPostureRecord.CutterHeadElevationalDeviation = ReadDouble(reader, "CutterHeadElevationalDeviation");
                    rec.TBMPostureRecord.TBMTailHorizontalDeviation = ReadDouble(reader, "TBMTailHorizontalDeviation");
                    rec.TBMPostureRecord.TBMTailElevationalDeviation = ReadDouble(reader, "TBMTailElevationalDeviation");
                    rec.TBMPostureRecord.TBMSlope = ReadDouble(reader, "TBMSlope");
                    rec.TBMPostureRecord.TBMRotationAngle = ReadString(reader, "TBMRotationAngle");
                }
            }
            ///
            ///4.[SLPostureRecords]
            ///
            table = objs.rawDataSet.Tables[_dbContext.TableNamePrefix + "SLPostureRecords"];
            if (table != null)
            {
                foreach (DataRow reader in table.Rows)
                {
                    SegmentLining SL = _GetSL(objs, reader);
                    if (SL == null)
                        continue;

                    SLConstructionRecordType rec = SL.ConstructionRecord;
                    rec.SLPostureRecord.SLHorizontalDeviation = ReadDouble(reader, "SLHorizontalDeviation");
                    rec.SLPostureRecord.SLElevationalDeviation = ReadDouble(reader, "SLElevationalDeviation");
                    // gap detail
                    rec.SLPostureRecord.GapDetail.SLGapUp = ReadDouble(reader, "SLGapUp");
                    rec.SLPostureRecord.GapDetail.SLGapDown = ReadDouble(reader, "SLGapDown");
                    rec.SLPostureRecord.GapDetail.SLGapLeft = ReadDouble(reader, "SLGapLeft");
                    rec.SLPostureRecord.GapDetail.SLGapRight = ReadDouble(reader, "SLGapRight");
                    rec.SLPostureRecord.GapDetail.SLOtherGaps = new double?[8];
                    for (int index = 0; index < 8; ++index)
                        rec.SLPostureRecord.GapDetail.SLOtherGaps[index] = ReadDouble(reader, "SLGap" + (index + 1).ToString());
                    // diameter detail
                    rec.SLPostureRecord.DiameterDetail.HorizontalDiameter = ReadDouble(reader, "HorizontalDiameter");
                    rec.SLPostureRecord.DiameterDetail.VerticalDiameter = ReadDouble(reader, "VerticalDiameter");
                    rec.SLPostureRecord.DiameterDetail.OtherDiameters = new double?[2];
                    for (int index = 0; index < 2; ++index)
                        rec.SLPostureRecord.DiameterDetail.OtherDiameters[index] = ReadDouble(reader, "Diamter" + (index + 1).ToString());
                    rec.SLPostureRecord.DiameterDetail.DeformationDescription = ReadString(reader, "DeformationDescription");
                }
            }
            ///
            ///5.[SLQualityRecords]
            ///
            table = objs.rawDataSet.Tables[_dbContext.TableNamePrefix + "SLQualityRecords"];
            if (table != null)
            {
                foreach (DataRow reader in table.Rows)
                {
                    SegmentLining SL = _GetSL(objs, reader);
                    if (SL == null)
                        continue;

                    SLConstructionRecordType rec = SL.ConstructionRecord;
                    SLQualityRecordType qualityRec = new SLQualityRecordType();
                    qualityRec.QualityDescription = ReadString(reader, "QualityDescription");
                    qualityRec.InspectionDate = ReadString(reader, "InspectionDate");
                    qualityRec.Repairment = ReadString(reader, "Repairment");
                    rec.SLQualityRecords.Add(qualityRec);
                }
            }
            ///
            ///6.[SLSettlementRecords]
            ///
            table = objs.rawDataSet.Tables[_dbContext.TableNamePrefix + "SLSettlement"];
            if (table != null)
            {
                foreach (DataRow reader in table.Rows)
                {
                    SegmentLining SL = _GetSL(objs, reader);
                    if (SL == null)
                        continue;

                    SLConstructionRecordType rec = SL.ConstructionRecord;
                    rec.SLSettlementRecords.LineNo = rec.LineNo;
                    rec.SLSettlementRecords.RingNo = rec.RingNo;
                    rec.SLSettlementRecords.Name = SL.name;
                    rec.SLSettlementRecords.ID = SL.id;

                    SLSettlementItem item = new SLSettlementItem();
                    item.Name = ReadString(reader, "Name");
                    item.Date = ReadDateTime(reader, "Date");
                    item.Total = ReadDouble(reader, "Total");
                    item.Inc = ReadDouble(reader, "Increasement");
                    item.Rate = ReadDouble(reader, "Rate");
                    item.InitialElev = ReadDouble(reader, "InitialElev");
                    item.Elevation = ReadDouble(reader, "Elevation");
                    rec.SLSettlementRecords.SLSettlementItems.Add(item);
                }
            }

            ///
            ///7.[SLConvergenceRecords]
            ///
            table = objs.rawDataSet.Tables[_dbContext.TableNamePrefix + "SLConvergence"];
            if (table != null)
            {
                foreach (DataRow reader in table.Rows)
                {
                    SegmentLining SL = _GetSL(objs, reader);
                    if (SL == null)
                        continue;

                    SLConstructionRecordType rec = SL.ConstructionRecord;
                    rec.SLConvergenceRecords.LineNo = rec.LineNo;
                    rec.SLConvergenceRecords.RingNo = rec.RingNo;
                    rec.SLConvergenceRecords.Name = SL.name;
                    rec.SLConvergenceRecords.ID = SL.id;

                    SLConvergenceItem item = new SLConvergenceItem();
                    //item.Name = ReadString(reader, "Name");
                    item.Date = ReadDateTime(reader, "Date");
                    item.HorizontalRad = ReadDouble(reader, "HorizontalRad");
                    item.HorizontalDev = ReadDouble(reader, "HorizontalDeviation");
                    item.VerticalRad = ReadDouble(reader, "VerticalRad");
                    item.VerticalDev = ReadDouble(reader, "VerticalDeviation");
                    rec.SLConvergenceRecords.SLConvergenceItems.Add(item);
                }
            }

            ///
            ///8.[SLSpallRecords]
            ///
            table = objs.rawDataSet.Tables[_dbContext.TableNamePrefix + "SLSpall"];
            if (table != null)
            {
                DataColumn column = table.Columns.Add("RingNo", typeof(int));
                foreach (DataRow reader in table.Rows)
                {
                    int? ringNo = ReadInt(reader, "StartRingNo");
                    reader.SetField(column, ringNo);
                    SegmentLining SL = _GetSL(objs, reader);
                    if (SL == null)
                        continue;

                    SLInspectionRecordType rec = SL.InspectionRecords;
                    rec.LineNo = SL.LineNo;
                    rec.RingNo = SL.RingNo;

                    SLSpallRecordItem item = new SLSpallRecordItem();
                    //item.Name = ReadString(reader, "Name");
                    item.LineNo = SL.LineNo;
                    item.StartMilage = ReadDouble(reader, "StartMilage");
                    item.EndMilage = ReadDouble(reader, "EndMilage");
                    item.StartRingNo = ReadInt(reader, "StartRingNo");
                    item.EndRingNo = ReadInt(reader, "EndRingNo");
                    item.SLCode = ReadString(reader, "SLCode");
                    item.LocalX = ReadDouble(reader, "LocalX");
                    item.LocalY = ReadDouble(reader, "LocalY");
                    item.Shape = ReadString(reader, "Shape");
                    item.Area = ReadDouble(reader, "Area");
                    item.Length = ReadDouble(reader, "Length");
                    item.Width = ReadDouble(reader, "Width");
                    item.Depth = ReadDouble(reader, "Depth");
                    item.Date = ReadDateTime(reader, "Date");
                    item.Document = ReadString(reader, "Document");
                    item.Discription = ReadString(reader,"Discription");
                    SL.InspectionRecords.SLSpallRecords.Add(item);
                }
            }

            ///
            ///9.[SLCrackRecords]
            ///
            table = objs.rawDataSet.Tables[_dbContext.TableNamePrefix + "SLCrackRecord"];
            if (table != null)
            {
                DataColumn column = table.Columns.Add("RingNo", typeof(int));
                foreach (DataRow reader in table.Rows)
                {
                    int? ringNo = ReadInt(reader, "StartRingNo");
                    reader.SetField(column, ringNo);
                    SegmentLining SL = _GetSL(objs, reader);
                    if (SL == null)
                        continue;

                    SLInspectionRecordType rec = SL.InspectionRecords;
                    rec.LineNo = SL.LineNo;
                    rec.RingNo = SL.RingNo;

                    SLCrackRecordItem item = new SLCrackRecordItem();
                    item.LineNo = SL.LineNo;
                    item.StartMilage = ReadDouble(reader, "StartMilage");
                    item.EndMilage = ReadDouble(reader, "EndMilage");
                    item.StartRingNo = ReadInt(reader, "StartRingNo");
                    item.EndRingNo = ReadInt(reader, "EndRingNo");
                    item.SLCode = ReadString(reader, "SLCode");
                    item.Direction = ReadString(reader, "Direction");
                    item.Length = ReadDouble(reader, "Length");
                    item.Width = ReadDouble(reader, "Width");
                    item.Date = ReadDateTime(reader, "Date");
                    item.Document = ReadString(reader, "Document");
                    item.Discription = ReadString(reader, "Discription");
                    SL.InspectionRecords.SLCrackRecords.Add(item);
                }
            }

            ///
            ///10.[DislocationRecords]
            ///
            table = objs.rawDataSet.Tables[_dbContext.TableNamePrefix + "Dislocation"];
            if (table != null)
            {
                DataColumn column = table.Columns.Add("RingNo", typeof(int));
                foreach (DataRow reader in table.Rows)
                {
                    int? ringNo = ReadInt(reader, "StartRingNo");
                    reader.SetField(column, ringNo);
                    SegmentLining SL = _GetSL(objs, reader);
                    if (SL == null)
                        continue;

                    SLInspectionRecordType rec = SL.InspectionRecords;
                    rec.LineNo = SL.LineNo;
                    rec.RingNo = SL.RingNo;

                    DislocationRecordItem item = new DislocationRecordItem();
                    item.LineNo = SL.LineNo;
                    item.StartMilage = ReadDouble(reader, "StartMilage");
                    item.EndMilage = ReadDouble(reader, "EndMilage");
                    item.StartRingNo = ReadInt(reader, "StartRingNo");
                    item.EndRingNo = ReadInt(reader, "EndRingNo");
                    item.SLCode = ReadString(reader, "SLCode");
                    item.Horizontal = ReadDouble(reader, "Horizontal");
                    item.Vertical = ReadDouble(reader, "Vertical");
                    item.Total = ReadDouble(reader, "Total");
                    item.Date = ReadDateTime(reader, "Date");
                    item.Document = ReadString(reader, "Document");
                    item.Discription = ReadString(reader, "Discription");
                    SL.InspectionRecords.DislocationRecords.Add(item);
                }
            }

            ///
            ///11.[JointOpeningRecords]
            ///
            table = objs.rawDataSet.Tables[_dbContext.TableNamePrefix + "JointOpening"];
            if (table != null)
            {
                DataColumn column = table.Columns.Add("RingNo", typeof(int));
                foreach (DataRow reader in table.Rows)
                {
                    int? ringNo = ReadInt(reader, "StartRingNo");
                    reader.SetField(column, ringNo);
                    SegmentLining SL = _GetSL(objs, reader);
                    if (SL == null)
                        continue;

                    SLInspectionRecordType rec = SL.InspectionRecords;
                    rec.LineNo = SL.LineNo;
                    rec.RingNo = SL.RingNo;

                    JointOpeningRecordItem item = new JointOpeningRecordItem();
                    item.LineNo = SL.LineNo;
                    item.StartMilage = ReadDouble(reader, "StartMilage");
                    item.EndMilage = ReadDouble(reader, "EndMilage");
                    item.StartRingNo = ReadInt(reader, "StartRingNo");
                    item.EndRingNo = ReadInt(reader, "EndRingNo");
                    item.SLCode1 = ReadString(reader, "SLCode1");
                    item.SLCode2 = ReadString(reader, "SLCode2");
                    item.Total = ReadDouble(reader, "Total");
                    item.Date = ReadDateTime(reader, "Date");
                    item.Document = ReadString(reader, "Document");
                    item.Discription = ReadString(reader, "Discription");
                    SL.InspectionRecords.JointOpeningRecords.Add(item);
                }
            }

            ///
            ///11.[LeakageRecord]
            ///
            table = objs.rawDataSet.Tables[_dbContext.TableNamePrefix + "Leakage"];
            if (table != null)
            {
                DataColumn column = table.Columns.Add("RingNo", typeof(int));
                foreach (DataRow reader in table.Rows)
                {
                    int? ringNo = ReadInt(reader, "StartRingNo");
                    reader.SetField(column, ringNo);
                    SegmentLining SL = _GetSL(objs, reader);
                    if (SL == null)
                        continue;

                    SLInspectionRecordType rec = SL.InspectionRecords;
                    rec.LineNo = SL.LineNo;
                    rec.RingNo = SL.RingNo;

                    LeakageRecordItem item = new LeakageRecordItem();
                    item.LineNo = SL.LineNo;
                    item.StartMilage = ReadDouble(reader, "StartMilage");
                    item.EndMilage = ReadDouble(reader, "EndMilage");
                    item.StartRingNo = ReadInt(reader, "StartRingNo");
                    item.EndRingNo = ReadInt(reader, "EndRingNo");
                    item.SLCode = ReadString(reader, "SLCode");
                    item.StartAngle = ReadDouble(reader, "StartAngle");
                    item.EndAngle = ReadDouble(reader, "EndAngle");
                    item.Shape = ReadString(reader, "Shape");
                    item.Area = ReadDouble(reader, "Area");
                    item.Speed = ReadDouble(reader, "Speed");
                    item.Status = ReadString(reader, "Status");
                    item.p = ReadString(reader, "p");
                    item.pH = ReadDouble(reader, "pH");
                    item.Date = ReadDateTime(reader, "Date");
                    item.Document = ReadString(reader, "Document");
                    item.Discription = ReadString(reader, "Discription");
                    item.WaterSample = ReadString(reader, "WaterSample");
                    SL.InspectionRecords.LeakageRecords.Add(item);
                }
            }
        }
    }
}
