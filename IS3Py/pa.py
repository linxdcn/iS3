# -*- coding:gb2312 -*-
import is3

from System.Windows.Media import Colors

def addBaseMap():
    is3.mainframe.LoadProject('pa.xml')
    is3.prj = is3.mainframe.prj
    is3.MainframeWrapper.loadDomainPanels()
    
    emap = is3.EngineeringMap('BaseMap',
                              12696330, 2575360, 12696940, 2576090, 0.1)
    emap.LocalTileFileName1 = 'Shenzhen.tpk'
    emap.LocalTileFileName2 = 'pingan.tpk'
    emap.LocalGeoDbFileName = 'pingan.geodatabase'

    viewWP = is3.MainframeWrapper.addView(emap)
    return viewWP

def addLayer(viewWP):
    layerDef = is3.LayerDef()
    layerDef.Name = 'HorDis_RetainingPile'
    layerDef.GeometryType = is3.GeometryType.Point
    layerDef.Color = Colors.Blue
    layerDef.MarkerSize = 12
    layerDef.MarkerStyle = is3.SimpleMarkerStyle.Circle
    layerDef.EnableLabel = True
    layerDef.LabelTextExpression = '[Name]'
    layerWrapper = is3.addGdbLayer(viewWP, layerDef)

    layerDef = is3.LayerDef()
    layerDef.Name = 'Settl_Ground'
    layerDef.GeometryType = is3.GeometryType.Point
    layerDef.Color = Colors.Green
    layerDef.MarkerSize = 12
    layerDef.MarkerStyle = is3.SimpleMarkerStyle.Triangle
    layerDef.EnableLabel = True
    layerDef.LabelTextExpression = '[Name]'
    layerWrapper = is3.addGdbLayer(viewWP, layerDef)

    layerDef = is3.LayerDef()
    layerDef.Name = 'Settl_GroundGroup'
    layerDef.GeometryType = is3.GeometryType.Polyline
    layerDef.Color = Colors.Cyan
    layerDef.LineWidth = 3
    layerDef.EnableLabel = True
    layerDef.LabelTextExpression = '[Name]'
    layerWrapper = is3.addGdbLayer(viewWP, layerDef)

    return layerWrapper

def test():
    print("--- Add base map ---")
    viewWP = addBaseMap()
    is3.addGdbLayerLazy(viewWP, 'RetainingPiles', is3.GeometryType.Polygon)
    is3.addGdbLayerLazy(viewWP, 'Strut_Faces', is3.GeometryType.Polygon)
    is3.addGdbLayerLazy(viewWP, 'Strut_Plates', is3.GeometryType.Polygon)
    addLayer(viewWP)

test()
