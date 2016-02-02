using System;
using System.Net;
using System.Windows;
using System.Windows.Media;
using System.Collections.Generic;

using IS3.Core;

namespace IS3.Monitoring
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
    public enum MonitoringValueCategory { Total, Inc, Rate };

    public enum MonitoringDataPresentationMethod
    {
        None,
        HorizontalBar,
        VerticalBar,
        HorizontalCurve,
        VerticalCurve,
        Vector2D
    }

    public enum MonitoringDataPresentationTextOption
    {
        MinMaxTextPerLayer,
        MinMaxTextPerItem
    }

    public enum MonitoringSymbolStyle
    {
        Circle,
        Rectangle,
        Triangle,
        Diamond,
        Symbol5,
        Symbol6,
        Symbol7,
        Symbol8,
        Symbol9,
        Symbol10
    }


    public class MonitoringWarningLevel
    {
        public Color WarningColor { get; set; }
        public string Description { get; set; }
        public bool Flash { get; set; }
    }

    public class MonitoringsDef
    {
        public double TimeReplayIntervals { get; set; }
        public MonitoringLayerList MonitoringLayers { get; set; }
        public List<MonitoringWarningLevel> WarningLevels { get; set; }
        public MonitoringValueCategory InterestedMonitoringValue { get; set; }

        public MonitoringsDef()
        {
            TimeReplayIntervals = 1.0;
            MonitoringLayers = new MonitoringLayerList();
            WarningLevels = new List<MonitoringWarningLevel>();
        }

    }

    public class MonitoringLayer
    {
        public string Unit { get; set; }
        public bool Visible { get; set; }
        public MonitoringSymbolStyle SymbolStyle { get; set; }
        public MonitoringDataPresentationMethod PresentationMethod { get; set; }
        public MonitoringDataPresentationTextOption PresentationTextOption { get; set; }
        public double PresentationScale { get; set; }
        public double PresentationWidth { get; set; }
        public string FontName { get; set; }
        public double FontSize { get; set; }
        public List<MonitoringWarningGroup> WarningGroups { get; set; }

        public MonitoringLayer()
        {
            WarningGroups = new List<MonitoringWarningGroup>();
            PresentationScale = 0.0;
            PresentationWidth = 2.0;
            FontName = "Arial";
            FontSize = 14;
        }

        public MonitoringWarningGroup GetWarningGroup(MonitoringValueCategory category)
        {
            foreach (MonitoringWarningGroup group in WarningGroups)
            {
                if (group.CategoryName == category)
                    return group;
            }
            return null;
        }
    }

    public class MonitoringLayerList : List<MonitoringLayer>
    {
        public int id { get; set; }
        public MonitoringLayer GetByLayerName(string layerName)
        {
            foreach (MonitoringLayer mLayer in this)
            {
                //if (mLayer.Name == layerName)
                //    return mLayer;
            }
            return null;
        }
    }

    public class MonitoringWarningLevelValue
    {
        public double MinValue { get; set; }
        public double MaxValue { get; set; }
        public int WarningLevelIndex { get; set; }
    }

    public class MonitoringWarningGroup
    {
        public MonitoringValueCategory CategoryName { get; set; }
        public List<MonitoringWarningLevelValue> WarningLevelValues { get; set; }

        public MonitoringWarningGroup()
        {
            WarningLevelValues = new List<MonitoringWarningLevelValue>();
        }

        public int GetWarningLevelIndex(double value)
        {
            foreach (MonitoringWarningLevelValue level in WarningLevelValues)
            {
                if (value >= level.MinValue && value <= level.MaxValue)
                    return level.WarningLevelIndex;
            }
            return -1;
        }
    }
}
