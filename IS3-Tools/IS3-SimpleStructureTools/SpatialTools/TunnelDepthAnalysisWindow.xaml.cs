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

namespace IS3.SimpleStructureTools.SpatialTools
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

    public class DepthResult
    {
        public string Name { get; set; }
        public double Mileage { get; set; }
        public double Depth { get; set; }

        public int Index { get; set; }      // index for the axis point
        public IGraphicCollection gc { get; set; }

        public DepthResult()
        {
            gc = Runtime.graphicEngine.newGraphicCollection();
        }
    }
    
    /// <summary>
    /// Interaction logic for TunnelDepthAnalysisWindow.xaml
    /// </summary>
    public partial class TunnelDepthAnalysisWindow : Window
    {
        //Desktop members
        Project _prj;                       // the project
        Domain _structureDomain;              // the structure domain of the project
        IMainFrame _mainFrame;              // the main frame
        IView _inputView;                   // the input view 
        bool _initFailed;                   // set to true if initialization failed

        //DGObject menbers
        DGObjectsCollection _allTunnels;        // all the tunnels
        HashSet<string> _tunnelLayerIDs;           // tunnel layer IDs
        Dictionary<string, IEnumerable<DGObject>> _selectedTunnelsDict;  // selected tunnels

        //graphics members
        ISpatialReference _spatialRef;
        ISimpleLineSymbol _linesymbol;

        //result
        Dictionary<int, IGraphicCollection> _depthGraphics;
        
        public TunnelDepthAnalysisWindow()
        {
            InitializeComponent();

            //Initialize
            _linesymbol = Runtime.graphicEngine.newSimpleLineSymbol(
                                Colors.Blue, Core.Graphics.SimpleLineStyle.Solid, 1.0);
            _selectedTunnelsDict = new Dictionary<string, IEnumerable<DGObject>>();
            _depthGraphics = new Dictionary<int, IGraphicCollection>();

            Loaded += TunnelDepthAnalysisWindow_Loaded;
            Unloaded += TunnelDepthAnalysisWindow_Unloaded;

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

        void TunnelDepthAnalysisWindow_Loaded(object sender,
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
            List<IView> profileViews = new List<IView>();
            foreach (IView view in _mainFrame.views)
            {
                if (view.eMap.MapType == EngineeringMapType.GeneralProfileMap)
                    profileViews.Add(view);
            }
            InputCB.ItemsSource = profileViews;
            if (profileViews.Count > 0)
            {
                _inputView = profileViews[0];
                InputCB.SelectedIndex = 0;         
            }
            else
            {
                _initFailed = true; return;
            }

            // fill tunnel combobox
            _inputView_objSelectionChangedListener(null, null);              
        }

        void TunnelDepthAnalysisWindow_Unloaded(object sender,
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
            {
                TunnelLB.ItemsSource = _tunnels;
                TunnelLB.SelectedIndex = 0;
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

        private void TunnelLB_SelectionChanged(object sender, 
            SelectionChangedEventArgs e)
        {
            Tunnel tunnel = TunnelLB.SelectedItem as Tunnel;
            TB_Start.Text = ((double)tunnel.StartMileage).ToString("0.00");
            TB_End.Text = ((double)tunnel.EndMileage).ToString("0.00");
        }

        private void DepthList_SelectionChanged(object sender, 
            SelectionChangedEventArgs e)
        {

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
            IView view = InputCB.SelectedItem as IView;
            _spatialRef = view.spatialReference;

            if (view == null)
            {
                return;
            }

            // get selected tunnel
            Tunnel tunnel = TunnelLB.SelectedItem as Tunnel;
            if (tunnel == null)
                return;
            Domain domain = HelperFunction.GetAnalysisDomain();
            DGObjectsCollection alleLPResults = domain.getObjects("LPResult");
            LPResult LPresult = alleLPResults[tunnel.id.ToString() + view.eMap.MapID] as LPResult;
            if (LPresult == null)
                return;

            double startMilage = double.Parse(TB_Start.Text);
            double endMilage = double.Parse(TB_End.Text);

            List<DepthResult> results = DoAnalysis(LPresult, view.eMap.MapID, startMilage, endMilage);

            DepthList.ItemsSource = results;

            if (LPresult == null)
            {
                return;
            }
            double zScale = LPresult.Setting.zScale;
            TunnelAxis axis = LPresult.ProfileAxis;
            double x = 0.0, y = 0.0;

            IGraphicCollection gc = Runtime.graphicEngine.newGraphicCollection();

            foreach (DepthResult result in results)
            {
                x = axis.AxisPoints[result.Index].X;
                y = result.Depth * zScale;

                IGraphic g = Runtime.graphicEngine.newLine(x, y, x, 0,_spatialRef);
                g.Symbol = _linesymbol;
                result.gc.Add(g);
                gc.Add(g);

                IMapPoint mp = Runtime.geometryEngine.newMapPoint(x, y / 2,_spatialRef);
                g = Runtime.graphicEngine.newText(result.Depth.ToString("#.00"), mp, Colors.Blue, "Arial", 10.0);
                result.gc.Add(g);
                gc.Add(g);
            }
            _depthGraphics[tunnel.LineNo.Value] = gc;
        }

        #region analysis code
        public static List<DepthResult> DoAnalysis(LPResult result, string mapID, double startMilage, double endMilage)
        {   
            double zScale = result.Setting.zScale;

            TunnelAxis axis = result.ProfileAxis;
            int num = axis.AxisPoints.Count;
            int iMax = -1, iMin = -1;
            double zMax = -1e10, zMin = 1e+10;
            for (int i = 0; i < num; ++i)
            {
                TunnelAxisPoint axisPt = axis.AxisPoints[i];
                double m = axisPt.Mileage;
                if (m >= startMilage && m <= endMilage)
                {
                    if (axisPt.Z > zMax) { zMax = axisPt.Z; iMax = i; }
                    if (axisPt.Z < zMin) { zMin = axisPt.Z; iMin = i; }
                }
            }

            if (iMax == -1 || iMin == -1)
                return null;

            Tunnel tunnel = result.Obj as Tunnel;
            double h = 0.0;
            if (tunnel.Height != null)
                h = tunnel.Height.Value;

            DepthResult r1 = new DepthResult();
            r1.Name = "Shallowest";
            r1.Mileage = axis.AxisPoints[iMin].Mileage;
            r1.Depth = axis.AxisPoints[iMin].Z / zScale + h / 2;
            r1.Index = iMin;

            DepthResult r2 = new DepthResult();
            r2.Name = "Deepest";
            r2.Mileage = axis.AxisPoints[iMax].Mileage;
            r2.Depth = axis.AxisPoints[iMax].Z / zScale + h / 2;
            r2.Index = iMax;

            List<DepthResult> results = new List<DepthResult>();
            results.Add(r1);
            results.Add(r2);

            return results;
        }
        #endregion

        void GenerateGraphics()
        {

        }

        void SyncToView()
        {
            // add graphic to the view
            IView view = InputCB.SelectedItem as IView;
            string layerID = "TunnelDepth";
            IGraphicsLayer gLayerSt = HelperFunction.GeteLayer(view, layerID);
            foreach (IGraphicCollection gc in _depthGraphics.Values)
                gLayerSt.addGraphics(gc);

            // calculate extent
            IEnvelope ext = null;
            foreach (IGraphicCollection gc in _depthGraphics.Values)
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
