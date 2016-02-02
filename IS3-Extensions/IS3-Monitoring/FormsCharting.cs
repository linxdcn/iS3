using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Drawing;
using System.Windows;
using System.Windows.Forms.Integration;
using System.Windows.Forms.DataVisualization.Charting;

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

    // Summary:
    //     Charting using System.Windows.Forms.DataVisualization.Charting
    // Remarks:
    //     There are two versions of charting from Microsoft:
    //     System.Windows.Forms.DataVisualization.Charting in .Net 4.0+
    //     System.Windows.Controls.DataVisualization.Charting in WPFToolkit
    //
    public class FormsCharting
    {
        public static FrameworkElement getMonPointChart(
            IEnumerable<DGObject> objs, double width, double height)
        {
            if (objs == null || objs.Count() == 0)
                return null;
            MonPoint firstMonPoint = objs.First() as MonPoint;
            string unit = getMonPointUnit(firstMonPoint);

            Chart chart1 = new Chart();
            chart1.Name = "Chart1";
            chart1.Text = "Chart1";

            ChartArea chartArea1 = new ChartArea("ChartArea1");
            setChartAreaStyle(chartArea1);
            chartArea1.AxisX.LabelAutoFitStyle =
                LabelAutoFitStyles.IncreaseFont | 
                LabelAutoFitStyles.DecreaseFont |
                LabelAutoFitStyles.WordWrap;
            chartArea1.AxisX.Title = "Date";
            chartArea1.AxisX.ScrollBar.LineColor = Color.Black;
            chartArea1.AxisX.ScrollBar.Size = 10;
            chartArea1.AxisY.Title = "Value (" + unit + ")";
            chartArea1.AxisY.ScrollBar.LineColor = Color.Black;
            chartArea1.AxisY.ScrollBar.Size = 10;
            chartArea1.CursorX.IsUserEnabled = true;
            chartArea1.CursorX.IsUserSelectionEnabled = true;
            chartArea1.CursorY.IsUserEnabled = true;
            chartArea1.CursorY.IsUserSelectionEnabled = true;
            chart1.ChartAreas.Add(chartArea1);

            Legend legend1 = new Legend("Legend1");
            legend1.DockedToChartArea = "ChartArea1";
            chart1.Legends.Add(legend1);

            foreach (DGObject obj in objs)
            {
                MonPoint monPnt = obj as MonPoint;
                if (monPnt == null)
                    continue;
                foreach (string key in monPnt.readingsDict.Keys)
                {
                    List<MonReading> readings = monPnt.readingsDict[key];
                    if (readings.Count == 0)
                        continue;

                    Series series1 = new Series();
                    series1.Name = monPnt.name + ":" + key;
                    series1.ChartType = SeriesChartType.FastLine;
                    series1.ChartArea = "ChartArea1";
                    series1.Points.DataBind(readings, "time", "value", null);
                    series1.BorderWidth = 2;

                    chart1.Series.Add(series1);
                }
            }

            WindowsFormsHost formsHost = new WindowsFormsHost();
            formsHost.Name = "PointCurve";
            formsHost.Child = chart1;
            formsHost.Width = width - 10;
            formsHost.Height = height - 20;
            return formsHost;
        }

        static string getMonPointUnit(MonPoint monPoint)
        {
            if (monPoint == null || monPoint.readingsDict == null ||
                monPoint.readingsDict.Count == 0)
                return null;
            List<MonReading> readings = monPoint.readingsDict.Values.First();
            if (readings == null || readings.Count == 0)
                return null;
            string unit = readings.First().unit;
            return unit;
        }
        static string getMonGroupUnit(MonGroup group)
        {
            if (group == null || group.monPntDict == null
                || group.monPntDict.Count == 0)
                return null;
            MonPoint monPoint = group.monPntDict.Values.First();
            string unit = getMonPointUnit(monPoint);
            return unit;
        }

        static void setChartAreaStyle(ChartArea chartArea)
        {
            chartArea.AxisX.LabelStyle.Font =
                new Font("Times New Roman", 8.25F, System.Drawing.FontStyle.Bold);
            chartArea.AxisX.LineColor = Color.FromArgb(64, 64, 64, 64);
            chartArea.AxisX.MajorGrid.LineColor = Color.FromArgb(64, 64, 64, 64);
            chartArea.AxisY.LabelStyle.Font =
                new System.Drawing.Font("Times New Roman", 8.25F, System.Drawing.FontStyle.Bold);
            chartArea.AxisY.LineColor = Color.FromArgb(64, 64, 64, 64);
            chartArea.AxisY.MajorGrid.LineColor = Color.FromArgb(64, 64, 64, 64);
            chartArea.BackColor = Color.Gainsboro;
            chartArea.BackGradientStyle = GradientStyle.TopBottom;
            chartArea.BackSecondaryColor = Color.White;
            chartArea.BorderColor = Color.FromArgb(64, 64, 64, 64);
            chartArea.BorderDashStyle = ChartDashStyle.Solid;
            chartArea.ShadowColor = System.Drawing.Color.Transparent;
        }

        public static FrameworkElement getMonGroupChart(
            IEnumerable<DGObject> objs, double width, double height)
        {
            if (objs == null || objs.Count() == 0)
                return null;
            MonGroup firstMonGroup = objs.First() as MonGroup;
            if (firstMonGroup == null || 
                firstMonGroup.monPntDict == null ||
                firstMonGroup.monPntDict.Count == 0)
                return null;
            MonPoint lastMonPoint = firstMonGroup.monPntDict.Values.Last();
            if (lastMonPoint == null)
                return null;

            string shape = firstMonGroup.groupShape.ToLower();
            if (shape == "line" || shape == "linear")
            {
                if (lastMonPoint.distanceZ != null &&
                    lastMonPoint.distanceZ.Value != 0)
                    return getMonGroupChart_LinearZ(objs, width, height);
                else
                    return getMonGroupChart_LinearX(objs, width, height);

            }
            else if (shape == "circle" || shape == "circular")
            {
                return getMonGroupChart_Circular(objs, width, height);
            }
            return null;
        }

        static FrameworkElement getMonGroupChart_LinearX(
            IEnumerable<DGObject> objs, double width, double height)
        {
            if (objs == null || objs.Count() == 0)
                return null;
            MonGroup firstMonGroup = objs.First() as MonGroup;
            string unit = getMonGroupUnit(firstMonGroup);

            Chart chart1 = new Chart();
            chart1.Name = "Chart1";
            chart1.Text = "Chart1";

            ChartArea chartArea1 = new ChartArea("ChartArea1");
            setChartAreaStyle(chartArea1);
            chartArea1.AxisX.Title = "Distance (m)";
            chartArea1.AxisY.Title = "Value (" + unit + ")";
            chart1.ChartAreas.Add(chartArea1);

            Legend legend1 = new Legend("Legend1");
            legend1.DockedToChartArea = "ChartArea1";
            chart1.Legends.Add(legend1);

            int markStyle = 1;
            foreach (DGObject obj in objs)
            {
                MonGroup monGroup = obj as MonGroup;
                if (monGroup == null)
                    continue;
                if (monGroup.monPntDict.Count == 0)
                    continue;

                MonPoint firstPnt = monGroup.monPntDict.Values.First();
                foreach (string key in firstPnt.readingsDict.Keys)
                {
                    List<MonReading> firstPnt_readings = firstPnt.readingsDict[key];
                    MonReading firstPnt_lastReading = firstPnt_readings.Last();
                    DateTime time = firstPnt_lastReading.time;

                    Series series1 = new Series();
                    series1.ChartType = SeriesChartType.Line;
                    series1.ChartArea = "ChartArea1";
                    series1.Name = string.Format("{0}: {1} ({2:d})", 
                        monGroup.name, key, time);
                    series1.BorderWidth = 2;
                    series1.MarkerStyle = (MarkerStyle) (markStyle++ % 9);
                    series1.MarkerSize = 8;

                    foreach (MonPoint monPoint in monGroup.monPntDict.Values)
                    {
                        List<MonReading> readings = monPoint.readingsDict[key];
                        MonReading reading = readings.Last();
                        double x = monPoint.distanceX.Value;
                        double y = reading.value;
                        DataPoint dataPoint = new DataPoint(x, y);
                        dataPoint.Label = monPoint.name;
                        dataPoint.ToolTip = "#VALY";
                        series1.Points.Add(dataPoint);
                    }

                    chart1.Series.Add(series1);
                }
            }

            WindowsFormsHost formsHost = new WindowsFormsHost();
            formsHost.Name = "GroupCurve";
            formsHost.Child = chart1;
            formsHost.Width = width - 10;
            formsHost.Height = height - 20;
            return formsHost;
        }

        static FrameworkElement getMonGroupChart_LinearZ(
            IEnumerable<DGObject> objs, double width, double height)
        {
            if (objs == null || objs.Count() == 0)
                return null;
            MonGroup firstMonGroup = objs.First() as MonGroup;
            string unit = getMonGroupUnit(firstMonGroup);

            Chart chart1 = new Chart();
            chart1.Name = "Chart1";
            chart1.Text = "Chart1";

            ChartArea chartArea1 = new ChartArea("ChartArea1");
            setChartAreaStyle(chartArea1);
            chartArea1.AxisX.Title = "Value (" + unit + ")";
            chartArea1.AxisY.Title = "Distance (m)";
            chart1.ChartAreas.Add(chartArea1);

            Legend legend1 = new Legend("Legend1");
            legend1.DockedToChartArea = "ChartArea1";
            chart1.Legends.Add(legend1);

            int markStyle = 1;
            foreach (DGObject obj in objs)
            {
                MonGroup monGroup = obj as MonGroup;
                if (monGroup == null)
                    continue;
                if (monGroup.monPntDict.Count == 0)
                    continue;

                MonPoint firstPnt = monGroup.monPntDict.Values.First();
                foreach (string key in firstPnt.readingsDict.Keys)
                {
                    List<MonReading> firstPnt_readings = firstPnt.readingsDict[key];
                    MonReading firstPnt_lastReading = firstPnt_readings.Last();
                    DateTime time = firstPnt_lastReading.time;

                    Series series1 = new Series();
                    series1.ChartType = SeriesChartType.Line;
                    series1.ChartArea = "ChartArea1";
                    series1.Name = string.Format("{0}: {1} ({2:d})",
                        monGroup.name, key, time);
                    series1.BorderWidth = 2;
                    series1.MarkerStyle = (MarkerStyle)(markStyle++ % 9);
                    series1.MarkerSize = 8;

                    foreach (MonPoint monPoint in monGroup.monPntDict.Values)
                    {
                        List<MonReading> readings = monPoint.readingsDict[key];
                        MonReading reading = readings.Last();
                        double x = reading.value;
                        double y = monPoint.distanceZ.Value;
                        DataPoint dataPoint = new DataPoint(x, y);
                        dataPoint.Label = monPoint.name;
                        dataPoint.ToolTip = "#VALY";
                        series1.Points.Add(dataPoint);
                    }

                    chart1.Series.Add(series1);
                }
            }

            WindowsFormsHost formsHost = new WindowsFormsHost();
            formsHost.Name = "GroupCurve";
            formsHost.Child = chart1;
            formsHost.Width = width - 10;
            formsHost.Height = height - 20;
            return formsHost;
        }

        static FrameworkElement getMonGroupChart_Circular(
            IEnumerable<DGObject> objs, double width, double height)
        {
            // Not implemented yet.
            // 
            // Use Polar Chart -> SeriesChartType.Polar
            // and use PrePaint and PostPaint Events.
            return null;
        }
    }
}
