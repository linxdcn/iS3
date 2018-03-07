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
using IS3.SimpleStructureTools.Helper.Analysis;

namespace IS3.SimpleStructureTools.StructureAnalysis
{       
    /// <summary>
    /// Interaction logic for TSIWindow.xaml
    /// </summary>
    public partial class TSIWindow : Window
    {
        Project _prj;
        Domain _structureDomain;
        IMainFrame _mainFrame;
        IView _inputView;

        //DGObject members
        DGObjectsCollection _allSLs;
        DGObjectsCollection _allTunnels;
        List<string> _slLayerIDs;
        List<string> _tunnelLayerIDs;
        Dictionary<string, IEnumerable<DGObject>> _selectedSLsDict;
        Dictionary<string, IEnumerable<DGObject>> _selectedTunnelsDict;

        //graphics members
        ISpatialReference _spatialRef;

        //result
        Dictionary<int, int> _slsGrade;
        Dictionary<int, IGraphicCollection> _slsGraphics;
        
        public TSIWindow()
        {
            InitializeComponent();

            _selectedSLsDict = new Dictionary<string, IEnumerable<DGObject>>();
            _selectedTunnelsDict = new Dictionary<string, IEnumerable<DGObject>>();
            _slsGrade = new Dictionary<int, int>();
            _slsGraphics = new Dictionary<int, IGraphicCollection>();

            _mainFrame = Globals.mainframe;
            _prj = Globals.project;
            _structureDomain = _prj.getDomain(DomainType.Structure);
            _allSLs = _structureDomain.getObjects("SegmentLining");
            _slLayerIDs = new List<string>();
            foreach (DGObjects objs in _allSLs)
                _slLayerIDs.Add(objs.definition.GISLayerName);

            _allTunnels = _structureDomain.getObjects("Tunnel");
            _tunnelLayerIDs = new List<string>();
            foreach (DGObjects objs in _allTunnels)
                _tunnelLayerIDs.Add(objs.definition.GISLayerName);

            Loaded += TSIWindow_Loaded;
            Unloaded += TSIWindow_Unloaded;
        }

        void TSIWindow_Loaded(object sender,
            RoutedEventArgs e)
        {
            Application curApp = Application.Current;
            Window mainWindow = curApp.MainWindow;
            this.Owner = mainWindow;
            this.Left = mainWindow.Left +
                (mainWindow.Width - this.ActualWidth - 10);
            this.Top = mainWindow.Top +
                (mainWindow.Height - this.ActualHeight - 10);

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
                return;
            }

            _inputView_objSelectionChangedListener(null, null);
        }

        void TSIWindow_Unloaded(object sender,
            RoutedEventArgs e)
        {
            _inputView.addSeletableLayer("_ALL");
            _inputView.objSelectionChangedTrigger -=
                _inputView_objSelectionChangedListener;
        }

        void _inputView_objSelectionChangedListener(object sender,
            ObjSelectionChangedEventArgs e)
        {
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
                LineLB.ItemsSource = _tunnels;
        }

        private void InputCB_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            _inputView.addSeletableLayer("_ALL");
            _inputView.objSelectionChangedTrigger -=
                    _inputView_objSelectionChangedListener;

            _inputView = InputCB.SelectedItem as IView;
            _inputView.removeSelectableLayer("_ALL");
            foreach (string layerID in _tunnelLayerIDs)
                _inputView.addSeletableLayer(layerID);

            _inputView.objSelectionChangedTrigger +=
                _inputView_objSelectionChangedListener;
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

        void StartAnalysis()
        {
            IView view = InputCB.SelectedItem as IView;
            _spatialRef = view.spatialReference;

            foreach (string TunnelLayerID in _selectedTunnelsDict.Keys)
            {
                IEnumerable<DGObject> tunnels = _selectedTunnelsDict[TunnelLayerID];
                List<DGObject> tunnelList = tunnels.ToList();
                IGraphicsLayer gLayer = _inputView.getLayer("DES_RIN");
                
                foreach (DGObject dg in tunnelList)
                {
                    Tunnel tunnel = dg as Tunnel;
                    if (tunnel.LineNo == null)
                        return;

                    List<SegmentLining> sls = TunnelTools.getSLsByLineNo((int)tunnel.LineNo);
                    List<RingTSI> results = TSIAnalysis.getTSIResult(sls);

                    for (int i = 0; i < results.Count; i++)
                    {
                        RingTSI result = results[i];

                        result.sl.result = "FastTSI: " + result.tsi.ToString("0.00");

                        //symbol
                        Color color = Helper.ColorTools.GradeColor.GetTSIColor(result.tsi);
                        ISimpleLineSymbol linesymbol = Runtime.graphicEngine.newSimpleLineSymbol(
                                    color, SimpleLineStyle.Solid, 0.5);
                        ISymbol symbol = Runtime.graphicEngine.newSimpleFillSymbol(color, SimpleFillStyle.Solid, linesymbol);

                        IGraphicCollection gcollection = gLayer.getGraphics(result.sl);
                        IGraphic g = gcollection[0];
                        g.Symbol = symbol;
                        IGraphicCollection gc = Runtime.graphicEngine.newGraphicCollection();
                        gc.Add(g);

                        // add text
                        if (i % 100 == 0)
                        {
                            string strK = "TSI:" + result.tsi.ToString("#0.0");
                            
                            IPolygon polygon = g.Geometry as IPolygon;
                            IPointCollection pointCollection = polygon.GetPoints();
                            IMapPoint p1_temp = pointCollection[0];
                            IMapPoint p3_temp = pointCollection[2];
                            double x = (p1_temp.X + p3_temp.X) / 2.0;
                            double y = (p1_temp.Y + p3_temp.Y) / 2.0;
                            IMapPoint p = Runtime.geometryEngine.newMapPoint(x, y, _spatialRef);
                            g = Runtime.graphicEngine.newText(strK, p, Colors.Red, "Arial", 10.0);
                            gc.Add(g);
                        }
                        
                        _slsGraphics[result.sl.id] = gc;
                    }          
                }
            }
        }

        void SyncToView()
        {
            IView view = InputCB.SelectedItem as IView;

            //将图形添加到view中
            string layerID = "TSILayer"; //图层ID
            IGraphicsLayer gLayer = view.getLayer(layerID);
            if (gLayer == null)
            {
                gLayer = Runtime.graphicEngine.newGraphicsLayer(
                    layerID, layerID);
                var sym_fill = GraphicsUtil.GetDefaultFillSymbol();
                var renderer = Runtime.graphicEngine.newSimpleRenderer(sym_fill);
                gLayer.setRenderer(renderer);
                gLayer.Opacity = 1.0;
                view.addLayer(gLayer);
            }
            foreach (int id in _slsGraphics.Keys)
            {
                IGraphicCollection gc = _slsGraphics[id];
                gLayer.addGraphics(gc);
            }

            //// sync objects with graphics
            //List<DGObject> SLs = _allSLs.merge();
            //gLayer.syncObjects(SLs);


            //计算新建图形范围，并在地图中显示该范围
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
