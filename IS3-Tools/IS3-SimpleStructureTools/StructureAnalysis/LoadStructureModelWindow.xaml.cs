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
using System.IO;
using System.Xml;

using IS3.Core;
using IS3.Core.Geometry;
using IS3.Core.Graphics;
using IS3.ShieldTunnel;
using IS3.Geology;
using IS3.SimpleStructureTools.Helper;
using IS3.SimpleStructureTools.Helper.Analysis;
using IS3.SimpleStructureTools.Helper.Mapping;
using IS3.SimpleStructureTools.Helper.File;

namespace IS3.SimpleStructureTools.StructureAnalysis
{
    /// <summary>
    /// Interaction logic for LoadStructureModelWindow.xaml
    /// </summary>
    public partial class LoadStructureModelWindow : Window
    {
        //Desktop members
        Project _prj;                       // the project
        Domain _structureDomain;              // the structure domain of the project
        Domain _geologyDomain;              // the geology domain of the project
        IMainFrame _mainFrame;              // the main frame
        IView _inputView;                   // the input view
        IView _outputView;                  //the output view
        bool _initFailed;                   // set to true if initialization failed
        String ansysPath;                   // path of ansys

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
        Dictionary<string, IGraphicCollection> _slsGraphics;       //result graphic

        public LoadStructureModelWindow()
        {
            InitializeComponent();

            ISimpleLineSymbol outline = Runtime.graphicEngine.newSimpleLineSymbol(
                                Colors.Black, Core.Graphics.SimpleLineStyle.Solid, 1.0);
            _fillSymbol = Runtime.graphicEngine.newSimpleFillSymbol(
                                Colors.Cyan, SimpleFillStyle.Solid, outline);
            _lineSymbol = Runtime.graphicEngine.newSimpleLineSymbol(
                                Colors.Blue, Core.Graphics.SimpleLineStyle.Solid, 1.0);
            _arrowFillSymbol = Runtime.graphicEngine.newSimpleFillSymbol(
                                Colors.Blue, SimpleFillStyle.Solid, outline);
            _slsGraphics = new Dictionary<string, IGraphicCollection>();

            Loaded += LoadStructureModelWindow_Loaded;
            Unloaded += LoadStructureModelWindow_Unloaded;

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

            ansysPath = Runtime.rootPath + "//Conf//ansysPath.xml";
        }

        #region window interaction
        void LoadStructureModelWindow_Loaded(object sender,
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

            // fill ansys path
            string path = "";
            if (File.Exists(ansysPath))
            {
                StreamReader sr = new System.IO.StreamReader(ansysPath);
                XmlTextReader r = new XmlTextReader(sr);
                while (r.Read())
                {
                    if (r.NodeType == XmlNodeType.Element)
                        if (r.Name.ToLower().Equals("path"))
                            path = r.GetAttribute("content");
                }
            }
            TB_Path.Text = path;

            // fill segmentlining combobox
            _inputView_objSelectionChangedListener(null, null);

            //fill initial value
            
        }

        void LoadStructureModelWindow_Unloaded(object sender,
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
            IView view = InputViewCB.SelectedItem as IView;
            SegmentLining sl = SLLB.SelectedItem as SegmentLining;
            SLType slType = TunnelTools.getSLType(sl.SLTypeID);
            if (slType == null)
                return;
            double r = (slType.OuterDiameter + slType.InnerDiameter) / 4;
            double verticalLoad = 0;
            double horizontalLoad1 = 0;
            double horizontalLoad2 = 0;

            Domain analysisDomain = HelperFunction.GetAnalysisDomain();
            DGObjectsCollection allLCSLoads = analysisDomain.getObjects("CSLoad");
            DGObjects loadDGObjects = HelperFunction.GetDGObjsByName(allLCSLoads, "AllLCSLoads");
            string name = sl.ToString() + view.eMap.MapID;
            if (loadDGObjects.containsKey(name))
            {
                SoilInitalStress sis = loadDGObjects[name] as SoilInitalStress;
                verticalLoad = sis.result.Pe1 + sis.result.Pw1;
                horizontalLoad1 = sis.result.Qe1 + sis.result.Qw1;
                horizontalLoad2 = sis.result.Qe2 + sis.result.Qw2;
            }

            tb_Radius.Text = r.ToString() + " m";
            tb_Thickness.Text = slType.Thickness.ToString() + " m";
            tb_Width.Text = slType.Width.ToString() + " m";
            tb_Density.Text = "2500 kg/m^3";
            tb_E.Text = "35500000 kPa";
            tb_u.Text = "0.2";
            tb_kground.Text = "2500 kN/m^3";
            tb_kjoint.Text = "30000 kN*m/rad";
            tb_VerticalLoad.Text = verticalLoad.ToString() + " KPa";
            tb_HorizontalLoad1.Text = horizontalLoad1.ToString() + " KPa";
            tb_HorizontalLoad2.Text = horizontalLoad2.ToString() + " KPa";
        }
        #endregion

        private void Path_Click(object sender, RoutedEventArgs e)
        {
            string ansysPath = PathHelper.GetPath(".exe");
            TB_Path.Text = ansysPath;

            if (ansysPath != "")
            {
                FileStream fs;
                if (File.Exists(ansysPath))
                    fs = new FileStream(ansysPath, FileMode.Open);
                else
                    fs = new FileStream(ansysPath, FileMode.Create);
                XmlTextWriter w = new XmlTextWriter(fs, Encoding.UTF8);
                w.WriteStartDocument();
                w.WriteStartElement("path");
                w.WriteAttributeString("content", ansysPath);
                w.WriteEndElement();
                w.Flush();
                fs.Close();
            }
        }

        private void Start_Click(object sender, RoutedEventArgs e)
        {

        }
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
