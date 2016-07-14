using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

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

    public class DrawTunnelsSettings
    {
        public bool drawProjectionLine { get; set; }
        public bool reverseAxisDir { get; set; }
        public double xOffset { get; set; }
        public double zScale { get; set; }
        public int interval { get; set; }

        public DrawTunnelsSettings()
        {
            drawProjectionLine = true;
            reverseAxisDir = false;
            xOffset = 0.0;
            zScale = 1.0;
            interval = 100;
        }
    }
    
    /// <summary>
    /// Interaction logic for DrawTunnelsWindow.xaml
    /// </summary>
    public partial class DrawTunnelsWindow : Window
    {
        //Desktop members
        Project _prj;                       // the project
        Domain _structureDomain;              // the structure domain of the project
        IMainFrame _mainFrame;              // the main frame
        IView _inputView;                   // the input view 
        DrawTunnelsSettings _settings;          // the analysis settings
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
        Dictionary<int, IGraphicCollection> _tunnelsGraphics;
        List<DGObject> _lpResults;

        public DrawTunnelsWindow()
        {
            InitializeComponent();

            //Initialize
            ISimpleLineSymbol linesymbol = Runtime.graphicEngine.newSimpleLineSymbol(
                                Colors.Red, Core.Graphics.SimpleLineStyle.Solid, 1.0);
            _symbol = Runtime.graphicEngine.newSimpleFillSymbol(
                                Colors.Blue, SimpleFillStyle.Solid, linesymbol);
            _tunnelsGraphics = new Dictionary<int, IGraphicCollection>();
            _selectedTunnelsDict = new Dictionary<string, IEnumerable<DGObject>>();
            _lpResults = new List<DGObject>();

            _settings = new DrawTunnelsSettings();
            SettingsHolder.DataContext = _settings;
            Loaded += DrawTunnelsWindow_Loaded;
            Unloaded += DrawTunnelsWindow_Unloaded;

            _mainFrame = Globals.mainframe;
            _prj = Globals.project;

            if (_mainFrame == null || _prj == null) { _initFailed = true; return; }

            _structureDomain = _prj.getDomain(DomainType.Structure);
            if (_structureDomain == null) { _initFailed = true; return; }

            _allTunnels = _structureDomain.getObjects("Tunnel");
            _tunnelLayerIDs = new HashSet<string>();
            foreach (DGObjects objs in _allTunnels)
                _tunnelLayerIDs.Add(objs.definition.GISLayerName);

            _allAxes = _structureDomain.getObjects("TunnelAxis");
            if (_allAxes == null) { _initFailed = true; return; }
        }

        void DrawTunnelsWindow_Loaded(object sender,
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

            // fill output view combobox
            List<IView> profileViews = new List<IView>();
            foreach (IView view in _mainFrame.views)
            {
                if (view.eMap.MapType == EngineeringMapType.GeneralProfileMap)
                    profileViews.Add(view);
            }
            OutputViewCB.ItemsSource = profileViews;
            if (profileViews.Count > 0)
                OutputViewCB.SelectedIndex = 0;

            // fill tunnel combobox
            _inputView_objSelectionChangedListener(null, null);
        }

        void DrawTunnelsWindow_Unloaded(object sender,
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

        private void TunnelLB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //make the listBox not selectable
            ListView lv = sender as ListView;
            lv.SelectedIndex = -1;
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
            IView view = OutputViewCB.SelectedItem as IView;
            _spatialRef = view.spatialReference;
            IPolyline projLine = view.eMap.profileLine;
            if (projLine == null)
                return;
            if (view.eMap.MapType != EngineeringMapType.GeneralProfileMap)
                return;

            foreach (string tunnelLayerID in _selectedTunnelsDict.Keys)
            {
                IEnumerable<DGObject> tunnels = _selectedTunnelsDict[tunnelLayerID];
                foreach (DGObject obj in tunnels)
                {
                    Tunnel tunnel = obj as Tunnel;

                    TunnelAxis ta = _allAxes[tunnel.id] as TunnelAxis;
                    if (ta == null)
                        continue;

                    LPResult lpResult = TunnelMappingUtility.ProjectTunnel_LS(view.eMap, obj, _spatialRef, _settings);
                    _lpResults.Add(lpResult);
                }
            }
            
        }

        void GenerateGraphics()
        {
            //generate the result graphics
            //assign name attribute to each graphic
            foreach (var obj in _lpResults)
            {
                LPResult lpResult = obj as LPResult;
                Tunnel tunnel = lpResult.Obj as Tunnel;
                IGraphicCollection gc = Runtime.graphicEngine.newGraphicCollection();
                gc.Add(lpResult.Graphic);
                foreach (IGraphic g in gc)
                    g.Attributes["Name"] = tunnel.name;
                _tunnelsGraphics[tunnel.id] = gc;
            }
        }

        void SyncToView()
        {
            IView view = OutputViewCB.SelectedItem as IView;
            //get the type, name
            string type = "LPResult";
            string name = "AllLPResults";

            //save the results to the analysis domain
            Domain analysisDomain = HelperFunction.GetAnalysisDomain();
            DGObjectsCollection allLPResults = analysisDomain.getObjects(type);
            if (allLPResults == null)
            {
                HelperFunction.NewObjsInAnalysisDomain(type, name);
                allLPResults = analysisDomain.getObjects(type);
            }

            DGObjects lpDGObjects = HelperFunction.GetDGObjsByName(allLPResults, name);
            foreach (LPResult lpResult in _lpResults)
            {
                lpDGObjects[lpResult.name] = lpResult;
            }

            // add graphic to the view
            HashSet<IGraphicsLayer> tunnelGraphicLayers = new HashSet<IGraphicsLayer>();
            foreach (int id in _tunnelsGraphics.Keys)
            {
                IGraphicCollection gc = _tunnelsGraphics[id];
                DGObject obj = _allTunnels[id];
                string layerID = obj.parent.definition.GISLayerName;
                IGraphicsLayer gLayerSt = HelperFunction.GetLayer(view, layerID);
                gLayerSt.addGraphics(gc);
                tunnelGraphicLayers.Add(gLayerSt);
            }

            // sync objects with graphics
            List<DGObject> tunnels = _allTunnels.merge();
            foreach (IGraphicsLayer gLayer in tunnelGraphicLayers)
                gLayer.syncObjects(tunnels);

            // calculate extent
            IEnvelope ext = null;
            foreach (IGraphicCollection gc in _tunnelsGraphics.Values)
            {
                IEnvelope itemExt = GraphicsUtil.GetGraphicsEnvelope(gc);
                if (ext == null)
                    ext = itemExt;
                else
                    ext = ext.Union(itemExt);
            }

            _mainFrame.activeView = view;
            view.zoomTo(ext);
        }
    }
}
