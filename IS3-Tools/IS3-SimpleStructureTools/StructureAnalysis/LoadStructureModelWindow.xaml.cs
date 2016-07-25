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
using IS3.SimpleStructureTools.Helper.Format;
using IS3.SimpleStructureTools.Helper.ColorTools;

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
        bool _initFailed;                   // set to true if initialization failed
        String _ansysPath;                   // path of ansys

        //DGObject members
        DGObjectsCollection _allSLs;        // all the tunnels
        List<string> _slLayerIDs;           // segmentlining layer IDs
        Dictionary<string, IEnumerable<DGObject>> _selectedSLsDict;  // selected segmentlining

        //graphics members
        ISpatialReference _spatialRef;
        ISymbol _whitefillSymbol;
        ISymbol _lineSymbol;
        ISymbol _arrowFillSymbol;

        //result
        LoadStructure _loadStructure;
        public Dictionary<int, IGraphicCollection> SLMomentGraphics { get; set; }
        public Dictionary<int, IGraphicCollection> SLAxialGraphics { get; set; }
        public Dictionary<int, IGraphicCollection> SLShearGraphics { get; set; }
        public Dictionary<int, IGraphicCollection> SLDisplacementGraphics { get; set; }

        public LoadStructureModelWindow()
        {
            InitializeComponent();

            ISimpleLineSymbol outline = Runtime.graphicEngine.newSimpleLineSymbol(
                                Colors.Black, Core.Graphics.SimpleLineStyle.Solid, 1.0);
            _whitefillSymbol = Runtime.graphicEngine.newSimpleFillSymbol(
                                Colors.White, SimpleFillStyle.Solid, outline);
            _lineSymbol = Runtime.graphicEngine.newSimpleLineSymbol(
                                Color.FromArgb(255, 0, 0, 0), Core.Graphics.SimpleLineStyle.Solid, 1.0);
            _arrowFillSymbol = Runtime.graphicEngine.newSimpleFillSymbol(
                                Colors.Blue, SimpleFillStyle.Solid, outline);

            SLMomentGraphics = new Dictionary<int, IGraphicCollection>();
            SLAxialGraphics = new Dictionary<int, IGraphicCollection>();
            SLShearGraphics = new Dictionary<int, IGraphicCollection>();
            SLDisplacementGraphics = new Dictionary<int, IGraphicCollection>();

            _loadStructure = new LoadStructure();

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

            _ansysPath = Runtime.rootPath + "//Conf//ansysPath.xml";
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
            if (File.Exists(_ansysPath))
            {
                StreamReader sr = new System.IO.StreamReader(_ansysPath);
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

        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            //updata the line list box
            if (RB1.IsChecked.Value)
                BrowserBtn.IsEnabled = false;
            else
                BrowserBtn.IsEnabled = true;
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
            StartAnalysis();
            SyncToView();
            Close();
        }
        private void Cancel_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void StartAnalysis()
        {
            _loadStructure.radius = FormatData.ToNumber(tb_Radius.Text);
            _loadStructure.thickness = FormatData.ToNumber(tb_Thickness.Text);
            _loadStructure.width = FormatData.ToNumber(tb_Width.Text);
            _loadStructure.density = FormatData.ToNumber(tb_Density.Text);
            _loadStructure.moe = FormatData.ToNumber(tb_E.Text);
            _loadStructure.pr = FormatData.ToNumber(tb_u.Text);
            _loadStructure.k_ground = FormatData.ToNumber(tb_kground.Text);
            _loadStructure.k_joint = FormatData.ToNumber(tb_kjoint.Text);
            _loadStructure.pv = FormatData.ToNumber(tb_VerticalLoad.Text);
            _loadStructure.ph1 = FormatData.ToNumber(tb_HorizontalLoad1.Text);
            _loadStructure.ph2 = FormatData.ToNumber(tb_HorizontalLoad2.Text);
            _loadStructure.strResult = LoadStructure.createCode(_loadStructure);

            string path = "D:/SLConvergenceAnalysis";
            System.IO.Directory.CreateDirectory(path);

            DirectoryInfo dir = new DirectoryInfo(path);
            dir.Create();

            DirectoryInfo dir2 = new DirectoryInfo(path);
            dir2.CreateSubdirectory("Result");

            if (!File.Exists("D:/SLConvergenceAnalysis/input.txt"))
            {
                FileStream fs1 = new FileStream("D:/SLConvergenceAnalysis/input.txt", FileMode.Create, FileAccess.Write);
                StreamWriter sw = new StreamWriter(fs1);
                sw.WriteLine(_loadStructure.strResult);
                sw.Close();
                fs1.Close();
            }
            else
            {
                FileStream fs = new FileStream("D:/SLConvergenceAnalysis/input.txt", FileMode.Open, FileAccess.Write);
                StreamWriter sr = new StreamWriter(fs);
                sr.WriteLine(_loadStructure.strResult);//开始写入值
                sr.Close();
                fs.Close();
            }

            if (RB1.IsChecked.Value)
            {

            }
            else
            {
                System.Diagnostics.Process proc = new System.Diagnostics.Process();
                string inputfile, outputfile;
                inputfile = "D:/SLConvergenceAnalysis/input.txt";
                outputfile = "D:/SLConvergenceAnalysis/output.txt";
                proc.StartInfo.FileName = TB_Path.Text;
                proc.StartInfo.Arguments = "-b -p ansysds -i " + inputfile + " -o " + outputfile;
                proc.StartInfo.WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal;
                proc.StartInfo.WorkingDirectory = "D:/SLConvergenceAnalysis/Result";
                proc.Start();
                proc.WaitForExit();
            }
            //start analysis
            

            //read data
            List<LoadStructure.NodeResult> listNode = new List<LoadStructure.NodeResult>();

            StreamReader reader = new System.IO.StreamReader("D:/SLConvergenceAnalysis/Result/result.txt");
            string ss;
            int i = 1;
            while ((ss = reader.ReadLine()) != null)
            {
                LoadStructure.NodeResult node = new LoadStructure.NodeResult();
                ss = FormatData.SpaceToComma(ss);
                string[] arr = ss.Split(',');
                node.n = i;
                node.x = double.Parse(arr[0]);
                node.y = double.Parse(arr[1]);
                node.moment = double.Parse(arr[2]);
                node.axial = double.Parse(arr[3]);
                node.shear = double.Parse(arr[4]);
                listNode.Add(node);
                i++;
            }

            _loadStructure.Result = listNode;
            Draw(_loadStructure);
        }

        private void Draw(LoadStructure loadStructure)
        {
            SegmentLining sl = SLLB.SelectedItem as SegmentLining;
            IView view = InputViewCB.SelectedItem as IView;
            _spatialRef = view.spatialReference;

            int num = _loadStructure.Result.Count();

            //draw moment, axial force, shear force result
            double moment_max = _loadStructure.Result[0].moment;
            double moment_min = _loadStructure.Result[0].moment;
            double axial_max = _loadStructure.Result[0].axial;
            double axial_min = _loadStructure.Result[0].axial;
            double shear_max = _loadStructure.Result[0].shear;
            double shear_min = _loadStructure.Result[0].shear;
            foreach (LoadStructure.NodeResult nr in _loadStructure.Result)
            {
                if (nr.moment > moment_max)
                    moment_max = nr.moment;
                if (nr.moment < moment_min)
                    moment_min = nr.moment;

                if (nr.axial > axial_max)
                    axial_max = nr.axial;
                if (nr.axial < axial_min)
                    axial_min = nr.axial;

                if (nr.shear > shear_max)
                    shear_max = nr.shear;
                if (nr.shear < shear_min)
                    shear_min = nr.shear;
            }

            //get radius
            double zScale = view.eMap.ScaleZ;
            SLType slType = TunnelTools.getSLType(sl.SLTypeID);
            if (slType == null)
                return;
            double r = slType.OuterDiameter / 2;

            //get x,y position
            double x = 0;
            double z = 0;

            Domain domain = HelperFunction.GetAnalysisDomain();
            DGObjectsCollection segGraphics = domain.getObjects("CSGraphic");
            if (segGraphics == null || segGraphics.merge().Count == 0)
                segGraphics = domain.getObjects("LSGraphic");
            if (segGraphics != null && segGraphics.merge().Count != 0)
            {
                SegmentGraphicBase segGraphic = segGraphics[sl.id.ToString() + view.eMap.MapID] as SegmentGraphicBase;
                x = segGraphic.CenterX;
                z = segGraphic.CenterZ;
            }
            else
            {
                foreach (string SLLayerID in _selectedSLsDict.Keys)
                {
                    IEnumerable<DGObject> sls = _selectedSLsDict[SLLayerID];
                    List<DGObject> slList = sls.ToList();
                    IGraphicsLayer gLayer = _inputView.getLayer(SLLayerID);

                    IGraphicCollection gcollection = gLayer.getGraphics(sl);
                    if (gcollection != null && gcollection.Count != 0)
                    {
                        IGraphic g = gcollection[0];
                        IPolygon polygon = g.Geometry as IPolygon;
                        IPointCollection pointCollection = polygon.GetPoints();
                        IMapPoint p1_temp = pointCollection[0];
                        IMapPoint p2_temp = pointCollection[1];
                        IMapPoint p3_temp = pointCollection[2];
                        IMapPoint p4_temp = pointCollection[3];
                        x = (p1_temp.X + p2_temp.X + p3_temp.X + p4_temp.X) / 4;
                        z = (p1_temp.Y + p2_temp.Y + p3_temp.Y + p4_temp.Y) / 4;
                        break;
                    }
                }
            }

            //start to draw
            IGraphicCollection gm = Runtime.graphicEngine.newGraphicCollection();
            IGraphicCollection ga = Runtime.graphicEngine.newGraphicCollection();
            IGraphicCollection gs = Runtime.graphicEngine.newGraphicCollection();
            IGraphicCollection gd = Runtime.graphicEngine.newGraphicCollection();

            DrawValueTable(x, z, r, moment_max, moment_min, gm);
            DrawValueTable(x, z, r, axial_max, axial_min, ga);
            DrawValueTable(x, z, r, shear_max, shear_min, gs);

            foreach (LoadStructure.NodeResult nr in _loadStructure.Result)
            {
                DrawForce(x, z, r, num, nr.n, nr.moment, moment_max, moment_min, gm);
                DrawForce(x, z, r, num, nr.n, nr.axial, axial_max, axial_min, ga);
                DrawForce(x, z, r, num, nr.n, nr.shear, shear_max, shear_min, gs);
            }

            //draw displacement
            DrawDisplacement(x, z, r, _loadStructure, gd);

            SLMomentGraphics[sl.id] = gm;
            SLAxialGraphics[sl.id] = ga;
            SLShearGraphics[sl.id] = gs;
            SLDisplacementGraphics[sl.id] = gd;
        }

        private void DrawForce(double x, double z, double r, int num, int n, double value, double max, double min, IGraphicCollection resultGraphic)
        {
            double m;
            double pi = 3.14159;
            if (Math.Abs(max) > Math.Abs(min))
                m = Math.Abs(max);
            else
                m = Math.Abs(min);
            IGraphic g;

            double x1 = x - Math.Sin(2 * pi * (n - 1) / num) * r;
            double y1 = z + Math.Cos(2 * pi * (n - 1) / num) * r;
            double x2 = x - Math.Sin(2 * pi * n / num) * r;
            double y2 = z + Math.Cos(2 * pi * n / num) * r;
            double x3 = x - Math.Sin(2 * pi * n / num) * (r + 0.25 * r * value / m);
            double y3 = z + Math.Cos(2 * pi * n / num) * (r + 0.25 * r * value / m);
            double x4 = x - Math.Sin(2 * pi * (n - 1) / num) * (r + 0.25 * r * value / m);
            double y4 = z + Math.Cos(2 * pi * (n - 1) / num) * (r + 0.25 * r * value / m);

            IMapPoint p1 = Runtime.geometryEngine.newMapPoint(x1, y1, _spatialRef);
            IMapPoint p2 = Runtime.geometryEngine.newMapPoint(x2, y2, _spatialRef);
            IMapPoint p3 = Runtime.geometryEngine.newMapPoint(x3, y3, _spatialRef);
            IMapPoint p4 = Runtime.geometryEngine.newMapPoint(x4, y4, _spatialRef);

            g = ShapeMappingUtility.NewLine(p1, p2);
            g.Symbol = _lineSymbol;
            resultGraphic.Add(g);

            ISimpleLineSymbol outline = Runtime.graphicEngine.newSimpleLineSymbol(
                                Color.FromArgb(255, 255, 255, 255), Core.Graphics.SimpleLineStyle.Solid, 1.0);
            ISimpleFillSymbol fillSymbol = Runtime.graphicEngine.newSimpleFillSymbol(
                                GradeColor.GetFEMColor(max, min, value), SimpleFillStyle.Solid, outline);
            g = ShapeMappingUtility.NewQuadrilateral(p1, p2, p3, p4);
            g.Symbol = fillSymbol;
            resultGraphic.Add(g);
        }

        private void DrawDisplacement(double x, double z, double r, LoadStructure loadStructure, IGraphicCollection resultGraphic)
        {
            double max_x = 0;
            double max_z = 0;
            double pi = 3.13159;
            double m = 0;
            double num = loadStructure.Result.Count();

            foreach (LoadStructure.NodeResult nr in loadStructure.Result)
            {
                if (Math.Abs(nr.x) > max_x)
                    max_x = Math.Abs(nr.x);
                if (Math.Abs(nr.y) > max_z)
                    max_z = Math.Abs(nr.y);
            }

            if (max_x > max_z)
                m = max_x;
            else
                m = max_z;
            IGraphic g;
            ISymbol lineSymbol = Runtime.graphicEngine.newSimpleLineSymbol(Colors.Transparent, SimpleLineStyle.Solid, 1.0);
            g = ShapeMappingUtility.NewCircle(x, z, r, _spatialRef);
            g.Symbol = lineSymbol;
            resultGraphic.Add(g);

            IPointCollection pc = Runtime.geometryEngine.newPointCollection();
            foreach (LoadStructure.NodeResult nr in loadStructure.Result)
            {
                double x1 = x - Math.Sin(2 * pi * (nr.n - 1) / num) * r + nr.x * 0.1 * r / m;
                double y1 = z + Math.Cos(2 * pi * (nr.n - 1) / num) * r + nr.y * 0.1 * r / m;

                IMapPoint p1 = Runtime.geometryEngine.newMapPoint(x1, y1, _spatialRef);
                pc.Add(p1);
            }
            double xf = x - Math.Sin(2 * pi * (loadStructure.Result[0].n - 1) / num) * r + loadStructure.Result[0].x * 0.1 * r / m;
            double yf = z + Math.Cos(2 * pi * (loadStructure.Result[0].n - 1) / num) * r + loadStructure.Result[0].y * 0.1 * r / m;
            IMapPoint first = Runtime.geometryEngine.newMapPoint(xf, yf, _spatialRef);
            pc.Add(first);

            ISymbol lineSymbol2 = Runtime.graphicEngine.newSimpleLineSymbol(Colors.Blue, SimpleLineStyle.Solid, 1.0);
            g = ShapeMappingUtility.NewPolyline(pc);
            g.Symbol = lineSymbol2;
            resultGraphic.Add(g);

            IGraphic text;
            for (int i = 1; i < 5; i++)
            {
                int n = i * 90;
                LoadStructure.NodeResult nr = loadStructure.Result[n - 1];
                double x1 = x - Math.Sin(2 * pi * (nr.n - 1) / num) * r;
                double y1 = z + Math.Cos(2 * pi * (nr.n - 1) / num) * r;

                IMapPoint p = Runtime.geometryEngine.newMapPoint(x1, y1, _spatialRef);
                text = Runtime.graphicEngine.newText(string.Format("x:{0:0.00000000},y:{1:0.00000000}", nr.x, nr.y), p, Colors.Red, "Arial", 12);
                resultGraphic.Add(text);
            }

        }

        private void DrawValueTable(double x, double z, double r, double max, double min, IGraphicCollection resultGraphic)
        {
            double n = (max - min) / 9.0;
            double dis = 3 * r / 9.0;
            double x_start = x - 1.5 * r;
            double y_start = z - 1.3 * r;

            IGraphic c = Runtime.graphicEngine.newGraphic();
            c = ShapeMappingUtility.NewCircle(x, z, r, _spatialRef);
            c.Symbol = _whitefillSymbol;
            resultGraphic.Add(c);

            for (int i = 1; i < 10; i++)
            {
                double value = min + n * i;

                double left = x_start + dis * (i - 1);
                double bottom = y_start;
                double right = x_start + dis * i;
                double top = y_start + 0.07 * r;
                IGraphic g;
                ISimpleLineSymbol outline = Runtime.graphicEngine.newSimpleLineSymbol(
                                Colors.White, Core.Graphics.SimpleLineStyle.Solid, 1.0);
                ISimpleFillSymbol fillSymbol = Runtime.graphicEngine.newSimpleFillSymbol(
                                    GradeColor.GetFEMColor(max, min, value), SimpleFillStyle.Solid, outline);
                g = ShapeMappingUtility.NewRectangle(left, top, right, bottom, _spatialRef);
                g.Symbol = fillSymbol;
                resultGraphic.Add(g);
            }

            double middle = (max + min) / 2.0;
            IGraphic text;
            text = Runtime.graphicEngine.newText(string.Format("{0:0.00}", min), 
                Runtime.geometryEngine.newMapPoint(x - 1.5 * r, z - 1.24 * r, _spatialRef), Colors.White, "Arial", 12);
            resultGraphic.Add(text);
            text = Runtime.graphicEngine.newText(string.Format("{0:0.00}", middle),
                Runtime.geometryEngine.newMapPoint(x - 0.1 * r, z - 1.24 * r, _spatialRef), Colors.White, "Arial", 12);
            resultGraphic.Add(text);
            text = Runtime.graphicEngine.newText(string.Format("{0:0.00}", max),
                Runtime.geometryEngine.newMapPoint(x + 1.2 * r, z - 1.24 * r, _spatialRef), Colors.White, "Arial", 12);
            resultGraphic.Add(text);
        }

        private void SyncToView()
        {
            IView view = InputViewCB.SelectedItem as IView;

            foreach (IGraphicsLayer layer in view.layers)
                layer.IsVisible = false;

            //add graphics to view
            string layerID = "MomentLayer";
            IGraphicsLayer gLayer = getLayer(view, layerID);
            foreach (int id in SLMomentGraphics.Keys)
            {
                IGraphicCollection gc = SLMomentGraphics[id];
                gLayer.addGraphics(gc);
            }

            layerID = "ShearLayer";
            gLayer = getLayer(view, layerID);
            foreach (int id in SLShearGraphics.Keys)
            {
                IGraphicCollection gc = SLShearGraphics[id];
                gLayer.addGraphics(gc);
            }
            gLayer.IsVisible = false;

            layerID = "AxiaForceLayer";
            gLayer = getLayer(view, layerID);
            foreach (int id in SLAxialGraphics.Keys)
            {
                IGraphicCollection gc = SLAxialGraphics[id];
                gLayer.addGraphics(gc);
            }
            gLayer.IsVisible = false;

            layerID = "DisplacementLayer";
            gLayer = getLayer(view, layerID);
            foreach (int id in SLDisplacementGraphics.Keys)
            {
                IGraphicCollection gc = SLDisplacementGraphics[id];
                gLayer.addGraphics(gc);
            }
            gLayer.IsVisible = false;

            //calculate the envelope
            IEnvelope ext = null;
            foreach (IGraphicCollection gc in SLMomentGraphics.Values)
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

        IGraphicsLayer getLayer(IView view, string layerID)
        {
            IGraphicsLayer gLayer = view.getLayer(layerID);
            if (gLayer == null)
            {
                gLayer = Runtime.graphicEngine.newGraphicsLayer(
                    layerID, layerID);
                var sym_fill = GraphicsUtil.GetDefaultFillSymbol();
                var renderer = Runtime.graphicEngine.newSimpleRenderer(sym_fill);
                gLayer.setRenderer(renderer);
                gLayer.Opacity = 0.9;
                view.addLayer(gLayer);
            }
            return gLayer;
        }
    }
}
