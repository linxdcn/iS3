using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.Data;

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

    public class TunnelAxisPoint
    {
        public double Mileage { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Z { get; set; }
    }

    public class TunnelAxis : DGObject
    {
        public int LineNo { get; set; }
        public List<TunnelAxisPoint> AxisPoints { get; set; }

        public TunnelAxis()
        {
            AxisPoints = new List<TunnelAxisPoint>();
        }

        public TunnelAxis(DataRow rawData)
            :base(rawData)
        {
            AxisPoints = new List<TunnelAxisPoint>();
        }

        public override bool LoadObjs(DGObjects objs, DbContext dbContext)
        {
            TunnelDGObjectLoader loader2 = new TunnelDGObjectLoader(dbContext);
            bool success = loader2.LoadAxes(objs);
            return success;
        }
    }

    public class TunnelEntity : DGObject
    {
        public double? StartMileage { get; set; }
        public double? EndMileage { get; set; }

        public TunnelEntity()
        {
        }

        public TunnelEntity(DataRow rawData)
            :base(rawData)
        {
        }
    }

    public class Tunnel : TunnelEntity
    {
        // LineNo can be null, e.g. Cut and Cover Portion of a shield tunnel may not have an ID.
        public int? LineNo { get; set; }
        public double? Width { get; set; }
        public double? Height { get; set; }
        public string ShapeDesc { get; set; }

        public DateTime? ConBeginDate { get; set; }
        public DateTime? ConEndDate { get; set; }

        // Use ID:Name as the key
        public override string key
        {
            get
            {
                return id.ToString() + ":" + name;
            }
        }

        public Tunnel()
        { }

        public Tunnel(DataRow rawData)
            : base(rawData)
        { }

        public override bool LoadObjs(DGObjects objs, DbContext dbContext)
        {
            TunnelDGObjectLoader loader2 = new TunnelDGObjectLoader(dbContext);
            bool success = loader2.LoadTunnels(objs);
            return success;
        }
    }

    // T2Connectoin: Tunnel-Tunnel Connection
    public class T2Connection
    {
        public int TID1 { get; set; }
        public int TID2 { get; set; }
        public double Mileage1 { get; set; }
        public double Mileage2 { get; set; }
    }

    public class CrossPassage : TunnelEntity
    {
        public virtual T2Connection GetConnection() { return null; }

        public CrossPassage()
        { }

        public CrossPassage(DataRow rawData)
            : base(rawData)
        { }
    }

}
