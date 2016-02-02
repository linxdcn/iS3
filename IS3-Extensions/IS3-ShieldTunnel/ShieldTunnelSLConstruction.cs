using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using System.Collections.ObjectModel;

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

    /// <summary>
    /// Segmental Lining's Posture Data Model
    /// </summary>
    public class SLPosture_GapDetail
    {
        public double? SLGapUp;
        public double? SLGapDown;
        public double? SLGapLeft;
        public double? SLGapRight;
        public double?[] SLOtherGaps;
    }

    public class SLPosture_DiameterDetail
    {
        public double? HorizontalDiameter;
        public double? VerticalDiameter;
        public double?[] OtherDiameters;
        public string DeformationDescription;
    }

    public class SLPostureRecordType
    {
        public double? SLHorizontalDeviation;
        public double? SLElevationalDeviation;
        public SLPosture_GapDetail GapDetail;
        public SLPosture_DiameterDetail DiameterDetail;

        public SLPostureRecordType()
        {
            GapDetail = new SLPosture_GapDetail();
            DiameterDetail = new SLPosture_DiameterDetail();
        }
    }

    /// <summary>
    /// Segmental Lining's TBM Driving Data Model
    /// </summary>
    public class TBMDriving_TimeDetail
    {
        public string DrivingStartTime;
        public string DrivingEndTime;
        public int? ExcavTime;
        public int? ExcavStopTime;
        public int? ExcavWaitingTime;
        public int? AssemTime;
        public int? AssemStopTime;
    }

    public class TBMDriving_GroutingDetail
    {
        public double?[] GroutingAmountOfPumps;
        public double?[] GroutingPressureOfPumps;
        public double? TotalGroutingAmountOfAllPumps;
        public double? GroutingOilAmountAtTBMTail;
        public double? GroutingOilAmountAtFrontChamber;
        public double? GroutingOilAmountAtMiddleChamber;
        public double? GroutingOilAmountAtRearChamber;
    }

    public class TBMDriving_ForceDetail
    {
        public double? TotalDrivingForce;
        public double? RearFrameTractionForce;
    }

    public class TBMDriving_PressurerDetail
    {
        public double? AirBubblePressure;
        public double? FrontChamberPressure;
    }

    public class TBMDriving_MudDetail
    {
        public double? MudLiquidLevel;
        public double? IncomingMudAmount;
        public double? DischargingMudAmount;
        public double? IncomingMudDensity;
        public double? DischargingMudDensity;
    }

    public class TBMDriving_CutterHeadDetail
    {
        public double? RotationSpeedOfCutterHead;
        public double? PenetrationOfCutterHead;
    }

    public class TBMDriving_DeviationDetail
    {
        public double? ExcavDeviation;
        public double? AbsoluteDeviationX;
        public double? AbsoluteDeviationY;
        public double? BeginFrontHorizontalDeviation;
        public double? BeginFrontElevationalDeviation;
        public double? BeginRearHorizontalDeviation;
        public double? BeginRearElevationalDeviation;
        public double? EndFrontHorizontalDeviation;
        public double? EndFrontElevationalDeviation;
        public double? EndRearHorizontalDeviation;
        public double? EndRearElevationalDeviation;
    }

    public class TBMDriving_AngleDetail
    {
        public double? FrontRotationAngleOfTBM;
        public double? DipAngle;
        public double? RotationAngle;
        public double? AzimuthAngle;
    }

    public class TBMDriving_TravelDetail
    {
        public double? BeginTravelDistance;
        public double? EndTravelDistance;
        public double? DrivingMilage;
    }

    public class TBMDrivingRecordType
    {
        public int? DataAquisitionNum;
        public DateTime? DrivingDate;
        public double? DrivingSpeed;
        public TBMDriving_TimeDetail TimeDetail;
        public TBMDriving_GroutingDetail GroutingDetail;
        public TBMDriving_ForceDetail ForceDetail;
        public TBMDriving_PressurerDetail PressureDetail;
        public TBMDriving_MudDetail MudDetail;
        public TBMDriving_CutterHeadDetail CutterHeadDetail;
        public TBMDriving_DeviationDetail DeviationDetail;
        public TBMDriving_AngleDetail AngleDetail;
        public TBMDriving_TravelDetail TravelDetail;

        public TBMDrivingRecordType()
        {
            TimeDetail = new TBMDriving_TimeDetail();
            GroutingDetail = new TBMDriving_GroutingDetail();
            ForceDetail = new TBMDriving_ForceDetail();
            PressureDetail = new TBMDriving_PressurerDetail();
            MudDetail = new TBMDriving_MudDetail();
            CutterHeadDetail = new TBMDriving_CutterHeadDetail();
            DeviationDetail = new TBMDriving_DeviationDetail();
            AngleDetail = new TBMDriving_AngleDetail();
            TravelDetail = new TBMDriving_TravelDetail();
        }
    }

    /// <summary>
    /// Segmental Lining's TBM Posture Data Model
    /// </summary>
    public class TBMPostureRecordType
    {
        public double? CutterHeadHorizontalDeviation;
        public double? CutterHeadElevationalDeviation;
        public double? TBMTailHorizontalDeviation;
        public double? TBMTailElevationalDeviation;
        public double? TBMSlope;
        public string TBMRotationAngle;
    }

    /// <summary>
    /// Segmental Lining's Quality Record Data Model
    /// </summary>
    public class SLQualityRecordType
    {
        public string QualityDescription;
        public string InspectionDate;
        public string Repairment;
    }

    /// <summary>
    /// Segmental Lining's Settlement Record Data Model
    /// </summary>

    public class SLSettlementRecordType
    {
        public int LineNo;
        public int RingNo;
        public string Name;
        public int ID;
        public List <SLSettlementItem> SLSettlementItems;
        public SLSettlementRecordType()
        {
            SLSettlementItems = new List<SLSettlementItem>();
        }
    }
    public class SLSettlementItem
    {
        public string Name;
        public DateTime?  Date;
        public double? Total;
        public double? Inc;
        public double? Rate;
        public double? InitialElev;
        public double? Elevation;
    }
    public class SLConvergenceRecordType
    {
        public int LineNo;
        public int RingNo;
        public string Name;
        public int ID;
        public List <SLConvergenceItem> SLConvergenceItems;
        public SLConvergenceRecordType()
        {
            SLConvergenceItems = new List<SLConvergenceItem>();
        }
    }
    public class SLConvergenceItem
    {
        public string Name;
        public DateTime? Date;
        public double? HorizontalRad;
        public double? HorizontalDev;
        public double? VerticalRad;
        public double? VerticalDev;
    }

    /// <summary>
    /// Segmental Lining's Construction Record Data Model
    /// </summary>

    public class SLConstructionRecordType
    {
        public int LineNo;
        public int RingNo;
        public DateTime? ConstructionDate;
        public double? MileageAsBuilt;
        public int? SLTypeIDAsBuilt;
        public int? KeySegmentPosition;
        public TBMDrivingRecordType TBMDrivingRecord;
        public TBMPostureRecordType TBMPostureRecord;
        public SLPostureRecordType SLPostureRecord;
        public Collection<SLQualityRecordType> SLQualityRecords;
        public SLSettlementRecordType SLSettlementRecords;
        public SLConvergenceRecordType SLConvergenceRecords;

        public string Remarks;

        public SLConstructionRecordType()
        {
            TBMDrivingRecord = new TBMDrivingRecordType();
            TBMPostureRecord = new TBMPostureRecordType();
            SLPostureRecord = new SLPostureRecordType();
            SLQualityRecords = new Collection<SLQualityRecordType>();
            SLSettlementRecords = new SLSettlementRecordType();
            SLConvergenceRecords = new SLConvergenceRecordType();
        }
    }
}
