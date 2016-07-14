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
using IS3.Geology;
using IS3.ShieldTunnel;

using Esri.ArcGISRuntime.Geometry;

namespace IS3.SimpleStructureTools.DrawTools
{
    public class DrawAxesSettings
    {
        public bool drawMilage { get; set; }
        public bool isReverse { get; set; }
        public int interval { get; set; }

        public double scale { get; set; }   // New! map scale.
        public string mapID { get; set; }

        public DrawAxesSettings()
        {
            drawMilage = true;
            isReverse = false;
            interval = 100;

            scale = 1.0;
        }
    }
    /// <summary>
    /// Interaction logic for DrawTunnelAxesWindow.xaml
    /// </summary>
    public partial class DrawTunnelAxesWindow : Window
    {
        Project _prj;                       // the project
        Domain _structureDomain;              // the structure domain of the project
        IMainFrame _mainFrame;              // the main frame
        IView _inputView;                   // the input view 
        IEnumerable<DGObject> _axes;         // selected boreholes
        string _axisLayerID = "DES_AXL";      
        DrawAxesSettings _settings;          // the analysis settings
        bool _initFailed;                   // set to true if initialization failed

        public DrawTunnelAxesWindow()
        {
            InitializeComponent();

            _settings = new DrawAxesSettings();
            SettingsHolder.DataContext = _settings;
            Loaded += DrawTunnelAxesWindow_Loaded;
            Unloaded += DrawTunnelAxesWindow_Unloaded;

            _mainFrame = Globals.mainframe;
            _prj = Globals.project;

            if (_mainFrame == null || _prj == null) { _initFailed = true; return; }

            _structureDomain = _prj.getDomain(DomainType.Structure);
            if (_structureDomain == null) { _initFailed = true; return; }

            // set the input view
            _inputView = _mainFrame.activeView;
            if (_inputView == null || 
                _inputView.eMap.MapType != EngineeringMapType.FootPrintMap)
                _inputView = _mainFrame.views.FirstOrDefault(
                    x => x.eMap.MapType == EngineeringMapType.FootPrintMap);
            if (_inputView == null) { _initFailed = true; return; }
            InputViewTB.DataContext = _inputView;

            DGObjects objs = _structureDomain.getObjects("TunnelAxis");
            if (objs != null)
                _axisLayerID = objs.definition.GISLayerName;

            // add borehole layer as selectable layer
            _inputView.removeSelectableLayer("_ALL");
            _inputView.addSeletableLayer("0");     // "0" is the drawing layer ID
            _inputView.addSeletableLayer(_axisLayerID);

            // add a listener to object selection changed event
            _inputView.objSelectionChangedTrigger += 
                _inputView_objSelectionChangedListener;
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

            // fill axes combobox
            _axes = _prj.getSelectedObjs(_structureDomain, "TunnelAxis");
            if (_axes != null && _axes.Count() > 0)
            {
                AxisCB.ItemsSource = _axes;
                AxisCB.SelectedIndex = 0;
            }
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

        void _inputView_objSelectionChangedListener(object sender,
            ObjSelectionChangedEventArgs e)
        {
            // fill axes combobox
            _axes = _prj.getSelectedObjs(_structureDomain, "TunnelAxis");
            if (_axes != null && _axes.Count() > 0)
            {
                AxisCB.ItemsSource = _axes;
                AxisCB.SelectedIndex = 0;
            }
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
            string mapID = _inputView.eMap.MapID;
            _settings.mapID = mapID;

            // check all needed data is set up correctly
            if (_axes == null || _axes.Count() == 0)
                return;

            IGraphicsLayer gLayer = _inputView.getLayer(_axisLayerID);
            if (gLayer == null)
                return;

            // get axes points (x,y) coordinates,
            // and generate a list of Tuple<TunnelAxis, IPolyline>.
            List<Tuple<TunnelAxis, IPolyline>> input =
                new List<Tuple<TunnelAxis, IPolyline>>();
            foreach (var obj in _axes)
            {
                TunnelAxis ta = obj as TunnelAxis;
                if (ta == null)
                    continue;

                DGObjects tunnels = _structureDomain.getObjects("Tunnel");
                Tunnel tunnel = tunnels.id2Obj[ta.LineNo] as Tunnel;

                //analysis the axis, change the engineering coordinate to map coordinate
                //
                TunnelAxis engineeringAxis = ta;
                int count = engineeringAxis.AxisPoints.Count;

                IGraphicCollection gc = gLayer.getGraphics(ta);
                if (gc.Count == 0)
                    continue;
                IGraphic g = gc[0];
                IPolyline p = g.Geometry as IPolyline;
                if (p == null)
                    continue;
                Polyline pLine = p as Polyline;
                var a = pLine.Parts[0];
                var b = a.AsEnumerable();
                var test = new PolylineBuilder(pLine);
                var c = test.Parts[0];
                var d = c.AsEnumerable;
                
                /*
                if (pc.Count != count)
                {
                    return;
                }

                TunnelAxis mapAxis = new TunnelAxis();
                mapAxis.LineNo = engineeringAxis.LineNo;
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
                 */




                input.Add(new Tuple<TunnelAxis, IPolyline>(ta, p));

            }           
        }

        void AfterAnalysis()
        {

        }

    }
}

