# -*- coding:gb2312 -*-
import is3
import System

from System.Windows.Media import Colors

emap = is3.EngineeringMap('test', 0, 0, 100, 100, 0.01)
safe_view = is3.MainframeWrapper.addView(emap)
safe_view.addLocalTiledLayer('C:\\IS3\\Data\\TPKs\\Empty.tpk', 'baselayer')


sym_point = is3.graphicsEngine.newSimpleMarkerSymbol(
    Colors.Red, 12.0, is3.SimpleMarkerStyle.X)
renderer1 = is3.graphicsEngine.newSimpleRenderer(sym_point)
p1 = is3.graphicsEngine.newPoint(50, 50)
layer1WP = is3.newGraphicsLayer('layer1', 'layer1')
layer1WP.setRenderer(renderer1)
layer1WP.layer.Graphics.Add(p1)
safe_view.addLayer(layer1WP.layer)



sym_line = is3.graphicsEngine.newSimpleLineSymbol(
    Colors.Blue, is3.SimpleLineStyle.Solid, 1.0)
renderer2 = is3.graphicsEngine.newSimpleRenderer(sym_line)
line1 = is3.graphicsEngine.newLine(0, 0, 100, 100)
line2 = is3.graphicsEngine.newLine(20, 20, 80, 20)
layer2WP = is3.newGraphicsLayer('layer2', 'layer2')
layer2WP.setRenderer(renderer2)
layer2WP.layer.Graphics.Add(line1)
layer2WP.layer.Graphics.Add(line2)
safe_view.addLayer(layer2WP.layer)

