# -*- coding:gb2312 -*-
import is3

from System.Collections.ObjectModel import ObservableCollection
from System.Windows.Media import Colors

def add3dview():
    is3.addView3d('Map3D', 'Test.unity3d')

def addBaseMap():
    is3.mainframe.LoadProject('SH_MetroL13.xml')
    is3.prj = is3.mainframe.prj
    is3.MainframeWrapper.loadDomainPanels()
    
    emap = is3.EngineeringMap('BaseMap',
                              13501500, 3658000, 13520000, 3665000, 0.1)
    emap.LocalTileFileName1 = 'Shanghai_LOD16_CityBlocks.tpk'
    emap.LocalTileFileName2 = 'SHMetroLine13.tpk'
    emap.LocalGeoDbFileName = 'SHMetroLine13.geodatabase'

    viewWP = is3.MainframeWrapper.addView(emap)
    return viewWP

def addBhLayer(viewWP):
    layerDef = is3.LayerDef()
    layerDef.Name = 'GEO_BHL'
    layerDef.GeometryType = is3.GeometryType.Point
    layerDef.OutlineColor = Colors.Black
    layerDef.Color = Colors.Green
    layerDef.FillStyle = is3.SimpleFillStyle.Solid
    bhLayerWP = is3.addGdbLayer(viewWP, layerDef)
    return bhLayerWP

def addRinLayer(viewWP):
    layerDef = is3.LayerDef()
    layerDef.Name = 'DES_RIN'
    layerDef.GeometryType = is3.GeometryType.Polygon
    layerDef.OutlineColor = Colors.Blue
    layerDef.Color = Colors.LightGray
    layerDef.FillStyle = is3.SimpleFillStyle.Solid
    rinLayerWP = is3.addGdbLayer(viewWP, layerDef)
    return rinLayerWP

def addTunLayer(viewWP):
    layerDef = is3.LayerDef()
    layerDef.Name = 'DES_TUN'
    layerDef.GeometryType = is3.GeometryType.Polygon
    layerDef.OutlineColor = Colors.Blue
    layerDef.Color = Colors.Blue
    layerDef.FillStyle = is3.SimpleFillStyle.Solid
    tunLayerWP = is3.addGdbLayer(viewWP, layerDef)
    return tunLayerWP

def addStatLayer(viewWP):
    layerDef = is3.LayerDef()
    layerDef.Name = 'DES_STA'
    layerDef.GeometryType = is3.GeometryType.Polyline
    layerDef.OutlineColor = Colors.Blue
    layerDef.Color = Colors.Blue
    layerDef.FillStyle = is3.SimpleFillStyle.Solid
    statLayerWP = is3.addGdbLayer(viewWP, layerDef)
    return statLayerWP

def addAxesLayer(viewWP):
	layerDef = is3.LayerDef()
	layerDef.Name = 'DES_AXL'
	layerDef.GeometryType = is3.GeometryType.Polyline
	layerDef.OutlineColor = Colors.Green
	layerDef.Color = Colors.Green
	layerDef.FillStyle = is3.SimpleFillStyle.Solid
	axesLayerWP = is3.addGdbLayer(viewWP, layerDef)
	return axesLayerWP

def addStrLayer(viewWP):
    layerDef = is3.LayerDef()
    layerDef.Name = 'GEO_STR'
    layerDef.GeometryType = is3.GeometryType.Polygon
    layerDef.OutlineColor = Colors.Gray
    layerDef.Color = Colors.LightGray
    layerDef.FillStyle = is3.SimpleFillStyle.Solid
    strLayerWP = is3.addGdbLayer(viewWP, layerDef)
    return strLayerWP

def changeRenderer():
    defaultsymbol = is3.graphicsEngine.newSimpleMarkerSymbol(Colors.Red, 12.0, is3.SimpleMarkerStyle.X)
    symbol1 = is3.graphicsEngine.newSimpleMarkerSymbol(Colors.Green, 12.0, is3.SimpleMarkerStyle.X)
    symbol2 = is3.graphicsEngine.newSimpleMarkerSymbol(Colors.Black, 12.0, is3.SimpleMarkerStyle.X)
    fields = ObservableCollection[str](['BoreholeType'])
    info1 = is3.graphicsEngine.newUniqueValueInfo(symbol1, ObservableCollection[object](['取土孔']))
    info2 = is3.graphicsEngine.newUniqueValueInfo(symbol2, ObservableCollection[object](['静力触探孔']))
    infos = ObservableCollection[is3.IUniqueValueInfo]((info1, info2))
    uniquevalue_renderer = is3.graphicsEngine.newUniqueValueRenderer(defaultsymbol, fields, infos)
    bhLayerWP.setRenderer(uniquevalue_renderer)

def addShapefile(name, type, file, start = 0, maxFeatures = 0):
    layerDef = is3.LayerDef()
    layerDef.Name = name
    layerDef.GeometryType = type
    bkgLayerWP = is3.addShpLayer(viewWP, layerDef, file, start, maxFeatures)
    return bkgLayerWP



global viewWP1, safe_view

print("--- Add base map ---")
viewWP1 = addBaseMap()
addAxesLayer(viewWP1)
addStatLayer(viewWP1)
addTunLayer(viewWP1)
addRinLayer(viewWP1)
addBhLayer(viewWP1)

print ("--- Add a empty profile map ---")
emap = is3.EngineeringMap('Profile', 0, 0, 100, 100, 0.01)
emap.MapType = is3.EngineeringMapType.GeneralProfileMap;
safe_view = is3.MainframeWrapper.addView(emap)
tilefile = is3.Runtime.tilePath + "\\Empty.tpk"
safe_view.addLocalTiledLayer(tilefile, 'baselayer')
