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
using IS3.Geology;
using IS3.SimpleStructureTools.Helper;
using IS3.SimpleStructureTools.Helper.Analysis;
using IS3.SimpleStructureTools.Helper.Mapping;

namespace IS3.SimpleStructureTools.LoadTools
{
    /// <summary>
    /// Interaction logic for TunnelCSLoadWindow.xaml
    /// </summary>
    public partial class TunnelCSLoadWindow : Window
    {
        //Desktop members
        Project _prj;                       // the project
        Domain _structureDomain;              // the structure domain of the project
        Domain _geologyDomain;              // the geology domain of the project
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
        ISymbol _lineSymbol;
        ISymbol _arrowFillSymbol;

        //result
        SoilInitalStress _IniStress;                            //result object
        Dictionary<string, IGraphicCollection> _slsGraphics;       //result graphic
        
        public TunnelCSLoadWindow()
        {
            InitializeComponent();

            ISimpleLineSymbol outline = Runtime.graphicEngine.newSimpleLineSymbol(
                                Colors.Black, Core.Graphics.SimpleLineStyle.Solid, 1.0);
            _fillSymbol = Runtime.graphicEngine.newSimpleFillSymbol(
                                Colors.Cyan, SimpleFillStyle.Solid, outline);
            _lineSymbol = Runtime.graphicEngine.newSimpleLineSymbol(
                                Colors.Blue, Core.Graphics.SimpleLineStyle.Solid, 1.0);
            _arrowFillSymbol = Runtime.graphicEngine.newSimpleLineSymbol(
                                Colors.Blue, Core.Graphics.SimpleLineStyle.Solid, 1.0);
            _IniStress = new SoilInitalStress();
            _slsGraphics = new Dictionary<string, IGraphicCollection>();

            Loaded += TunnelCSLoadWindow_Loaded;
            Unloaded += TunnelCSLoadWindow_Unloaded;

            _mainFrame = Globals.mainframe;
            _prj = Globals.project;

            if (_mainFrame == null || _prj == null) { _initFailed = true; return; }

            _structureDomain = _prj.getDomain(DomainType.Structure);
            if (_structureDomain == null) { _initFailed = true; return; }
            _geologyDomain = _prj.getDomain(DomainType.Geology);
            if (_geologyDomain == null) { _initFailed = true; return; }

            _allSLs = _structureDomain.getObjects("SegmentLining");
            _slLayerIDs = new List<string>();
            foreach (DGObjects objs in _allSLs)
                _slLayerIDs.Add(objs.definition.GISLayerName);
        }

        void TunnelCSLoadWindow_Loaded(object sender,
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
            InputViewCB.ItemsSource = profileViews;
            if (profileViews.Count > 0)
            {
                _inputView = profileViews[0];
                InputViewCB.SelectedIndex = 0;
            }
            else
            {
                _initFailed = true; return;
            }

            // fill segmentlining combobox
            _inputView_objSelectionChangedListener(null, null);

            //fill initial value
            DGObjectsCollection crwObjs = _geologyDomain.getObjects("RiverWater");
            RiverWater rwObj = null;
            if(crwObjs != null)
            {
                List<DGObject> crwObjsList = crwObjs.merge();
                if (crwObjsList.Count() != 0)
                    rwObj = crwObjsList[0] as RiverWater;
            }
            
            DGObjectsCollection cpwObjs = _geologyDomain.getObjects("PhreaticWater");
            PhreaticWater pwObj = null;
            if(cpwObjs != null)
            {
                List<DGObject> cpwObjsList = cpwObjs.merge();
                if (cpwObjsList.Count() != 0)
                    pwObj = cpwObjsList[0] as PhreaticWater;
            }
            
            // Try to use river water at first
            if (rwObj != null)
                TB_WaterTable.Text = rwObj.AvHighTidalLevel.Value.ToString("0.0") + " m";
            else if (pwObj != null)
                TB_WaterTable.Text = pwObj.AvElevation.Value.ToString("0.0") + " m";
            else
            {
                TB_WaterTable.Text = "0 m";
            }
            TB_Surcharge.Text = "0 kPa";
        }

        void TunnelCSLoadWindow_Unloaded(object sender,
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
            {
                SLLB.ItemsSource = _sls;
                SLLB.SelectedIndex = 0;
            }
                
        }

        private void InputCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            //last input view
            _inputView.addSeletableLayer("_ALL");
            _inputView.objSelectionChangedTrigger -=
                    _inputView_objSelectionChangedListener;

