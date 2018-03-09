using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using IS3.Core;
using IS3.SimpleStructureTools.DrawTools;
using IS3.SimpleStructureTools.SpatialTools;
using IS3.SimpleStructureTools.LoadTools;
using IS3.SimpleStructureTools.StructureAnalysis;
using IS3.SimpleStructureTools.Http;

namespace IS3.SimpleStructureTools
{
    public class StructureTools : Tools
    {
        public override string name() { return "iS3.SimpleStructureTools"; }
        public override string provider() { return "Tongji iS3 team"; }
        public override string version() { return "1.0"; }

        List<ToolTreeItem> items;
        public override IEnumerable<ToolTreeItem> treeItems()
        {
            return items;
        }

        #region Windows menber and initialized
        DrawTunnelAxesWindow drawTunnelAxesWindow;
        DrawTunnelsWindow drawTunnelsWindow;
        DrawSLWindow drawSLsWindow;
        TunnelDepthAnalysisWindow tunnelDepthAnalysisWindow;
        TunnelCSLoadWindow tunnelCSLoadWindow;
        LSDynaDemo lsDynaDemo;
        TSIWindow tsiWindow;
        FAHPWindow fahpWindow;
        LoadStructureModelWindow loadStructureWindow;
        TestWindow testWindow;

        HttpRequestWindow httpRequestWindow;
        
        public void drawTunnelAxes() { Init(drawTunnelAxesWindow); }
        public void drawTunnels() { Init(drawTunnelsWindow); }
        public void drawSLs() { Init(drawSLsWindow); }
        public void tunnelDepthAnalysis() { Init(tunnelDepthAnalysisWindow); }
        public void tunnelCSLoad() { Init(tunnelCSLoadWindow); }
        public void lsDyna() { Init(lsDynaDemo); }
        public void test() { Init(testWindow); }
        public void tsi() { Init(tsiWindow); }
        public void fahp() { Init(fahpWindow); }
        public void loadStructure() { Init(loadStructureWindow); }
        public void httpRequest() { Init(httpRequestWindow); }
        #endregion

        public void Init<T>(T window) where T : System.Windows.Window, new()
        {
            if (window != null)
            {
                window.Show();
                return;
            }

            window = new T();
            window.Closed += (o, args) =>
            {
                window = null;
            };
            window.Show();
        }

        public StructureTools()
        {
            items = new List<ToolTreeItem>();
            ToolTreeItem item = new ToolTreeItem("Structure|Draw Tools", "DrawAxes", drawTunnelAxes);
            items.Add(item);
            item = new ToolTreeItem("Structure|Draw Tools", "DrawTunnels", drawTunnels);
            items.Add(item);
            item = new ToolTreeItem("Structure|Draw Tools", "DrawSLs", drawSLs);
            items.Add(item);
            item = new ToolTreeItem("Structure|Spatial Analysis", "TunnelDepth", tunnelDepthAnalysis);
            items.Add(item);
            item = new ToolTreeItem("Structure|Load", "TunnelCSLoad", tunnelCSLoad);
            items.Add(item);
            item = new ToolTreeItem("Structure|Load", "Load Structure Model", loadStructure);
            items.Add(item);
            //item = new ToolTreeItem("FEM|Load", "Test", test);
            //items.Add(item);
            item = new ToolTreeItem("Maintenance|TSI", "TSI", tsi);
            items.Add(item);
            item = new ToolTreeItem("Maintenance|TSI", "FAHP", fahp);
            items.Add(item);
            item = new ToolTreeItem("Web|Http", "Json", httpRequest);
            items.Add(item);
        }
    }
}
