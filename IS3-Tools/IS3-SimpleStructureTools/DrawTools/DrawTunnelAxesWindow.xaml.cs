using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Media;

using IS3.Core;
using IS3.Core.Geometry;
using IS3.Core.Graphics;
using IS3.ShieldTunnel;
using IS3.SimpleStructureTools.Helper;

namespace IS3.SimpleStructureTools.DrawTools
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

    public class DrawAxesSettings
    {    
        public bool drawMilage { get; set; }
        public bool isReverse { get; set; }
        public int interval { get; set; }
        public double tickLen { get; set; }

        public double scale { get; set; }   // New! map scale.
        public string mapID { get; set; }

        public DrawAxesSettings()
        {
            drawMilage = true;
            isReverse = false;
            interval = 100;
            tickLen = 3;
            scale = 1.0;
        }
    }
    /// <summary>
    /// Interaction logic for DrawTunnelAxesWindow.xaml
    /// </summary>
    public partial class DrawTunnelAxesWindow : Window
    {
        //Desktop members
        Project _prj;                       // the project
        Domain _structureDomain;              // the structure domain of the project
        IMainFrame _mainFrame;              // the main frame
        IView _inputView;                   // the input view 
        DrawAxesSettings _settings;          // the analysis settings
        bool _initFailed;                   // set to true if initialization failed

        //DGObject menbers
        DGObjectsCollection _allAxes;        // all the axes
        DGObjectsCollection _allTunnels;        // all the tunnels
        HashSet<string> _tunnelLayerIDs;           // tunnel layer IDs
        Dictionary<string, IEnumerable<DGObject>> _selectedTunnelsDict;  // selected tunnels
        
        //graphics members
        ISpatialReference _spatialRef;
        ISymbol _symbol;

        //result
        List<TunnelAxis> _results;
        Dictionary<string, IGraphicCollection> _axesGraphics;

        public DrawTunnelAxesWindow()
        {
            InitializeComponent();

            //Initialize
            _symbol = Runtime.graphicEngine.newSimpleLineSymbol(
                                Colors.Red, Core.Graphics.SimpleLineStyle.Solid, 1.0);
            _results = new List<TunnelAxis>();
            _axesGraphics = new Dictionary<string, IGraphicCollection>();

            _settings = new DrawAxesSettings();
            SettingsHolder.DataContext = _settings;
            Loaded += DrawTunnelAxesWindow_Loaded;
            Unloaded += DrawTunnelAxesWindow_Unloaded;

            _mainFrame = Globals.mainframe;
            _prj = Globals.project;
            if (_mainFrame == null || _prj == null) { _initFailed = true; return; }

            _structureDomain = _prj.getDomain(DomainType.Structure);
            if (_structureDomain == null) { _initFailed = true; return; }

            _allTunnels = _structureDomain.getObjects("Tunnel");
            _tunnelLayerIDs = new HashSet<string>();
            foreach (DGObjects objs in _allTunnels)
                _tunnelLayerIDs.Add(objs.definition.GISLayerName);
        }

        void DrawTunnelAxesWindow_Loaded(object sender,
            RoutedEventArgs e)
        {
            Application curApp = Application.Current;
            Window mainWindow = curApp.MainWindow;
            this.Owner = mainWindow;

            // position window to the bottom-right
            this.Left = mainWindow.Left +
                (mainWindow.Width - this.ActualWidth - 10);
            this.Top = mainWindow.Top +
                (mainWindow.Height - this.ActualHeight - 10);

            if (_initFailed)
                return;

            // fill input view combobox
            List<IView> planViews = new List<IView>();
            foreach (IView view in _mainFrame.views)
            {
                if (view.eMap.MapType == EngineeringMapType.FootPrintMap)
                    planViews.Add(view);
            }
            InputCB.ItemsSource = planViews;
            if (planViews.Count > 0)
            {
                _inputView = planViews[0];
                InputCB.SelectedIndex = 0;             
            }
            else
            {
                _initFailed = true; return;
            }

            //fill the tunnels combobox
            _inputView_objSelectionChangedListener(null, null);
        }

        void DrawTunnelAxesWindow_Unloaded(object sender,
            RoutedEventArgs e)
        {
            if (!_initFailed)
            {
                _inputView.addSeletableLayer("_ALL");
                // remove the listener to object selection changed event
                _inputView.objSelectionChangedTrigger -=
                    _inputView_objSelectionChangedListener;
            }
        }

        private void InputCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //last input view
            _inputView.addSeletableLayer("_ALL");
            _inputView.objSelectionChangedTrigger -=
                    _inputView_objSelectionChangedListener;

            //new input view
            _inputView = InputCB.SelectedItem as IView;
            _inputView.removeSelectableLayer("_ALL");
            foreach (string layerID in _tunnelLayerIDs)
                _inputView.addSeletableLayer(layerID);

            // add a listener to object selection changed event
            _inputView.objSelectionChangedTrigger +=
                _inputView_objSelectionChangedListener;
        }

        void _inputView_objSelectionChangedListener(object sender,
            ObjSelectionChangedEventArgs e)
        {
            // fill tunnel combobox
            _selectedTunnelsDict = _prj.getSelectedObjs(_structureDomain, "Tunnel");
            List<DGObject> _tunnels = new List<DGObject>();
            foreach (var item in _selectedTunnelsDict.Values)
            {
                foreach (var obj in item)
                {
                    _tunnels.Add(obj);
                }
            }
            if (_tunnels != null && _tunnels.Count() > 0)
                TunnelLB.ItemsSource = _tunnels;
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            if (_initFailed)
                return;
            StartAnalysis();
            GenerateGraphics();
            SyncToView();
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        void StartAnalysis()
        {
            string mapID = _inputView.eMap.MapID;
            _settings.mapID = mapID;

            _spatialRef = _inputView.spatialReference;

            // check all needed data is set up correctly
            if (_selectedTunnelsDict == null || _selectedTunnelsDict.Count() == 0)
                return;

            foreach (string tunnelLayerID in _selectedTunnelsDict.Keys)
            {
                IGraphicsLayer gLayer = _inputView.getLayer("DES_AXL");
                if (gLayer == null)
                    continue;

                IEnumerable<DGObject> tunnels = _selectedTunnelsDict[tunnelLayerID];
                foreach (var obj in tunnels)
                {
                    Tunnel tunnel = obj as Tunnel;
                    if (tunnel.StartMileage == null || tunnel.EndMileage == null || tunnel.LineNo == null)
                        continue;

                    _allAxes = _structureDomain.getObjects("TunnelAxis");
                    TunnelAxis ta = _allAxes[tunnel.id] as TunnelAxis;
                    if (ta == null)
                        continue;

                    //analysis the axis, change the engineering coordinate to map coordinate
                    TunnelAxis engineeringAxis = ta;
                    int count = engineeringAxis.AxisPoints.Count;

                    IGraphicCollection gc = gLayer.getGraphics(ta);
                    if (gc.Count == 0)
                        continue;
                    IGraphic g = gc[0];
                    IPolyline pline = g.Geometry as IPolyline;
                    if (pline == null)
                        continue;
                    IPointCollection pc = pline.GetPoints();
                    if (pc.Count != count)
                    {
                        return;
                    }

                    TunnelAxis mapAxis = new TunnelAxis();
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
                    mapAxis = TunnelMappingUtility.ClipAxis(mapAxis, (double)tunnel.StartMileage, (double)tunnel.EndMileage);
                    if (mapAxis.AxisPoints[0].Mileage != tunnel.StartMileage)
                    {
                        TunnelAxisPoint pt = TunnelMappingUtility.MileageToAxisPoint((double)tunnel.StartMileage, mapAxis);
                        mapAxis.AxisPoints.Insert(0, pt);
                    }
                    count = mapAxis.AxisPoints.Count;
                    if (mapAxis.AxisPoints[count - 1].Mileage != tunnel.EndMileage)
                    {
                        TunnelAxisPoint pt = TunnelMappingUtility.MileageToAxisPoint((double)tunnel.EndMileage, mapAxis);
                        mapAxis.AxisPoints.Add(pt);
                    }
                    mapAxis.LineNo = ta.LineNo;
                    mapAxis.id = ta.id;
                    mapAxis.name = ta.name;
                    _results.Add(mapAxis);
                }
            }        
        }

        void GenerateGraphics()
        {
            //generate the result graphics
            foreach (var mapAxis in _results)
            {
                IPointCollection axisPts = TunnelMappingUtility.AxisTo2DPoints(mapAxis,_spatialRef);
                if (axisPts == null)
                    return;

                IGraphic g = Runtime.graphicEngine.newPolyline(axisPts);
                g.Symbol = _symbol;

                IGraphicCollection gc = Runtime.graphicEngine.newGraphicCollection();
                gc.Add(g);

                if (_settings.drawMilage)
                    DrawAxisMileage(mapAxis, gc, _settings.interval);
                _axesGraphics[mapAxis.name] = gc;
            }

            //assign name attribute to each graphic
            foreach (var mapAxis in _results)
            {
                TunnelAxis ta = mapAxis as TunnelAxis;
                IGraphicCollection gc = _axesGraphics[ta.name];
                foreach (IGraphic g in gc)
                    g.Attributes["Name"] = ta.name;
            }
        }

        void SyncToView()
        {
            //get the type, name, gislayername for the new draw axes
            string type = "";
            string name = "";
            string gislayername = "";
            foreach(DGObjects dgObjs in _allAxes)
            {
                type = dgObjs.definition.Type;
                name = dgObjs.definition.Name;
                gislayername = dgObjs.definition.GISLayerName;
                if (type != "" || name != "" || gislayername != "")
                    break;
            }
            
            //save the results to the analysis domain
            Domain analysisDomain = HelperFunction.GetAnalysisDomain();
            DGObjectsCollection allAxes = analysisDomain.getObjects(type);
            if (allAxes == null)
            {
                HelperFunction.NewObjsInAnalysisDomain(type, name, true, gislayername);
                allAxes = analysisDomain.getObjects(type);
            }

            DGObjects axesDGObjects = HelperFunction.GetDGObjsByName(allAxes, name);
            foreach (TunnelAxis eAxis in _results)
            {
                axesDGObjects[eAxis.name] = eAxis;
            }

            // add graphic to the view
            HashSet<IGraphicsLayer> axesGraphicLayers = new HashSet<IGraphicsLayer>();
            foreach (string idName in _axesGraphics.Keys)
            {
                IGraphicCollection gc = _axesGraphics[idName];
                DGObject obj = allAxes[idName];
                string layerID = obj.parent.definition.GISLayerName;
                IGraphicsLayer gLayerSt = HelperFunction.GeteLayer(_inputView, layerID);
                gLayerSt.addGraphics(gc);
                axesGraphicLayers.Add(gLayerSt);
            }

            // sync objects with graphics
            List<DGObject> axes = allAxes.merge();
            foreach (IGraphicsLayer gLayer in axesGraphicLayers)
                gLayer.syncObjects(axes);

            // calculate extent
            IEnvelope ext = null;
            foreach (IGraphicCollection gc in _axesGraphics.Values)
            {
                IEnvelope itemExt = GraphicsUtil.GetGraphicsEnvelope(gc);
                if (ext == null)
                    ext = itemExt;
                else
                    ext = ext.Union(itemExt);
            }

            _mainFrame.activeView = _inputView;
            _inputView.zoomTo(ext);
        }

        void DrawAxisMileage(TunnelAxis axis, IGraphicCollection gc, int interval)
        {         
            int count = axis.AxisPoints.Count;
            double start = axis.AxisPoints[0].Mileage;
            double end = axis.AxisPoints[count - 1].Mileage;
            double len = end - start;
            int num = (int)len / interval;
            num += 3;       // add begin, end points, and a truncated interval

            for (int i = 0; i < num; ++i)
            {
                double m = start + i * interval;
                m = ((int)m / interval) * interval;
                if (m < start) m = start;
                if (m > end) m = end;

                int m1 = (int)m / 1000;
                int m2 = (int)m % 1000;
                string strK = "K" + m1.ToString() + "+" + m2.ToString();

                TunnelAxisPoint axisPt = TunnelMappingUtility.MileageToAxisPoint(m, axis);
                IMapPoint p = Runtime.geometryEngine.newMapPoint(axisPt.X + 3, axisPt.Y + 3, _spatialRef);
                IGraphic g = Runtime.graphicEngine.newText(strK, p, Colors.Red, "Arial", 10.0);
                gc.Add(g);

                IPointCollection pc = TunnelMappingUtility.ComputeTunnelCrossLine(m, _settings.tickLen, _settings.tickLen, axis, _spatialRef);
                g = Runtime.graphicEngine.newPolyline(pc);
                g.Symbol = _symbol;
                gc.Add(g);
            }
        }      
    }
}