            //new input view
            _inputView = InputViewCB.SelectedItem as IView;
            _inputView.removeSelectableLayer("_ALL");
            foreach (string layerID in _slLayerIDs)
                _inputView.addSeletableLayer(layerID);

            // add a listener to object selection changed event
            _inputView.objSelectionChangedTrigger +=
                _inputView_objSelectionChangedListener;
        }

        private void SLLB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void StrataList_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        void GetParam()
        {
            _IniStress.WaterTable = HelperFunction.ToNumber(TB_WaterTable.Text);
            _IniStress.P0 = HelperFunction.ToNumber(TB_Surcharge.Text);
            _IniStress.Method = SoilInitalStress.ISMethod.OverburdenPressure;
            if (RB1.IsChecked.Value)
                _IniStress.Method = SoilInitalStress.ISMethod.OverburdenPressure;
            else
                _IniStress.Method = SoilInitalStress.ISMethod.TerzaghiFormula;
        }

        void FormatResult(SoilInitalStress.IResult rIS)
        {
            TB_Pe1.Text = rIS.Pe1.ToString("0.0") + " kPa";
            TB_Pe2.Text = rIS.Pe2.ToString("0.0") + " kPa";
            TB_Qe1.Text = rIS.Qe1.ToString("0.0") + " kPa";
            TB_Qe2.Text = rIS.Qe2.ToString("0.0") + " kPa";
            TB_Pw1.Text = rIS.Pw1.ToString("0.0") + " kPa";
            TB_Pw2.Text = rIS.Pw2.ToString("0.0") + " kPa";
            TB_Qw1.Text = rIS.Qw1.ToString("0.0") + " kPa";
            TB_Qw2.Text = rIS.Qw2.ToString("0.0") + " kPa";
            TB_Pg.Text = rIS.Pg.ToString("0.0") + " kPa";
            TB_Result.Text = _IniStress.StrResult;
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {
            if (_initFailed)
                return;
            StartAnalysis();
            SyncToView();
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        void StartAnalysis()
        {
            IView view = InputViewCB.SelectedItem as IView;
            _spatialRef = view.spatialReference;

            // get user inputed parameter 
            GetParam();

            // get the segment lining
            DGObject obj = SLLB.SelectedItem as DGObject;
            SegmentLining sl = obj as SegmentLining;
            if (sl.ConstructionRecord.TBMDrivingRecord.PressureDetail.AirBubblePressure != null)
                _IniStress.SigmaT = sl.ConstructionRecord.TBMDrivingRecord.PressureDetail.AirBubblePressure.Value * 100;
            SLType slType = TunnelTools.getSLType(sl.SLTypeID);
            if (slType == null)
                return;
            double width = slType.Width;
            double outerDiameter = slType.OuterDiameter;
            double innerDiameter = slType.InnerDiameter;

            // get water table and soil top data
            Domain domain = HelperFunction.getAnalysisDomain();
            DGObjectsCollection segGraphics = domain.getObjects("CSGraphic");
            if (segGraphics.merge().Count == 0)
                segGraphics = domain.getObjects("LSGraphic");
            SegmentGraphicBase segGraphic = segGraphics[sl.id.ToString() + view.eMap.MapID] as SegmentGraphicBase;
            double? top = StratumMappingUtility.StratumTop(segGraphic.CenterX, view, "GEO_STR");
            if (top == null)
            {
                return;
            }
            _IniStress.SoilTop = top.Value;
            _IniStress.TunnelTop = segGraphic.CenterZ + _IniStress.D / 2.0;

            // get soil parameters : take the bottom of the tunnel to compute soil profile
            StratumMappingUtility.Setting setting = new StratumMappingUtility.Setting();
            setting.AllStrata = false;
            List<StratumMappingUtility.Result> strataResults =
                StratumMappingUtility.StrataOnTunnel(segGraphic.CenterX,
                segGraphic.CenterZ - _IniStress.D / 2.0,
                0, view, "GEO_STR", setting);

            List<SoilInitalStress.IStratum> strata = new List<SoilInitalStress.IStratum>();
            foreach (StratumMappingUtility.Result r in strataResults)
            {
                SoilInitalStress.IStratum s = new SoilInitalStress.IStratum();
                DGObject stratumObj = r.StratumObj;
                s.StratumObj = stratumObj;
                s.Name = stratumObj.name;
                s.Thickness = r.Thickness;
                s.Top = r.Top;

                double startmileage;
                if (sl.ConstructionRecord.MileageAsBuilt != null)
                    startmileage = sl.ConstructionRecord.MileageAsBuilt.Value;
                else if (sl.StartMileage != null)
                    startmileage = (double)sl.StartMileage;
                else
                    return;
                SoilProperty sp = GeologyTools.GetSoilProperty(stratumObj.name, startmileage);
                if (sp != null)
                {
                    if (sp.StaticProp.gama != null)
                        s.Gama = sp.StaticProp.gama.Value;
                    if (sp.StaticProp.K0 != null)
                        s.K0 = sp.StaticProp.K0.Value;
                    if (sp.StaticProp.c != null)
                        s.c = sp.StaticProp.c.Value;
                    if (sp.StaticProp.fai != null)
                        s.fai = sp.StaticProp.fai.Value;
                }
                strata.Add(s);
            }
            _IniStress.Strata = strata;
            StrataList.ItemsSource = strata;

            SoilInitalStress.IResult rIS = _IniStress.CalculateIS();
            FormatResult(rIS);
            _IniStress.result = rIS;
            _IniStress.name = sl.ToString() + view.eMap.MapID;
            _IniStress.id = sl.id;
            GenerateGraphics(sl, segGraphic.CenterX, segGraphic.CenterZ, rIS);
        }

        void GenerateGraphics(SegmentLining sl, double centerX, double centerZ, SoilInitalStress.IResult rIS)
        {          
            IView view = InputViewCB.SelectedItem as IView;

            double zScale = view.eMap.ScaleZ;
            double radius = _IniStress.D / 2.0;
            double len = 5.0;   // drawing length for soil pressure, this is for Qe2.
            double gap = 1.0;   // drawing gap between soil pressure and water pressure.
            double x1, x2, x3, x4, x5, x6;
            double y, y1, y2, y3, y4, y5, y6;
            double max = rIS.Pw2;

            // left
            x1 = centerX - radius * zScale - gap;
            x2 = x1 - (rIS.Qe1 / max) * len;
            x3 = x1 - (rIS.Qe2 / max) * len;
            x4 = x3 - gap;
            x5 = x4 - (rIS.Qw1 / max) * len;
            x6 = x4 - (rIS.Qw2 / max) * len;
            y = centerZ;
            y1 = y - zScale * radius;
            y2 = y + zScale * radius;

            IGraphicCollection graphicResult = Runtime.graphicEngine.newGraphicCollection();
            IGraphicCollection gc = GraphicsUtil.DistributedLoad_Vertical(x1, x2, x3, y1, y2,
                _fillSymbol,_arrowFillSymbol,_lineSymbol);
            foreach (IGraphic g in gc)
            {
                graphicResult.Add(g);
            }

            gc = GraphicsUtil.DistributedLoad_Vertical(x4, x5, x6, y1, y2,
                _fillSymbol, _arrowFillSymbol, _lineSymbol);
            foreach (IGraphic g in gc)
            {
                graphicResult.Add(g);
            }

            // right
            x1 = centerX + radius * zScale + gap;
            x2 = x1 + (rIS.Qe1 / max) * len;
            x3 = x1 + (rIS.Qe2 / max) * len;
            x4 = x3 + gap;
            x5 = x4 + (rIS.Qw1 / max) * len;
            x6 = x4 + (rIS.Qw2 / max) * len;

            gc = GraphicsUtil.DistributedLoad_Vertical(x1, x2, x3, y1, y2,
                _fillSymbol, _arrowFillSymbol, _lineSymbol);
            foreach (IGraphic g in gc)
            {
                graphicResult.Add(g);
            }
            string str = string.Format("Qe1={0:0.0} kPa", rIS.Qe1);
            IMapPoint p = Runtime.geometryEngine.newMapPoint(x2, y2, _spatialRef);
            IGraphic gText = Runtime.graphicEngine.newText(str, p, Colors.Blue, "Arial", 14.0);
            graphicResult.Add(gText);
            str = string.Format("Qe2={0:0.0} kPa", rIS.Qe2);
            p = Runtime.geometryEngine.newMapPoint(x3, y1, _spatialRef);
            gText = Runtime.graphicEngine.newText(str, p, Colors.Blue, "Arial", 14.0);
            graphicResult.Add(gText);

            gc = GraphicsUtil.DistributedLoad_Vertical(x4, x5, x6, y1, y2,
                _fillSymbol, _arrowFillSymbol, _lineSymbol);
            foreach (IGraphic g in gc)
            {
                graphicResult.Add(g);
            }
            str = string.Format("Qw1={0:0.0} kPa", rIS.Qw1);
            p = Runtime.geometryEngine.newMapPoint(x5, y2, _spatialRef);
            gText = Runtime.graphicEngine.newText(str, p, Colors.Blue, "Arial", 14.0);
            graphicResult.Add(gText);
            str = string.Format("Qw2={0:0.0} kPa", rIS.Qw2);
            p = Runtime.geometryEngine.newMapPoint(x6, y1, _spatialRef);
            gText = Runtime.graphicEngine.newText(str, p, Colors.Blue, "Arial", 14.0);
            graphicResult.Add(gText);

            // top
            x1 = centerX - radius * zScale;
            x2 = centerX + radius * zScale;
            y1 = centerZ + radius * zScale + gap;
            y2 = y1 + (rIS.Pg / max) * len;
            y3 = y2 + gap;
            y4 = y3 + (rIS.Pe1 / max) * len;
            y5 = y4 + gap;
            y6 = y5 + (rIS.Pw1 / max) * len;

            gc = GraphicsUtil.DistributedLoad_Horizontal(x1, x2, y1, y2, y2,
                _fillSymbol, _arrowFillSymbol, _lineSymbol);
            foreach (IGraphic g in gc)
            {
                graphicResult.Add(g);
            }
            str = string.Format("Pg={0:0.0} kPa", rIS.Pg);
            p = Runtime.geometryEngine.newMapPoint(x2, y2, _spatialRef);
            gText = Runtime.graphicEngine.newText(str, p, Colors.Blue, "Arial", 14.0);
            graphicResult.Add(gText);

            gc = GraphicsUtil.DistributedLoad_Horizontal(x1, x2, y3, y4, y4,
                _fillSymbol, _arrowFillSymbol, _lineSymbol);
            foreach (IGraphic g in gc)
            {
                graphicResult.Add(g);
            }
            str = string.Format("Pe1={0:0.0} kPa", rIS.Pe1);
            p = Runtime.geometryEngine.newMapPoint(x2, y4, _spatialRef);
            gText = Runtime.graphicEngine.newText(str, p, Colors.Blue, "Arial", 14.0);
            graphicResult.Add(gText);

            gc = GraphicsUtil.DistributedLoad_Horizontal(x1, x2, y5, y6, y6,
                _fillSymbol, _arrowFillSymbol, _lineSymbol);
            foreach (IGraphic g in gc)
            {
                graphicResult.Add(g);
            }
            str = string.Format("Pw1={0:0.0} kPa", rIS.Pw1);
            p = Runtime.geometryEngine.newMapPoint(x2, y6, _spatialRef);
            gText = Runtime.graphicEngine.newText(str, p, Colors.Blue, "Arial", 14.0);
            graphicResult.Add(gText);

            // bottom
            y1 = centerZ - radius * zScale - gap;
            y2 = y1 - (rIS.Pe2 / max) * len;
            y3 = y2 - gap;
            y4 = y3 - (rIS.Pw2 / max) * len;

            gc = GraphicsUtil.DistributedLoad_Horizontal(x1, x2, y1, y2, y2,
                _fillSymbol, _arrowFillSymbol, _lineSymbol);
            foreach (IGraphic g in gc)
            {
                graphicResult.Add(g);
            }
            str = string.Format("Pe2={0:0.0} kPa", rIS.Pe2);
            p = Runtime.geometryEngine.newMapPoint(x2, y2, _spatialRef);
            gText = Runtime.graphicEngine.newText(str, p, Colors.Blue, "Arial", 14.0);
            graphicResult.Add(gText);

            gc = GraphicsUtil.DistributedLoad_Horizontal(x1, x2, y3, y4, y4,
                _fillSymbol, _arrowFillSymbol, _lineSymbol);
            foreach (IGraphic g in gc)
            {
                graphicResult.Add(g);
            }
            str = string.Format("Pw2={0:0.0} kPa", rIS.Pw2);
            p = Runtime.geometryEngine.newMapPoint(x2, y4, _spatialRef);
            gText = Runtime.graphicEngine.newText(str, p, Colors.Blue, "Arial", 14.0);
            graphicResult.Add(gText);

            _slsGraphics[sl.id.ToString() + view.eMap.MapID] = graphicResult;
        }

        void SyncToView()
        {
            // add graphic to the view
            IView view = InputViewCB.SelectedItem as IView;
            string layerID = "CSLoad";
            IGraphicsLayer gLayerSt = HelperFunction.getLayer(view, layerID);
            foreach (IGraphicCollection gc in _slsGraphics.Values)
                gLayerSt.addGraphics(gc);

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
