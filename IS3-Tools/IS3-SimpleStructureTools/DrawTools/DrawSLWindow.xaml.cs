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

    /// <summary>
    /// Interaction logic for DrawSLWindow.xaml
    /// </summary>
    public partial class DrawSLWindow : Window
    {
        //Desktop members
        Project _prj;                       // the project
        Domain _structureDomain;              // the geology domain of the project
        IMainFrame _mainFrame;              // the main frame
        IView _inputView;                   // the input view
        IView _outputView;                  //the output view
        bool _initFailed;                   // set to true if initialization failed

        //DGObject members
        DGObjectsCollection _allSLs;        // all the tunnels
        List<string> _slLayerIDs;           // segmentlining layer IDs
        Dictionary<string, IEnumerable<DGObject>> _selectedSLsDict;  // selected segmentlining

        //graphics members
        ISpatialReference _spatialRef;
        ISymbol _fillSymbol;
        ISymbol _whiteFillSymbol;

        //result
        Dictionary<int, IGraphicCollection> _slsGraphics;
        Dictionary<string, LSGraphics> _lsGraphics;
        Dictionary<string, CSGraphics> _csGraphics;
        
        public DrawSLWindow()
        {
            InitializeComponent();

            //Initialize
            ISimpleLineSymbol linesymbol = Runtime.graphicEngine.newSimpleLineSymbol(
                                Colors.Black, Core.Graphics.SimpleLineStyle.Solid, 1.0);
            _fillSymbol = Runtime.graphicEngine.newSimpleFillSymbol(
                                Color.FromArgb(150, 0, 0, 255), SimpleFillStyle.Solid, linesymbol);
            _whiteFillSymbol = Runtime.graphicEngine.newSimpleFillSymbol(
                                Color.FromArgb(150, 255, 255, 255), SimpleFillStyle.Solid, linesymbol);
            _selectedSLsDict = new Dictionary<string, IEnumerable<DGObject>>();
            _slsGraphics = new Dictionary<int, IGraphicCollection>();
            _lsGraphics = new Dictionary<string, LSGraphics>();
            _csGraphics = new Dictionary<string, CSGraphics>();

            Loaded += DrawSLWindow_Loaded;
            Unloaded += DrawSLWindow_Unloaded;

            _mainFrame = Globals.mainframe;
            _prj = Globals.project;

            if (_mainFrame == null || _prj == null) { _initFailed = true; return; }

            _structureDomain = _prj.getDomain(DomainType.Structure);
            if (_structureDomain == null) { _initFailed = true; return; }

            _allSLs = _structureDomain.getObjects("SegmentLining");
            _slLayerIDs = new List<string>();
            foreach (DGObjects objs in _allSLs)
                _slLayerIDs.Add(objs.definition.GISLayerName);
        }

        void DrawSLWindow_Loaded(object sender,
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

            // fill segmentlining combobox
            _inputView_objSelectionChangedListener(null, null);

            // fill output view combobox
            List<IView> views = new List<IView>();
            foreach (IView view in _mainFrame.views)
            {
                if (view.eMap.MapType == EngineeringMapType.GeneralProfileMap)
                    views.Add(view);
            }
            OutputViewCB.ItemsSource = views;
            if (views.Count > 0)
            {
                OutputViewCB.SelectedIndex = 0;
                _outputView = views[0];
            }           

            //draw type
            TypeCB.SelectedIndex = 0;
        }

        void DrawSLWindow_Unloaded(object sender,
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
            // fill segmentlining combobox
            _selectedSLsDict = _prj.getSelectedObjs(_structureDomain, "SegmentLining");
            List<DGObject> _sls = new List<DGObject>();
            foreach (var item in _selectedSLsDict.Values)
            {
                foreach (var obj in item)
                {
                    _sls.Add(obj);
                }
            }
            if (_sls != null && _sls.Count() > 0)
                SLLB.ItemsSource = _sls;
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
            foreach (string layerID in _slLayerIDs)
                _inputView.addSeletableLayer(layerID);

            // add a listener to object selection changed event
            _inputView.objSelectionChangedTrigger +=
                _inputView_objSelectionChangedListener;
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

            foreach (string SLLayerID in _selectedSLsDict.Keys)
            {
                IEnumerable<DGObject> sls = _selectedSLsDict[SLLayerID];

                List<DGObject> slList = sls.ToList();

                if (view.eMap.MapType == EngineeringMapType.FootPrintMap)
                {
                    Draw_PlanView(slList);
                }
                else if (view.eMap.MapType == EngineeringMapType.GeneralProfileMap)
                {
                    int drawType = TypeCB.SelectedIndex;
                    if (drawType == 0)
                    {
                        drawType = AutoSelectDrawType(slList);
                        if (drawType == -1)
                        {
                            return;
                        }
                    }

                    if (drawType == 1)
                    {
                        Drwa_ProfileView(slList);
                    }
                    else if (drawType == 2)
                    {
                        Draw_CrossSectionView(slList);
                    }
                }
            }     
        }

        // Auto select draw type by comparing the ratio of segment length to the projected length
        // on the profile line.
        int AutoSelectDrawType(List<DGObject> slList)
        {
            SegmentLining sl = slList[0] as SegmentLining;
            SLType slType = TunnelTools.getSLType(sl.SLTypeID);
            if (slType == null)
                return -1;
            double width = slType.Width;
            double outerDiameter = slType.OuterDiameter;
            double innerDiameter = slType.InnerDiameter;
            double mileage;
            if (sl.ConstructionRecord.MileageAsBuilt != null)
                mileage = sl.ConstructionRecord.MileageAsBuilt.Value;
            else if (sl.StartMileage != null)
                mileage = (double)sl.StartMileage;
            else
                return -1;
            

            TunnelAxis planAxis = TunnelTools.GetTunnelFootprintAxis(sl.LineNo) as TunnelAxis;

            TunnelAxisPoint axisPt1 = TunnelMappingUtility.MileageToAxisPoint(mileage, planAxis);
            TunnelAxisPoint axisPt2 = TunnelMappingUtility.MileageToAxisPoint(mileage + width, planAxis);
            IMapPoint p1 = Runtime.geometryEngine.newMapPoint(axisPt1.X, axisPt1.Y, _spatialRef);
            IMapPoint p2 = Runtime.geometryEngine.newMapPoint(axisPt2.X, axisPt2.Y, _spatialRef);

            IView view = OutputViewCB.SelectedItem as IView;
            IPolyline pline = view.eMap.profileLine;
            if (pline == null)
            {
                return -1;
            }
            IPointCollection pts = pline.GetPoints();

            IMapPoint p3 = Runtime.geometryEngine.newMapPoint(0, 0, _spatialRef);
            IMapPoint p4 = Runtime.geometryEngine.newMapPoint(0, 0, _spatialRef);

            double d1 = 0, d2 = 0;

            GeomUtil.ProjectPointToPolyline(p1, pts, ref d1, ref p3);
            GeomUtil.ProjectPointToPolyline(p2, pts, ref d2, ref p4);

            double prjLen = Math.Abs(d1 - d2) / view.eMap.Scale;
            if (prjLen > 0.5 * width)    // > 30 degree    
                return 1;   // Longitudinal view
            else
                return 2;   // Cross section view
        }

        void Draw_PlanView(List<DGObject> slList)
        {
            IView view = OutputViewCB.SelectedItem as IView;
            double mapScale = view.eMap.Scale;

            foreach (DGObject obj in slList)
            {
                SegmentLining sl = obj as SegmentLining;

                IGraphicCollection gc = Runtime.graphicEngine.newGraphicCollection();

                //get axis
                TunnelAxis planAxis = TunnelTools.GetTunnelFootprintAxis(sl.LineNo);

                SLType slType = TunnelTools.getSLType(sl.SLTypeID);
                if (slType == null)
                    return;
                double width = slType.Width;
                double outerDiameter = slType.OuterDiameter;
                double innerDiameter = slType.InnerDiameter;

                double m1;
                if (sl.ConstructionRecord.MileageAsBuilt != null && sl.ConstructionRecord.MileageAsBuilt > 0)
                    m1 = sl.ConstructionRecord.MileageAsBuilt.Value - width / 2;
                else if (sl.StartMileage != null)
                    m1 = (double)sl.StartMileage - width / 2;
                else
                    return;
                double m2 = m1 + width;
                TunnelAxisPoint axisP1 = TunnelMappingUtility.MileageToAxisPoint(m1, planAxis);
                TunnelAxisPoint axisP2 = TunnelMappingUtility.MileageToAxisPoint(m2, planAxis);

                double orient;
                orient = GeometryAlgorithms.LineOrientation(axisP1.X, axisP1.Y, axisP2.X, axisP2.Y);
                double d1 = orient + Math.PI / 2;
                double d2 = orient - Math.PI / 2;
                double x1 = mapScale * outerDiameter * Math.Cos(d1) / 2;
                double y1 = mapScale * outerDiameter * Math.Sin(d1) / 2;
                double x2 = mapScale * outerDiameter * Math.Cos(d2) / 2;
                double y2 = mapScale * outerDiameter * Math.Sin(d2) / 2;

                IMapPoint p1 = Runtime.geometryEngine.newMapPoint(axisP1.X + x1, axisP1.Y + y1,_spatialRef);
                IMapPoint p2 = Runtime.geometryEngine.newMapPoint(axisP2.X + x1, axisP2.Y + y1,_spatialRef);
                IMapPoint p3 = Runtime.geometryEngine.newMapPoint(axisP2.X + x2, axisP2.Y + y2,_spatialRef);
                IMapPoint p4 = Runtime.geometryEngine.newMapPoint(axisP1.X + x2, axisP1.Y + y2,_spatialRef);

                IGraphic g = Runtime.graphicEngine.newQuadrilateral(p1, p2, p3, p4);
                g.Symbol = _fillSymbol;
                gc.Add(g);

                x1 = mapScale * innerDiameter * Math.Cos(d1) / 2;
                y1 = mapScale * innerDiameter * Math.Sin(d1) / 2;
                x2 = mapScale * innerDiameter * Math.Cos(d2) / 2;
                y2 = mapScale * innerDiameter * Math.Sin(d2) / 2;

                IMapPoint p5 = Runtime.geometryEngine.newMapPoint(axisP1.X + x1, axisP1.Y + y1,_spatialRef);
                IMapPoint p6 = Runtime.geometryEngine.newMapPoint(axisP2.X + x1, axisP2.Y + y1,_spatialRef);
                IMapPoint p7 = Runtime.geometryEngine.newMapPoint(axisP2.X + x2, axisP2.Y + y2,_spatialRef);
                IMapPoint p8 = Runtime.geometryEngine.newMapPoint(axisP1.X + x2, axisP1.Y + y2,_spatialRef);

                g = Runtime.graphicEngine.newQuadrilateral(p5, p6, p7, p8);
                g.Symbol = _whiteFillSymbol;
                gc.Add(g);
                _slsGraphics[sl.id] = gc;
            }
        }

        void Drwa_ProfileView(List<DGObject> slList)
        {
            IView view = OutputViewCB.SelectedItem as IView;

            foreach (DGObject obj in slList)
            {
                SegmentLining sl = obj as SegmentLining;

                IGraphicCollection gc = Runtime.graphicEngine.newGraphicCollection();

                TunnelAxis profileAxis = TunnelTools.GetTunnelProfileAxis(sl.LineNo, view.eMap);

                Domain analysisDomain = HelperFunction.GetAnalysisDomain();
                DGObjectsCollection lps = analysisDomain.getObjects("LPResult");
                if (lps == null)
                    return;

                Tunnel tunnel = TunnelTools.GetTunnel(sl.LineNo);
                if (tunnel == null)
                {
                    return;
                }

                string name = tunnel.id.ToString() + view.eMap.MapID;
                LPResult lpResult = lps[name] as LPResult;
                if (lpResult == null)
                {
                    return;
                }
                double zScale = lpResult.Setting.zScale;

                SLType slType = TunnelTools.getSLType(sl.SLTypeID);
                if (slType == null)
                    return;
                double width = slType.Width;
                double outerDiameter = slType.OuterDiameter;
                double innerDiameter = slType.InnerDiameter;

                double m1;
                if (sl.ConstructionRecord.MileageAsBuilt != null && sl.ConstructionRecord.MileageAsBuilt > 0)
                    m1 = sl.ConstructionRecord.MileageAsBuilt.Value - width / 2;
                else if (sl.StartMileage != null)
                    m1 = (double)sl.StartMileage - width / 2;
                else
                    return;

                double m2 = m1 + width;
                TunnelAxisPoint axisP1 = TunnelMappingUtility.MileageToAxisPoint(m1, profileAxis);
                TunnelAxisPoint axisP2 = TunnelMappingUtility.MileageToAxisPoint(m2, profileAxis);

                LSGraphics lsGraphics = new LSGraphics();
                lsGraphics.Obj = sl;
                lsGraphics.CenterX = (axisP1.X + axisP2.X) / 2.0;
                lsGraphics.CenterZ = (axisP1.Z + axisP2.Z) / 2.0;
                lsGraphics.OuterDiameter = outerDiameter;

                IMapPoint p1 = Runtime.geometryEngine.newMapPoint(axisP1.X, axisP1.Z + outerDiameter * zScale / 2,_spatialRef);
                IMapPoint p2 = Runtime.geometryEngine.newMapPoint(axisP2.X, axisP2.Z + outerDiameter * zScale / 2,_spatialRef);
                IMapPoint p3 = Runtime.geometryEngine.newMapPoint(axisP2.X, axisP2.Z - outerDiameter * zScale / 2,_spatialRef);
                IMapPoint p4 = Runtime.geometryEngine.newMapPoint(axisP1.X, axisP1.Z - outerDiameter * zScale / 2,_spatialRef);

                IGraphic g1 = Runtime.graphicEngine.newQuadrilateral(p1, p2, p3, p4);
                g1.Symbol = _fillSymbol;
                lsGraphics.Segment = g1;
                gc.Add(g1);

                IMapPoint p5 = Runtime.geometryEngine.newMapPoint(axisP1.X, axisP1.Z + innerDiameter * zScale / 2,_spatialRef);
                IMapPoint p6 = Runtime.geometryEngine.newMapPoint(axisP2.X, axisP2.Z + innerDiameter * zScale / 2,_spatialRef);
                IMapPoint p7 = Runtime.geometryEngine.newMapPoint(axisP2.X, axisP2.Z - innerDiameter * zScale / 2,_spatialRef);
                IMapPoint p8 = Runtime.geometryEngine.newMapPoint(axisP1.X, axisP1.Z - innerDiameter * zScale / 2,_spatialRef);

                IGraphic g2 = Runtime.graphicEngine.newQuadrilateral(p5, p6, p7, p8);
                g2.Symbol = _whiteFillSymbol;
                lsGraphics.Interior = g2;
                gc.Add(g2);

                _slsGraphics[sl.id] = gc;
                lsGraphics.id = sl.id;
                lsGraphics.name = sl.id.ToString() + view.eMap.MapID;
                _lsGraphics[lsGraphics.name] = lsGraphics;
            }
        }

        void Draw_CrossSectionView(List<DGObject> slList)
        {
            IView view = OutputViewCB.SelectedItem as IView;
            double zScale = view.eMap.ScaleZ;
            IPolyline pline = view.eMap.profileLine;
            if (pline == null)
            {
                return;
            }
            IPointCollection pts = pline.GetPoints();

            foreach (DGObject obj in slList)
            {
                SegmentLining sl = obj as SegmentLining;

                SLType slType = TunnelTools.getSLType(sl.SLTypeID);
                if (slType == null)
                    return;
                List<Segment> segments = slType.Segments;
                double width = slType.Width;
                double outerDiameter = slType.OuterDiameter * zScale;
                double innerDiameter = slType.InnerDiameter * zScale;

                double m1;
                if (sl.ConstructionRecord.MileageAsBuilt != null && sl.ConstructionRecord.MileageAsBuilt > 0)
                    m1 = sl.ConstructionRecord.MileageAsBuilt.Value - width / 2;
                else if (sl.StartMileage != null)
                    m1 = (double)sl.StartMileage - width / 2;
                else
                    return;

                TunnelAxis planAxis = TunnelTools.GetTunnelFootprintAxis(sl.LineNo) as TunnelAxis;
                TunnelAxisPoint axisPt1 = TunnelMappingUtility.MileageToAxisPoint(m1, planAxis);

                IMapPoint p1 = Runtime.geometryEngine.newMapPoint(axisPt1.X, axisPt1.Y,_spatialRef);
                IMapPoint p2 = Runtime.geometryEngine.newMapPoint(0, 0,_spatialRef);
                double x = 0;
                double z = axisPt1.Z * zScale;
                GeomUtil.ProjectPointToPolyline(p1, pts, ref x, ref p2);
                x /= view.eMap.Scale;

                CSGraphics csGraphics = new CSGraphics();
                csGraphics.Obj = sl;
                csGraphics.CenterX = x;
                csGraphics.CenterZ = axisPt1.Z;
                csGraphics.OuterDiameter = slType.OuterDiameter;

                // Interior
                IGraphic gInterior = HelperFunction.NewCircle(x, z, innerDiameter / 2.0 , _spatialRef);
                gInterior.Symbol = _whiteFillSymbol;
                csGraphics.Interior = gInterior;
                IGraphicCollection gc = Runtime.graphicEngine.newGraphicCollection();
                gc.Add(gInterior);

                if (slType.NumberOfSegments == segments.Count)
                {
                    int totalKeyPos = slType.TotalKeyPos;
                    int keyPos = 0;
                    if (sl.ConstructionRecord.KeySegmentPosition != null)
                        keyPos = sl.ConstructionRecord.KeySegmentPosition.Value;
                    IGraphicCollection igc = ShieldTunnelMappingUtility.NewSegments(x, z,
                        innerDiameter / 2.0, outerDiameter / 2.0,
                        segments, keyPos, totalKeyPos, _spatialRef);
                    foreach (IGraphic g in igc)
                        gc.Add(g);

                    csGraphics.IsSegmental = true;
                    csGraphics.Segments = igc;
                }
                else
                {
                    /*
                    Graphic g = ArcGISMappingUtility.NewDonut(x, z, innerDiameter / 2.0, outerDiameter / 2.0);
                    g.Symbol = _fillSymbol;
                    ArcGISMappingUtility.SetGraphicAttr(g, sl.ID, sl);
                    csGraphics.IsSegmental = false;
                    csGraphics.Outline = g;
                    _Graphics.Add(g);
                     */
                }
                _slsGraphics[sl.id] = gc;
                csGraphics.id = sl.id;
                csGraphics.name = sl.id.ToString() + view.eMap.MapID;
                _csGraphics[csGraphics.name] = csGraphics;
            }
        }

        void GenerateGraphics()
        {
            //generate the result graphics
            //assign name attribute to each graphic
            foreach (int id in _slsGraphics.Keys)
            {
                SegmentLining sl = _allSLs[id] as SegmentLining;
                IGraphicCollection gc = _slsGraphics[sl.id];
                foreach (IGraphic g in gc)
                    g.Attributes["Name"] = sl.name;
            }
        }

        void SyncToView()
        {
            IView view = OutputViewCB.SelectedItem as IView;
            //get the type, name
            string type_ls = "LSGraphic";
            string name_ls = "AllLSGraphics";
            string type_cs = "CSGraphic";
            string name_cs = "AllCSGraphics";

            //save the results to the analysis domain
            Domain analysisDomain = HelperFunction.GetAnalysisDomain();
            DGObjectsCollection allLSGraphics = analysisDomain.getObjects(type_ls);
            DGObjectsCollection allCSGraphics = analysisDomain.getObjects(type_cs);
            if (allLSGraphics == null)
            {
                HelperFunction.NewObjsInAnalysisDomain(type_ls, name_ls);
                allLSGraphics = analysisDomain.getObjects(type_ls);
            }
            if (allCSGraphics == null)
            {
                HelperFunction.NewObjsInAnalysisDomain(type_cs, name_cs);
                allCSGraphics = analysisDomain.getObjects(type_cs);
            }

            DGObjects lsDGObjects = HelperFunction.GetDGObjsByName(allLSGraphics, name_ls);
            foreach (LSGraphics lsResult in _lsGraphics.Values)
            {
                lsDGObjects[lsResult.name] = lsResult;
            }
            DGObjects csDGObjects = HelperFunction.GetDGObjsByName(allCSGraphics, name_cs);
            foreach (CSGraphics csResult in _csGraphics.Values)
            {
                csDGObjects[csResult.name] = csResult;
            }

            // add graphic to the view
            HashSet<IGraphicsLayer> slGraphicLayers = new HashSet<IGraphicsLayer>();
            foreach (int id in _slsGraphics.Keys)
            {
                IGraphicCollection gc = _slsGraphics[id];
                DGObject obj = _allSLs[id];
                string layerID = obj.parent.definition.GISLayerName;
                IGraphicsLayer gLayer = HelperFunction.GetLayer(view, layerID);
                gLayer.addGraphics(gc);
                slGraphicLayers.Add(gLayer);
            }

            // sync objects with graphics
            List<DGObject> sls = _allSLs.merge();
            foreach (IGraphicsLayer gLayer in slGraphicLayers)
                gLayer.syncObjects(sls);

            // calculate extent
            IEnvelope ext = null;
            foreach (IGraphicCollection gc in _slsGraphics.Values)
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
