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
    /// Segmental Lining's Crack Data Model
    /// </summary>
    public class SLSpallRecordItem
    {
        public int? LineNo;
        public double? StartMilage;
        public double? EndMilage;
        public int? StartRingNo;
        public int? EndRingNo;
        public string SLCode;
        public double? LocalX;
        public double? LocalY;
        public string Shape;
        public double? Area;
        public double? Length;
        public double? Width;
        public double? Depth;
        public DateTime? Date;
        public string Document;
        public string Discription;
    }

    /// <summary>
    /// Segmental Lining's Crack Data Model
    /// </summary>
    public class SLCrackRecordItem
    {
        public int? LineNo;
        public double? StartMilage;
        public double? EndMilage;
        public int? StartRingNo;
        public int? EndRingNo;
        public string SLCode;
        public string Direction;
        public double? Length;
        public double? Width;
        public DateTime? Date;
        public string Document;
        public string Discription;
    }

    /// <summary>
    /// Segmental Lining's Dislocation Data Model
    /// </summary>
    public class DislocationRecordItem
    {
        public int? LineNo;
        public double? StartMilage;
        public double? EndMilage;
        public int? StartRingNo;
        public int? EndRingNo;
        public string SLCode;
        public double? Horizontal;
        public double? Vertical;
        public double? Total;
        public DateTime? Date;
        public string Document;
        public string Discription;
    }

    /// <summary>
    /// Segmental Lining's JointOpening Data Model
    /// </summary>
    public class JointOpeningRecordItem
    {
        public int? LineNo;
        public double? StartMilage;
        public double? EndMilage;
        public int? StartRingNo;
        public int? EndRingNo;
        public string SLCode1;
        public string SLCode2;
        public double? Total;
        public DateTime? Date;
        public string Document;
        public string Discription;
    }

    /// <summary>
    /// Segmental Lining's Leakage Data Model
    /// </summary>
    public class LeakageRecordItem
    {
        public int? LineNo;
        public double? StartMilage;
        public double? EndMilage;
        public int? StartRingNo;
        public int? EndRingNo;
        public string SLCode;
        public double? StartAngle;
        public double? EndAngle;
        public string Shape;
        public double? Area;
        public double? Speed;
        public string Status;
        public string p;
        public double? pH;
        public string WaterSample;
        public DateTime? Date;
        public string Document;
        public string Discription;
    }

    //Settlement and Convergence are in the monitoring extension
    /*
    /// <summary>
    /// Segmental Lining's Settlement Record Data Model
    /// </summary>
    public class SLSettlementRecordItem
    {
        public double? InitialElev;
        public double? Elevation;
        public double? Increasement;
        public double? Total;
        public double? Rate;
        public DateTime? Date;
    }

    /// <summary>
    /// Segmental Lining's Convergence Record Data Model
    /// </summary>
    public class SLConvergenceRecordItem
    {
        public double? HorizontalRad;
        public double? HorizontalDeviation;
        public double? VerticalRad;
        public double? VerticalDeviation;
        public DateTime? Date;
    }
     */

    /// <summary>
    /// Segmental Lining's inspection Data Model
    /// </summary>

    public class SLInspectionRecordType
    {
        public int LineNo;
        public int RingNo;
        public Collection<SLSpallRecordItem> SLSpallRecords;
        public Collection<SLCrackRecordItem> SLCrackRecords;
        public Collection<DislocationRecordItem> DislocationRecords;
        public Collection<JointOpeningRecordItem> JointOpeningRecords;
        public Collection<LeakageRecordItem> LeakageRecords;

        public SLInspectionRecordType()
        {
            SLSpallRecords = new Collection<SLSpallRecordItem>();
            SLCrackRecords = new Collection<SLCrackRecordItem>();
            DislocationRecords = new Collection<DislocationRecordItem>();
            JointOpeningRecords = new Collection<JointOpeningRecordItem>();
            LeakageRecords = new Collection<LeakageRecordItem>();
        }
    }
}
