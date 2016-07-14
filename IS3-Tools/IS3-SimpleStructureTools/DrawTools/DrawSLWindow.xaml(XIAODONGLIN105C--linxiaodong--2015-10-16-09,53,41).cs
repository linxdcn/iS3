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
                InputCB.SelectedIndex = 0;
                _inputView = planViews[0];
            }
            else
            {
                _initFailed = true; return;
            }

            // add tunnel layer as selectable layer
            _inputView.removeSelectableLayer("_ALL");
            foreach (string layerID in _slLayerIDs)
                _inputView.addSeletableLayer(layerID);

            // add a listener to object selection changed event
            _inputView.objSelectionChangedTrigger +=
                _inputView_objSelectionChangedListener;

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

            // fill output view combobox
            List<IView> views = new List<IView>();
            foreach (IView view in _mainFrame.views)
            {
                if (view.eMap.MapType == EngineeringMapType.GeneralProfileMap ||
                    view.eMap.MapType == EngineeringMapType.FootPrintMap)
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
            _inputView = sender as IView;
            _inputView.removeSelectableLayer("_ALL");
            foreach (string layerID in _slLayerIDs)
                _inputView.addSeletableLayer(layerID);

            // add a listener to object selection changed event
            _inputView.objSelectionChangedTrigger +=
                _inputView_objSelectionChangedListener;
        }

        private void SLLB_SelectionChanged(object sender, SelectionChangedEventArgs e)
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
            foreach (string SLLayerID in _selectedSLsDict.Keys)
            {
                IEnumerable<DGObject> sls = _selectedSLsDict[SLLayerID];

                List<DGObject> slList = sls.ToList();

                IView view = OutputViewCB.SelectedItem as IView;
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
            IMapPoint p1 = HelperFunction.NewMapPoint(axisPt1.X, axisPt1.Y);
            IMapPoint p2 = HelperFunction.NewMapPoint(axisPt2.X, axisPt2.Y);

            Tuple<IGraphic, string> selectedLine =
                LineCB.SelectedItem as Tuple<IGraphic, string>;
            IGraphic g = selectedLine.Item1;
            IPolyline pline = g.Geometry as IPolyline;
            if (pline == null)
            {
                return -1;
            }
            IPointCollection pts = pline.GetPoints();

            IMapPoint p3 = HelperFunction.NewMapPoint(0,0);
            IMapPoint p4 = HelperFunction.NewMapPoint(0, 0);

            double d1 = 0, d2 = 0;

            GeomUtil.ProjectPointToPolyline(p1, pts, ref d1, ref p3);
            GeomUtil.ProjectPointToPolyline(p2, pts, ref d2, ref p4);

            IView view = OutputViewCB.SelectedItem as IView;
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

                IGraphicCollection gc = HelperFunction.NewGraphicCollection();

                //get axis
                TunnelAxis planAxis = TunnelTools.GetTunnelFootprintAxis(sl.LineNo);

                ShieldTunnelProjectInformation subPrj = _prj.projDef.GetSubProjectInformation("BoredTunnel") as ShieldTunnelProjectInformation;
                double width = 1.2;
                double outerDiameter = subPrj.OuterDiameter;
                double innerDiameter = subPrj.InnerDiamter;

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

                IMapPoint p1 = HelperFunction.NewMapPoint(axisP1.X + x1, axisP1.Y + y1);
                IMapPoint p2 = HelperFunction.NewMapPoint(axisP2.X + x1, axisP2.Y + y1);
                IMapPoint p3 = HelperFunction.NewMapPoint(axisP2.X + x2, axisP2.Y + y2);
                IMapPoint p4 = HelperFunction.NewMapPoint(axisP1.X + x2, axisP1.Y + y2);

                IGraphic g = HelperFunction.NewQuadrilateral(p1, p2, p3, p4);
                g.Symbol = _fillSymbol;
                gc.Add(g);

                x1 = mapScale * innerDiameter * Math.Cos(d1) / 2;
                y1 = mapScale * innerDiameter * Math.Sin(d1) / 2;
                x2 = mapScale * innerDiameter * Math.Cos(d2) / 2;
                y2 = mapScale * innerDiameter * Math.Sin(d2) / 2;

                IMapPoint p5 = HelperFunction.NewMapPoint(axisP1.X + x1, axisP1.Y + y1);
                IMapPoint p6 = HelperFunction.NewMapPoint(axisP2.X + x1, axisP2.Y + y1);
                IMapPoint p7 = HelperFunction.NewMapPoint(axisP2.X + x2, axisP2.Y + y2);
                IMapPoint p8 = HelperFunction.NewMapPoint(axisP1.X + x2, axisP1.Y + y2);

                g = HelperFunction.NewQuadrilateral(p5, p6, p7, p8);
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

                IGraphicCollection gc = HelperFunction.NewGraphicCollection();

                TunnelAxis profileAxis = TunnelTools.GetTunnelProfileAxis(sl.LineNo, view.eMap);

                DGObjects lps = _structureDomain.getObjects("LPResult");
                if (lps == null)
                    return;

                Tunnel tunnel = TunnelTools.GetTunnel(sl.LineNo);
                if (tunnel == null)
                {
                    return;
                }

                string name = tunnel.id.ToString() + view.eMap.MapID;
                if (!lps.containsKey(name))
                    return;

                LPResult lpResult = lps[name] as LPResult;
                if (lpResult == null)
                {
                    return;
                }
                double zScale = lpResult.Setting.zScale;

                ShieldTunnelProjectInformation subPrj = _prj.projDef.GetSubProjectInformation("BoredTunnel") as ShieldTunnelProjectInformation;
                double width = 1.2;
                double outerDiameter = subPrj.OuterDiameter;
                double innerDiameter = subPrj.InnerDiamter;

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

                IMapPoint p1 = HelperFunction.NewMapPoint(axisP1.X, axisP1.Z + outerDiameter * zScale / 2);
                IMapPoint p2 = HelperFunction.NewMapPoint(axisP2.X, axisP2.Z + outerDiameter * zScale / 2);
                IMapPoint p3 = HelperFunction.NewMapPoint(axisP2.X, axisP2.Z - outerDiameter * zScale / 2);
                IMapPoint p4 = HelperFunction.NewMapPoint(axisP1.X, axisP1.Z - outerDiameter * zScale / 2);

                IGraphic g1 = HelperFunction.NewQuadrilateral(p1, p2, p3, p4);
                g1.Symbol = _fillSymbol;
                lsGraphics.Segment = g1;
                gc.Add(g1);

                IMapPoint p5 = HelperFunction.NewMapPoint(axisP1.X, axisP1.Z + innerDiameter * zScale / 2);
                IMapPoint p6 = HelperFunction.NewMapPoint(axisP2.X, axisP2.Z + innerDiameter * zScale / 2);
                IMapPoint p7 = HelperFunction.NewMapPoint(axisP2.X, axisP2.Z - innerDiameter * zScale / 2);
                IMapPoint p8 = HelperFunction.NewMapPoint(axisP1.X, axisP1.Z - innerDiameter * zScale / 2);

                IGraphic g2 = HelperFunction.NewQuadrilateral(p5, p6, p7, p8);
                g2.Symbol = _whiteFillSymbol;
                lsGraphics.Interior = g2;
                gc.Add(g2);

                _slsGraphics[sl.id] = gc;
                lsGraphics.name = sl.id.ToString() + view.eMap.MapID;
                _lsGraphics[lsGraphics.name] = lsGraphics;
            }
        }

        void Draw_CrossSectionView(List<DGObject> slList)
        {
            IView view = OutputViewCB.SelectedItem as IView;
            double zScale = view.eMap.ScaleZ;

            Tuple<IGraphic, string> selectedLine =
                LineCB.SelectedItem as Tuple<IGraphic, string>;
            IGraphic g = selectedLine.Item1;
            IPolyline pline = g.Geometry as IPolyline;
            if (pline == null)
            {
                return;
            }
            IPointCollection pts = pline.GetPoints();

            foreach (DGObject obj in slList)
            {
                SegmentLining sl = obj as SegmentLining;

                ShieldTunnelProjectInformation subPrj = _prj.projDef.GetSubProjectInformation("BoredTunnel") as ShieldTunnelProjectInformation;
                Tunnel tunnel = TunnelTools.GetTunnel(sl.LineNo);
                double width = (double)tunnel.Width;
                double outerDiameter = subPrj.OuterDiameter * zScale;
                double innerDiameter = subPrj.InnerDiamter * zScale;

                double m1;
                if (sl.ConstructionRecord.MileageAsBuilt != null && sl.ConstructionRecord.MileageAsBuilt > 0)
                    m1 = sl.ConstructionRecord.MileageAsBuilt.Value - width / 2;
                else if (sl.StartMileage != null)
                    m1 = (double)sl.StartMileage - width / 2;
                else
                    return;

                TunnelAxis planAxis = TunnelTools.GetTunnelFootprintAxis(sl.LineNo) as TunnelAxis;
                TunnelAxisPoint axisPt1 = TunnelMappingUtility.MileageToAxisPoint(m1, planAxis);

                IMapPoint p1 = HelperFunction.NewMapPoint(axisPt1.X, axisPt1.Y);
                IMapPoint p2 = HelperFunction.NewMapPoint(0, 0);
                double x = 0;
                double z = axisPt1.Z * zScale;
                GeomUtil.ProjectPointToPolyline(p1, pts, ref x, ref p2);
                //x /= view.EMap.RefEMap.Scale;
                x /= view.eMap.Scale;

                CSGraphics csGraphics = new CSGraphics();
                csGraphics.Obj = sl;
                csGraphics.CenterX = x;
                csGraphics.CenterZ = axisPt1.Z;
                csGraphics.OuterDiameter = subPrj.OuterDiameter;

                // Interior
                IGraphic gInterior = HelperFunction.NewCircle(x, z, innerDiameter / 2.0);
                gInterior.Symbol = _whiteFillSymbol;
                csGraphics.Interior = gInterior;
                IGraphicCollection gc = HelperFunction.NewGraphicCollection();
                gc.Add(gInterior);
                /*
                if (slType.NumberOfSegments == segments.Count)
                {
                    int totalKeyPos = slType.TotalKeyPos;
                    int keyPos = 0;
                    if (sl.ConstructionRecord.KeySegmentPosition != null)
                        keyPos = sl.ConstructionRecord.KeySegmentPosition.Value;
                    GraphicCollection gc = ShieldTunnelMappingUtility.NewSegments(x, z,
                        innerDiameter / 2.0, outerDiameter / 2.0,
                        segments, keyPos, totalKeyPos);
                    foreach (Graphic g in gc)
                    {
                        ArcGISMappingUtility.SetGraphicAttr(g, sl.ID, sl);
                        _Graphics.Add(g);
                    }

                    csGraphics.IsSegmental = true;
                    csGraphics.Segments = gc;
                }
                else
                {
                    Graphic g = ArcGISMappingUtility.NewDonut(x, z, innerDiameter / 2.0, outerDiameter / 2.0);
                    g.Symbol = _fillSymbol;
                    ArcGISMappingUtility.SetGraphicAttr(g, sl.ID, sl);
                    csGraphics.IsSegmental = false;
                    csGraphics.Outline = g;
                    _Graphics.Add(g);
                }
                 */
                _slsGraphics[sl.id] = gc;
                csGraphics.name = sl.id.ToString() + view.eMap.MapID;
                _csGraphics[csGraphics.name] = csGraphics;
            }
        }

        void GenerateGraphics()
        {

        }

        void SyncToView()
        {
            //generate a new DGObjects
            DGObjects alleDGObjects = _structureDomain.getObjects("LSGraphic");
            if (alleDGObjects == null)
            {
                DGObjectsDefinition def = new DGObjectsDefinition();
                def.Type = "LSGraphic";
                def.Name = "AllLSGraphics";
                def.HasGeometry = false;
                alleDGObjects = new DGObjects(def);
                alleDGObjects.parent = _structureDomain;
                _structureDomain.objsDefinitions[def.Name] = def;
                _structureDomain.objsContainer[def.Name] = alleDGObjects;
            }
            foreach (LSGraphics ls in _lsGraphics.Values)
                alleDGObjects[ls.name] = ls;
            alleDGObjects.buildIDIndex();

            //generate a new DGObjects
            alleDGObjects = _structureDomain.getObjects("CSGraphic");
            if (alleDGObjects == null)
            {
                DGObjectsDefinition def = new DGObjectsDefinition();
                def.Type = "CSGraphic";
                def.Name = "AllCSGraphics";
                def.HasGeometry = false;
                alleDGObjects = new DGObjects(def);
                alleDGObjects.parent = _structureDomain;
                _structureDomain.objsDefinitions[def.Name] = def;
                _structureDomain.objsContainer[def.Name] = alleDGObjects;
            }
            foreach (CSGraphics cs in _csGraphics.Values)
                alleDGObjects[cs.name] = cs;
            alleDGObjects.buildIDIndex();  
            
            // Assign name to each graphic objects,
            // so that the graphics layer can sync them with corresponding object.
            DGObjects alleSLs = _structureDomain.getObjects("SegmentLining");
            foreach (int slID in _slsGraphics.Keys)
            {
                SegmentLining sl = alleSLs.id2Obj[slID] as SegmentLining;
                IGraphicCollection gc = _slsGraphics[slID];
                foreach (IGraphic g in gc)
                    g.Attributes["Name"] = sl.name;
            }

            // get the sl layer
            IView outputView = OutputViewCB.SelectedItem as IView;
            IGraphicsLayer gLayerTun = HelperFunction.getLayer(outputView, _slLayerID);

            // add graphic to the view, and sync objects with graphics
            foreach (int id in _slsGraphics.Keys)
            {
                IGraphicCollection gc = _slsGraphics[id];
                gLayerTun.addGraphics(gc);
            }
            gLayerTun.syncObjects(alleSLs);

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

            _mainFrame.activeView = outputView;
            outputView.zoomTo(ext);
        }
    }
}
