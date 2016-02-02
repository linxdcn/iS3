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
        Project _prj;                       // the project
        Domain _structureDomain;              // the geology domain of the project
        IMainFrame _mainFrame;              // the main frame
        IView _inputView;                   // the input view
        IEnumerable<DGObject> _sls;
        string _slLayerID = "DES_RIN";
        string _csLayerID = "CSLoad";
        string _stLayerID = "GEO_STR";      
        bool _initFailed;                   // set to true if initialization failed
        ISymbol _symbol;

        SoilInitalStress _IniStress;
        Dictionary<int, IGraphicCollection> _slsGraphics;
        
        public TunnelCSLoadWindow()
        {
            InitializeComponent();

            _IniStress = new SoilInitalStress();
            _slsGraphics = new Dictionary<int, IGraphicCollection>();

            Loaded += TunnelCSLoadWindow_Loaded;
            Unloaded += TunnelCSLoadWindow_Unloaded;

            _mainFrame = Globals.mainframe;
            _prj = Globals.project;

            if (_mainFrame == null || _prj == null) { _initFailed = true; return; }

            _structureDomain = _prj.getDomain(DomainType.Structure);
            if (_structureDomain == null) { _initFailed = true; return; }

            DGObjects objs = _structureDomain.getObjects("Segmentlining");
            if (objs != null)
                _slLayerID = objs.definition.GISLayerName;

            // add borehole layer as selectable layer
            _inputView.removeSelectableLayer("_ALL");
            _inputView.addSeletableLayer(_slLayerID);

            // add a listener to object selection changed event
            _inputView.objSelectionChangedTrigger +=
                _inputView_objSelectionChangedListener;
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
            List<IView> views = new List<IView>();
            foreach (IView view in _mainFrame.views)
            {
                if (view.eMap.MapType == EngineeringMapType.GeneralProfileMap)
                    views.Add(view);
            }
            InputViewCB.ItemsSource = views;
            if (views.Count > 0)
                InputViewCB.SelectedIndex = 0;

            // fill tunnels combobox
            _sls = _prj.getSelectedObjs(_structureDomain, "Segmentlining");
            if (_sls != null && _sls.Count() > 0)
            {
                SLLB.ItemsSource = _sls;
                SLLB.SelectedIndex = 0;
            }
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
            // fill tunnels combobox
            _sls = _prj.getSelectedObjs(_structureDomain, "Segmentlining");
            if (_sls != null && _sls.Count() > 0)
            {
                SLLB.ItemsSource = _sls;
                SLLB.SelectedIndex = 0;
            }
        }

        private void SLLB_SelectionChanged(object sender, SelectionChangedEventArgs e)
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
            AfterAnalysis();
            Close();
        }

        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        void StartAnalysis()
        {
            IView view = InputViewCB.SelectedItem as IView;

            // get user inputed parameter 
            GetParam();

            // get the segment lining
            DGObject obj = SLLB.SelectedItem as DGObject;
            SegmentLining sl = obj as SegmentLining;
            if (sl.ConstructionRecord.TBMDrivingRecord.PressureDetail.AirBubblePressure != null)
                _IniStress.SigmaT = sl.ConstructionRecord.TBMDrivingRecord.PressureDetail.AirBubblePressure.Value * 100;
            //SLType slType = TunnelTools.GetSLType(cdgObj, sl.SLTypeID);
            _IniStress.D = 6.2; //待解决
            _IniStress.Thickness = 0.35; //待解决

            // get water table and soil top data
            DGObjects segGraphics = _structureDomain.getObjects("CSGraphic");
            if (segGraphics == null)
                segGraphics = _structureDomain.getObjects("LSGraphic");
            SegmentGraphicBase segGraphic = segGraphics[sl.id.ToString() + view.eMap.MapID] as SegmentGraphicBase;
            double? top = StratumMappingUtility.StratumTop(segGraphic.CenterX, view, _stLayerID);
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
                0, view, _stLayerID, setting);

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
                SoilProperty sp = StrataTools.GetSoilProperty(stratumObj.name, startmileage);
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
            FormatResult(sl.RingNo, rIS);
            _IniStress.result = rIS;
            sl.Attributes["csLoad"] = _IniStress;

            DrawResult(sl, segGraphic.CenterX, segGraphic.CenterZ, rIS);
        }

        void AfterAnalysis()
        {
            IView view = _mainFrame.ActiveView;

            double zScale = view.EMap.ScaleZ;
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

            GraphicCollection gc = ArcGISMappingUtility.DistributedLoad_Vertical(x1, x2, x3, y1, y2,
                _fillSymbol, _arrowFillSymbol, _lineSymbol);
            foreach (Graphic g in gc)
            {
                _Graphics.Add(g);
            }

            gc = ArcGISMappingUtility.DistributedLoad_Vertical(x4, x5, x6, y1, y2,
                _fillSymbol, _arrowFillSymbol, _lineSymbol);
            foreach (Graphic g in gc)
            {
                _Graphics.Add(g);
            }

            // right
            x1 = centerX + radius * zScale + gap;
            x2 = x1 + (rIS.Qe1 / max) * len;
            x3 = x1 + (rIS.Qe2 / max) * len;
            x4 = x3 + gap;
            x5 = x4 + (rIS.Qw1 / max) * len;
            x6 = x4 + (rIS.Qw2 / max) * len;

            gc = ArcGISMappingUtility.DistributedLoad_Vertical(x1, x2, x3, y1, y2,
                _fillSymbol, _arrowFillSymbol, _lineSymbol);
            foreach (Graphic g in gc)
            {
                _Graphics.Add(g);
            }
            string str = string.Format("Qe1={0:0.0} kPa", rIS.Qe1);
            Graphic gText = ArcGISMappingUtility.NewText(str, x2, y2, Colors.Blue, "Arial", 14.0);
            _Graphics.Add(gText);
            str = string.Format("Qe2={0:0.0} kPa", rIS.Qe2);
            gText = ArcGISMappingUtility.NewText(str, x3, y1, Colors.Blue, "Arial", 14.0);
            _Graphics.Add(gText);

            gc = ArcGISMappingUtility.DistributedLoad_Vertical(x4, x5, x6, y1, y2,
                _fillSymbol, _arrowFillSymbol, _lineSymbol);
            foreach (Graphic g in gc)
            {
                _Graphics.Add(g);
            }
            str = string.Format("Qw1={0:0.0} kPa", rIS.Qw1);
            gText = ArcGISMappingUtility.NewText(str, x5, y2, Colors.Blue, "Arial", 14.0);
            _Graphics.Add(gText);
            str = string.Format("Qw2={0:0.0} kPa", rIS.Qw2);
            gText = ArcGISMappingUtility.NewText(str, x6, y1, Colors.Blue, "Arial", 14.0);
            _Graphics.Add(gText);

            // top
            x1 = centerX - radius * zScale;
            x2 = centerX + radius * zScale;
            y1 = centerZ + radius * zScale + gap;
            y2 = y1 + (rIS.Pg / max) * len;
            y3 = y2 + gap;
            y4 = y3 + (rIS.Pe1 / max) * len;
            y5 = y4 + gap;
            y6 = y5 + (rIS.Pw1 / max) * len;

            gc = ArcGISMappingUtility.DistributedLoad_Horizontal(x1, x2, y1, y2, y2,
                _fillSymbol, _arrowFillSymbol, _lineSymbol);
            foreach (Graphic g in gc)
            {
                _Graphics.Add(g);
            }
            str = string.Format("Pg={0:0.0} kPa", rIS.Pg);
            gText = ArcGISMappingUtility.NewText(str, x2, y2, Colors.Blue, "Arial", 14.0);
            _Graphics.Add(gText);

            gc = ArcGISMappingUtility.DistributedLoad_Horizontal(x1, x2, y3, y4, y4,
                _fillSymbol, _arrowFillSymbol, _lineSymbol);
            foreach (Graphic g in gc)
            {
                _Graphics.Add(g);
            }
            str = string.Format("Pe1={0:0.0} kPa", rIS.Pe1);
            gText = ArcGISMappingUtility.NewText(str, x2, y4, Colors.Blue, "Arial", 14.0);
            _Graphics.Add(gText);

            gc = ArcGISMappingUtility.DistributedLoad_Horizontal(x1, x2, y5, y6, y6,
                _fillSymbol, _arrowFillSymbol, _lineSymbol);
            foreach (Graphic g in gc)
            {
                _Graphics.Add(g);
            }
            str = string.Format("Pw1={0:0.0} kPa", rIS.Pw1);
            gText = ArcGISMappingUtility.NewText(str, x2, y6, Colors.Blue, "Arial", 14.0);
            _Graphics.Add(gText);

            // bottom
            y1 = centerZ - radius * zScale - gap;
            y2 = y1 - (rIS.Pe2 / max) * len;
            y3 = y2 - gap;
            y4 = y3 - (rIS.Pw2 / max) * len;

            gc = ArcGISMappingUtility.DistributedLoad_Horizontal(x1, x2, y1, y2, y2,
                _fillSymbol, _arrowFillSymbol, _lineSymbol);
            foreach (Graphic g in gc)
            {
                _Graphics.Add(g);
            }
            str = string.Format("Pe2={0:0.0} kPa", rIS.Pe2);
            gText = ArcGISMappingUtility.NewText(str, x2, y2, Colors.Blue, "Arial", 14.0);
            _Graphics.Add(gText);

            gc = ArcGISMappingUtility.DistributedLoad_Horizontal(x1, x2, y3, y4, y4,
                _fillSymbol, _arrowFillSymbol, _lineSymbol);
            foreach (Graphic g in gc)
            {
                _Graphics.Add(g);
            }
            str = string.Format("Pw2={0:0.0} kPa", rIS.Pw2);
            gText = ArcGISMappingUtility.NewText(str, x2, y4, Colors.Blue, "Arial", 14.0);
            _Graphics.Add(gText);

            TunnelCSLoadGraphics[sl.ID] = _Graphics;
        }
    }
}
